using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;

namespace HBv2_2
{
    public delegate byte[] HashingHandler(byte[] guessBytes);

    class Program
    {
        static string help = "Usage: hash-brute [option(s)]\n" +
                             " The options are:\n" +
                            $"   {"-i <hash>", -15} The hash to be cracked by the application. Not optional yet.\n" +
                            $"   {"-m <max_length>", -15} The maximum number of characters the application can use to generate guesses. Optional, if used, must be after -i";

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
        static List<int>[] wordCharVals;
        static int[] currentLengths;
        static bool[] finishesArray;
        static int finishes = 0;
        static int hashI;

        static List<string>[] output;
        static string outputName = "output.txt";

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
            wordCharVals[indexOfThr][0] = indexOfThr;

            while (!finishesArray[indexOfThr])
            {
                // Put guess bytes together
                byte[] guessBytes = new byte[currentLengths[indexOfThr]];
                for (int j = 0; j < currentLengths[indexOfThr]; j++)
                    guessBytes[j] = charsByte[wordCharVals[indexOfThr][j]];

                // Create hash from guess
                byte[] hashBytes = hhandler(guessBytes);

                // Check if a hash was given and the hashes match
                if (argI && hashToCrackBytes.SequenceEqual(hashBytes))
                {
                    for (int k = 0; k < finishesArray.Length; k++)
                        finishesArray[k] = true;

                    WriteResult(true, hashBytes, new string(guessBytes.Select(a => (char)a).ToArray()));
                }

                // Save guess and hash to the correct list if requested
                if (argS)
                {
                    // Distribution between lists if requested with -n option using the modulo approach
                    int m = 0;
                    if (output.Length != 1)
                    {
                        var hc = hashBytes.GetHashCode();
                        m = hc % output.Length;
                    }

                    var sb = new StringBuilder();
                    for (int j = 0; j < hashBytes.Length; j++)
                        sb.Append(hashBytes[j].ToString("X2"));

                    output[m].Add($"{new string(guessBytes.Select(a => (char)a).ToArray())} {sb.ToString()}");
                }

                // 2.
                wordCharVals[indexOfThr][currentLengths[indexOfThr] - 1] += maxThreads;

                // 3.
                if (wordCharVals[indexOfThr][currentLengths[indexOfThr] - 1] >= numOfChars)
                {
                    wordCharVals[indexOfThr][currentLengths[indexOfThr] - 1] -= numOfChars;
                    if (currentLengths[indexOfThr] > 1)
                    {
                        wordCharVals[indexOfThr][currentLengths[indexOfThr] - 2]++;
                        RecursiveCheck(currentLengths[indexOfThr] - 2, indexOfThr);
                    }
                    else
                    {
                        currentLengths[indexOfThr]++;
                        wordCharVals[indexOfThr].Add(0);

                        wordCharVals[indexOfThr][currentLengths[indexOfThr] - 1] = wordCharVals[indexOfThr][currentLengths[indexOfThr] - 2];
                        wordCharVals[indexOfThr][currentLengths[indexOfThr] - 2] = 0;
                    }
                }
            }
        }

        static void RecursiveCheck(int index, int thrI)
        {
            if (wordCharVals[thrI][index] == numOfChars && index > 0)
            {
                wordCharVals[thrI][index] = 0;
                wordCharVals[thrI][index - 1]++;
                RecursiveCheck(index - 1, thrI);
            }
            else if (wordCharVals[thrI][index/*= 0*/] == numOfChars && currentLengths[thrI] < maxLength)
            {
                currentLengths[thrI]++;
                wordCharVals[thrI].Add(0);

                wordCharVals[thrI][currentLengths[thrI] - 1] = wordCharVals[thrI][currentLengths[thrI] - 2];
                wordCharVals[thrI][currentLengths[thrI] - 2] = 0;
                wordCharVals[thrI][index] = 0;
            }
            else if (wordCharVals[thrI][index/*= 0*/] == numOfChars)
            {
                finishes++;
                if (finishes == maxLength) WriteResult(false);
                else finishesArray[thrI] = true;
            }
        }

        static void WriteResult(bool found, byte[] hashBytes = null, string guessStr = null)
        {
            sw.Stop();

            string[] hashTypes = { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" };

            // Write created rainbow tables to files
            if (argS)
            {
                var split2 = outputName.Split('.');
                string extension = split2[split2.Length - 1];

                string fileN = outputName.Substring(0, outputName.Length - extension.Length - 1);

                for (int i = 0; i < output.Length; i++)
                {
                    output[i][0] += "type=" + hashTypes[hashI];

                    File.WriteAllLines($"{fileN}_{i + 1}_of_{output.Length}.{extension}", output[i]);
                }
            }

            // Display result
            if (found)
            {
                while (!displayDone) ;

                var sb = new StringBuilder();
                for (int j = 0; j < hashBytes.Length; j++)
                    sb.Append(hashBytes[j].ToString("X2"));

                string hashStr = sb.ToString();


                //string[] hashTypes = { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" };

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

        static void Warn(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("!!! WARNING !!!   ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
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

        static void CheckHash()
        {
            // Hash length validation
            int[] validLengths = { 32, 40, 64, 96, 128 }; // MD5, SHA1, SHA256-384-512

            if (!validLengths.Contains(hashToCrack.Length))
                Exit("Invalid hash detected!\nPlease check if there are any typos or if your type of hash is supported!", 1);

            // Hash charset validation
            char[] validChars = "0123456789ABCDEF".ToCharArray();

            int wrong = hashToCrack.Where(a => !validChars.Contains(a)).Count();
            if (wrong > 0) 
                Exit("Invalid hash detected!\nYour hash contains non-hexadecimal values, please check for any typos!", 1);
        }

        static string hashToCrack;
        static byte[] hashToCrackBytes;
        static bool displayDone = false;
        static Stopwatch sw = new Stopwatch();
        static bool argI = false, argS = false;
        static void Main(string[] args)
        {
            string instruction = "Use --help or read README.md for more information on the syntax!";

            // Input parsing v2 // GOALS: [-i <hash>] [-m <maxl>] [-o <filename>] [-n <num of parts>] [-t <hash type> (if -i wasn't specified)] [--help]
            if (args.Length == 0)
                Exit($"No parameters were given.\n{instruction}", 1);

            var args2 = new List<string>(args);
            if (args2.Contains("--help"))
            {
                Console.WriteLine(help);
                Exit();
            }

            while (args2.Count > 0)
            {
                switch (args2[0])
                {
                    case "-i":
                        if (args2.Count() < 2) 
                            Exit($"There were no hash given after -i.\n{instruction}", 1);

                        hashToCrack = args2[1].ToUpper();
                        CheckHash();
                        hashToCrackBytes = Enumerable.Range(0, hashToCrack.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hashToCrack.Substring(x, 2), 16))
                                 .ToArray();

                        argI = true;
                        args2.RemoveRange(0, 2);
                        break;
                    case "-m":
                        if (!argI) 
                            Exit($"Incorrect usage of -m. The hash you want to crack must be given, before specifying the max length!\n{instruction}", 1);
                        if (args2.Count() < 2) 
                            Exit($"Incorrect usage of -m. An integer value, which must be at least 4, must be given after -m!\n{instruction}", 1);
                        if (!int.TryParse(args2[1], out int result))
                            Exit($"Incorrect value given for -m. The value must be an integer!\n{instruction}", 1);
                        ///{ For later use, when "unlimited" maxLength will be introduced.
                        ///    if (args2[1] == "-") 
                        ///        maxLength = Int32.MaxValue;
                        ///    else 
                        ///        Exit("Incorrect value given for -m. The value must be an integer or '-'!\nUse --help or read README.md for more information on the syntax!", 1);
                        ///}

                        if (result >= 4)
                        {
                            maxLength = result;

                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("The maximum length is set to: ");
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(result);
                        }
                        else Warn("The given max length for the guesses is smaller than the minimum (4) value therefore, the default (6) value is being used!");

                        args2.RemoveRange(0, 2);
                        break;
                    case "-o":
                        if (args2.Count() < 2) 
                            Warn("No filename was given so it will be save with the default name(s)!");
                        else
                        {
                            var prohibited = "#%&{}\\<>*?/ &!'\":@+`|=".ToCharArray();

                            var cleaned = new string(args2[1].Where(a => !prohibited.Contains(a)).ToArray());
                            if (cleaned == String.Empty)
                                Warn("No filename was given so the it will be save with the default name(s)!");
                            else
                            {
                                var split = cleaned.Split('.');

                                if (split.Length == 1)
                                    outputName = cleaned += ".txt";
                                else if (split[split.Length - 1] == String.Empty)
                                    outputName = cleaned += "txt";
                                else
                                    outputName = cleaned;
                            }
                        }
                        
                        argS = true;
                        args2.RemoveRange(0, 2);
                        break;
                    case "-n":
                        if (!argS)
                            Exit($"The -o option must be given before the -n option!\n{instruction}", 1);
                        if (args2.Count() < 2)
                        {
                            output = new List<string>[1];
                            output[0] = new List<string>() { "1/1 " };
                        }
                        else
                        {
                            if (!int.TryParse(args2[1], out int numOfOutputs))
                                Exit($"The given value for -n was not an integer!\n{instruction}", 1);

                            output = new List<string>[numOfOutputs];
                            output = Enumerable.Range(0, numOfOutputs)
                                               .Select(a => output[a] = new List<string>() { $"m={a} " })
                                               .ToArray();
                        }

                        args2.RemoveRange(0, 2);
                        break;
                    ///case "-t":
                    ///
                    ///    break;
                    default:
                        Exit($"Invalid arguments were given.\n{instruction}", 1);
                        break;
                }
            }
            wordCharVals = new List<int>[maxThreads].Select(b => b = new List<int>() { 0 }).ToArray();
            currentLengths = new int[maxThreads].Select(a => a = 1).ToArray();
            finishesArray = new bool[maxThreads];


            /// For more interaction
            ///do 
            ///{
            ///    Console.Write("Please enter your hash to crack: ");
            ///    hashToCrack = Console.ReadLine().Split(' ')[0];
            ///} while (hashToCrack == "");
            ///
            
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Available logical cores: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(maxThreads);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Starting allocation...");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("[" + new String(' ', 10) + "]  0%");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.CursorVisible = false;

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

            // Timer
            sw.Start();

            int i = -1;
            while (++i < maxThreads)
            {
                Console.SetCursorPosition(1, 3);

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
                    Console.CursorVisible = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Maximum number of threads have been succesfully allocated!");

                    displayDone = true;
                }
            }

            Console.ReadKey();
        }
    }
}
