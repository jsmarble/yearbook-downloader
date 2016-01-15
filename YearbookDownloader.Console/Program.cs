using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace YearbookDownloader
{
    class Program
    {
        private static void Main(string[] args)
        {
            ArgumentParser argp = new ArgumentParser();
            var a = argp.Parse(args);
            string username = a.GetValue("u", "user", "username");
            string password = a.GetValue("p", "pwd", "pass", "password");
            string school = a.GetValue("s", "school");
            string year = a.GetValue("y", "year");

            if (string.IsNullOrWhiteSpace(username))
                username = Program.Prompt("username");
            if (string.IsNullOrWhiteSpace(username))
                return;

            if (string.IsNullOrWhiteSpace(password))
                password = Program.Prompt("password");
            if (string.IsNullOrWhiteSpace(password))
                return;

            int schoolNumber = 0;
            if (string.IsNullOrWhiteSpace(school))
                school = Program.Prompt("school");
            if (string.IsNullOrWhiteSpace(school) || !int.TryParse(school, out schoolNumber))
                return;

            int yearNumber = 0;
            if (string.IsNullOrWhiteSpace(year))
                year = Program.Prompt("year");
            if (string.IsNullOrWhiteSpace(year) || !int.TryParse(year, out yearNumber))
                return;

            try
            {
                Console.Clear();
                Console.WriteLine("Downloading...");
                YearbookClient yb = new YearbookClient(username, password);
                yb.DownloadRoot = Environment.CurrentDirectory;
                int downloadCount = 0;
                yb.ImageDownloaded += (sender, e) =>
                {
                    downloadCount = e.ImageNumber;
                    Console.Clear();
                    Console.WriteLine($"Downloaded {downloadCount} images...");
                };
                string dir = yb.DownloadYearbook(schoolNumber, yearNumber).Result;
                Console.Clear();
                Console.WriteLine($"Complete. Downloaded {downloadCount} images to '{dir}'.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press [Enter] to exit.");
                Console.ReadLine();
            }
        }

        static string Prompt(string description)
        {
            Console.Write($"Please enter {description}: ");
            string value = Console.ReadLine();
            return value;
        }
    }
}
