using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Console_Alpha_V2
{
    public class Series
    {
        string seriesName, seriesAbbreviation;
        int maxEntries, seriesIndex, classSpacer;

        List<string> eligiblePlatforms;
        List<int> calendarSpacers, entrantSpacers;

        List<CarModel> modelList;
        List<Class> classList;
        List<Entrant> entryList;
        List<Round> calendar;

        public Series(string name, string abbreviation, int entries, int index)
        {
            seriesName = name;
            seriesAbbreviation = abbreviation;

            maxEntries = entries;
            seriesIndex = index;

            eligiblePlatforms = new List<string>();
            entrantSpacers = new List<int>();

            entryList = new List<Entrant>();

            LoadClasses();
            LoadGarageValues();
            LoadModelList();
            LoadCalendar();
        }

        public Series() { }

        private void LoadClasses()
        {
            classList = new List<Class>();
            Class newClass;

            List<string> platformList;
            string[] platforms;

            string classFile = Path.Combine(CommonData.GetSetupPath(), string.Format("Series - {0}", seriesAbbreviation), "Class Setup.csv");

            string[] classData = FileHandler.ReadFile(classFile);
            string[] splitLine;

            classSpacer = 0;

            for (int i = 1; i < classData.Length; i++)
            {
                splitLine = classData[i].Split(',');

                if (splitLine[1] != "")
                {
                    platformList = new List<string>();
                    platforms = splitLine[3].Split('/');

                    foreach (string platform in platforms)
                    {
                        platformList.Add(platform);

                        if (!eligiblePlatforms.Contains(platform))
                        {
                            eligiblePlatforms.Add(platform);
                        }
                    }

                    newClass = new Class(splitLine, platformList, i);

                    if (newClass.GetClassName().Length > classSpacer)
                    {
                        classSpacer = newClass.GetClassName().Length;
                    }

                    classList.Add(newClass);
                }
            }
        }

        private void LoadGarageValues()
        {
            string[] garageData = FileHandler.ReadFile(Path.Combine(CommonData.GetSetupPath(), string.Format("Series - {0}", seriesAbbreviation), "Time Lost In Garage.csv")), classData;

            for (int i = 1; i < garageData.Length; i++)
            {
                classData = garageData[i].Split(',');

                if (i <= classList.Count())
                {
                    classList[i - 1].SetWECGarageValues(Convert.ToInt32(classData[3]), Convert.ToInt32(classData[4]), Convert.ToInt32(classData[5]));
                    classList[i - 1].SetIMSAGarageValues(Convert.ToInt32(classData[7]), Convert.ToInt32(classData[8]), Convert.ToInt32(classData[9]));
                    classList[i - 1].SetLapGarageValues(Convert.ToInt32(classData[11]), Convert.ToInt32(classData[12]), Convert.ToInt32(classData[13]));
                }
            }
        }

        private void LoadModelList()
        {
            modelList = new List<CarModel>();

            foreach (CarModel model in CommonData.GetModelList())
            {
                if (eligiblePlatforms.Contains(model.GetPlatform()))
                {
                    modelList.Add(model);
                }
            }
        }

        private void LoadCalendar()
        {
            string[] calendarData = FileHandler.ReadFile(Path.Combine(CommonData.GetSetupPath(), string.Format("Series - {0}", seriesAbbreviation), "Calendar.csv")), roundData;

            List<string> racingClasses = new List<string>();

            calendarSpacers = new List<int>();

            calendar = new List<Round>();

            for (int i = 1; i < calendarData.Length; i++)
            {
                roundData = calendarData[i].Split(',');

                if (calendarSpacers.Count() == 0)
                {
                    calendarSpacers.Add(roundData[0].Length);
                    calendarSpacers.Add(roundData[1].Length);
                }

                else
                {
                    if (roundData[0].Length > calendarSpacers[0])
                    {
                        calendarSpacers[0] = roundData[0].Length;
                    }

                    if (roundData[1].Length > calendarSpacers[1])
                    {
                        calendarSpacers[1] = roundData[1].Length;
                    }
                }

                racingClasses.Clear();

                for (int j = 14; j < roundData.Length; j++)
                {
                    racingClasses.Add(roundData[j]);
                }

                calendar.Add(new Round(roundData, racingClasses, this));
            }
        }

        public string GetSeriesName()
        {
            return seriesName;
        }

        public string GetAbbreviation()
        {
            return seriesAbbreviation;
        }

        public int GetMaxEntries()
        {
            return maxEntries;
        }

        public int GetSeriesIndex()
        {
            return seriesIndex;
        }

        public List<Round> GetCalendar()
        {
            return calendar;
        }

        public bool IsLastRound(Round checkRound)
        {
            return checkRound == calendar[calendar.Count() - 1];
        }

        public List<Class> GetClassList()
        {
            return classList;
        }

        public void LoadEntryList(int seasonNumber)
        {
            entryList.Clear();

            CarModel model;
            Class enteredClass;

            Entrant newEntrant;

            string basePath, filePath;
            string[] entrantData, splitLine;
            int entrantIndex = 1;

            entrantSpacers = new List<int>();

            if (seasonNumber == 1)
            {
                basePath = Path.Combine(CommonData.GetSetupPath(), string.Format("Series - {0}", seriesAbbreviation), CommonData.GetEntrantsFolder());
            }

            else
            {
                basePath = Path.Combine(CommonData.GetSeasonFolder(), seriesName, CommonData.GetEntrantsFolder());
            }

            for (int classIndex = 0; classIndex <  classList.Count(); classIndex++)
            {
                filePath = Path.Combine(basePath, string.Format("Class {0}.csv", classIndex + 1));

                entrantData = FileHandler.ReadFile(filePath);

                for (int i = 0; i < entrantData.Length; i++)
                {
                    if (entrantData[i] != "")
                    {
                        splitLine = entrantData[i].Split(',');

                        model = GetCarModel(splitLine[3]);
                        enteredClass = GetClass(splitLine[0]);

                        newEntrant = new Entrant(splitLine[1], splitLine[2], Convert.ToInt32(splitLine[5]), Convert.ToInt32(splitLine[6]), Convert.ToInt32(splitLine[8]), Convert.ToInt32(splitLine[10]), entrantIndex, model, enteredClass, this);
                        entryList.Add(newEntrant);

                        entrantIndex++;
                        UpdateEntrantSpacers(newEntrant);
                    }
                }
            }
        }

        public void LoadEntryList(string basePath, Team playerTeam)
        {
            entryList.Clear();

            CarModel model;
            Class enteredClass;

            Entrant newEntrant;

            basePath = Path.Combine(basePath, seriesName, "Entrants");

            string filePath;
            string[] entrantData, splitLine;
            int entrantIndex = 1;

            entrantSpacers = new List<int>();

            for (int classIndex = 0; classIndex < classList.Count(); classIndex++)
            {
                filePath = Path.Combine(basePath, string.Format("Class {0}.csv", classIndex + 1));

                entrantData = FileHandler.ReadFile(filePath);

                for (int i = 0; i < entrantData.Length; i++)
                {
                    if (entrantData[i] != "")
                    {
                        splitLine = entrantData[i].Split(',');

                        model = GetCarModel(splitLine[3]);
                        enteredClass = GetClass(splitLine[0]);

                        newEntrant = new Entrant(splitLine[1], splitLine[2], Convert.ToInt32(splitLine[5]), Convert.ToInt32(splitLine[6]), Convert.ToInt32(splitLine[8]), Convert.ToInt32(splitLine[10]), entrantIndex, model, enteredClass, this);
                        entryList.Add(newEntrant);

                        entrantIndex++;
                        UpdateEntrantSpacers(newEntrant);
                    }
                }
            }

            foreach (Entrant playerCrew in playerTeam.GetTeamEntries())
            {
                if (playerCrew.GetSeriesName() == seriesName)
                {
                    playerCrew.SetIndex(entrantIndex);
                    entrantIndex++;

                    entryList.Add(playerCrew);
                }
            }
        }

        public void ClearEntryList()
        {
            entryList.Clear();
        }

        public void AddEntrant(Entrant newEntrant)
        {
            entryList.Add(newEntrant);
            UpdateEntrantSpacers(newEntrant);
        }

        public List<Entrant> GetEntryList()
        {
            return entryList;
        }

        private CarModel GetCarModel(string modelName)
        {
            foreach (CarModel checkModel in modelList)
            {
                if (checkModel.GetModelName() == modelName)
                {
                    return checkModel;
                }
            }

            return new CarModel();
        }

        private Class GetClass(string className)
        {
            foreach (Class checkClass in classList)
            {
                if (checkClass.GetClassName() == className)
                {
                    return checkClass;
                }
            }

            return new Class();
        }

        private void UpdateEntrantSpacers(Entrant newEntrant)
        {
            if (entrantSpacers.Count() == 0)
            {
                entrantSpacers.Add(newEntrant.GetCarNo().Length);
                entrantSpacers.Add(newEntrant.GetTeamName().Length);
                entrantSpacers.Add(newEntrant.GetManufacturer().Length);
                entrantSpacers.Add(newEntrant.GetModelName().Length);
            }

            else
            {
                if (newEntrant.GetCarNo().Length > entrantSpacers[0])
                {
                    entrantSpacers[0] = newEntrant.GetCarNo().Length;
                }

                if (newEntrant.GetTeamName().Length > entrantSpacers[1])
                {
                    entrantSpacers[1] = newEntrant.GetTeamName().Length;
                }

                if (newEntrant.GetManufacturer().Length > entrantSpacers[2])
                {
                    entrantSpacers[2] = newEntrant.GetManufacturer().Length;
                }

                if (newEntrant.GetModelName().Length > entrantSpacers[3])
                {
                    entrantSpacers[3] = newEntrant.GetModelName().Length;
                }
            }
        }

        public List<int> GetEntrantSpacers()
        {
            return entrantSpacers;
        }

        public int GetClassSpacer()
        {
            return classSpacer;
        }
    }
}
