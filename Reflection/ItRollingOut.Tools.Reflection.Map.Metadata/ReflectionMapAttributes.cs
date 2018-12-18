using System;
using System.Collections.Generic;
using System.Text;

namespace ItRollingOut.Tools.Reflection.Map.Metadata
{
    /// <summary>
    /// Этим атрибутом вы должны отметить методы, которые хотите отобразить в ReflectionMap.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodReflectionMapAttribute : BaseReflectionMapAttribute
    {        
    }

    /// <summary>
    /// Этим атрибутом вы должны отметить поля, которые хотите преобразовать в методы в ReflectionMap.
    /// Еще раз, вместо поля вы получите методы get_[имя свойства] и  set_[имя свойства].
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SimplePropReflectionMapAttribute : BaseReflectionMapAttribute
    {
        public bool CanGet { get; set; } = true;
        public bool CanSet { get; set; } = true;

        public SimplePropReflectionMapAttribute()
        {
            ///throw new Exception("SimpleProp was deprecated becouse propertyes can`t be normally wrapped to PureApiResponse<>.");
            //Can be used in some cases, but not where you use PureApiResponse
        }
    }

    /// <summary>
    /// Этим атрибутом отмечайте поля, для которых вы также хотите построить ReflectionMap. Их методы станут доступны
    /// через такую конструкцию [имя свойства].[имя метода]
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IncludedObjReflectionMapAttribute : BaseReflectionMapAttribute
    {
        
    }

    public abstract class BaseReflectionMapAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
