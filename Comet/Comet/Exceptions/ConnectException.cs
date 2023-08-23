namespace Comet.Exceptions;


public enum ConnectErrorCode
{
    BufferInvalid = 1001
}

public class ConnectException : Exception
{
    public ConnectErrorCode ErrorCode { get; }

    public ConnectException(ConnectErrorCode code, string? message) : base(message)
    {
        ErrorCode = code;
    }

}