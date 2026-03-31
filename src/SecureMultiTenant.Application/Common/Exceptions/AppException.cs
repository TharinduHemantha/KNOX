namespace SecureMultiTenant.Application.Common.Exceptions;

public class AppException(string message) : Exception(message);
public sealed class NotFoundException(string message) : AppException(message);
public sealed class ForbiddenException(string message) : AppException(message);
public sealed class ValidationException(string message) : AppException(message);
public sealed class TenantResolutionException(string message) : AppException(message);
