namespace ItRollingOut.MvcExceptionHandler.Services
{
    public interface IErrorKeyValidator
    {
        bool IsValid(string errorKey);
    }
}