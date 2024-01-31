using System.Net;
using Core.Results.Base;

namespace Core.Results.Results
{
    public class UnauthorizedResult : Result
    {
        public UnauthorizedResult(string message) : base(StatusTypeEnum.Unauthorized, (int)HttpStatusCode.Unauthorized,
            message)
        {
        }

        public UnauthorizedResult() : base(StatusTypeEnum.Unauthorized, (int)HttpStatusCode.Unauthorized)
        {
        }
    }
}