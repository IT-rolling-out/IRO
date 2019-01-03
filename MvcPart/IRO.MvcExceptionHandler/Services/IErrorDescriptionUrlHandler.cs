using IRO.MvcExceptionHandler.Models;

namespace IRO.MvcExceptionHandler.Services
{
    public interface IErrorDescriptionUrlHandler
    {
        string GenerateUrl(string errorKey);
    }
}
