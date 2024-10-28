using System;
using System.Collections.Generic;

namespace Console_Alpha_V2
{
    public class Round
    {
        string roundName, trackName, lengthType, pointsSystem, folderName;
        int raceLength, roundNumber, raceNumber, incidentRate, dnfRate;

        List<string> classesLong, classesNamed;

        Series memberSeries;

        public Round(string[] roundData, List<string> racingClasses, Series series)
        {
            roundName = roundData[0];
            trackName = roundData[1];

            roundNumber = Convert.ToInt32(roundData[3]);
            raceNumber = Convert.ToInt32(roundData[4]);

            raceLength = Convert.ToInt32(roundData[6]);
            lengthType = roundData[7];

            incidentRate = Convert.ToInt32(roundData[9]);
            dnfRate = Convert.ToInt32(roundData[10]);

            pointsSystem = roundData[12];
            
            memberSeries = series;

            LoadClasses(racingClasses);
        }

        public void LoadClasses(List<string> racingClasses)
        {
            classesLong = new List<string>();
            classesNamed = new List<string>();

            for (int i = 0; i < racingClasses.Count; i++)
            {
                if (racingClasses[i] != "")
                {
                    classesLong.Add(racingClasses[i].Replace("C", "Class "));

                    classesNamed.Add(memberSeries.GetClassList()[Convert.ToInt32(racingClasses[i].Replace("C", "")) - 1].GetClassName());
                }
            }
        }

        public string GetRoundName()
        {
            return roundName;
        }

        public string GetTrackName()
        {
            return trackName;
        }

        public string GetLengthType()
        {
            return lengthType;
        }

        public int GetRoundNumber()
        {
            return roundNumber;
        }

        public int GetRaceNumber()
        {
            return raceNumber;
        }

        public void SetFolder(string newFolder)
        {
            folderName = newFolder;
        }

        public string GetFolder()
        {
            return folderName;
        }

        public List<string> GetLongRacingClasses()
        {
            return classesLong;
        }

        public List<string> GetNamedClasses()
        {
            return classesNamed;
        }

        public int GetRaceLength()
        {
            if (lengthType == "WEC")
            {
                return raceLength * 2;
            }

            else
            {
                return raceLength;
            }
        }

        public int GetDefaultIncidentRate()
        {
            return incidentRate;
        }

        public int GetDefaultDNFRate()
        {
            return dnfRate;
        }

        public string GetPointsSystem()
        {
            return pointsSystem;
        }

        public Series GetSeries()
        {
            return memberSeries;
        }

        public string GetSeriesName()
        {
            return memberSeries.GetSeriesName();
        }
    }
}