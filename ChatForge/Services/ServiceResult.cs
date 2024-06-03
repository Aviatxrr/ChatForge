namespace ChatForge.Services;

public class ServiceResult
{
    public bool Successful { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    public static ServiceResult Success(string message = "", object data = null)
    {
        return new ServiceResult { Successful = true, Message = message, Data = data };
    }

    public static ServiceResult Failure(string message)
    {
        return new ServiceResult { Successful = false, Message = message };
    }
}