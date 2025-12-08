namespace BaseDock.Application.Abstractions.Messaging;

using BaseDock.Domain.Primitives;

public interface ICommand<TResponse>;

public interface ICommand : ICommand<Result>;
