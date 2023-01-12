using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Net;
using System.IO.Compression;

namespace OsuBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select which action you wish to perform. (1 or 2). \r\n[1] Back up existing files \r\n[2] Recover from existing backup");
            int action;
            bool isAction = Int32.TryParse(Console.ReadLine(), out action);

            if (!isAction || action > 2 || action < 1)
            {
                Console.WriteLine("Please select which action you wish to perform.");
            }

            if (action == 1)
            {
                Console.WriteLine("Please input your songs directory.");
                string? directory = Console.ReadLine();

                if (Directory.Exists(directory))
                    createBackupFile(directory);

                else
                    Console.WriteLine("Please input a valid songs directory.");
            }

            if (action == 2)
            {
                if (Directory.GetFiles("./backups/").Count() > 0)
                    createSongsFolder();

                else
                    Console.WriteLine("No backups found.");
            }
        }

        static void createBackupFile(string directory)
        {
            DirectoryInfo[] directories = new DirectoryInfo(directory).GetDirectories();
            List<string> IDs = new List<string>();

            foreach (DirectoryInfo i in directories)
            {
                string fileName = i.Name;
                int ID;

                if (fileName.IndexOf(' ') >= 0)
                {
                    string start = fileName.Split(' ')[0];
                    bool isValidID = Int32.TryParse(start, out ID);

                    if(isValidID)
                        IDs.Add(ID.ToString());
                }
            }

            if (IDs.Count >= 0)
            {
                int number = 1;
                while (File.Exists($"./backups/backup{number}.txt"))
                    number++;

                TextWriter tw = new StreamWriter($"./backups/backup{number}.txt");

                foreach (string i in IDs)
                    tw.WriteLine(i);

                tw.Close();
            }

            else
                Console.WriteLine("No songs detected!");
        }

        static void createSongsFolder()
        {
            if (Directory.Exists("./Songs/"))
                Directory.Delete("./Songs/", true);

            Directory.CreateDirectory("./Songs/");
            
            int number = 1;
                while (File.Exists($"./backups/backup{number + 1}.txt"))
                    number++;

            string[] IDs = File.ReadAllLines($"./backups/backup{number}.txt");
            List<string> failedIDs = new List<string>();

            if (IDs.Count() == 0)
                Console.WriteLine("No IDs saved!");

            else
            {
                WebClient webClient = new WebClient();

                foreach (string i in IDs)
                {
                    double currIndex = Array.IndexOf(IDs, i);
                    double indexLength = IDs.Count();
                    double percentage = Math.Round(currIndex / indexLength * 100, 1);

                    Console.WriteLine($"{percentage}%");

                    try
                    {
                        webClient.DownloadFile($"https://beatconnect.io/b/{i}/", $"./Songs/{i}.osz");
                        ZipFile.ExtractToDirectory($"./Songs/{i}.osz", $"./Songs/{i}");
                        File.Delete($"./Songs/{i}.osz");
                    }
                    catch(System.Net.WebException)
                    {
                        Console.WriteLine($"Beatmap ID {i} not found.");
                        failedIDs.Add(i);
                    }       

                    Thread.Sleep(500);
                }

                Console.WriteLine("All done!");

                if (failedIDs.Count > 0)
                {
                    Console.WriteLine($"{failedIDs.Count()} beatmaps not found.");

                    foreach (string i in failedIDs)
                        Console.Write($"{i} ");
                }
            }
        }
    }
}
