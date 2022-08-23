using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics;

namespace HBv2_2
{
    public delegate byte[] HashingHandler(byte[] guessBytes);

    class Program
    {
        // Threads
        static int maxThreads = Environment.ProcessorCount;
        static List<Thread> threads = new List<Thread>();

        // Charset
        //static char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}|;':,./<>?".ToCharArray(); // Declared as an array of bytes below to save resources by not having to convert string to byte array, before hashing.
        static byte[] charsByte = { 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113,
                                    114, 115, 116, 117, 118, 119, 120, 121, 122, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74,
                                    75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 48, 49, 50, 51, 52, 53,
                                    54, 55, 56, 57, 33, 64, 35, 36, 37, 94, 38, 42, 40, 41, 95, 43, 45, 61, 91, 93, 123,
                                    125, 124, 59, 39, 58, 44, 46, 47, 60, 62, 63 };
        static int numOfChars = charsByte.Length;

        static int maxLength = 6;
        static int[,] wordCharVals;
        static int[] currentLengths;
        static bool[] finishesArray;
        static int finishes = 0;
        static int hashI;

        static void Bruteforcer(object i)
        {
            HashingHandler hhandler;
            switch (hashI)
            {
                case 0:
                    hhandler = MD5.Create().ComputeHash;
                    break;
                case 1:
                    hhandler = SHA1.Create().ComputeHash;
                    break;
                case 2:
                    hhandler = SHA256.Create().ComputeHash;
                    break;
                case 3:
                    hhandler = SHA384.Create().ComputeHash;
                    break;
                default:
                    hhandler = SHA512.Create().ComputeHash;
                    break;
            }

            int indexOfThr = Convert.ToInt32(i);
            wordCharVals[indexOfThr, 0] = indexOfThr;

            while (!finishesArray[indexOfThr])
            {
                // Put guess together
                byte[] guessBytes = new byte[currentLengths[indexOfThr]];
                for (int j = 0; j < currentLengths[indexOfThr]; j++)
                     guessBytes[j] = charsByte[wordCharVals[indexOfThr, j]];

                // Create hash from guess
                byte[] hashBytes = hhandler(guessBytes);

                // Check if hashes match
                if (hashToCrackBytes.SequenceEqual(hashBytes))
                {
                    for (int k = 0; k < finishesArray.Length; k++)
                        finishesArray[k] = true;

                    WriteResult(true, hashBytes, new string(guessBytes.Select(a => (char)a).ToArray()));
                }

                // 2.
                wordCharVals[indexOfThr, currentLengths[indexOfThr] - 1] += maxThreads;

                // 3.
                if (wordCharVals[indexOfThr, currentLengths[indexOfThr] - 1] >= numOfChars)
                {
                    wordCharVals[indexOfThr, currentLengths[indexOfThr] - 1] -= numOfChars;
                    if (currentLengths[indexOfThr] > 1)
                    {
                        wordCharVals[indexOfThr, currentLengths[indexOfThr] - 2]++;
                        RecursiveCheck(currentLengths[indexOfThr] - 2, indexOfThr);
                    }
                    else
                    {
                        currentLengths[indexOfThr]++;
                        wordCharVals[indexOfThr, currentLengths[indexOfThr] - 1] = wordCharVals[indexOfThr, currentLengths[indexOfThr] - 2];
                        wordCharVals[indexOfThr, currentLengths[indexOfThr] - 2] = 0;
                    }
                }
            }
        }

        static void RecursiveCheck(int index, int thrI)
        {
            if (wordCharVals[thrI, index] == numOfChars && index > 0)
            {
                wordCharVals[thrI, index] = 0;
                wordCharVals[thrI, index - 1]++;
                RecursiveCheck(index - 1, thrI);
            }
            else if (wordCharVals[thrI, index/*= 0*/] == numOfChars && currentLengths[thrI] < maxLength)
            {
                currentLengths[thrI]++;
                wordCharVals[thrI, currentLengths[thrI] - 1] = wordCharVals[thrI, currentLengths[thrI] - 2];
                wordCharVals[thrI, currentLengths[thrI] - 2] = 0;
                wordCharVals[thrI, index] = 0;
            }
            else if (wordCharVals[thrI, index/*= 0*/] == numOfChars)
            {
                finishes++;
                if (finishes == maxLength) WriteResult(false);
                else finishesArray[thrI] = true;
            }
        }

        static void WriteResult(bool found, byte[] hashBytes = null, string guessStr = null)
        {
            sw.Stop();

            if (found)
            {
                while (!displayDone) ;

                var sb = new StringBuilder();
                for (int j = 0; j < hashBytes.Length; j++)
                    sb.Append(hashBytes[j].ToString("X2"));

                string hashStr = sb.ToString();


                string[] hashTypes = { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" };

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{hashTypes[hashI]} match found!");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\"");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(hashStr); 
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\"");

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("  -->  ");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\"");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(guessStr);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\"");


                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("Elapsed time: ");
                Console.ForegroundColor = ConsoleColor.Red;
                if (sw.Elapsed.Days > 0) Console.Write($"{sw.Elapsed.Days} days ");
                Console.WriteLine(sw.Elapsed);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"No {hashI} matches found!");

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Try increasing the maximum length to have a better chance at finding the original hash!");
            }
        }

        static void Warn()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("!!!  WARNING  !!!   ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The maximum length for the guesses wasn't given or the value was smaller than the minimum (4), so the default (6) value is being used!");
        }

        static void Exit(string msg = null, int code = 0)
        {
            if (msg != null)
            {
                if (code > 0) Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
            }

            Console.ReadKey();
            Environment.Exit(code);
        }

        static string hashToCrack;
        static byte[] hashToCrackBytes;
        static bool displayDone = false;
        static Stopwatch sw = new Stopwatch();
        static void Main(string[] args)
        {
            // Input parsing
            switch (args.Length)
            {
                case 0:
                    Exit("No parameters were given!\nPlease use the following syntax: \"hash-brute <hash> <max length of guesses>\"");
                    break;
                case 1:
                    Warn();
                    break;
                default:
                    if (int.TryParse(args[1], out int result) && result >= 4)
                    {
                        maxLength = result;
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.Write("The maximum length is set to: ");
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(result);
                    }
                    else Warn();
                    break;
            }
            wordCharVals = new int[maxThreads, maxLength];
            currentLengths = new int[maxThreads].Select(a => a = 1).ToArray();
            finishesArray = new bool[maxThreads];


            /// For more interaction
            ///do 
            ///{
            ///    Console.Write("Please enter your hash to crack: ");
            ///    hashToCrack = Console.ReadLine().Split(' ')[0];
            ///} while (hashToCrack == "");
            ///
            hashToCrack = args[0].ToUpper();

            // Hash length validation
            int[] validLengths = { 32, 40, 64, 96, 128 }; // MD5, SHA1, SHA256-384-512

            if (!validLengths.Contains(hashToCrack.Length))
                Exit("Invalid input detected!\nPlease check if there are any typos or if your type of hash is supported!", 1);

            // Hash charset validation
            char[] validChars = "0123456789ABCDEF".ToCharArray();

            foreach (var character in hashToCrack.ToCharArray())
            {
                if (!validChars.Contains(character))
                {
                    Exit("Invalid input detected!\nYour input contains non-hexadecimal values, please check for any typos!", 1);
                    break;
                }
            }

            hashToCrackBytes = Enumerable.Range(0, hashToCrack.Length)
                                     .Where(x => x % 2 == 0)
                                     .Select(x => Convert.ToByte(hashToCrack.Substring(x, 2), 16))
                                     .ToArray();
            

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Available logical cores: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(maxThreads);
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Starting allocation...");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("[" + new String(' ', 10) + "]  0%");
            Console.ForegroundColor = ConsoleColor.Cyan;

            // Timer
            sw.Start();

            int i = -1;
            while (++i < maxThreads)
            {
                Console.SetCursorPosition(1, 3);

                switch (hashToCrack.Length)
                {
                    case 32:
                        hashI = 0;
                        break;
                    case 40:
                        hashI = 1;
                        break;
                    case 64:
                        hashI = 2;
                        break;
                    case 96:
                        hashI = 3;
                        break;
                    case 128:
                        hashI = 4;
                        break;
                }
                threads.Add(new Thread(Bruteforcer));
                threads[i].Start(i);
                
                int x = (int)Math.Round((decimal)(i + 1) / maxThreads * 100, 0);
                var s = new String('-', x / 10);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(s);

                Console.CursorLeft = 14;
                if (x <= 25) Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (x <= 50) Console.ForegroundColor = ConsoleColor.Yellow;
                else if (x <= 75) Console.ForegroundColor = ConsoleColor.DarkGreen;
                else Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"{x}%");

                if (i == maxThreads - 1)
                {
                    Console.SetCursorPosition(0, 4);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Maximum number of threads have been succesfully allocated!");

                    displayDone = true;
                }
            }

            Console.ReadKey();
        }
    }
}
