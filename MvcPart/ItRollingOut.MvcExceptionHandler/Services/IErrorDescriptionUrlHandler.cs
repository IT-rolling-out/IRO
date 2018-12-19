using ItRollingOut.MvcExceptionHandler.Models;

namespace ItRollingOut.MvcExceptionHandler.Services
{
    public interface IErrorDescriptionUrlHandler
    {
        string GenerateUrl(string errorKey);
    }
}
