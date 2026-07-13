namespace EmployeeManagement.Api.Exceptions;

/// <summary>Base type for exceptions the middleware knows how to translate into HTTP responses.</summary>
public abstract class ApiException : Exception
{
    public int StatusCode { get; }

    protected ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(StatusCodes.Status404NotFound, message) { }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message) : base(StatusCodes.Status403Forbidden, message) { }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(StatusCodes.Status400BadRequest, message) { }
}

public class ConflictException : ApiException
{
    public ConflictException(string message) : base(StatusCodes.Status409Conflict, message) { }
}

// Minimal shim so this file doesn't need a using for Microsoft.AspNetCore.Http in every caller.
internal static class StatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status403Forbidden = 403;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
}
