namespace Knox.Application.Abstractions.Cqrs;

public interface ICommand<out TResponse> { }
public interface ICommand : ICommand<Unit> { }

public readonly record struct Unit;
