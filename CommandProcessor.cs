﻿using System;
using System.IO;
using System.Linq;

namespace UtmCliUtility
{
    public abstract class CommandProcessor : ICommandProcessor
    {
        protected class TemporaryDirectory : IDisposable
        {
            private static readonly Random Randomizer = new Random();

            public string FullName { get; private set; }

            internal TemporaryDirectory(string workingDirectory)
            {
                var temp = Path.Combine(workingDirectory, "tmp_" + DateTime.Now.Ticks.ToString().Substring(8) + Randomizer.Next(int.MaxValue));
                var path = new DirectoryInfo(temp);
                if (path.Exists) path.Delete();
                path.Create();
                FullName = path.FullName;
            }

            public void Dispose()
            {
                if (Directory.Exists(FullName)) Directory.Delete(FullName, true);
            }
        }

        public abstract string Name { get; }

        public string[] Arguments
        {
            get;
            set;
        }

        protected TemporaryDirectory CreateTemporaryDirectory()
        {
            return new TemporaryDirectory(WorkingDirectory.FullName);
        }

        protected ArgumentParser Parser
        {
            get
            {
                return Arguments != null ? new ArgumentParser(Arguments) : null;
            }
        }

        public DirectoryInfo WorkingDirectory { get; set; }

        public TransportWrapper TransportWrapper
        {
            get;
            set;
        }

        protected void InfoWriteLineFormat(string format, params object[] args)
        {
            if (InfoWriter != null) InfoWriter(string.Format(format, args));
        }

        protected void InfoWriteLineFormat()
        {
            if (InfoWriter != null) InfoWriter(Environment.NewLine);
        }

        public Action<string> InfoWriter { get; set; }

        public ProcessingResult Process()
        {
            Console.WriteLine("Выполнение команды {0}", Name);
            try
            {
                var result = ProcessInternal();
                return new ProcessingResult()
                {
                    Success = true,
                    ResultTextData = result
                };
            }
            catch (Exception ex)
            {
                return new ProcessingResult()
                {
                    Success = false,
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                };
            }
        }

        protected string SearchInDirectory(string startFromDirectory, string searchFor, bool isDirectory)
        {
            var dirs = Directory.GetDirectories(startFromDirectory);
            var files = Directory.GetFiles(startFromDirectory);
            if ((isDirectory && dirs.Select(Path.GetDirectoryName).Contains(searchFor)) ||
                (!isDirectory && files.Select(Path.GetFileName).Contains(searchFor)))
                return Path.Combine(startFromDirectory, searchFor);
            foreach (var dir in dirs)
            {
                var result = SearchInDirectory(dir, searchFor, isDirectory);
                if (result != null) return result;
            }
            return null;
        }

        protected string GetFullPath(string relativePath)
        {
            if (relativePath.StartsWith(".") || relativePath.StartsWith("\\"))
            {
                return Path.GetFullPath(relativePath);
            }
            return relativePath;
        }

        protected abstract string ProcessInternal();
    }
}
