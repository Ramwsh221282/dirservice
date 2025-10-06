using ResultLibrary;

namespace DirectoryService.UseCases.Common.Cqrs;

public interface ICommandHandler<TResult, in TCommand>
    where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken ct = default);
}

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken ct = default);
}
