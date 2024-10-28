using System;
using System.Collections.Generic;
using System.Linq;

namespace Console_Alpha_V2
{
    public class Manufacturer
    {
        string manufacturerName, bestResult, bestResultString;
        int points, timesFinished;

        List<string> pastResults;
        List<int> rawResults;

        public Manufacturer(string name)
        {
            manufacturerName = name;

            points = 0;

            timesFinished = 0;
            bestResult = "";

            pastResults = new List<string>();
            rawResults = new List<int>();
        }

        public Manufacturer(string name, int scoredPoints, string result, int finished, List<string> resultsPast, List<int> resultsRaw)
        {
            manufacturerName = name;

            points = scoredPoints;

            bestResult = result;
            timesFinished = finished;

            SetBestResultString();

            pastResults = resultsPast;
            rawResults = resultsRaw;
        }

        public string GetManufacturerName()
        {
            return manufacturerName;
        }

        public void AddResult(string newResult, int rawNewResult)
        {
            pastResults.Add(newResult);

            if (newResult == "NC")
            {
                rawResults.Add(rawNewResult + 100);
            }

            else if (newResult == "DNF")
            {
                rawResults.Add(rawNewResult + 200);
            }

            else
            {
                rawResults.Add(rawNewResult);
            }

            SetBestResult(newResult);
        }

        private void SetBestResult(string currentPositionClass)
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


        public List<int> GetResults()
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

            if (pastResults.Count > 0)
            {
                resultString = pastResults[0];

                for (int i = 1; i < pastResults.Count(); i++)
                {
                    resultString += string.Format(";{0}", pastResults[i]);
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

        public void AddPoints(int scoredPoints)
        {
            points += scoredPoints;
        }

        public int GetPoints()
        {
            return points;
        }
    }
}
