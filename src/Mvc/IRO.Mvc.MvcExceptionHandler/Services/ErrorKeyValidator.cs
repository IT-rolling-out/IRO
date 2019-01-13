using System;
using System.Collections.Generic;
using System.Linq;

namespace IRO.Mvc.MvcExceptionHandler.Services
{
    public class ErrorKeyValidator : IErrorKeyValidator
    {
        public const string AllowedChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        static HashSet<char> _allowedCharsHashSet;

        public ErrorKeyValidator()
        {
            _allowedCharsHashSet = _allowedCharsHashSet ?? AllowedChars.ToHashSet();
        } 


        public bool IsValid(string errorKey)
        {
            bool isValid = true;
            foreach(var c in errorKey)
            {
                if (!_allowedCharsHashSet.Contains(c))
                {
                    isValid = false;
                    break;
                }
            }
            return isValid;
        }
    }
}
