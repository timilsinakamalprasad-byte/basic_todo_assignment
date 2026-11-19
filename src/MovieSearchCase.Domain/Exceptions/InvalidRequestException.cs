namespace MovieSearchCase.Domain.Exceptions;

public class InvalidRequestException : ProblemDetailsException
{
    public InvalidRequestException(string details)
        : base("Invalid request", ErrorType.InvalidRequest, details)
    {
    }
}
