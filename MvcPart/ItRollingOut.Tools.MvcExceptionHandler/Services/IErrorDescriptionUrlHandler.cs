using ItRollingOut.Tools.MvcExceptionHandler.Models;

namespace ItRollingOut.Tools.MvcExceptionHandler.Services
{
    public interface IErrorDescriptionUrlHandler
    {
        string GenerateUrl(string errorKey);
    }
}
