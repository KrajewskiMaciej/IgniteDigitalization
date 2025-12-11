namespace backend.Exceptions
{
    public class GameException : Exception
    {
        public string ErrorCode { get; }

        public GameException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
