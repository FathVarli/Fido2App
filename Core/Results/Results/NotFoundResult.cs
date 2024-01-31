using System.Net;
using Core.Results.Base;

namespace Core.Results.Results
{
    public class NotFoundResult : Result
    {
        public NotFoundResult(string message) : base(StatusTypeEnum.NotFound, (int)HttpStatusCode.NotFound, message)
        {
        }

        public NotFoundResult() : base(StatusTypeEnum.NotFound, (int)HttpStatusCode.OK)
        {
        }
    }
}