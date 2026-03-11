namespace HugoLand.Core.Services
{
    public class ResultService
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ResultService(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static ResultService SuccessResult(string message = "")
        {
            return new ResultService(true, message);
        }

        public static ResultService FailureResult(string message)
        {
            return new ResultService(false, message);
        }
    }
}
