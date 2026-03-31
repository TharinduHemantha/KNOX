namespace Knox.Application.Common.Exceptions;

public class TenantResolutionException : ApplicationException
{
    public TenantResolutionException(string message) : base(message) { }
    public TenantResolutionException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : ApplicationException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors) 
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : this(new Dictionary<string, string[]> { { propertyName, [errorMessage] } })
    {
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.") { }
}

public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException() : base("Access denied.") { }
    public ForbiddenAccessException(string message) : base(message) { }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message) : base(message) { }
}
