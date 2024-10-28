using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Console_Alpha_V2
{
    public class Game
    {
        string roundLength, saveFolder, currentRoundName, waypointString;
        int fileNumber, racingCount, seasonNumber, currentRaceNumber, startingRound, minimumTeamNameLength, raceLength, currentWaypoint;
        bool playGame, setManufacturers = true, teamCompeting, crewCompeting;

        Random randomiser;

        Entrant currentEntrant;
        Round currentRound;
        Series currentSeries;
        Simulator gameSimulator;
        Team playerTeam;

        List<string> foundManufacturers;
        List<int> pointsSystem, entrantSpacers, spacerList;

        List<Class> classList;
        List<Entrant> entryList;
        List<Round> roundOrder;
        List<Series> seriesList;


        // Game Setup Functions

        public Game()
        {
            CommonData.Setup();

            seasonNumber = 1;
            minimumTeamNameLength = 5;

            foundManufacturers = new List<string>();

            roundOrder = new List<Round>();

            randomiser = new Random();
            gameSimulator = new Simulator(randomiser);

            playGame = true;

            LoadSeries();
            SetRoundOrder();
        }

        private void LoadSeries()
        {
            seriesList = new List<Series>();

            string[] seriesData = FileHandler.ReadFile(CommonData.GetSeriesFile()), splitLine;

            Series newSeries;

            for (int i = 1; i < seriesData.Length; i++)
            {
                splitLine = seriesData[i].Split(',');

                newSeries = new Series(splitLine[0], splitLine[1], Convert.ToInt32(splitLine[2]), i);
                newSeries.LoadEntryList(seasonNumber);
                seriesList.Add(newSeries);
            }
        }

        private void SetRoundOrder()
        {
            foreach (Series series in seriesList)
            {
                foreach (Round round in series.GetCalendar())
                {
                    roundOrder.Add(round);
                }
            }

            bool swap;

            for (int i = 0; i < roundOrder.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < roundOrder.Count() - i - 1; j++)
                {
                    if (roundOrder[j].GetRaceNumber() > roundOrder[j + 1].GetRaceNumber())
                    {
                        swap = true;

                        (roundOrder[j], roundOrder[j + 1]) = (roundOrder[j + 1], roundOrder[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }


        // Play Game Functions

        public void PlayGame()
        {
            if (!FileHandler.SavesExist())
            {
                SetupPlayerTeam();
                FileHandler.SetGameSaveFolder(playerTeam);
                
                startingRound = 0;
            }

            else
            {
                StartGame();
            }

            DisplayTeamDetails(true);

            Console.ReadLine();

            Console.Clear();

            while (playGame)
            {
                Console.Clear();

                FileHandler.SetSeasonFolder(seasonNumber, seriesList);

                if (seasonNumber == 1)
                {
                    FileHandler.SetSeasonFolder(seasonNumber, seriesList);
                }

                SetSeasonEntryLists();

                if (setManufacturers)
                {
                    LoadManufacturers();
                }

                FileHandler.WriteTeamData(playerTeam, seasonNumber);

                for (currentRaceNumber = startingRound; currentRaceNumber < roundOrder.Count(); currentRaceNumber++)
                {
                    currentRound = roundOrder[currentRaceNumber];
                    currentSeries = currentRound.GetSeries();

                    currentRoundName = currentRound.GetRoundName();

                    saveFolder = FileHandler.GetSaveFolder(currentRound);
                    fileNumber = 1;
                    currentWaypoint = 1;

                    gameSimulator.SetRound(currentRound);

                    teamCompeting = playerTeam.GetCompeting(currentSeries.GetSeriesName());
                    SetEntryList();

                    if (teamCompeting)
                    {
                        crewCompeting = GetCrewCompeting();
                    }

                    entrantSpacers = currentSeries.GetEntrantSpacers();

                    switch (currentRound.GetLengthType())
                    {
                        case "WEC":
                            roundLength = CommonData.GetDistancesList("WEC")[currentRound.GetRaceLength() - 1];
                            break;
                        case "IMSA":
                            roundLength = CommonData.GetDistancesList("IMSA")[currentRound.GetRaceLength() - 1];
                            break;
                        case "Laps":
                            roundLength = currentRound.GetRaceLength() + " Laps";
                            break;
                    }

                    Console.WriteLine("Season {0} - Race {1}:\n{2} R{3}\n{4} - {5}\n{6}\n", seasonNumber, currentRaceNumber + 1, currentSeries.GetSeriesName(), currentRound.GetRoundNumber(), currentRoundName, currentRound.GetTrackName(), roundLength);

                    for (int i = 0; i < 3; i++)
                    {
                        gameSimulator.Qualifying(entryList, racingCount);
                    }

                    SetPositions();
                    SaveResults("Qualifying Results");

                    gameSimulator.SetGrid(entryList, 5);

                    if (teamCompeting)
                    {
                        if (crewCompeting)
                        {
                            DisplayTeamEntrants(string.Format("{0} Qualifying Results for The {1}", playerTeam.GetTeamName(), currentRoundName));
                        }

                        DisplayClassLeaders(string.Format("Pole Sitters for The {0}", currentRoundName));
                        DisplayEntrants(string.Format("Qualifying Results for The {0}", currentRoundName));
                    }

                    raceLength = currentRound.GetRaceLength();

                    if (currentRound.GetLengthType() == "Laps")
                    {
                        waypointString = string.Format("{0} Laps", raceLength / 4);
                    }

                    else
                    {
                        waypointString = CommonData.GetDistancesList(currentRound.GetLengthType())[(raceLength / 4) - 1];
                    }

                    for (int stintNumber = 1; stintNumber < raceLength; stintNumber++)
                    {
                        gameSimulator.Race(entryList, stintNumber, racingCount);

                        if (teamCompeting && stintNumber == currentWaypoint * (raceLength / 4))
                        {
                            SetPositions();

                            SaveResults(waypointString);

                            if (crewCompeting)
                            {
                                DisplayTeamEntrants(string.Format("{0} Running Positions after {1}", playerTeam.GetTeamName(), waypointString));
                            }

                            DisplayClassLeaders(string.Format("{0} Class Leaders after {1}", currentRoundName, waypointString));
                            DisplayEntrants(string.Format("{0} Running Order after {1}", currentRoundName, waypointString));

                            currentWaypoint++;

                            if (currentRound.GetLengthType() == "Laps")
                            {
                                waypointString = string.Format("{0} Laps", currentWaypoint * (raceLength / 4));
                            }

                            else
                            {
                                waypointString = CommonData.GetDistancesList(currentRound.GetLengthType())[(currentWaypoint * (raceLength / 4)) - 1];
                            }
                        }
                    }

                    for (int i = 0; i < entryList.Count(); i++)
                    {
                        if (entryList[i].GetRacing() && entryList[i].GetInGarage() && entryList[i].GetOVR() != 1)
                        {
                            entryList[i].UpdateOVR(100);
                        }
                    }

                    gameSimulator.Sort(entryList, 0, racingCount);

                    SetPositions();

                    for (int i = 0; i < entryList.Count(); i++)
                    {
                        if (entryList[i].GetRacing())
                        {
                            entryList[i].AddResult();
                        }
                    }

                    SaveResults("Race Results");

                    if (teamCompeting)
                    {
                        if (crewCompeting)
                        {
                            DisplayTeamEntrants(string.Format("{0} Race Results for The {1}", playerTeam.GetTeamName(), currentRoundName));
                        }

                        DisplayClassLeaders(string.Format("Class Winners for The {0}", currentRoundName));
                        DisplayEntrants(string.Format("Race Results for The {0}", currentRoundName));
                    }

                    else
                    {
                        DisplayClassLeaders(string.Format("Class Winners for The {0}", currentRoundName));
                        DisplayEntrants(string.Format("Race Results for The {0}", currentRoundName));
                    }

                    AwardEntrantPoints();
                    AwardManufacturersPoints();
                    StandingsSort(entryList);
                    SetStandingsPositions();

                    SaveStandings(Path.Combine(saveFolder, "Post Race Standings"));

                    if (teamCompeting)
                    {
                        DisplayTeamPoints(string.Format("{0} Points + Positions after The {1}:", playerTeam.GetTeamName(), currentRoundName));
                        DisplayStandings(string.Format("Standings after The {0}:\n", currentRoundName));
                    }
                    
                    else
                    {
                        DisplayPointsLeaders(string.Format("Championship Leaders after The {0}", currentRoundName));
                    }

                    if (currentSeries.IsLastRound(currentRound))
                    {
                        SaveStandings(Path.Combine(CommonData.GetSeasonFolder(), currentSeries.GetSeriesName(), "Final Standings"));

                        Console.ReadLine();
                        Console.Clear();

                        DisplayFinalStandings(string.Format("{0} Class Champions:", currentSeries.GetSeriesName()));
                    }

                    SaveGame(currentRaceNumber);
                    
                    Console.Clear();
                }

                playerTeam.SaveTeamResults(seasonNumber);

                playGame = GetBoolean(string.Format("Continue to Season {0}?", seasonNumber + 1));
                Console.Clear();

                if (playGame)
                {
                    seasonNumber++;
                    startingRound = 0;
                    setManufacturers = true;

                    FileHandler.SetSeasonFolder(seasonNumber, seriesList);

                    if (GetBoolean("Make Team Changes?"))
                    {
                        Console.Clear();
                        UpdatePlayerTeam();
                    }

                    else
                    {
                        playerTeam.UpdateCrewStats(randomiser);
                    }

                    FileHandler.WriteTeamData(playerTeam, seasonNumber);
                    UpdateGameCrewStats();
                    WriteEntryLists();

                    SaveGame(-1);

                    setManufacturers = true;
                }

                else
                {
                    Console.WriteLine("Thank you for Playing!");
                    Console.WriteLine("Press Enter to Exit...");
                    Console.ReadLine();
                }
            }
        }


        // Save Loading

        private void StartGame()
        {
            string saveName = SelectSave();

            Console.Clear();

            if (saveName == "Create Team")
            {
                SetupPlayerTeam();
                FileHandler.SetGameSaveFolder(playerTeam);
                
                startingRound = 0;
            }

            else
            {
                string saveFolder = Path.Combine(CommonData.GetMainFolder(), "Saves", saveName);
                CommonData.SetSaveFolder(saveFolder);

                LoadGame();

                setManufacturers = false;
            }
        }

        private void LoadGame()
        {
            string[] gameData = FileHandler.ReadFile(Path.Combine(CommonData.GetSaveFolder(), "Current Race.csv"));

            seasonNumber = Convert.ToInt32(gameData[0].Split(',')[1]);
            startingRound = Convert.ToInt32(gameData[1].Split(',')[1]);

            string seasonFolder = Path.Combine(CommonData.GetSaveFolder(), string.Format("Season {0}", seasonNumber));

            CommonData.SetSeasonFolder(seasonFolder);
            seasonFolder = Path.Combine(seasonFolder, "Save Data");

            LoadTeam(seasonFolder);
            playerTeam.SetSpacerList();

            for (int seriesIndex = 0; seriesIndex < seriesList.Count(); seriesIndex++)
            {
                currentSeries = seriesList[seriesIndex];

                LoadEntryList(seasonFolder);
                LoadManufacturers(seasonFolder);
            }
        }

        private string SelectSave()
        {
            string[] availableSaves = FileHandler.GetSaves();

            for (int saveIndex = 0; saveIndex < availableSaves.Length; saveIndex++)
            {
                Console.WriteLine("Save {0}: {1}", saveIndex + 1, availableSaves[saveIndex].Replace(Path.Combine(CommonData.GetMainFolder(), "Saves"), ""));
            }
            Console.WriteLine("C: Create Save");
            Console.Write("Choice: ");

            string saveChoice = Console.ReadLine().ToLower();
            int saveNumber;

            if (int.TryParse(saveChoice, out saveNumber))
            {
                if (saveNumber > 0 && saveNumber <= availableSaves.Length)
                {
                    return availableSaves[saveNumber - 1];
                }
            }

            else if (saveChoice == "c" || saveChoice == "create save")
            {
                return "Create Team";
            }

            else
            {
                foreach (string save in availableSaves)
                {
                    if (saveChoice == save.ToLower())
                    {
                        return save;
                    }
                }
            }

            Console.WriteLine("Invalid Save Selected");
            return SelectSave();
        }

        private void LoadTeam(string folderPath)
        {
            string teamFile = Path.Combine(folderPath, "Team Details.csv");

            string[] teamData = FileHandler.ReadFile(teamFile);

            string teamName = teamData[0].Split(',')[0];
            int teamOVR = Convert.ToInt32(teamData[1].Split(',')[1]);

            playerTeam = new Team(teamName, teamOVR);

            string[] entrantData;

            string carNumber, bestResult;
            int crewOVR, stintMod, crewReliability, points, timesFinished;

            List<string> pastResults;
            List<int> rawResults;

            CarModel enteredModel;
            Class enteredClass;
            Entrant newEntrant;
            Series enteredSeries;

            for (int i = 3; i < teamData.Length; i++)
            {
                entrantData = teamData[i].Split(',');

                enteredSeries = GetSeries(entrantData[1]);
                enteredClass = GetClass(enteredSeries, entrantData[2]);
                enteredModel = GetCarModel(entrantData[4]);

                carNumber = entrantData[3];

                crewOVR = Convert.ToInt32(entrantData[6]);

                stintMod = Convert.ToInt32(entrantData[8]);
                crewReliability = Convert.ToInt32(entrantData[9]);

                points = Convert.ToInt32(entrantData[11]);

                bestResult = entrantData[13];
                timesFinished = Convert.ToInt32(entrantData[14]);

                pastResults = GetPastResultsList(entrantData[16]);
                rawResults = GetRawResultsList(entrantData[18]);

                newEntrant = new Entrant(carNumber, teamName, teamOVR, crewOVR, stintMod, crewReliability, -1,
                    points, bestResult, timesFinished, pastResults, rawResults, enteredModel, enteredClass, enteredSeries);

                playerTeam.AddCrew(newEntrant);
            }
        }

        private void LoadEntryList(string seasonFolder)
        {
            string basePath, filePath;
            string[] seriesEntrants, entrantData;
            int classIndex;

            string carNumber, teamName, bestResult;
            int teamOVR, crewOVR, stintMod, crewReliability, entrantIndex, points, timesFinished;

            List<string> pastResults;
            List<int> rawResults;

            CarModel enteredModel;
            Entrant newEntrant;

            currentSeries.ClearEntryList();

            basePath = Path.Combine(seasonFolder, currentSeries.GetSeriesName(), "Entrants");

            classList = currentSeries.GetClassList();

            entrantIndex = 1;

            for (classIndex = 0; classIndex < classList.Count(); classIndex++)
            {
                filePath = Path.Combine(basePath, string.Format("Class {0}.csv", classIndex + 1));

                seriesEntrants = FileHandler.ReadFile(filePath);

                for (int i = 0; i < seriesEntrants.Length; i++)
                {
                    entrantData = seriesEntrants[i].Split(',');

                    enteredModel = GetCarModel(entrantData[3]);

                    carNumber = entrantData[1];
                    teamName = entrantData[2];

                    teamOVR = Convert.ToInt32(entrantData[5]);
                    crewOVR = Convert.ToInt32(entrantData[6]);

                    stintMod = Convert.ToInt32(entrantData[8]);
                    crewReliability = Convert.ToInt32(entrantData[9]);

                    points = Convert.ToInt32(entrantData[11]);

                    bestResult = entrantData[13];
                    timesFinished = Convert.ToInt32(entrantData[14]);

                    pastResults = GetPastResultsList(entrantData[16]);
                    rawResults = GetRawResultsList(entrantData[18]);

                    newEntrant = new Entrant(carNumber, teamName, teamOVR, crewOVR, stintMod, crewReliability, entrantIndex,
                        points, bestResult, timesFinished, pastResults, rawResults, enteredModel, classList[classIndex], currentSeries);
                    currentSeries.AddEntrant(newEntrant);

                    entrantIndex++;
                }
            }
        }

        private void LoadManufacturers(string seasonFolder)
        {
            string manufacturersPath = Path.Combine(seasonFolder, currentSeries.GetSeriesName(), "Manufacturers"), fileName;

            string[] manufacturersData, manufacturerData;

            classList = currentSeries.GetClassList();

            string manufacturerName, bestResult;
            int points, timesFinished;

            List<string> pastResults;
            List<int> rawResults;

            Manufacturer newManufacturer;
            List<Manufacturer> manufacturerList;

            for (int classIndex = 0; classIndex < classList.Count(); classIndex++)
            {
                manufacturerList = new List<Manufacturer>();

                fileName = Path.Combine(manufacturersPath, string.Format("Class {0}.csv", classIndex + 1));

                manufacturersData = FileHandler.ReadFile(fileName);

                for (int i = 0; i < manufacturersData.Length; i++)
                {
                    manufacturerData = manufacturersData[i].Split(',');

                    manufacturerName = manufacturerData[0];

                    points = Convert.ToInt32(manufacturerData[1]);

                    bestResult = manufacturerData[3];
                    timesFinished = Convert.ToInt32(manufacturerData[4]);

                    pastResults = GetPastResultsList(manufacturerData[6]);
                    rawResults = GetRawResultsList(manufacturerData[8]);

                    newManufacturer = new Manufacturer(manufacturerName, points, bestResult, timesFinished, pastResults, rawResults);
                    manufacturerList.Add(newManufacturer);
                }

                classList[classIndex].SetManufacturerList(manufacturerList);
            }
        }

        private Series GetSeries(string seriesAbbreviation)
        {
            foreach (Series currentSeries in seriesList)
            {
                if (currentSeries.GetAbbreviation() == seriesAbbreviation)
                {
                    return currentSeries;
                }
            }

            return new Series();
        }

        private Class GetClass(Series targetSeries, string className)
        {
            foreach (Class currentClass in targetSeries.GetClassList())
            {
                if (currentClass.GetClassName() == className)
                {
                    return currentClass;
                }
            }

            return new Class();
        }

        private CarModel GetCarModel(string modelName)
        {
            foreach (CarModel checkModel in CommonData.GetModelList())
            {
                if (checkModel.GetModelName() == modelName)
                {
                    return checkModel;
                }
            }

            return new CarModel();
        }

        private List<string> GetPastResultsList(string resultsString)
        {
            List<string> resultsList = new List<string>();

            string[] splitResults = resultsString.Split(';');

            for (int i = 0; i < splitResults.Length; i++)
            {
                if (splitResults[i] != "")
                {
                    resultsList.Add(splitResults[i]);
                }
            }

            return resultsList;
        }

        private List<int> GetRawResultsList(string resultsString)
        {
            List<int> resultsList = new List<int>();

            string[] splitResults = resultsString.Split(';');

            for (int i = 0; i < splitResults.Length; i++)
            {
                if (splitResults[i] != "")
                {
                    resultsList.Add(Convert.ToInt32(splitResults[i]));
                }
            }

            return resultsList;
        }


        // Game Saving

        private void SaveGame(int raceNumber)
        {
            string saveFolder = Path.Combine(CommonData.GetSeasonFolder(), "Save Data"), currentFolder, entrantsFolder, manufacturersFolder;

            Directory.CreateDirectory(saveFolder);

            string savePath = Path.Combine(CommonData.GetSaveFolder(), "Current Race.csv"),
                dataString = string.Format("Season,{0}\nRound,{1}", seasonNumber, raceNumber + 1);

            FileHandler.WriteFile(dataString, savePath);

            string className, fileName, writeString;
            int classIndex;

            SavePlayerTeam(saveFolder);

            foreach (Series currentSeries in seriesList)
            {
                currentFolder = Path.Combine(saveFolder, currentSeries.GetSeriesName());

                entrantsFolder = Path.Combine(currentFolder, "Entrants");
                Directory.CreateDirectory(entrantsFolder);

                manufacturersFolder = Path.Combine(currentFolder, "Manufacturers");
                Directory.CreateDirectory(manufacturersFolder);

                classList = currentSeries.GetClassList();
                className = classList[0].GetClassName();

                writeString = "";
                classIndex = 1;

                fileName = Path.Combine(entrantsFolder, "Class 1.csv");

                entryList = currentSeries.GetEntryList();
                IndexSort(entryList);
                ClassSort(entryList);

                foreach (Entrant currentEntrant in entryList)
                {
                    if (currentEntrant.GetTeamName() != playerTeam.GetTeamName())
                    {
                        if (currentEntrant.GetClassName() != className)
                        {
                            FileHandler.WriteFile(writeString, fileName);
                            WriteManufacturers(classList[classIndex - 1], Path.Combine(manufacturersFolder, string.Format("Class {0}.csv", classIndex)));
                            writeString = "";

                            className = classList[classIndex].GetClassName();

                            classIndex++;
                            fileName = Path.Combine(entrantsFolder, string.Format("Class {0}.csv", classIndex));
                        }

                        writeString += string.Format("{0},{1},{2},{3},,{4},{5},,{6},{7},,{8},,{9},,{10},,{11}\n", className, currentEntrant.GetCarNo(),
                            currentEntrant.GetTeamName(), currentEntrant.GetCarModel().GetModelName(), currentEntrant.GetTeamOVR(),
                            currentEntrant.GetCrewOVR(), currentEntrant.GetSRM(), currentEntrant.GetBaseCrewReliability(),
                            currentEntrant.GetPoints(), currentEntrant.GetBestResultSave(),
                            currentEntrant.GetResultsStringSave(), currentEntrant.GetRawResultsStringSave());
                    }
                }

                FileHandler.WriteFile(writeString, fileName);
                WriteManufacturers(classList[classIndex - 1], Path.Combine(manufacturersFolder, string.Format("Class {0}.csv", classIndex)));
            }

            Console.WriteLine();

            if (GetBoolean("Autosave Complete\n\nExit Game?"))
            {
                Environment.Exit(0);
            }
        }

        private void WriteManufacturers(Class currentClass, string fileName)
        {
            string writeString = "";

            List<Manufacturer> manufacturerList = currentClass.GetManufacturerList();

            foreach (Manufacturer currentManufacturer in manufacturerList)
            {
                writeString += string.Format("{0},{1},,{2},,{3},,{4}\n", currentManufacturer.GetManufacturerName(), currentManufacturer.GetPoints(),
                    currentManufacturer.GetBestResultSave(), currentManufacturer.GetResultsStringSave(), currentManufacturer.GetRawResultsStringSave());
            }

            FileHandler.WriteFile(writeString, fileName);
        }

        private void SavePlayerTeam(string folderPath)
        {
            string writeString = string.Format("{0}\nTeam OVR,{1}\nCrew No,Series,Class,Car No,Car Model,,Crew OVR,,SRM,Reliability,,Points,,Best Result,,,Past Results,,Raw Results", playerTeam.GetTeamName(), playerTeam.GetTeamOVR()),
                filePath = Path.Combine(folderPath, "Team Details.csv");

            List<Entrant> crewList = playerTeam.GetTeamEntries();

            for (int i = 0; i < crewList.Count(); i++)
            {
                currentEntrant = crewList[i];

                writeString += string.Format("\nCrew {0},{1},{2},{3},{4},,{5},,{6},{7},,{8},,{9},,{10},,{11}",
                    i + 1, currentEntrant.GetSeries().GetAbbreviation(), currentEntrant.GetClassName(),
                    currentEntrant.GetCarNo(), currentEntrant.GetModelName(),
                    currentEntrant.GetCrewOVR(), currentEntrant.GetSRM(), currentEntrant.GetBaseCrewReliability(),
                    currentEntrant.GetPoints(), currentEntrant.GetBestResultSave(),
                    currentEntrant.GetResultsStringSave(), currentEntrant.GetRawResultsStringSave());
            }

            FileHandler.WriteFile(writeString, filePath);
        }


        // Player Team Creation / Update Functions

        private void SetupPlayerTeam()
        {
            string teamName = GetTeamName();
            int teamOVR = randomiser.Next(97, 101);

            playerTeam = new Team(teamName, teamOVR);

            CreateCrews();

            playerTeam.OrderCrews();
            playerTeam.SetSpacerList();

            DisplayTeamDetails(false);

            Console.ReadLine();
            Console.Clear();

            ConfirmTeamDetails();

            Console.Clear();

            playerTeam.OrderCrews();
            playerTeam.SetSpacerList();

            Console.Clear();
        }

        private int ConfirmTeamDetails()
        {
            if (!GetBoolean("Change Team Details?"))
            {
                return 0;
            }

            Console.Clear();

            if (GetBoolean("Change Team Name?"))
            {
                Console.Clear();
                playerTeam.SetTeamName(GetTeamName());
            }

            Console.Clear();

            foreach (Entrant currentEntrant in playerTeam.GetTeamEntries())
            {
                UpdateCrew(currentEntrant);
            }

            return 0;
        }

        private void UpdatePlayerTeam()
        {
            if (GetBoolean("Change Team Name?"))
            {
                Console.Clear();
                playerTeam.SetTeamName(GetTeamName());
            }

            Console.Clear();

            string crewString;
            int newOVR, newReliability;

            foreach (Entrant currentEntrant in playerTeam.GetTeamEntries())
            {
                crewString = string.Format("{0} {1} - {2} {3}", currentEntrant.GetSeriesName(), currentEntrant.GetClassName(), currentEntrant.GetCarNo(), currentEntrant.GetManufacturer());

                Console.Clear();

                if (!GetBoolean(string.Format("Delete Crew {0}?", crewString)))
                {
                    Console.Clear();

                    if (GetBoolean(string.Format("Update Crew {0}?", crewString)))
                    {
                        Console.Clear();
                        UpdateCrew(currentEntrant);
                    }

                    (newOVR, newReliability) = playerTeam.UpdateCrewStat(currentEntrant, randomiser);

                    currentEntrant.SetCrewOVR(newOVR);
                    currentEntrant.SetBaseReliability(newReliability);
                    currentEntrant.ResetPastResults();
                }

                else
                {
                    playerTeam.DeleteCrew(currentEntrant);
                }
            }
        
            playerTeam.ResetCrewResults();

            CreateCrews();

            playerTeam.OrderCrews();

            DisplayTeamDetails(true);

            Console.ReadLine();
        }

        private void CreateCrews()
        {
            int[] classEntrants;

            do
            {
                foreach (Series potentialSeries in seriesList)
                {
                    Console.Clear();

                    classEntrants = new int[potentialSeries.GetClassList().Count()];

                    for (int i = 0; i < potentialSeries.GetClassList().Count(); i++)
                    {
                        classEntrants[i] = playerTeam.GetClassEnteredCount(potentialSeries.GetClassList()[i]);
                    }

                    if (TotalEntrants(classEntrants) < potentialSeries.GetMaxEntries())
                    {
                        if (GetBoolean(string.Format("Enter {0}?", potentialSeries.GetSeriesName())))
                        {
                            do
                            {
                                Console.Clear();
                                playerTeam.AddCrew(CreateCrew(potentialSeries, classEntrants));
                                Console.Clear();
                            }
                            while (AddNewCrew(potentialSeries, classEntrants));
                        }
                    }
                }

                if (playerTeam.GetTeamEntries().Count == 0)
                {
                    Console.WriteLine("You must enter at least 1 Crew across the Available Series to Continue");
                }
            }
            while (playerTeam.GetTeamEntries().Count == 0);

            Console.Clear();
        }

        private void UpdateCrew(Entrant updateCrew)
        {
            string crewString = string.Format("{0} - {1} - {2} {3}\n", updateCrew.GetSeriesName(), updateCrew.GetClassName(), updateCrew.GetCarNo(), updateCrew.GetManufacturer());

            Console.WriteLine(crewString);

            if (GetBoolean("Modify Crew?"))
            {
                Console.Clear();
                Series entrantSeries = updateCrew.GetSeries();

                Console.WriteLine(crewString);

                if (GetBoolean("Change Car Number?"))
                {
                    updateCrew.SetCarNumber(GetCarNumber(entrantSeries));
                }

                Console.Clear();
                Console.WriteLine(crewString);

                if (GetBoolean("Change Car Class?"))
                {
                    Class chosenClass = entrantSeries.GetClassList()[GetClass(entrantSeries)];
                    updateCrew.SetClass(chosenClass);

                    Console.Clear();
                    Console.WriteLine(crewString);

                    updateCrew.SetCarModel(GetCarModel(chosenClass));
                }

                else
                {
                    Console.Clear();
                    Console.WriteLine(crewString);

                    if (GetBoolean("Change Car Model?"))
                    {
                        Console.Clear();
                        Console.WriteLine(crewString);

                        updateCrew.SetCarModel(GetCarModel(updateCrew.GetClass()));
                    }
                }
            }

            Console.Clear();
        }

        private bool AddNewCrew(Series potentialSeries, int[] classEntrants)
        {
            int totalSeriesEntrants = 0;

            foreach (int classEntries in classEntrants)
            {
                totalSeriesEntrants += classEntries;
            }

            if (totalSeriesEntrants < potentialSeries.GetMaxEntries())
            {
                return GetBoolean(string.Format("Create New {0} Crew?", potentialSeries.GetSeriesName()));
            }

            Console.WriteLine("Max Crews entered for {0}", potentialSeries.GetSeriesName());
            return false;
        }

        private string GetTeamName()
        {
            string teamName = "";

            while (teamName.Length < minimumTeamNameLength)
            {
                Console.Write("Please Enter Team Name: ");
                teamName = Console.ReadLine();

                if (teamName.Length < minimumTeamNameLength)
                {
                    Console.WriteLine("Team Name must be at least {0} Characters Long)", minimumTeamNameLength);
                }
            }

            return teamName;
        }

        private Entrant CreateCrew(Series chosenSeries, int[] classEntrants)
        {
            int classIndex = GetClass(chosenSeries);

            if (classEntrants[classIndex] + 1 > 2)
            {
                Console.WriteLine("Too many Entries for Desired Class\n");
                return CreateCrew(chosenSeries, classEntrants);
            }

            Class enteredClass = chosenSeries.GetClassList()[classIndex];

            Console.Clear();

            CarModel chosenModel = GetCarModel(enteredClass);

            Console.Clear();

            string carNumber = GetCarNumber(chosenSeries);

            int crewOVR = randomiser.Next(enteredClass.GetMinOVR(), enteredClass.GetMaxOVR() + 1),
                stintModifier = randomiser.Next(1, 6),
                reliability = randomiser.Next(24, 31);

            classEntrants[classIndex]++;

            return new Entrant(carNumber, playerTeam.GetTeamName(), playerTeam.GetTeamOVR(), crewOVR, stintModifier, reliability, -1,
                chosenModel, enteredClass, chosenSeries);
        }

        private int GetClass(Series chosenSeries)
        {
            string strClass;
            int selectedClass;

            classList = chosenSeries.GetClassList();

            Console.WriteLine("Select Class:");

            for (int i = 0; i < classList.Count(); i++)
            {
                Console.WriteLine("{0} - {1}", i + 1, classList[i].GetClassName());
            }

            Console.Write("Choice: ");
            strClass = Console.ReadLine().ToUpper();

            bool validClass = int.TryParse(strClass, out selectedClass);

            if (validClass && selectedClass > 0 && selectedClass <= classList.Count())
            {
                return selectedClass - 1;
            }

            else
            {
                for (int i = 0; i < classList.Count(); i++)
                {
                    if (classList[i].GetClassName().ToUpper() == strClass)
                    {
                        return i;
                    }
                }
            }

            Console.WriteLine("Invalid Series Selected");
            return GetClass(chosenSeries);
        }

        private CarModel GetCarModel(Class chosenClass)
        {
            List<string> platforms = chosenClass.GetEligiblePlatforms();
            List<CarModel> models = new List<CarModel>();

            string strModel;
            int selectedModel, platformSpacer = 0;

            foreach (CarModel potentialModel in CommonData.GetModelList())
            {
                if (platforms.Contains(potentialModel.GetPlatform()))
                {
                    models.Add(potentialModel);
                }
            }

            foreach (string platform in platforms)
            {
                if (platform.Length > platformSpacer)
                {
                    platformSpacer = platform.Length;
                }
            }

            Console.WriteLine("Eligible Car Models:");

            for (int i = 0; i < models.Count(); i++)
            {
                Console.WriteLine("{0} - {1} - {2}", Convert.ToString(i + 1).PadRight(2, ' '), models[i].GetPlatform().PadRight(platformSpacer, ' '), models[i].GetModelName());
            }

            Console.Write("Choice: ");
            strModel = Console.ReadLine().ToLower();

            bool validModel = int.TryParse(strModel, out selectedModel);

            if (validModel && selectedModel > 0 && selectedModel <= models.Count())
            {
                return models[selectedModel - 1];
            }

            else
            {
                foreach (CarModel potentialModel in models)
                {
                    if (potentialModel.GetModelName().ToLower() == strModel)
                    {
                        return potentialModel;
                    }
                }
            }

            Console.WriteLine("Invalid Car Model Selected");
            return GetCarModel(chosenClass);
        }

        private string GetCarNumber(Series chosenSeries)
        {
            Console.Write("Enter your Desired Car Number: #");
            string desiredNumber = Console.ReadLine().Replace("#", "");

            if (UniqueCarNumber(chosenSeries, desiredNumber))
            {
                return string.Format("#{0}", desiredNumber);
            }

            Console.WriteLine("Invalid / Non-Unique Car Number");
            return GetCarNumber(chosenSeries);
        }

        private bool UniqueCarNumber(Series chosenSeries, string desiredNumber)
        {
            if (!int.TryParse(desiredNumber, out int x))
            {
                return false;
            }

            if (desiredNumber.Length < 1 && desiredNumber.Length > 3)
            {
                return false;
            }

            desiredNumber = string.Format("#{0}", desiredNumber);

            foreach (Entrant testEntrant in chosenSeries.GetEntryList())
            {
                if (testEntrant.GetCarNo() == desiredNumber)
                {
                    return false;
                }
            }

            foreach (Entrant testEntrant in playerTeam.GetTeamEntries())
            {
                if (testEntrant.GetSeriesName() == chosenSeries.GetSeriesName() && testEntrant.GetCarNo() == desiredNumber)
                {
                    return false;
                }
            }

            return true;
        }

        private int TotalEntrants(int[] classEntrants)
        {
            int totalEntrants = 0;

            foreach (int entrantCount in classEntrants)
            {
                totalEntrants += entrantCount;
            }

            return totalEntrants;
        }


        // Entry List Functions

        private void SetSeasonEntryLists()
        {
            List<Entrant> crewList = playerTeam.GetTeamEntries();

            string seriesName;
            int seriesIndex = 0, entrantIndex;

            foreach (Series workingSeries in seriesList)
            {
                if (seasonNumber > 1)
                {
                    workingSeries.LoadEntryList(seasonNumber);
                }

                seriesName = workingSeries.GetSeriesName();
                entrantIndex = workingSeries.GetEntryList().Count();

                for (int i = seriesIndex; i < crewList.Count; i++)
                {
                    if (crewList[i].GetSeriesName() == seriesName)
                    {
                        crewList[i].SetIndex(entrantIndex);
                        workingSeries.AddEntrant(crewList[i]);
                    }

                    else
                    {
                        seriesIndex = i;
                        break;
                    }
                }
            }
        }

        private void SetEntryList()
        {
            entryList = currentSeries.GetEntryList();

            IndexSort(entryList);
            racingCount = 0;

            for (int i = 0; i < entryList.Count(); i++)
            {
                entryList[i].SetRound(currentRound);

                if (currentRound.GetNamedClasses().Contains(entryList[i].GetClass().GetClassName()))
                {
                    entryList[i].SetRacing(true);
                    racingCount++;
                }

                else
                {
                    entryList[i].SetRacing(false);
                }
            }

            IsRacingSort(entryList);
        }

        private void LoadManufacturers()
        {
            List<string> manufacturerList = new List<string>();

            Class currentClass;

            string className, manufacturer;
            int classIndex;

            foreach (Series currentSeries in seriesList)
            {
                classList = currentSeries.GetClassList();
                
                classIndex = 0;
                
                currentClass = classList[classIndex];
                className = currentClass.GetClassName();

                manufacturerList.Clear();
                entryList = currentSeries.GetEntryList();

                ClassSort(entryList);

                for (int i = 0; i < entryList.Count(); i++)
                {
                    currentEntrant = entryList[i];

                    if (currentEntrant.GetClassName() != className)
                    {
                        currentClass.SetManufacturerList(manufacturerList);
                        manufacturerList.Clear();

                        classIndex++;

                        currentClass = classList[classIndex];
                        className = currentClass.GetClassName();
                    }

                    manufacturer = currentEntrant.GetManufacturer();

                    if (!manufacturerList.Contains(manufacturer))
                    {
                        manufacturerList.Add(manufacturer);
                    }
                }

                currentClass.SetManufacturerList(manufacturerList);

                IndexSort(entryList);
            }
        }

        private bool GetCrewCompeting()
        {
            entryList = currentSeries.GetEntryList();

            string playerTeamName = playerTeam.GetTeamName();
            int entrantIndex = 0;

            while (entryList[entrantIndex].GetRacing())
            {
                if (entryList[entrantIndex].GetTeamName() == playerTeamName)
                {
                    return true;
                }

                entrantIndex++;
            }

            return false;
        }


        // Season 2+ Game Crew Functions

        private void UpdateGameCrewStats()
        {
            int newOVR, newReliability;

            foreach (Series currentSeries in seriesList)
            {
                entryList = currentSeries.GetEntryList();

                IndexSort(entryList);

                foreach (Entrant currentEntrant in entryList)
                {
                    if (currentEntrant.GetTeamName() != playerTeam.GetTeamName())
                    {
                        (newOVR, newReliability) = UpdateCrewStat(currentEntrant);

                        currentEntrant.SetCrewOVR(newOVR);
                        currentEntrant.SetBaseReliability(newReliability);
                    }
                }
            }
        }

        private void WriteEntryLists()
        {
            string folderPath, className, fileName, writeString;
            int classIndex;

            foreach (Series currentSeries in seriesList)
            {
                classList = currentSeries.GetClassList();
                className = classList[0].GetClassName();

                writeString = "";
                classIndex = 1;

                folderPath = Path.Combine(CommonData.GetSeasonFolder(), currentSeries.GetSeriesName(), "Entrants");
                Directory.CreateDirectory(folderPath);

                fileName = Path.Combine(folderPath, "Class 1.csv");

                entryList = currentSeries.GetEntryList();
                ClassSort(entryList);

                foreach (Entrant currentEntrant in entryList)
                {
                    if (currentEntrant.GetTeamName() != playerTeam.GetTeamName())
                    {
                        if (currentEntrant.GetClassName() != className)
                        {
                            FileHandler.WriteFile(writeString, fileName);
                            writeString = "";

                            className = classList[classIndex].GetClassName();

                            classIndex++;
                            fileName = Path.Combine(folderPath, string.Format("Class {0}.csv", classIndex));
                        }

                        writeString += string.Format("{0},{1},{2},{3},,{4},{5},,{6},,{7}\n", className, currentEntrant.GetCarNo(),
                            currentEntrant.GetTeamName(), currentEntrant.GetCarModel().GetModelName(), currentEntrant.GetTeamOVR(),
                            currentEntrant.GetCrewOVR(), currentEntrant.GetSRM(), currentEntrant.GetBaseCrewReliability());
                    }
                }

                FileHandler.WriteFile(writeString, fileName);
            }
        }

        private (int, int) UpdateCrewStat(Entrant currentEntrant)
        {
            int currentOVR = currentEntrant.GetOVR(),
                    currentReliability = currentEntrant.GetBaseReliability();

            int ovrUpperRange = currentEntrant.GetClass().GetMaxOVR() - currentOVR,
                reliabilityUpperRange = 30 - currentReliability;

            if (ovrUpperRange > 3)
            {
                ovrUpperRange = 3;
            }

            if (reliabilityUpperRange > 3)
            {
                reliabilityUpperRange = 3;
            }

            int ovrLowerRange = ovrUpperRange - randomiser.Next(0, 5),
                reliabilityLowerRange = reliabilityUpperRange - randomiser.Next(2, 5);

            int newOVR = currentOVR + randomiser.Next(ovrLowerRange, ovrUpperRange + 1),
                newReliability = currentReliability + randomiser.Next(reliabilityLowerRange, reliabilityUpperRange + 1);

            if (newOVR > currentEntrant.GetClass().GetMaxOVR())
            {
                newOVR = currentEntrant.GetClass().GetMaxOVR();
            }

            if (newReliability > 30)
            {
                newReliability = 30;
            }

            return (newOVR, newReliability);
        }


        // Display Functions

        private void DisplayTeamDetails(bool displayCrewStats)
        {
            spacerList = playerTeam.GetSpacerList();

            Console.WriteLine("Team Details - {0}", playerTeam.GetTeamName());

            foreach (Entrant teamCrew in playerTeam.GetTeamEntries())
            {
                Console.Write("{0} - {1} - {2} - {3} - {4}", teamCrew.GetSeriesName().PadRight(spacerList[0], ' '),
                    teamCrew.GetClassName().PadRight(spacerList[1], ' '), teamCrew.GetCarNo().PadRight(spacerList[2], ' '),
                    teamCrew.GetModelName().PadRight(spacerList[3], ' '), teamCrew.GetManufacturer().PadRight(spacerList[4], ' '));

                if (displayCrewStats)
                {
                    Console.WriteLine(" - {0} - {1} - {2} - {3}", teamCrew.GetTeamOVR(), Convert.ToString(teamCrew.GetCrewOVR()).PadLeft(3, ' '),
                        teamCrew.GetSRM(), teamCrew.GetBaseCrewReliability());
                }

                else
                {
                    Console.WriteLine();
                }
            }
        }

        private void DisplayTeamEntrants(string outputString)
        {
            Console.WriteLine(outputString);

            string overallPosition, classPosition;

            List<Entrant> crewList = playerTeam.GetTeamEntries();

            spacerList = playerTeam.GetSpacerList();

            for (int i = 0; i < crewList.Count(); i++)
            {
                currentEntrant = crewList[i];

                if (currentEntrant.GetRacing())
                {
                    (overallPosition, classPosition) = currentEntrant.GetCurrentPosition();

                    Console.WriteLine("{0} {1} - {2} Overall / {3} In {4}",
                        currentEntrant.GetCarNo().PadRight(spacerList[2], ' '),currentEntrant.GetManufacturer().PadRight(spacerList[4], ' '),
                        overallPosition.PadRight(3, ' '), classPosition.PadRight(3, ' '), currentEntrant.GetClass().GetClassName());
                }
            }

            Console.ReadLine();
        }

        private void DisplayClassLeaders(string outputString)
        {
            Console.WriteLine(outputString);

            string className;
            List<string> foundClasses = new List<string>();
            List<int> leaderSpacers = new List<int>();

            Entrant currentEntrant;
            List<Entrant> classLeaders = new List<Entrant>();

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];
                className = currentEntrant.GetClass().GetClassName();

                if (!foundClasses.Contains(className) && currentEntrant.GetRacing())
                {
                    foundClasses.Add(className);
                    classLeaders.Add(currentEntrant);

                    if (leaderSpacers.Count() == 0)
                    {
                        leaderSpacers.Add(currentEntrant.GetCarNo().Length);
                        leaderSpacers.Add(currentEntrant.GetTeamName().Length);
                        leaderSpacers.Add(currentEntrant.GetManufacturer().Length);
                    }

                    else
                    {
                        if (currentEntrant.GetCarNo().Length > leaderSpacers[0])
                        {
                            leaderSpacers[0] = currentEntrant.GetCarNo().Length;
                        }

                        if (currentEntrant.GetTeamName().Length > leaderSpacers[1])
                        {
                            leaderSpacers[1] = currentEntrant.GetTeamName().Length;
                        }

                        if (currentEntrant.GetManufacturer().Length > leaderSpacers[2])
                        {
                            leaderSpacers[2] = currentEntrant.GetManufacturer().Length;
                        }
                    }
                }
            }

            ClassSort(classLeaders);

            for (int i = 0; i < classLeaders.Count(); i++)
            {
                currentEntrant = classLeaders[i];

                Console.WriteLine("{0}: {1} {2} - {3} - {4} Overall", currentEntrant.GetClassName().PadRight(currentSeries.GetClassSpacer(), ' '),
                    currentEntrant.GetCarNo().PadRight(leaderSpacers[0], ' '), currentEntrant.GetTeamName().PadRight(leaderSpacers[1], ' '),
                    currentEntrant.GetManufacturer().PadRight(leaderSpacers[2], ' '), currentEntrant.GetCurrentPosition().Item1.PadRight(3, ' '));
            }

            Console.ReadLine();
        }

        private void DisplayEntrants(string outputString)
        {
            Console.WriteLine(outputString);

            string overallPosition, classPosition;

            entrantSpacers = currentSeries.GetEntrantSpacers();

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];

                if (currentEntrant.GetRacing())
                {
                    (overallPosition, classPosition) = currentEntrant.GetCurrentPosition();

                    Console.WriteLine("{0} Overall / {1} In {6} - {2} {3} - {4} - {5}", overallPosition.PadRight(3, ' '),
                        classPosition.PadRight(3, ' '), currentEntrant.GetCarNo().PadRight(entrantSpacers[0], ' '),
                        currentEntrant.GetTeamName().PadRight(entrantSpacers[1], ' '), currentEntrant.GetModelName().PadRight(entrantSpacers[3], ' '),
                        currentEntrant.GetOVR(), currentEntrant.GetClassName().PadRight(currentSeries.GetClassSpacer(), ' '));
                }
            }

            Console.ReadLine();
        }

        private void DisplayTeamPoints(string outputString)
        {
            Console.WriteLine(outputString);

            string currentSeriesName = currentSeries.GetSeriesName();

            List<Entrant> playerCrews = playerTeam.GetTeamEntries();
            spacerList = playerTeam.GetSpacerList();

            foreach (Entrant currentCrew in playerCrews)
            {
                if (currentCrew.GetSeriesName() == currentSeriesName)
                {
                    Console.WriteLine("{0} {1} - {2} Points - {3} in {4} - {5}", currentCrew.GetCarNo().PadRight(spacerList[2], ' '),
                        currentCrew.GetManufacturer().PadRight(spacerList[4], ' '), Convert.ToString(currentCrew.GetPoints()).PadRight(3, ' '),
                        currentCrew.GetStandingsPosition().PadRight(3, ' '), currentCrew.GetClassName().PadRight(spacerList[1], ' '), currentCrew.GetBestResult());
                }
            }

            Console.ReadLine();
        }

        private void DisplayStandings(string outputString)
        {
            Console.WriteLine(outputString);

            classList = currentSeries.GetClassList();
            Class currentClass = classList[0];

            string currentClassName = currentSeries.GetClassList()[0].GetClassName();
            int classPosition = 1, classIndex = 1;

            Entrant currentEntrant;

            Console.WriteLine("{0}:\n\nEntrant Standings:", currentClassName);

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];

                if (currentEntrant.GetClass().GetClassName() != currentClassName)
                {
                    DisplayManufacturersStandings(currentClass);

                    currentClass = currentSeries.GetClassList()[classIndex];
                    currentClassName = currentClass.GetClassName();
                    Console.WriteLine("\n\n{0}:\n\nEntrant Standings:", currentClassName);
                    classIndex++;
                }

                Console.WriteLine("{0}: {1} {2} - {3} - {4} Points - {5}", currentEntrant.GetStandingsPosition().PadRight(3, ' '),
                    currentEntrant.GetCarNo().PadRight(entrantSpacers[0], ' '), currentEntrant.GetTeamName().PadRight(entrantSpacers[1], ' '),
                    currentEntrant.GetModelName().PadRight(entrantSpacers[3], ' '), Convert.ToString(currentEntrant.GetPoints()).PadRight(3, ' '),
                    currentEntrant.GetBestResult());
                classPosition++;
            }

            DisplayManufacturersStandings(currentClass);
        }

        private void DisplayManufacturersStandings(Class currentClass)
        {
            Console.WriteLine("\nManufacturers Standings:");

            List<Manufacturer> manufacturerList = currentClass.GetManufacturerList();

            entrantSpacers = currentSeries.GetEntrantSpacers();

            for (int i = 0; i < manufacturerList.Count(); i++)
            {
                Console.WriteLine("{0}: {1} - {2} Points - {3}", ("P" + (i + 1)).PadRight(3, ' '),
                    manufacturerList[i].GetManufacturerName().PadRight(entrantSpacers[2], ' '), Convert.ToString(manufacturerList[i].GetPoints()).PadRight(3, ' '),
                    manufacturerList[i].GetBestResult());
            }
        }

        private void DisplayPointsLeaders(string outputString)
        {
            Console.WriteLine(outputString);

            classList = currentSeries.GetClassList();

            List<int> leaderSpacers;
            List<Entrant> classLeaders;
            List<Manufacturer> manufacturerLeaders = GetManufacturersLeaders();

            (classLeaders, leaderSpacers) = GetChampionshipLeaders();

            Entrant classLeader;
            Manufacturer manufacturersLeader;

            for (int i = 0; i < classList.Count(); i++)
            {
                classLeader = classLeaders[i];
                manufacturersLeader = manufacturerLeaders[i];

                Console.WriteLine("\n{0}:", classList[i].GetClassName());
                Console.WriteLine(" {0} {1} - {2}\n  {3} Points - {4}", classLeader.GetCarNo().PadRight(leaderSpacers[0], ' '),
                    classLeader.GetTeamName(), classLeader.GetManufacturer(), classLeader.GetPoints(),classLeader.GetBestResult());
                Console.WriteLine(" {0} - {1} Points - {2}", manufacturersLeader.GetManufacturerName(),
                    manufacturersLeader.GetPoints(), manufacturersLeader.GetBestResult());
            }
        }

        private void DisplayFinalStandings(string outputString)
        {
            Console.WriteLine(outputString);

            classList = currentSeries.GetClassList();

            List<int> leaderSpacers;
            List<Entrant> entrantChampions;
            List<Manufacturer> manufacturerChampions = GetManufacturersLeaders();

            (entrantChampions, leaderSpacers) = GetChampionshipLeaders();

            Entrant entrantChampion;
            Manufacturer manufacturerChampion;

            for (int classIndex = 0; classIndex < classList.Count(); classIndex++)
            {
                Console.WriteLine("\n{0}:", classList[classIndex].GetClassName());

                entrantChampion = entrantChampions[classIndex];
                manufacturerChampion = manufacturerChampions[classIndex];

                Console.WriteLine(" Entrant Champion:\n  {0} {1} - {2}\n   {3} Points - {4}", entrantChampion.GetCarNo().PadRight(leaderSpacers[0], ' '), entrantChampion.GetTeamName(), entrantChampion.GetManufacturer(), Convert.ToString(entrantChampion.GetPoints()), entrantChampion.GetBestResult());
                Console.WriteLine(" Manufacturer Champion:\n  {0}\n   {1} Points - {2}", manufacturerChampion.GetManufacturerName(), manufacturerChampion.GetPoints(), manufacturerChampion.GetBestResult());
            }
        }


        // Position Functions

        private void SetPositions()
        {
            string posString, classPosString;
            int classIndex, classPosRaw;
            List<int> classPositions = new List<int>();

            for (int i = 0; i < currentSeries.GetClassList().Count(); i++)
            {
                classPositions.Add(1);
            }

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];
                classIndex = currentEntrant.GetClassIndex() - 1;

                if (currentEntrant.GetOVR() == 1)
                {
                    posString = "DNF";
                    classPosString = "DNF";
                    classPosRaw = classPositions[classIndex];

                    classPositions[classIndex]++;
                }

                else if (currentEntrant.GetOVR() == 100 && currentEntrant.GetInGarage())
                {
                    posString = "NC";
                    classPosString = "NC";
                    classPosRaw = classPositions[classIndex];

                    classPositions[classIndex]++;
                }

                else if (currentEntrant.GetInGarage())
                {
                    posString = "GAR";
                    classPosString = "GAR";
                    classPosRaw = classPositions[classIndex];

                    classPositions[classIndex]++;
                }

                else
                {
                    posString = "P" + (i + 1);
                    classPosString = "P" + classPositions[classIndex];
                    classPosRaw = classPositions[classIndex];

                    classPositions[classIndex]++;
                }

                currentEntrant.SetCurrentPositions(posString, classPosString, classPosRaw);
            }
        }

        private void SetStandingsPositions()
        {
            string posString;
            int classIndex;
            List<int> classPositions = new List<int>();

            for (int i = 0; i < currentSeries.GetClassList().Count(); i++)
            {
                classPositions.Add(1);
            }

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];
                classIndex = currentEntrant.GetClassIndex() - 1;

                posString = "P" + classPositions[classIndex];
                classPositions[classIndex]++;

                currentEntrant.SetStandingsPosition(posString);
            }
        }



        // Standings Functions

        private void AwardEntrantPoints()
        {
            classList = currentSeries.GetClassList();

            string currentClass = classList[0].GetClassName();
            int classPosition = 0;

            ClassSort(entryList);
            LoadPointsSystem();

            for (int i = 0; i < entryList.Count(); i++)
            {
                if (entryList[i].GetClass().GetClassName() != currentClass)
                {
                    currentClass = entryList[i].GetClass().GetClassName();
                    classPosition = 0;
                }

                if (classPosition < pointsSystem.Count() && entryList[i].GetOVR() > 100 && entryList[i].GetRacing())
                {
                    entryList[i].SetPoints(entryList[i].GetPoints() + pointsSystem[classPosition]);
                    classPosition++;
                }
            }
        }

        private void AwardManufacturersPoints()
        {
            ClassSort(entryList);

            classList = currentSeries.GetClassList();
            Class currentClass = classList[0];

            foundManufacturers.Clear();

            int classPosition = 0, classIndex = 1, manufacturerIndex;
            Entrant currentEntrant;

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];

                if (currentEntrant.GetClass().GetClassName() != currentClass.GetClassName())
                {
                    classPosition = 0;

                    currentClass = classList[classIndex];
                    classIndex++;

                    foundManufacturers.Clear();
                }

                if (!foundManufacturers.Contains(currentEntrant.GetManufacturer()) && currentEntrant.GetRacing())
                {   
                    manufacturerIndex = currentClass.GetManufacturerIndex(currentEntrant.GetManufacturer());

                    if (classPosition >= pointsSystem.Count() || currentEntrant.GetOVR() <= 100)
                    {
                        currentClass.GetManufacturerList()[manufacturerIndex].AddPoints(0);
                    }

                    else
                    {
                        currentClass.GetManufacturerList()[manufacturerIndex].AddPoints(pointsSystem[classPosition]);
                    }

                    currentClass.GetManufacturerList()[manufacturerIndex].AddResult(currentEntrant.GetCurrentPosition().Item2, classPosition + 1);
                    foundManufacturers.Add(currentEntrant.GetManufacturer());
                }

                classPosition++;
            }
        }

        private void LoadPointsSystem()
        {
            pointsSystem = new List<int>();

            string pointsSystemFile = Path.Combine(CommonData.GetSetupPath(), "Points Systems", "Race Systems", "System " + currentRound.GetPointsSystem() + ".csv");
            string[] pointsSystemData = FileHandler.ReadFile(pointsSystemFile);

            for (int i = 0; i < pointsSystemData.Length; i++)
            {
                pointsSystem.Add(Convert.ToInt32(pointsSystemData[i]));
            }
        }

        private (List<Entrant>, List<int>) GetChampionshipLeaders()
        {
            string className;
            List<string> foundClasses = new List<string>();
            List<int> leaderSpacers = new List<int>();

            List<Entrant> classLeaders = new List<Entrant>();

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];
                className = currentEntrant.GetClass().GetClassName();

                if (!foundClasses.Contains(className))
                {
                    foundClasses.Add(className);
                    classLeaders.Add(currentEntrant);

                    if (leaderSpacers.Count() == 0)
                    {
                        leaderSpacers.Add(currentEntrant.GetCarNo().Length);
                        leaderSpacers.Add(currentEntrant.GetTeamName().Length);
                        leaderSpacers.Add(currentEntrant.GetManufacturer().Length);
                    }

                    else
                    {
                        if (currentEntrant.GetCarNo().Length > leaderSpacers[0])
                        {
                            leaderSpacers[0] = currentEntrant.GetCarNo().Length;
                        }

                        if (currentEntrant.GetTeamName().Length > leaderSpacers[1])
                        {
                            leaderSpacers[1] = currentEntrant.GetTeamName().Length;
                        }

                        if (currentEntrant.GetManufacturer().Length > leaderSpacers[2])
                        {
                            leaderSpacers[2] = currentEntrant.GetManufacturer().Length;
                        }
                    }
                }
            }

            return (classLeaders, leaderSpacers);
        }

        private List<Manufacturer> GetManufacturersLeaders()
        {
            classList = currentSeries.GetClassList();

            List<Manufacturer> manufacturerChampions = new List<Manufacturer>();

            foreach (Class currentClass in classList)
            {
                manufacturerChampions.Add(currentClass.GetManufacturerList()[0]);
            }

            return manufacturerChampions;
        }


        // Saving Functions

        private void SaveResults(string stintName)
        {
            string filePath = Path.Combine(saveFolder, string.Format("{0} - {1}.csv", fileNumber, stintName)),
                writeString = "", overallPosition, classPosition;
            fileNumber++;

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];

                if (currentEntrant.GetRacing())
                {
                    (overallPosition, classPosition) = currentEntrant.GetCurrentPosition();

                    writeString += string.Format("{0},{1},{2},{3} {4},{5},,{6},,{7},,{8},{9},,{10}",
                        overallPosition, currentEntrant.GetClass().GetClassName(), classPosition,
                        currentEntrant.GetCarNo(), currentEntrant.GetTeamName(), currentEntrant.GetModelName(), currentEntrant.GetOVR(),
                        currentEntrant.GetInGarage(), currentEntrant.GetStintsInGarage(), currentEntrant.GetTotalStintsInGarage(), currentEntrant.GetTotalStops()); ;

                    if (i < entryList.Count() - 1)
                    {
                        writeString += "\n";
                    }
                }
            }

            FileHandler.WriteFile(writeString, filePath);
        }

        private void SaveStandings(string folderPath)
        {
            classList = currentSeries.GetClassList();

            string currentClassName = classList[0].GetClassName(),
                writeString = "Pos,Crew No,Team Name,Car Model,Points,,Best Result,,Results\n",
                folderName = Path.Combine(folderPath, string.Format("Class 1 - {0}", currentClassName)),
                fileName = Path.Combine(folderName, "Entrant Standings.csv");
            int classIndex = 0;

            Directory.CreateDirectory(folderPath);
            Directory.CreateDirectory(folderName);

            for (int i = 0; i < entryList.Count(); i++)
            {
                currentEntrant = entryList[i];

                if (currentEntrant.GetClassName() != currentClassName)
                {
                    FileHandler.WriteFile(writeString, fileName);

                    SaveManufacturersStandings(classList[classIndex], folderName);

                    classIndex++;
                    currentClassName = classList[classIndex].GetClassName();

                    writeString = "Pos,Crew No,Team Name,Car Model,Points,,Best Result,,Results\n";

                    folderName = Path.Combine(folderPath, string.Format("Class {0} - {1}", classIndex + 1, currentClassName));
                    fileName = Path.Combine(folderName, "Entrant Standings.csv");

                    Directory.CreateDirectory(folderName);
                }

                writeString += string.Format("{0},{1},{2},{3},{4},,{5},,{6}\n", currentEntrant.GetStandingsPosition(), currentEntrant.GetCarNo(),
                    currentEntrant.GetTeamName(), currentEntrant.GetModelName(), currentEntrant.GetPoints(),
                    currentEntrant.GetBestResultOutput(), currentEntrant.GetResultString());
            }

            FileHandler.WriteFile(writeString, fileName);
            SaveManufacturersStandings(classList[classIndex], folderName);
        }

        private void SaveManufacturersStandings(Class currentClass, string folderPath)
        {
            string filePath = Path.Combine(folderPath, "Manufacturers Standings.csv"),
                writeString = "Pos,Manufacturer,Points,,Best Result,,Results\n";

            Manufacturer currentManufacturer;
            List<Manufacturer> manufacturerList = currentClass.GetManufacturerList();

            for (int i = 0; i < manufacturerList.Count(); i++)
            {
                currentManufacturer = manufacturerList[i];

                writeString += string.Format("P{0},{1},{2},,{3},,{4}\n", i + 1, currentManufacturer.GetManufacturerName(), currentManufacturer.GetPoints(),
                    currentManufacturer.GetBestResultOutput(), currentManufacturer.GetResultString());
            }

            FileHandler.WriteFile(writeString, filePath);
        }


        // Sort Functions

        private void IndexSort(List<Entrant> sortList)
        {
            bool swap;

            for (int i = 0; i < sortList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < sortList.Count() - i - 1; j++)
                {
                    if (sortList[j].GetIndex() > sortList[j + 1].GetIndex())
                    {
                        swap = true;

                        (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }

        private void IsRacingSort(List<Entrant> sortList)
        {
            bool swap;

            for (int i = 0; i < sortList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < sortList.Count() - i - 1; j++)
                {
                    if (sortList[j].GetRacingIndex() > sortList[j + 1].GetRacingIndex())
                    {
                        swap = true;

                        (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }

        private void ClassSort(List<Entrant> sortList)
        {
            bool swap;

            for (int i = 0; i < sortList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < sortList.Count() - i - 1; j++)
                {
                    if (sortList[j].GetClassIndex() > sortList[j + 1].GetClassIndex())
                    {
                        swap = true;

                        (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }

        private void StandingsSort(List<Entrant> sortList)
        {
            string className = sortList[0].GetClassName();
            int startIndex = 0;

            for (int i = 0; i < sortList.Count(); i++)
            {
                if (entryList[i].GetClassName() != className)
                {
                    PointsSort(sortList, startIndex, i);
                    startIndex = i;

                    className = sortList[i].GetClassName();
                }
            }

            PointsSort(sortList, startIndex, sortList.Count());

            foreach (Class currentClass in currentSeries.GetClassList())
            {
                currentClass.SortStandings();
            }
        }

        private void PointsSort(List<Entrant> sortList, int startIndex, int endIndex)
        {
            bool swap;

            int roundIndex = 0;
            List<int> driver1Results, driver2Results;

            for (int i = 0; i < endIndex - 1; i++)
            {
                swap = false;

                for (int j = startIndex; j < endIndex - i - 1; j++)
                {
                    if (sortList[j].GetPoints() < entryList[j + 1].GetPoints())
                    {
                        swap = true;

                        (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
                    }

                    else if (sortList[j].GetPoints() == entryList[j + 1].GetPoints())
                    {
                        driver1Results = ResultsSort(sortList[j].GetRawResults());
                        driver2Results = ResultsSort(sortList[j + 1].GetRawResults());

                        while (roundIndex < driver1Results.Count())
                        {
                            if (driver1Results[roundIndex] > driver2Results[roundIndex])
                            {
                                swap = true;

                                (sortList[j], sortList[j + 1]) = (sortList[j + 1], sortList[j]);
                                break;
                            }

                            else if (driver1Results[roundIndex] == driver2Results[roundIndex])
                            {
                                roundIndex++;
                            }

                            else
                            {
                                break;
                            }
                        }

                        roundIndex = 0;
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }

        private List<int> ResultsSort(List<int> results)
        {
            if (results.Count() > 1)
            {
                bool swap;

                for (int i = 0; i < results.Count() - 1; i++)
                {
                    swap = false;

                    for (int j = 0; j < results.Count() - i - 1; j++)
                    {
                        if (results[j] > results[j + 1])
                        {
                            swap = true;

                            (results[j], results[j + 1]) = (results[j + 1], results[j]);
                        }
                    }

                    if (!swap)
                    {
                        break;
                    }
                }
            }

            return results;
        }


        // Miscellaneous Functions

        private bool GetBoolean(string outputString)
        {
            Console.WriteLine("{0}\nY - Yes\nN - No", outputString);
            Console.Write("Choice: ");

            string strInput = Console.ReadLine().ToUpper();

            if (strInput == "Y" || strInput == "YES")
            {
                return true;
            }

            else if (strInput == "N" || strInput == "NO")
            {
                return false;
            }

            Console.WriteLine("Invalid Input");
            return GetBoolean(outputString);
        }
    }
}
