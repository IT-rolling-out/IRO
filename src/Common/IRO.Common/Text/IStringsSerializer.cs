using System;

namespace IRO.Common.Text
{
    public interface IStringsSerializer
    {
        object Deserialize(Type type, string val);
        string Serialize(object val);
    }
}