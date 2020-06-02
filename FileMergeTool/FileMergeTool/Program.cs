using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FileMergeTool
{
    class Program
    {
        static string SourcePath;
        static string DestinationFile;

        private static object _eventLock = new object();
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine($"USAGE: {args[0]} sourceFolder destinationFile");
                return -1;
            }

            SourcePath = args[0];

            if (!Directory.Exists(SourcePath))
            {
                Console.Error.WriteLine($"Directory {SourcePath} doesn't exist!");
                return -1;
            }

            DestinationFile = args[1];
                       
            GenerateOutput();
            MonitorDirectory(SourcePath);
            Console.WriteLine($"Monitoring {SourcePath}. Hit enter to exit...");
            Console.ReadLine();
            return 0;
        }

        private static void GenerateOutput()
        {
            List<string> usings = new List<string>();
            StringBuilder resultBuilder = new StringBuilder();

            lock (_eventLock)
            {
                foreach (var file in Directory.GetFiles(SourcePath, "*.cs", SearchOption.AllDirectories))
                {
                    using (TextReader reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Trim().StartsWith("using") && line.Trim().EndsWith(";") )
                            {
                                if(!usings.Contains(line.Trim()))
                                    usings.Add(line.Trim());
                                continue;
                            }
                            if (line.Trim().StartsWith("[assembly:"))
                                continue;

                            resultBuilder.AppendLine(line);
                        }
                    }
                }
                using (TextWriter tw = new StreamWriter(DestinationFile, false))
                {
                    StringBuilder usingsBuilder = new StringBuilder();
                    foreach (var use in usings)
                        usingsBuilder.AppendLine(use);

                    tw.Write(usingsBuilder.ToString());
                    tw.Write(resultBuilder.ToString());
                }
                Console.Write($"{DateTime.Now.ToShortTimeString()} wrote {DestinationFile}");
            } // end lock
        }

        private static void MonitorDirectory(string path)
        {

            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = path;

            fileSystemWatcher.Filter = "*.cs";

            fileSystemWatcher.Created += FileSystemWatcher_Created;

            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;

            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcher.Changed += FileSystemWatcher_Changed;

            fileSystemWatcher.EnableRaisingEvents = true;

        }

        private static void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(".cs"))
                GenerateOutput();
        }

        private static void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name.EndsWith(".cs"))
                GenerateOutput();
        }

        private static void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (e.Name.EndsWith(".cs"))
                GenerateOutput();
        }

        private static void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if(e.Name.EndsWith(".cs"))
                GenerateOutput();
        }
    }
}
