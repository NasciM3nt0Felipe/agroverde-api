namespace AgroVerde.Application.Common;

public class AppResponse<T>
{
    public bool Success { get; set; } = false;

    public string? Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<string> Errors { get; set; } = [];

    public AppResponse(
        bool success,
        string message,
        T? data,
        List<string> errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public static AppResponse<T> Ok(string message, T? data)
    {
        var appResponse = new AppResponse<T>(
            true,
            message,
            data,
            []
        );

        return appResponse;
    }

    public static AppResponse<T> Fail(
        string message,
        List<string> errors)
    {
        var appResponse = new AppResponse<T>(
            false,
            message,
            default,
            errors
        );

        return appResponse;
    }

    public static AppResponse<T> Fail(
        string message,
        Exception? ex = null)
    {
        var appResponse = Fail(message, []);

        if (ex != null)
        {
            appResponse.Errors.Add(ex.Message);
        }

        return appResponse;
    }
}