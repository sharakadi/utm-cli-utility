using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(Name = "get-schemas",
        Description = "Получение XSD из УТМ",
        Usage = "\t\t-dir <Директория> - указывает директорию для сохранения схем")
    ]
    public class GetSchemas : CommandProcessor
    {
        //public GetSchemas(TransportWrapper transport, string[] argments, string workingDirectory, Action<string> infoWriter)
        //{
        //    TransportWrapper = transport;
        //    Arguments = argments;
        //    WorkingDirectory = new DirectoryInfo(workingDirectory);
        //    InfoWriter = infoWriter;
        //}

        public GetSchemas()
        {
            
        }

        private string GetHomePageContent()
        {
            return TransportWrapper.GetPageContent(TransportWrapper.Address);
        }

        public override string Name { get { return "get-schemas"; } }

        protected override string ProcessInternal()
        {
            InfoWriteLineFormat("Чтение URL схем с домашней страницы УТМ...");
            var outputDir = Parser.ParameterExists("dir") ? Parser.GetValues("dir").FirstOrDefault() : null; 
            IEnumerable<Uri> schemas = TransportWrapper.ParseXsdUrls(GetHomePageContent());
            var files =
                schemas.Select(
                    x => new
                    {
                        Directory = outputDir ?? WorkingDirectory.FullName,
                        Filename = x.PathAndQuery.Split('/').Last(),
                        Content = TransportWrapper.GetPageContent(x)
                    }).ToArray();

            InfoWriteLineFormat("OK");

            var sb = new StringBuilder(30*files.Length);
            foreach (var file in files)
            {
                if (!Directory.Exists(file.Directory)) Directory.CreateDirectory(file.Directory);
                var fullpath = Path.Combine(file.Directory, file.Filename);
                File.WriteAllText(fullpath, file.Content);
                sb.AppendLine(fullpath);
                InfoWriteLineFormat("Сохранена схема: " + fullpath);
            }
            return sb.ToString();
        }
    }
}
