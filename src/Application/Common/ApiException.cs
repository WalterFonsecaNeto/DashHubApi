namespace DashHubApi.Application.Common;

public class ExcecaoApi : Exception
{
    public int StatusCode { get; }

    public ExcecaoApi(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
