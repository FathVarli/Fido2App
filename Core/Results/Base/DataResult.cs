namespace Core.Results.Base
{
    public class DataResult<T> : Result, IDataResult<T>
    {
        protected DataResult(T data, StatusTypeEnum status, int statusCode, string message) : base(status, statusCode,
            message)
        {
            Data = data;
        }

        protected DataResult(T data, StatusTypeEnum status, int statusCode) : base(status, statusCode)
        {
            Data = data;
        }

        public T Data { get; }
    }
}