//Developer: SangonomiyaSakunovi

namespace FantasyOfSango_DailyMissionRecommendAlgorithm
{
    public class AlgorithmConfig
    {
        //This class must Init before all class in this namespace!
        //You can change the config by the following enums, and we suggest do not call the value directly.
        //Attention! If you Add a new config, you should also set the value in following method after case.
        private MongoDBNameCode mongoDBNameCode = MongoDBNameCode.SangoServerGameDB;
        private MongoDBAddressCode mongoDBAddressCode = MongoDBAddressCode.LocalAddress;

        public static AlgorithmConfig Instance;

        public void InitConfig()
        {
            Instance = this;
        }

        #region MongoDBConfig
        public string GetMongoDBName()
        {
            string mongoDBName = "";
            switch (mongoDBNameCode)
            {
                case MongoDBNameCode.SangoServerGameDB:
                    {
                        mongoDBName = "SangoServerGameDB";
                    }
                    break;
                case MongoDBNameCode.RemoteServerDB:
                    {
                        mongoDBName = "RemoteServerDB";
                    }
                    break;
            }
            return mongoDBName;
        }

        public string GetMongoDBAddress()
        {
            string mongoDBAddress = "";
            switch (mongoDBAddressCode)
            {
                case MongoDBAddressCode.LocalAddress:
                    {
                        mongoDBAddress = "mongodb://127.0.0.1:27017";
                    }
                    break;
                case MongoDBAddressCode.RemoteAddress:
                    {
                        mongoDBAddress = "mongodb://RemoteIP:RemotePort";
                    }
                    break;
            }
            return mongoDBAddress;
        }

        private enum MongoDBNameCode
        {
            SangoServerGameDB,
            RemoteServerDB
        }

        private enum MongoDBAddressCode
        {
            LocalAddress,
            RemoteAddress
        }
        #endregion
    }
}
