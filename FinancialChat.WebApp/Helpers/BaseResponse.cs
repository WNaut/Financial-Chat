namespace FinancialChat.WebApp.Helpers
{
    public sealed class BaseResponse
    {
        public string Message { get; }
        public object Data { get; }

        public BaseResponse(string message)
        {
            Message = message;
        }
        
        public BaseResponse(object data, string message = "")
        {
            Data = data;
            Message = message;
        }
    }
}