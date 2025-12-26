using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager;

/// <summary>
/// Result wrapper for MT5 operations
/// </summary>
public class MT5Result
{
    public bool IsSuccess { get; }
    public MTRetCode RetCode { get; }
    public string Message { get; }

    protected MT5Result(bool isSuccess, MTRetCode retCode, string message)
    {
        IsSuccess = isSuccess;
        RetCode = retCode;
        Message = message;
    }

    public static MT5Result Success(string message = "Success")
    {
        return new MT5Result(true, MTRetCode.MT_RET_OK, message);
    }

    public static MT5Result Failure(MTRetCode retCode, string message)
    {
        return new MT5Result(false, retCode, message);
    }

    public static MT5Result Failure(string message)
    {
        return new MT5Result(false, MTRetCode.MT_RET_ERROR, message);
    }
}

/// <summary>
/// Result wrapper with data
/// </summary>
public class MT5Result<T> : MT5Result
{
    public T? Data { get; }

    private MT5Result(bool isSuccess, MTRetCode retCode, string message, T? data = default)
        : base(isSuccess, retCode, message)
    {
        Data = data;
    }

    public static MT5Result<T> Success(T data, string message = "Success")
    {
        return new MT5Result<T>(true, MTRetCode.MT_RET_OK, message, data);
    }

    public new static MT5Result<T> Failure(MTRetCode retCode, string message)
    {
        return new MT5Result<T>(false, retCode, message);
    }

    public new static MT5Result<T> Failure(string message)
    {
        return new MT5Result<T>(false, MTRetCode.MT_RET_ERROR, message);
    }
}
