
namespace Core.Results.Base
{
    public interface IResult
    {
        StatusTypeEnum Status { get; }
        string Message { get; }
        int StatusCode { get; }
    }
}