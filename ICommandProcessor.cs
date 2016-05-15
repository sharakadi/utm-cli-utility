using System;
using System.IO;

namespace UtmCliUtility
{
    public interface ICommandProcessor
    {
        string[] Arguments { get; set; }
        DirectoryInfo WorkingDirectory { get; set; }
        TransportWrapper TransportWrapper { get; set; }
        Action<string> InfoWriter { get; set; } 
        ProcessingResult Process();
    }
}
