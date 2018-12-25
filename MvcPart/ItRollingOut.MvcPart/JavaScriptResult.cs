using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.MvcPart
{
    public class JavaScriptResult : ContentResult
    {
        public JavaScriptResult(string script)
        {
            Content = script;
            ContentType = "application/javascript";
        }
    }
}
