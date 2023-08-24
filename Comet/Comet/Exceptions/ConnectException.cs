namespace Comet.Exceptions;


public enum ConnectErrorCode
{
    BufferInvalid = 1001,
    ReceivedBufferTooLarge = 1002,
    ReceivedPacketInvalid = 1003,
}

public class ConnectException : Exception
{
    public ConnectErrorCode ErrorCode { get; }

    public ConnectException(ConnectErrorCode code, string? message = default) : base(message)
    {
        ErrorCode = code;
    }

}