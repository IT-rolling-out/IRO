using IRO.Mvc.MvcExceptionHandler.Models;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public interface IErrorDescriptionUrlHandler
    {
        string GenerateUrl(string errorKey);
    }
}
