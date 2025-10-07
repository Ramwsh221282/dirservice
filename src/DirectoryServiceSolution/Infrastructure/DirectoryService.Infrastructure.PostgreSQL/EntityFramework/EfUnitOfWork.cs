using DirectoryService.UseCases.Common.UnitOfWork;
using ResultLibrary;

namespace DirectoryService.Infrastructure.PostgreSQL.EntityFramework;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ServiceDbContext _dbContext;
    private readonly Serilog.ILogger _logger;

    public EfUnitOfWork(Serilog.ILogger logger, ServiceDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public async Task<Result> SaveChanges(CancellationToken ct = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch(Exception ex)
        {
            _logger.Fatal("Exception at saving changes: {Ex}", ex);
            return Error.ExceptionalError("Ошибка при транзакции.");
        }
    }
}