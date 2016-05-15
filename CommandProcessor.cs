using System;
using System.IO;

namespace UtmCliUtility
{
    public abstract class CommandProcessor : ICommandProcessor
    {
        public abstract string Name { get; }

        public string[] Arguments
        {
            get;
            set;
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

        protected abstract string ProcessInternal();
    }
}
