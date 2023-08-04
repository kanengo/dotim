namespace ApiGate.Models;

public class ApiReply<TValue>
{
    public int ErrorCode { get; set; }
    
    public string? ErrMsg { get; set; }
    
    public TValue? Data { get; set; }

    public ApiReply(int errorCode, string errMsg, TValue? data): this(errorCode, errMsg)
    {
        Data = data;
    }
    
    public ApiReply(TValue data): this(0, "")
    {
        Data = data;
    }

    public ApiReply(int errorCode, string errMsg)
    {
        ErrorCode = errorCode;
        ErrMsg = errMsg;
    }
}