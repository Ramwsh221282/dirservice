using System.Data;
using System.Text;
using Dapper;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Models;
using ResultLibrary;

namespace DirectoryService.Infrastructure.Identity.Repositories;

public sealed class SqliteIdentityRepository : IIdentityRepository
{
    private readonly IIdentityConnectionFactory _connectionFactory;
    private readonly Dictionary<Guid, SqliteUser> _snapshots = [];

    public SqliteIdentityRepository(IIdentityConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result> Add(
        User user,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        const string sql =
            """
            INSERT INTO users (id, login, password, created_at, updated_at, deleted_at)
            VALUES (@Id, @Login, @Password, @CreatedAt, @UpdatedAt, @DeletedAt)
            """;

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(
                sql,
                new
                {
                    user.Id,
                    user.Login,
                    user.Password,
                    user.CreatedAt,
                    user.UpdatedAt,
                    user.DeletedAt,
                },
                cancellationToken: ct
            );

            await connection.ExecuteAsync(command);
            _snapshots[user.Id] = SqliteUser.FromUser(user);
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
        User user,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        const string sql = "DELETE FROM users WHERE id = @Id";

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            DynamicParameters parameters = new();
            parameters.Add("@Id", user.Id);

            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            await connection.ExecuteAsync(command);

            _snapshots.Remove(user.Id);
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
        User user,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        if (!_snapshots.TryGetValue(user.Id, out SqliteUser? snapshot))
        {
            return Error.ConflictError(
                $"Пользователь с id {user.Id} не отслеживается репозиторием. Обновление невозможно."
            );
        }

        StringBuilder setClause = new();
        DynamicParameters parameters = new();
        parameters.Add("@Id", user.Id);

        if (snapshot.Login != user.Login)
        {
            AppendSetClause(setClause, "login = @Login");
            parameters.Add("@Login", user.Login);
        }

        if (snapshot.Password != user.Password)
        {
            AppendSetClause(setClause, "password = @Password");
            parameters.Add("@Password", user.Password);
        }

        if (snapshot.UpdatedAt != user.UpdatedAt)
        {
            AppendSetClause(setClause, "updated_at = @UpdatedAt");
            parameters.Add("@UpdatedAt", user.UpdatedAt);
        }

        if (snapshot.DeletedAt != user.DeletedAt)
        {
            AppendSetClause(setClause, "deleted_at = @DeletedAt");
            parameters.Add("@DeletedAt", user.DeletedAt);
        }

        if (setClause.Length == 0)
        {
            return Result.Success();
        }

        string sql = $"UPDATE users SET {setClause} WHERE id = @Id";
        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            await connection.ExecuteAsync(command);

            _snapshots[user.Id] = SqliteUser.FromUser(user);
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

    public async Task<User?> Get(
        UserSpecification specification,
        CancellationToken ct = default,
        IIdentityTransactionScope? transaction = null)
    {
        if (specification.IsEmpty)
        {
            return null;
        }

        StringBuilder whereClause = new("WHERE 1=1");
        DynamicParameters parameters = new();

        if (specification.Id.HasValue)
        {
            whereClause.Append(" AND id = @Id");
            parameters.Add("@Id", specification.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(specification.Login))
        {
            whereClause.Append(" AND login = @Login");
            parameters.Add("@Login", specification.Login);
        }

        string sql = $"""
            SELECT
                id as Id,
                login as Login,
                password as Password,
                created_at as CreatedAt,
                updated_at as UpdatedAt,
                deleted_at as DeletedAt
            FROM users
            {whereClause}
            """;

        (IDbConnection connection, bool owned) = ResolveConnection(transaction);
        try
        {
            CommandDefinition command = new(sql, parameters, cancellationToken: ct);
            User? user = await connection.QuerySingleOrDefaultAsync<User>(command);

            if (user != null)
            {
                _snapshots[user.Id] = SqliteUser.FromUser(user);
            }

            return user;
        }
        finally
        {
            if (owned)
            {
                connection.Dispose();
            }
        }
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
