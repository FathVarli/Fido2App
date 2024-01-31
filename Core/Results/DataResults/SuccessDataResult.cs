using System.Net;
using Core.Results.Base;

namespace Core.Results.DataResults
{
    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, string message) : base(data, StatusTypeEnum.Success, (int)HttpStatusCode.OK,
            message)
        {
        }

        public SuccessDataResult(T data) : base(data, StatusTypeEnum.Success, (int)HttpStatusCode.OK)
        {
        }

        public SuccessDataResult(string message) : base(default, StatusTypeEnum.Success, (int)HttpStatusCode.OK,
            message)
        {
        }

        public SuccessDataResult() : base(default, StatusTypeEnum.Success, (int)HttpStatusCode.OK)
        {
        }
    }
}