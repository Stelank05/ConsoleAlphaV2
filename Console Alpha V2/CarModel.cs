

namespace Console_Alpha_V2
{
    public class CarModel
    {
        string carModelName, manufacturerName, platformName;
        int mainOVR, backupOVR, bopValue, reliability;

        public CarModel(string modelName, string manuName, string platform, int ovr, int bopScore, int reliScore)
        {
            carModelName = modelName;
            manufacturerName = manuName;
            platformName = platform;

            mainOVR = ovr + bopScore;
            backupOVR = ovr + bopScore;
            bopValue = bopScore;

            reliability = reliScore;
        }

        public CarModel() { }

        public string GetModelName()
        {
            return carModelName;
        }

        public string GetManufacturer()
        {
            return manufacturerName;
        }

        public string GetPlatform()
        {
            return platformName;
        }

        public int GetMainOVR()
        {
            return mainOVR;
        }

        public int GetBackupOVR()
        {
            return backupOVR;
        }

        public int GetBOPValue()
        {
            return bopValue;
        }

        public int GetReliability()
        {
            return reliability;
        }
    }
}
