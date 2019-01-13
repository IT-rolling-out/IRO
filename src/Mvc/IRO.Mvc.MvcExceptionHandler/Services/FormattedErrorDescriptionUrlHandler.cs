using IRO.Mvc.MvcExceptionHandler.Models;
using System;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public class FormattedErrorDescriptionUrlHandler : IErrorDescriptionUrlHandler
    {
        readonly string _urlFormatter;

        public FormattedErrorDescriptionUrlHandler(string urlFormatter)
        {
            //Умнее проверки я не придумал.
            try
            {
                const string checkText = "!CHECK_TEXT_HERE!";
                string genUrl = string.Format(urlFormatter, checkText);
                if (!genUrl.Contains(checkText))
                    throw new Exception();
            }
            catch
            {
                throw new ErrorHandlerException($"String '{urlFormatter}' must contains formatter {{0}}.");
            }            
            _urlFormatter = urlFormatter;
        }


        public string GenerateUrl(string errorKey)
        {
            return string.Format(_urlFormatter, errorKey);
        }
    }
}
