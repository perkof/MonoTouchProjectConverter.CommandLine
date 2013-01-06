using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonoTouchProjectConverter.CommandLine
{
    class Program
    {
        private static void Main(string[] args)
        {
            var arguments = GetArguements();
            if (!arguments.Any())
            {
                DisplayHelp();
                throw new InvalidOperationException("Required arguments were missing.");
            }

            ConvertProject(arguments["SolutionPath"]);
        }

        private static void ConvertProject(string path)
        {
            var files = FindProjectFiles(path);

            foreach(var file in files)
            {
                ConvertMonoTouchFileToVisualStudio(file);
            }
        }

        private static void ConvertMonoTouchFileToVisualStudio(string file)
        {
            string content = File.ReadAllText(file);
            content = content.Replace("<ProjectTypeGuids>{6BC8ED88-2882-458C-8E55-DFD12B67127B};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>", "<ProjectGuids />");

            Regex regex = new Regex("<Page Include=\"(.*).xib\" />");
            content = regex.Replace(content, "<None Include=\"$1.xib\" />");

            // VS2010 only
            content = content.Replace("<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>", "<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");

            File.WriteAllText(file, content);

            Console.WriteLine("Converted {0} to MonoTouch file.", file);
        }

        private static IEnumerable<string> FindProjectFiles(string path)
        {
            return Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories).ToList();
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Usage: MonoTouchProjectConverter.CommandLine.exe SolutionPath=C:\\pathToSolution");
        }

        private static Dictionary<string, string> GetArguements()
        {
            return Environment.GetCommandLineArgs().Where(item => item.Split('=')[0] == "SolutionPath").Select(item => item.Split('=')).ToDictionary(parts => parts[0], parts => parts[1]);
        }
    }
}
