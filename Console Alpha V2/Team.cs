using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Console_Alpha_V2
{
    public class Team
    {
        string teamName;
        int teamOVR;

        List<int> spacerList;

        List<Entrant> crewList, oldCrews;
        List<string> enteredSeriesList;

        public Team(string name, int ovr)
        {
            teamName = name;
            teamOVR = ovr;

            crewList = new List<Entrant>();
            oldCrews = new List<Entrant>();

            enteredSeriesList = new List<string>();

            spacerList = new List<int>();
        }

        public void SetSpacerList()
        {
            spacerList.Clear();

            foreach (Entrant newEntrant in crewList)
            {
                if (spacerList.Count() == 0)
                {
                    spacerList.Add(newEntrant.GetSeriesName().Length);
                    spacerList.Add(newEntrant.GetClassName().Length);
                    spacerList.Add(newEntrant.GetCarNo().Length);
                    spacerList.Add(newEntrant.GetModelName().Length);
                    spacerList.Add(newEntrant.GetManufacturer().Length);
                }

                else
                {
                    if (newEntrant.GetSeriesName().Length > spacerList[0])
                    {
                        spacerList[0] = newEntrant.GetSeriesName().Length;
                    }

                    if (newEntrant.GetClassName().Length > spacerList[1])
                    {
                        spacerList[1] = newEntrant.GetClass().GetClassName().Length;
                    }

                    if (newEntrant.GetCarNo().Length > spacerList[2])
                    {
                        spacerList[2] = newEntrant.GetCarNo().Length;
                    }

                    if (newEntrant.GetModelName().Length > spacerList[3])
                    {
                        spacerList[3] = newEntrant.GetModelName().Length;
                    }

                    if (newEntrant.GetManufacturer().Length > spacerList[4])
                    {
                        spacerList[4] = newEntrant.GetManufacturer().Length;
                    }
                }
            }
        }

        public void SetTeamName(string newTeamName)
        {
            teamName = newTeamName;
        }

        public string GetTeamName()
        {
            return teamName;
        }

        public void SetTeamOVR(int newOVR)
        {
            teamOVR = newOVR;
        }

        public int GetTeamOVR()
        {
            return teamOVR;
        }

        public List<string> GetEnteredSeriesList()
        {
            return enteredSeriesList;
        }

        public bool GetCompeting(string currentSeries)
        {
            bool teamCompeting = false;

            foreach (Entrant currentEntrant in crewList)
            {
                if (currentEntrant.GetSeriesName() == currentSeries)
                {
                    teamCompeting = true;
                    currentEntrant.SetRacing(true);
                }

                else
                {
                    currentEntrant.SetRacing(false);
                }
            }

            return teamCompeting;
        }

        public void AddCrew(Entrant newCrew)
        {
            crewList.Add(newCrew);

            if (!enteredSeriesList.Contains(newCrew.GetSeriesName()))
            {
                enteredSeriesList.Add(newCrew.GetSeriesName());
            }
        }

        public List<Entrant> GetTeamEntries()
        {
            return crewList;
        }

        public int GetClassEnteredCount(Class checkClass)
        {
            int count = 0;

            if (crewList.Count > 0)
            {
                foreach (Entrant currentEntrant in crewList)
                {
                    if (currentEntrant.GetClass() == checkClass)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public void DeleteCrew(Entrant oldCrew)
        {
            crewList.Remove(oldCrew);
            oldCrews.Add(oldCrew);
        }

        public List<Entrant> GetOldCrews()
        {
            return oldCrews;
        }

        public List<int> GetSpacerList()
        {
            return spacerList;
        }

        public void OrderCrews()
        {
            // Order by Number

            int carNumber1, carNumber2;
            bool swap;

            for (int i = 0; i < crewList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < crewList.Count() - i - 1; j++)
                {
                    carNumber1 = Convert.ToInt32(crewList[j].GetCarNo().Replace("#", ""));
                    carNumber2 = Convert.ToInt32(crewList[j + 1].GetCarNo().Replace("#", ""));

                    if (carNumber1 > carNumber2)
                    {
                        swap = true;

                        (crewList[j], crewList[j + 1]) = (crewList[j + 1], crewList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }

            // Order by Class

            for (int i = 0; i < crewList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < crewList.Count() - i - 1; j++)
                {
                    if (crewList[j].GetClassIndex() > crewList[j + 1].GetClassIndex())
                    {
                        swap = true;

                        (crewList[j], crewList[j + 1]) = (crewList[j + 1], crewList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }

            // Order by Series

            for (int i = 0; i < crewList.Count() - 1; i++)
            {
                swap = false;

                for (int j = 0; j < crewList.Count() - i - 1; j++)
                {
                    if (crewList[j].GetSeriesIndex() > crewList[j + 1].GetSeriesIndex())
                    {
                        swap = true;

                        (crewList[j], crewList[j + 1]) = (crewList[j + 1], crewList[j]);
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }

        public void UpdateCrewStats(Random randomiser)
        {
            Entrant currentEntrant;

            int newOVR, newReliability;

            for (int i = 0; i < crewList.Count(); i++)
            {
                currentEntrant = crewList[i];

                (newOVR, newReliability) = UpdateCrewStat(currentEntrant, randomiser);

                currentEntrant.SetCrewOVR(newOVR);
                currentEntrant.SetBaseReliability(newReliability);
                currentEntrant.ResetPastResults();
            }
        }

        public (int, int) UpdateCrewStat(Entrant currentEntrant, Random randomiser)
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

            int ovrLowerRange = ovrUpperRange - randomiser.Next(2, 5),
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

        public void SaveTeamResults(int seasonNumber)
        {
            string fileName = Path.Combine(CommonData.GetSeasonFolder(), string.Format("Season {0} Results.csv", seasonNumber)),
                writeString = string.Format("{0} - Season {1} Results\n", teamName, seasonNumber) + "Crew No,Series,Class,Car No,Model,Manufacturer,Position,Points,Best Result,,Results";

            int crewIndex = 1;

            foreach (Entrant currentEntrant in crewList)
            {
                writeString += string.Format("\nCrew {0},{1},{2},{3},{4},{5},{6},,{7}", crewIndex, currentEntrant.GetSeries().GetAbbreviation(),
                    currentEntrant.GetClassName(), currentEntrant.GetCarNo(), currentEntrant.GetModelName(),
                    currentEntrant.GetManufacturer(), currentEntrant.GetStandingsPosition(), currentEntrant.GetPoints(),
                    currentEntrant.GetBestResultOutput(), currentEntrant.GetResultString());

                crewIndex++;
            }

            FileHandler.WriteFile(writeString, fileName);
        }

        public void ResetCrewResults()
        {
            foreach (Entrant currentEntrant in crewList)
            {
                currentEntrant.ResetPastResults();
            }
        }
    }
}
