using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class HorizontalReelsShiftConfig : BettrMechanicConfig
    {
        public HorizontalReelsShiftConfig(): base()
        {
            this.MechanicName = "HorizontalReelsShift";
            
            TileController.RegisterType<HorizontalReelsShiftConfig>("HorizontalReelsShiftConfig");
        }
    }
    
    public class HorizontalReelsShift
    {
        // Method to perform the swaps and return the list of swaps
        public static List<Tuple<int, int>> GetAdjacentSwaps(int[] initialArray, int[] finalArray)
        {
            List<Tuple<int, int>> swaps = new List<Tuple<int, int>>();
            int[] currentArray = (int[])initialArray.Clone();

            for (int i = 0; i < finalArray.Length; i++)
            {
                while (currentArray[i] != finalArray[i])
                {
                    // Find the index of the correct reel in the current array
                    int targetIndex = Array.IndexOf(currentArray, finalArray[i]);

                    // Swap the target reel with the one just before it until it's in the correct position
                    for (int j = targetIndex; j > i; j--)
                    {
                        // Perform the swap
                        Swap(currentArray, j, j - 1);

                        // Store the swap (swap positions are zero-indexed)
                        swaps.Add(new Tuple<int, int>(j - 1, j));
                    }
                }
            }

            return swaps;
        }

        // Helper method to swap two elements in the array
        private static void Swap(int[] array, int index1, int index2)
        {
            int temp = array[index1];
            array[index1] = array[index2];
            array[index2] = temp;
        }

        // Method to print the swaps
        public static void PrintSwaps(List<Tuple<int, int>> swaps)
        {
            foreach (var swap in swaps)
            {
                Console.WriteLine($"Swap reel at position {swap.Item1} with reel at position {swap.Item2}");
            }
        }

        // Example usage
        public static void Main(string[] args)
        {
            int[] initialArray = { 1, 2, 3, 4, 5 };
            int[] finalArray = { 2, 3, 4, 1, 5 };

            List<Tuple<int, int>> swaps = GetAdjacentSwaps(initialArray, finalArray);
            PrintSwaps(swaps);
        }
    }    
}

