using System.Collections.Generic;

namespace Console_Alpha_V2
{
    public class HyperPole
    {
        List<Entrant> entryList = new List<Entrant>();

        public void AddCar(Entrant entrant)
        {
            entryList.Add(entrant);
        }

        public Entrant GetEntrant(int entrantIndex)
        {
            return entryList[entrantIndex];
        }

        public int GetLength()
        {
            return entryList.Count;
        }

        public void Sort()
        {
            bool swap;

            for (int i = 0; i < entryList.Count - 1; i++)
            {
                swap = false;

                for (int j = 0; j < entryList.Count - i - 1; j++)
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
