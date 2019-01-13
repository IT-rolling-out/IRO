using System;

namespace IRO.Mvc.PureBinding.Metadata
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PureBindingMethodSettingsAttribute : Attribute
    {
        /// <summary>
        /// You can set here name of the http parameter, which value will be used for model binding.
        /// Note, that it will be ignored, when you sending all object as body of method (not as just one parameter).
        /// By default it is 'r'.
        /// </summary>
        public string NameGlobalHttpParameter { get; set; }
    }
}
