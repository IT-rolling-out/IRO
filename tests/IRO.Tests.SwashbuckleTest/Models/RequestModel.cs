using System;

namespace IRO.Tests.SwashbuckleTest.Models
{
    /*
    * По итогам теста сгенерированного c# не поддерживается:
    * - decimal
    * - timespan
    * - абстрактные классы (становятся инстанциируемыми)
    * - дженерики через костыль
    * - пользовательские структуры и их nullable работают костыльно.
    * Все остальное работает нормально, включая: 
    * - наследование
    * - енам
    * - классы
    * - нуллабл для стандартных типов.
    * - DateTime   
    */
    public class RequestModel<T1, T2>
        where T1:class
    {
        public string StrProp { get; set; }

        public int IntegerProp { get; set; }

        public decimal DecimalProp { get; set; }

        public double DoubleProp { get; set; }

        public DateTime DateTimeProp { get; set; }

        public TimeSpan TimeSpanProp { get; set; }

        public T1 Gen1 { get; set; }

        public T2 Gen2 { get; set; }

        public CustomStruct CustomStructProp { get; set; }

        public BaseCustomClass BaseCustomClassProp { get; set; }

        public CustomClass CustomClassProp { get; set; }

        public CustomEnum CustomEnumProp { get; set; }

        public int? NullableIntegerProp { get; set; }

        public decimal? NullableDecimalProp { get; set; }

        public double? NullableDoubleProp { get; set; }

        public DateTime? NullableDateTimeProp { get; set; }

        public TimeSpan? NullableTimeSpanProp { get; set; }

        public CustomStruct? NullableCustomStructProp { get; set; }

        public CustomClass NullableCustomClassProp { get; set; }

        public CustomEnum? NullableCustomEnumProp { get; set; }
    }

    
}
