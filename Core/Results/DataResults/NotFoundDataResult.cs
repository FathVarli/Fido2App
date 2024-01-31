using System.Net;
using Core.Results.Base;

namespace Core.Results.DataResults
{
    public class NotFoundDataResult<T> : DataResult<T>
    {
        public NotFoundDataResult(T data, string message) : base(data, StatusTypeEnum.NotFound,
            (int)HttpStatusCode.NotFound, message)
        {
        }

        public NotFoundDataResult(T data) : base(data, StatusTypeEnum.NotFound, (int)HttpStatusCode.NotFound)
        {
        }

        public NotFoundDataResult(string message) : base(default, StatusTypeEnum.NotFound, (int)HttpStatusCode.NotFound,
            message)
        {
        }

        public NotFoundDataResult() : base(default, StatusTypeEnum.NotFound, (int)HttpStatusCode.NotFound)
        {
        }
    }
}