namespace DocsifyNet
{
    public class ApiResult<T>
    {
        public int Code { get; set; }
        public string Msg { get; set; } = "OK";
        public T? Data { get; set; } = default(T?);
    }
}
