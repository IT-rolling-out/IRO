using System;

namespace IRO.Storage.DefaultStorages
{
    public interface IStorageSerializer
    {
        object Deserialize(Type type, string val);
        string Serialize(object val);
    }
}