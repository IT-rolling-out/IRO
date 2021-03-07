﻿using IRO.Storage.WithLiteDB;

namespace IRO.UnitTests.Storage
{
    public class LiteDbStorageTests : BaseStorageTests
    {
        public LiteDbStorageTests()
        {
            Storage = new LiteDatabaseStorage();
        }
    }
}