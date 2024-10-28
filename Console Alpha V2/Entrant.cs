using Console_Alpha_V2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Console_Alpha_V2
{
    public class Entrant
    {
        string carNo, teamName, currentPositionOverall, currentPositionClass, standingsPosition, bestResult, bestResultString;
        int mainOVR, baseOVR, teamOVR, crewOVR, stintRangeModifier, isRacingIndex,
            reliability, dnfScore, baseCrewReliability, baseReliability, baseDNFScore,
            lastStint, stintsInGarage, totalStintsInGarage, totalStaysInGarage,
            points, currentRawPosition, timesFinished, entrantIndex, stintsSincePit = 0, totalStops;
        bool isRacing, inGarage = false, extendedGarageStay = false;

        List<string> pastResults;
        List<int> rawResults;

        CarModel carModel;
        Class memberClass;
        Series enteredSeries;


        public Entrant(string number, string team, int tOVR, int cOVR, int stintMod, int baseReli,  int index,
            CarModel model, Class enteredClass, Series series)
        {
            carNo = number;
            teamName = team;

            mainOVR = tOVR + cOVR + model.GetMainOVR();
            baseOVR = tOVR + cOVR + model.GetMainOVR();

            teamOVR = tOVR;
            crewOVR = cOVR;

            baseCrewReliability = baseReli;
            baseReliability = baseReli + model.GetReliability() + enteredClass.GetIRM();
            baseDNFScore = enteredClass.GetDNFRM();

            stintRangeModifier = stintMod;

            entrantIndex = index;
            
            bestResult = "";
            timesFinished = 0;
            pastResults = new List<string>();
            rawResults = new List<int>();

            carModel = model;
            memberClass = enteredClass;
            enteredSeries = series;
        }

        public Entrant(string number, string team, int tOVR, int cOVR, int stintMod, int baseReli, int index,
            int scoredPoints, string result, int finished, List<string> resultsFormatted, List<int> resultsRaw,
            CarModel model, Class enteredClass, Series series)
        {
            carNo = number;
            teamName = team;

            mainOVR = tOVR + cOVR + model.GetMainOVR();
            baseOVR = tOVR + cOVR + model.GetMainOVR();

            teamOVR = tOVR;
            crewOVR = cOVR;

            baseCrewReliability = baseReli;
            baseReliability = baseReli + model.GetReliability() + enteredClass.GetIRM();
            baseDNFScore = enteredClass.GetDNFRM();

            stintRangeModifier = stintMod;

            entrantIndex = index;

            points = scoredPoints;
            bestResult = result;
            timesFinished = finished;

            pastResults = new List<string>();
            rawResults = new List<int>();

            for (int i = 0; i < resultsFormatted.Count(); i++)
            {
                pastResults.Add(resultsFormatted[i]);
            }

            for (int i = 0; i < resultsRaw.Count(); i++)
            {
                rawResults.Add(resultsRaw[i]);
            }

            SetBestResultString();

            carModel = model;
            memberClass = enteredClass;
            enteredSeries = series;
        }

        public Entrant() { }


        // Round Details

        public void SetRound(Round currentRound)
        {
            mainOVR = baseOVR;
            reliability = baseReliability + currentRound.GetDefaultIncidentRate();
            dnfScore = baseDNFScore + currentRound.GetDefaultDNFRate();

            lastStint = 0;

            stintsInGarage = 0;
            totalStintsInGarage = 0;
            totalStaysInGarage = 0;

            stintsSincePit = 0;
            totalStops = 0;

            inGarage = false;
            extendedGarageStay = false;

            //Console.WriteLine("{0} - {1}", carNo, mainOVR);
        }

        public void SetRacing(bool newRacing)
        {
            mainOVR = baseOVR;

            isRacing = newRacing;
            isRacingIndex = 1;

            if (!newRacing)
            {
                isRacingIndex = 2;
            }
        }

        public bool GetRacing()
        {
            return isRacing;
        }

        public int GetRacingIndex()
        {
            return isRacingIndex;
        }


        // Entrant Details

        public void SetIndex(int newIndex)
        {
            entrantIndex = newIndex;
        }

        public void SetClass(Class newClass)
        {
            memberClass = newClass;

            baseReliability = baseCrewReliability + carModel.GetReliability() + memberClass.GetIRM();
            baseDNFScore = memberClass.GetDNFRM();
        }

        public Class GetClass()
        {
            return memberClass;
        }

        public int GetClassIndex()
        {
            return memberClass.GetClassIndex();
        }

        public string GetClassName()
        {
            return memberClass.GetClassName();
        }

        public Series GetSeries()
        {
            return enteredSeries;
        }

        public int GetSeriesIndex()
        {
            return enteredSeries.GetSeriesIndex();
        }

        public string GetSeriesName()
        {
            return enteredSeries.GetSeriesName();
        }

        public void SetCarModel(CarModel newCarModel)
        {
            carModel = newCarModel;
            baseReliability = baseCrewReliability + carModel.GetReliability() + memberClass.GetIRM();
        }

        public CarModel GetCarModel()
        {
            return carModel;
        }

        public string GetModelName()
        {
            return carModel.GetModelName();
        }

        public void SetCarNumber(string newCarNumber)
        {
            carNo = newCarNumber;
        }

        public string GetCarNo()
        {
            return carNo;
        }

        public string GetTeamName()
        {
            return teamName;
        }

        public string GetManufacturer()
        {
            return carModel.GetManufacturer();
        }

        public string GetCarAsWriteString()
        {
            return carNo + " " + teamName + "," + carModel.GetModelName() + ",," + mainOVR;
        }

        public void AddToOVR(int AddValue)
        {
            mainOVR += AddValue;
        }

        public void SetGridOVR(int GridSpacing)
        {
            mainOVR = baseOVR + GridSpacing;
        }

        public void UpdateOVR(int NewOVR)
        {
            mainOVR = NewOVR;
        }

        public int GetOVR()
        {
            return mainOVR;
        }

        public int GetBaseOVR()
        {
            return baseOVR;
        }

        public int GetTeamOVR()
        {
            return teamOVR;
        }

        public void SetCrewOVR(int newOVR)
        {
            crewOVR = newOVR;
        }

        public int GetCrewOVR()
        {
            return crewOVR;
        }

        public void SetSRM(int newSRM)
        {
            stintRangeModifier = newSRM;
        }

        public int GetSRM()
        {
            return stintRangeModifier;
        }

        public int GetReliability()
        {
            return reliability;
        }

        public void SetBaseReliability(int newBaseReliability)
        {
            baseCrewReliability = newBaseReliability;
            baseReliability = newBaseReliability + carModel.GetReliability() + memberClass.GetIRM();
        }

        public int GetBaseReliability()
        {
            return baseReliability;
        }

        public int GetBaseCrewReliability()
        {
            return baseCrewReliability;
        }

        public int GetDNF()
        {
            return dnfScore;
        }

        public int GetIndex()
        {
            return entrantIndex;
        }


        // Positions + Standings

        public void SetPoints(int PTS)
        {
            points = PTS;
        }

        public int GetPoints()
        {
            return points;
        }

        public void SetCurrentPositions(string overallPosition, string classPosition, int rawPosition)
        {
            currentPositionOverall = overallPosition;
            currentPositionClass = classPosition;
            currentRawPosition = rawPosition;
        }

        public (string, string) GetCurrentPosition()
        {
            return (currentPositionOverall, currentPositionClass);
        }

        public void SetStandingsPosition(string newPosition)
        {
            standingsPosition = newPosition;
        }

        public string GetStandingsPosition()
        {
            return standingsPosition;
        }

        public void ResetPastResults()
        {
            pastResults.Clear();
            rawResults.Clear();

            bestResult = "";
            bestResultString = "";

            points = 0;
            timesFinished = 0;
        }

        public void AddResult()
        {
            pastResults.Add(currentPositionClass);

            if (currentPositionClass == "NC")
            {
                rawResults.Add(currentRawPosition + 100);
            }

            else if (currentPositionClass == "DNF")
            {
                rawResults.Add(currentRawPosition + 200);
            }

            else
            {
                rawResults.Add(currentRawPosition);
            }

            SetBestResult();
        }

        public List<string> GetPastResults()
        {
            return pastResults;
        }

        public List<int> GetRawResults()
        {
            return rawResults;
        }

        public string GetResultString()
        {
            string resultString = "";

            if (pastResults.Count > 0)
            {
                resultString = pastResults[0];

                for (int i = 1; i < pastResults.Count(); i++)
                {
                    resultString += string.Format(",{0}", pastResults[i]);
                }
            }

            return resultString;
        }

        public string GetResultsStringSave()
        {
            string resultString = "";

            if (pastResults.Count() > 0)
            {
                resultString = pastResults[0];

                for (int i = 1; i < pastResults.Count(); i++)
                {
                    resultString += string.Format(";{0}", pastResults[i]);
                }
            }

            return resultString;
        }

        public string GetRawResultString()
        {
            string resultString = "";

            if (rawResults.Count() > 0)
            {
                resultString = Convert.ToString(rawResults[0]);

                for (int i = 1; i < rawResults.Count(); i++)
                {
                    resultString += string.Format(",{0}", rawResults[i]);
                }
            }

            return resultString;
        }

        public string GetRawResultsStringSave()
        {
            string resultString = "";

            if (rawResults.Count > 0)
            {
                resultString = Convert.ToString(rawResults[0]);

                for (int i = 1; i < rawResults.Count(); i++)
                {
                    resultString += string.Format(";{0}", rawResults[i]);
                }
            }

            return resultString;
        }

        private void SetBestResult()
        {
            bool bestFinish = bestResult == "DNF" || bestResult == "NC",
                currentFinish = currentPositionClass.StartsWith("P");

            if (bestResult == "")
            {
                bestResult = currentPositionClass;
                timesFinished = 1;
            }

            else if (bestResult == currentPositionClass)
            {
                timesFinished++;
            }

            else if (bestFinish && currentFinish)
            {
                bestResult = currentPositionClass;
                timesFinished = 1;
            }

            else if (bestFinish && !currentFinish)
            {
                if (bestResult == "NC" && currentPositionClass == "NC")
                {
                    timesFinished++;
                }

                else if (bestResult == "DNF" && currentPositionClass == "NC")
                {
                    bestResult = currentPositionClass;
                    timesFinished = 1;
                }
            }

            else if (currentFinish)
            {
                int iBestResult = Convert.ToInt32(bestResult.Replace("P", "")),
                    iNewResult = Convert.ToInt32(currentPositionClass.Replace("P", ""));

                if (iNewResult < iBestResult)
                {
                    bestResult = currentPositionClass;
                    timesFinished = 1;
                }
            }

            // Set Standings Best Result String

            SetBestResultString();
        }

        private void SetBestResultString()
        {
            if (bestResult == "P1")
            {
                bestResultString = string.Format("{0} Win", timesFinished);

                if (timesFinished > 1)
                {
                    bestResultString += "s";
                }
            }

            else
            {
                bestResultString = string.Format("Best Result of {0} {1} Time", string.Format("{0},", bestResult).PadRight(4, ' '), timesFinished);

                if (timesFinished > 1)
                {
                    bestResultString += "s";
                }
            }
        }

        public string GetBestResult()
        {
            return bestResultString;
        }

        public string GetBestResultOutput()
        {
            return string.Format("{0}x{1}", bestResult, timesFinished);
        }

        public string GetBestResultSave()
        {
            return string.Format("{0},{1}", bestResult, timesFinished);
        }


        // Race Stuffs

        public void SetLastStint(int Stint)
        {
            lastStint = Stint;
        }

        public int GetLastStint()
        {
            return lastStint;
        }

        public bool GetInGarage()
        {
            return inGarage;
        }

        public void EnterGarage(Random Rand)
        {
            inGarage = true;

            stintsInGarage++;
            totalStintsInGarage++;
            stintsSincePit = 0;
            totalStaysInGarage++;

            Pit();

            if (Rand.Next(1, 11) == 1)
            {
                extendedGarageStay = true;
            }
        }

        public void LeaveGarage()
        {
            inGarage = false;
            stintsInGarage = 0;
        }

        public void StintInGarage()
        {
            stintsInGarage++;
            totalStintsInGarage++;
        }

        public int GetStintsInGarage()
        {
            return stintsInGarage;
        }

        public int GetTotalStintsInGarage()
        {
            return totalStintsInGarage;
        }

        public void Pit()
        {
            stintsSincePit = 0;
            totalStops++;
        }

        public void NotPit()
        {
            stintsSincePit++;
        }

        public int GetStintsSincePit()
        {
            return stintsSincePit;
        }

        public int GetTotalStops()
        {
            return totalStops;
        }

        public bool GetExtendedGarageStay()
        {
            return extendedGarageStay;
        }

        public int GetTotalStaysInGarage()
        {
            return totalStaysInGarage;
        }
    }
}
