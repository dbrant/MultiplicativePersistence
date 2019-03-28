using System;
using System.Numerics;
using System.Threading;

/// <summary>
/// A hastily thrown together, although fairly well optimized, solution searcher for
/// numbers with the highest multiplicative persistence.
/// </summary>
namespace MultiplicativePersistence
{
    class Program
    {
        const int MAX_SIZE = 500;

        static BigInteger[][] powersOf;
        static string[] stringOf2;
        static string[] stringOf3;


        static void Main(string[] args)
        {
            // Pre-calculate large powers of numbers from 2 to 9.
            powersOf = new BigInteger[10][];
            for (int n = 2; n <= 9; n++)
            {
                powersOf[n] = new BigInteger[MAX_SIZE + 1];
                BigInteger b = n;
                powersOf[n][0] = BigInteger.One;
                powersOf[n][1] = n;
                for (int i = 2; i <= MAX_SIZE; i++)
                {
                    powersOf[n][i] = BigInteger.Pow(b, i);
                }
            }

            stringOf2 = new string[4];
            stringOf2[0] = "";
            stringOf2[1] = "2";
            stringOf2[2] = "22";
            stringOf2[3] = "222";
            stringOf3 = new string[4];
            stringOf3[0] = "";
            stringOf3[1] = "3";
            stringOf3[2] = "33";
            stringOf3[3] = "333";

            
            // Distribute the work across threads...
            int numThreads = Environment.ProcessorCount;
            int startIndex = 0;
            int indexInc = MAX_SIZE / numThreads;

            Thread[] threads = new Thread[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                threads[i] = new Thread(new ThreadStart(new ScanThread(startIndex, startIndex + indexInc).ThreadFunc));
                threads[i].Start();
                startIndex += indexInc;
            }

            for (int i = 0; i < numThreads; i++)
            {
                threads[i].Join();
            }

            Console.WriteLine("Finished.");
            Console.ReadKey();
        }


        class ScanThread
        {
            readonly int start;
            readonly int end;
            readonly int[] digitCounts;

            public ScanThread(int start, int end)
            {
                this.start = start;
                this.end = end;
                digitCounts = new int[10];
            }

            public void ThreadFunc()
            {
                long millis = Environment.TickCount;
                string num = "";
                int n2, n3, n8, n9;

                for (int numTwos = start; numTwos <= end; numTwos++)
                {
                    for (int numThrees = 0; numThrees <= MAX_SIZE; numThrees++)
                    {
                        if (start == 0 && Environment.TickCount - millis > 1000)
                        {
                            Console.WriteLine("Progress: " + ((numTwos - start) * 100f / (end - start)) + "%");
                            millis = Environment.TickCount;
                        }

                        n8 = numTwos / 3;
                        n2 = numTwos % 3;
                        n9 = numThrees / 2;
                        n3 = numThrees % 2;

                        num = stringOf2[n2] + stringOf3[n3];

                        for (int numSevens = 0; numSevens <= MAX_SIZE; numSevens++)
                        {
                            CheckNumber(ref num, numSevens, n8, n9, digitCounts, false);
                        }
                    }
                }
            }
        }
        

        static void CheckNumber(ref string numStr, int numSevens, int numEights, int numNines, int[] digitCounts, bool print)
        {
            if (numStr.Length == 0) return;
            BigInteger i1;
            int steps = 0;
            string str;

            while (true)
            {
                str = steps == 0 ? numStr : i1.ToString();
                if (steps > 0 && str.Length < 2)
                {
                    break;
                }
                if (print)
                {
                    Console.WriteLine(str);
                }
                steps++;

                i1 = BigInteger.One;
                
                if (!CountDigits(ref str, digitCounts))
                {
                    i1 = BigInteger.Zero; continue;
                }
                if (digitCounts[2] > 0) i1 *= powersOf[2][digitCounts[2]];
                if (digitCounts[3] > 0) i1 *= powersOf[3][digitCounts[3]];
                if (digitCounts[4] > 0) i1 *= powersOf[4][digitCounts[4]];
                if (digitCounts[5] > 0) i1 *= powersOf[5][digitCounts[5]];
                if (digitCounts[6] > 0) i1 *= powersOf[6][digitCounts[6]];
                if (digitCounts[7] > 0) i1 *= powersOf[7][digitCounts[7]];
                if (digitCounts[8] > 0) i1 *= powersOf[8][digitCounts[8]];
                if (digitCounts[9] > 0) i1 *= powersOf[9][digitCounts[9]];

                if (steps == 1)
                {
                    i1 *= powersOf[7][numSevens];
                    i1 *= powersOf[8][numEights];
                    i1 *= powersOf[9][numNines];
                }
            }
            
            if (steps > 9 && !print)
            {
                Console.WriteLine("--------------- ZOMG ---------------");
                Console.WriteLine(numStr + new string('7', numSevens) + new string('8', numEights) + new string('9', numNines) + " -- Total steps: " + steps);
                CheckNumber(ref numStr, numSevens, numEights, numNines, digitCounts, true);
            }
        }


        static bool CountDigits(ref string str, int[] digitCounts)
        {
            Array.Clear(digitCounts, 2, 8);
            foreach (var c in str)
            {
                if (c == 0x30) { return false; }
                digitCounts[c - 0x30]++;
            }
            return true;
        }
        
    }
}
