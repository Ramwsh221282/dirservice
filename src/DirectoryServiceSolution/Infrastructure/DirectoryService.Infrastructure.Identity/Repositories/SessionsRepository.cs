using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Repositories;

public sealed class SessionsRepository : ISessionsRepository
{
    private readonly IIdentityConnectionFactory _connectionFactory;
    private readonly Dictionary<Guid, SessionSnapshot> _snapshots = [];

    public SessionsRepository(IIdentityConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result> Add(
        Session session,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        const string sql =
            """
            INSERT INTO sessions (
                session_id, user_id, refresh_token, access_token,
                created_at, access_token_expires_at, refresh_token_expires_at
            )
            VALUES (
                @SessionId, @UserId, @RefreshToken, @AccessToken,
                @CreatedAt, @AccessTokenExpiresAt, @RefreshTokenExpiresAt
            )
            """;

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(
                sql,
                new
                {
                    session.SessionId,
                    session.UserId,
                    session.RefreshToken,
                    session.AccessToken,
                    session.CreatedAt,
                    session.AccessTokenExpiresAt,
                    session.RefreshTokenExpiresAt,
                },
                cancellationToken: ct
            );

            await connection.ExecuteAsync(command);
            _snapshots[session.SessionId] = SessionSnapshot.FromSession(session);
            return Result.Success();
        }
        finally
        {
            if (owned)
            {
                connection.Dispose();
            }
        }
    }

    public async Task<Result> Delete(
        Session session,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        const string sql = "DELETE FROM sessions WHERE session_id = @SessionId";

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            DynamicParameters parameters = new();
            parameters.Add("@SessionId", session.SessionId);

            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            await connection.ExecuteAsync(command);

            _snapshots.Remove(session.SessionId);
            return Result.Success();
        }
        finally
        {
            if (owned)
            {
                connection.Dispose();
            }
        }
    }

    public async Task<Result> Update(
        Session session,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        if (!_snapshots.TryGetValue(session.SessionId, out SessionSnapshot? snapshot))
        {
            return Error.ConflictError(
                $"Сессия с id {session.SessionId} не отслеживается репозиторием. Обновление невозможно."
            );
        }

        StringBuilder setClause = new();
        DynamicParameters parameters = new();
        parameters.Add("@SessionId", session.SessionId);

        if (snapshot.RefreshToken != session.RefreshToken)
        {
            AppendSetClause(setClause, "refresh_token = @RefreshToken");
            parameters.Add("@RefreshToken", session.RefreshToken);
        }

        if (snapshot.AccessToken != session.AccessToken)
        {
            AppendSetClause(setClause, "access_token = @AccessToken");
            parameters.Add("@AccessToken", session.AccessToken);
        }

        if (snapshot.AccessTokenExpiresAt != session.AccessTokenExpiresAt)
        {
            AppendSetClause(setClause, "access_token_expires_at = @AccessTokenExpiresAt");
            parameters.Add("@AccessTokenExpiresAt", session.AccessTokenExpiresAt);
        }

        if (snapshot.RefreshTokenExpiresAt != session.RefreshTokenExpiresAt)
        {
            AppendSetClause(setClause, "refresh_token_expires_at = @RefreshTokenExpiresAt");
            parameters.Add("@RefreshTokenExpiresAt", session.RefreshTokenExpiresAt);
        }

        if (setClause.Length == 0)
        {
            return Result.Success();
        }

        string sql = $"UPDATE sessions SET {setClause} WHERE session_id = @SessionId";
        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            await connection.ExecuteAsync(command);

            _snapshots[session.SessionId] = SessionSnapshot.FromSession(session);
            return Result.Success();
        }
        finally
        {
            if (owned)
            {
                connection.Dispose();
            }
        }
    }

    public async Task<Session?> Get(
        SessionSpecification specification,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        if (specification.IsEmpty)
        {
            return null;
        }

        StringBuilder whereClause = new("WHERE 1=1");
        DynamicParameters parameters = new();

        if (specification.SessionId.HasValue)
        {
            whereClause.Append(" AND session_id = @SessionId");
            parameters.Add("@SessionId", specification.SessionId.Value);
        }

        if (specification.UserId.HasValue)
        {
            whereClause.Append(" AND user_id = @UserId");
            parameters.Add("@UserId", specification.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(specification.RefreshToken))
        {
            whereClause.Append(" AND refresh_token = @RefreshToken");
            parameters.Add("@RefreshToken", specification.RefreshToken);
        }

        if (!string.IsNullOrWhiteSpace(specification.AccessToken))
        {
            whereClause.Append(" AND access_token = @AccessToken");
            parameters.Add("@AccessToken", specification.AccessToken);
        }

        string sql = $"""
            SELECT
                session_id as SessionId,
                user_id as UserId,
                refresh_token as RefreshToken,
                access_token as AccessToken,
                created_at as CreatedAt,
                access_token_expires_at as AccessTokenExpiresAt,
                refresh_token_expires_at as RefreshTokenExpiresAt
            FROM sessions
            {whereClause}
            """;

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            Session? session = await connection.QuerySingleOrDefaultAsync<Session>(command);

            if (session != null)
            {
                _snapshots[session.SessionId] = SessionSnapshot.FromSession(session);
            }

            return session;
        }
        finally
        {
            if (owned)
            {
                connection.Dispose();
            }
        }
    }

    public async Task<Result> ClearExpiredAccessTokens(CancellationToken ct = default)
    {
        const string sql =
            """
            UPDATE sessions
            SET access_token = NULL
            WHERE access_token IS NOT NULL AND access_token_expires_at <= @Now
            """;

        using IDbConnection connection = _connectionFactory.Create();
        DynamicParameters parameters = new();
        parameters.Add("@Now", DateTime.UtcNow);

        CommandDefinition command = new(sql, parameters, cancellationToken: ct);
        await connection.ExecuteAsync(command);

        return Result.Success();
    }

    private (IDbConnection Connection, bool Owned) ResolveConnection(IIdentityTransactionScope? transaction)
    {
        if (transaction == null)
        {
            return (_connectionFactory.Create(), true);
        }

        if (transaction is not IInternalConnectionAccessor accessor)
        {
            throw new InvalidOperationException(
                $"Переданная транзакция типа {transaction.GetType().Name} не поддерживается репозиторием."
            );
        }

        return (accessor.Connection, false);
    }

    private static void AppendSetClause(StringBuilder setClause, string clause)
    {
        if (setClause.Length > 0)
        {
            setClause.Append(", ");
        }

        setClause.Append(clause);
    }
}
