using System;
using System.Collections.Generic;

namespace Console_Alpha_V2
{
    public class Simulator
    {
        Random randomiser;
        Round currentRound;

        public Simulator(Random ra)
        {
            randomiser = ra;
        }

        public void SetRound(Round newRound)
        {
            currentRound = newRound;
        }

        public void Qualifying(List<Entrant> entryList, int entryListCount)
        {
            int stintScore;

            for (int i = 0; i < entryListCount; i++)
            {
                if (entryList[i].GetRacing())
                {
                    stintScore = QualiStint(entryList[i]);

                    if (entryList[i].GetBaseOVR() + stintScore > entryList[i].GetOVR())
                    {
                        entryList[i].UpdateOVR(entryList[i].GetBaseOVR() + stintScore);
                        entryList[i].SetLastStint(stintScore);
                    }
                }
            }

            Sort(entryList, 0, entryListCount);
        }

        public void Qualifying(List<Entrant> entryList, int start, int end)
        {
            int StintScore;

            for (int i = start; i < end; i++)
            {
                StintScore = QualiStint(entryList[i]);

                if (entryList[i].GetBaseOVR() + StintScore > entryList[i].GetOVR())
                {
                    entryList[i].UpdateOVR(entryList[i].GetBaseOVR() + StintScore);
                    entryList[i].SetLastStint(StintScore);
                }
            }

            Sort(entryList, 0, end);
        }

        public List<HyperPole> HyperPole(List<HyperPole> hyperPoleEntrants)
        {
            foreach (HyperPole currentHyperPole in hyperPoleEntrants)
            {
                int stintScore;

                for (int i = 0; i < currentHyperPole.GetLength(); i++)
                {
                    stintScore = QualiStint(currentHyperPole.GetEntrant(i));

                    if (currentHyperPole.GetEntrant(i).GetBaseOVR() + stintScore > currentHyperPole.GetEntrant(i).GetOVR())
                    {
                        currentHyperPole.GetEntrant(i).UpdateOVR(currentHyperPole.GetEntrant(i).GetBaseOVR() + stintScore);
                        currentHyperPole.GetEntrant(i).SetLastStint(stintScore);
                    }
                }

                currentHyperPole.Sort();
            }

            return hyperPoleEntrants;
        }

        public int QualiStint(Entrant entrantData)
        {
            int stintScore, incidentScore;
            int incidentLow = 1, dnfScore = entrantData.GetDNF();

            Class entrantClass = entrantData.GetClass();

            stintScore = randomiser.Next(entrantClass.GetSRLow(), entrantClass.GetSRHigh() + entrantData.GetSRM());
            incidentScore = randomiser.Next(incidentLow, entrantData.GetReliability());

            if (incidentScore < dnfScore)
            {
                if (incidentScore == 1)
                {
                    stintScore = 5;
                }

                else
                {
                    stintScore -= entrantClass.GetSRInc();
                }
            }

            return stintScore;
        }

        public void Race(List<Entrant> entryList, int stintNumber, int racingCount)
        {
            int stintScore, pitScore = 0;

            foreach (Entrant currentEntrant in entryList)
            {
                if (currentEntrant.GetOVR() == 1)
                {
                    break;
                }

                else if (!currentEntrant.GetRacing())
                {
                    break;
                }

                else if (currentEntrant.GetInGarage())
                {
                    if (ShouldRetire(currentEntrant, stintNumber))
                    {
                        currentEntrant.UpdateOVR(1);
                    }

                    else
                    {
                        bool LeaveGarage = ShouldLeaveGarage(currentEntrant, stintNumber);
                        stintScore = RaceStint(currentEntrant, stintNumber, LeaveGarage);

                        if (stintScore == 1)
                        {
                            currentEntrant.UpdateOVR(1);
                        }

                        else if (LeaveGarage)
                        {
                            currentEntrant.LeaveGarage();
                            currentEntrant.AddToOVR(stintScore);
                        }

                        else
                        {
                            currentEntrant.StintInGarage();
                            currentEntrant.AddToOVR(stintScore);
                        }
                    }
                }

                else
                {
                    stintScore = RaceStint(currentEntrant, stintNumber, false);

                    if (stintScore == 1)
                    {
                        currentEntrant.UpdateOVR(1);
                    }

                    else if (stintScore <= 10)
                    {
                        currentEntrant.AddToOVR(stintScore);

                        currentEntrant.EnterGarage(randomiser);
                        currentEntrant.Pit();
                    }

                    else
                    {
                        if (ShouldPit(currentEntrant))
                        {
                            pitScore = randomiser.Next(15, 31);
                            currentEntrant.Pit();
                        }

                        else
                        {
                            currentEntrant.NotPit();
                        }

                        currentEntrant.AddToOVR(stintScore - pitScore);
                    }
                }
            }

            Sort(entryList, 0, racingCount);
        }

        public int RaceStint(Entrant entrantData, int stintNumber, bool leaveGarage)
        {
            int StintScore, Incident;
            int IL = 1, DNF = entrantData.GetDNF();

            Class entrantClass = entrantData.GetClass();

            StintScore = randomiser.Next(entrantClass.GetSRLow(), entrantClass.GetSRHigh() + entrantData.GetSRM());
            Incident = randomiser.Next(IL, entrantData.GetReliability());

            if (Incident < DNF)
            {
                if (Incident == 1)
                {
                    StintScore = 1;
                }

                else
                {
                    StintScore -= entrantClass.GetSRInc();
                }
            }

            else if (leaveGarage || Incident < CommonData.GetEnterGarageValues(currentRound.GetLengthType())[stintNumber - 1] + entrantData.GetDNF())
            {
                StintScore = TimeLostInGarage(entrantData, leaveGarage);
            }

            return StintScore;
        }

        public bool ShouldLeaveGarage(Entrant entrantData, int stintNumber)
        {
            bool leaveGarage = false;
            string lengthType = currentRound.GetLengthType();
            int leaveGarageMaxChance, minStints = 1;

            leaveGarageMaxChance = CommonData.GetLeaveGarageValues(lengthType)[stintNumber - 1];

            double Chance = (leaveGarageMaxChance) * 0.65;

            if (Chance < 1)
            {
                Chance = 1;
            }

            if (entrantData.GetExtendedGarageStay())
            {
                if (lengthType == "WEC")
                {
                    minStints = currentRound.GetRaceLength() / 6;
                }

                else if (lengthType == "IMSA")
                {
                    minStints = 2;
                }

                else
                {
                    minStints = currentRound.GetRaceLength() / 10;
                }
            }

            if (lengthType == "WEC")
            {
                if (entrantData.GetStintsInGarage() <= randomiser.Next(minStints, 11))
                {
                    if (randomiser.Next(1, leaveGarageMaxChance) <= Chance)
                    {
                        leaveGarage = true;
                    }
                }
            }

            else if (lengthType == "IMSA")
            {
                if (entrantData.GetStintsInGarage() <= randomiser.Next(minStints, 5))
                {
                    if (randomiser.Next(1, leaveGarageMaxChance) <= Chance)
                    {
                        leaveGarage = true;
                    }
                }
            }

            else
            {
                if (entrantData.GetStintsInGarage() <= randomiser.Next(minStints, 25))
                {
                    if (randomiser.Next(1, leaveGarageMaxChance) <= Chance)
                    {
                        leaveGarage = true;
                    }
                }
            }

            return leaveGarage;
        }

        public bool ShouldRetire(Entrant entrantData, int stintNumber)
        {
            bool shouldRetire = false;

            if (currentRound.GetLengthType() == "WEC")
            {
                if (stintNumber > 12)
                {
                    if (entrantData.GetTotalStaysInGarage() > 2 && entrantData.GetTotalStintsInGarage() > 10)
                    {
                        if (randomiser.Next(1, 6) <= 2)
                        {
                            shouldRetire = true;
                        }
                    }

                    else
                    {
                        if (entrantData.GetTotalStintsInGarage() > 8)
                        {
                            if (randomiser.Next(1, 4) == 1)
                            {
                                shouldRetire = true;
                            }
                        }

                        else if (entrantData.GetTotalStintsInGarage() > 4)
                        {
                            if (randomiser.Next(1, 9) == 1)
                            {
                                shouldRetire = true;
                            }
                        }
                    }
                }

                else
                {
                    if (entrantData.GetTotalStaysInGarage() > 2 && entrantData.GetTotalStintsInGarage() > 10)
                    {
                        if (randomiser.Next(1, 6) < 2)
                        {
                            shouldRetire = true;
                        }
                    }

                    else
                    {
                        if (entrantData.GetTotalStintsInGarage() > 8)
                        {
                            if (randomiser.Next(1, 6) == 1)
                            {
                                shouldRetire = true;
                            }
                        }

                        else if (entrantData.GetTotalStintsInGarage() > 4)
                        {
                            if (randomiser.Next(1, 11) == 1)
                            {
                                shouldRetire = true;
                            }
                        }
                    }
                }
            }

            else if (currentRound.GetLengthType() == "IMSA")
            {
                if (entrantData.GetStintsInGarage() >= 6)
                {
                    shouldRetire = true;
                }
            }

            else
            {
                if (entrantData.GetStintsInGarage() > randomiser.Next(8, 15))
                {
                    shouldRetire = true;
                }
            }

            return shouldRetire;
        }

        public bool ShouldPit(Entrant entrantData)
        {
            bool shouldPit = false;

            Class memberClass = entrantData.GetClass();

            if (currentRound.GetLengthType() == "WEC")
            {
                if (entrantData.GetStintsSincePit() >= memberClass.GetWECDTP() + randomiser.Next(-1, 2))
                {
                    shouldPit = true;
                }
            }

            else if (currentRound.GetLengthType() == "IMSA")
            {
                if (entrantData.GetStintsSincePit() >= memberClass.GetIMSADTP() + randomiser.Next(-1, 2))
                {
                    shouldPit = true;
                }
            }

            else
            {
                if (entrantData.GetStintsSincePit() >= memberClass.GetLapDTP() + randomiser.Next(-5, 6))
                {
                    shouldPit = true;
                }
            }

            return shouldPit;
        }

        public int TimeLostInGarage(Entrant entrantData, bool leaveGarage)
        {
            int timeLost = 0, minValue, maxValue, fullStint;

            Class memberClass = entrantData.GetClass();

            if (currentRound.GetLengthType() == "WEC")
            {
                (minValue, maxValue, fullStint) = memberClass.GetWECGarageValues();

                if (leaveGarage || entrantData.GetStintsInGarage() <= 1)
                {
                    timeLost = randomiser.Next(minValue, maxValue) - (2 * memberClass.GetSRInc());
                }

                else
                {
                    timeLost = fullStint; // -150;
                }
            }

            else if (currentRound.GetLengthType() == "IMSA")
            {
                (minValue, maxValue, fullStint) = memberClass.GetIMSAGarageValues();

                if (leaveGarage || entrantData.GetStintsInGarage() == 1)
                {
                    timeLost = randomiser.Next(minValue, maxValue) - (2 * memberClass.GetSRInc());
                }

                else
                {
                    timeLost = fullStint; // -150;
                }
            }

            else if (currentRound.GetLengthType() == "Laps")
            {
                (minValue, maxValue, fullStint) = memberClass.GetLapGarageValues();

                if (leaveGarage || entrantData.GetStintsInGarage() == 1)
                {
                    timeLost = randomiser.Next(minValue, maxValue) - (2 * memberClass.GetSRInc());
                }

                else
                {
                    timeLost = fullStint; // -150;
                }
            }

            return timeLost;
        }

        public void SetGrid(List<Entrant> entryList, int spacing)
        {
            int multiplier = entryList.Count;

            foreach (Entrant entrantData in entryList)
            {
                entrantData.SetGridOVR(multiplier * spacing);
                multiplier--;
            }
        }

        public void Sort(List<Entrant> entryList, int start, int end)
        {
            bool swap;

            for (int i = 0; i < end - 1; i++)
            {
                swap = false;

                for (int j = start; j < end - i - 1; j++)
                {
                    if (entryList[j].GetOVR() < entryList[j + 1].GetOVR())
                    {
                        swap = true;

                        (entryList[j], entryList[j + 1]) = (entryList[j + 1], entryList[j]);
                    }

                    else if (entryList[j].GetOVR() == entryList[j + 1].GetOVR())
                    {
                        if (entryList[j].GetOVR() == 1)
                        {
                            continue;
                        }

                        else if (entryList[j].GetLastStint() < entryList[j + 1].GetLastStint())
                        {
                            swap = true;

                            (entryList[j], entryList[j + 1]) = (entryList[j + 1], entryList[j]);
                        }
                    }
                }

                if (!swap)
                {
                    break;
                }
            }
        }
    }
}
