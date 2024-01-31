using System.Net;
using Core.Results.Base;

namespace Core.Results.Results
{
    public class ErrorResult : Result
    {
        public ErrorResult(string message) : base(StatusTypeEnum.Failed, (int)HttpStatusCode.BadRequest, message)
        {
        }

        public ErrorResult() : base(StatusTypeEnum.Failed, (int)HttpStatusCode.BadRequest)
        {
        }
    }
}