
namespace Core.Results.Base
{
    public class Result : IResult
    {
        protected Result(StatusTypeEnum status, int statusCode, string message)
            : this(status, statusCode)
        {
            Message = message;
        }

        protected Result(StatusTypeEnum status, int statusCode)
        {
            Status = status;
            StatusCode = statusCode;
        }

        public StatusTypeEnum Status { get; }
        public string Message { get; }
        public int StatusCode { get; }
    }
}