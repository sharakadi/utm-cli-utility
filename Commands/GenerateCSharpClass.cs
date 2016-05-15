using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(
        Name = "generate-csharp-class", 
        Description = "Генерирует класс на языке C# из файлов XSD",
        Usage="\t\t-xsd <ПутьКXsd.exe> - путь к утилите xsd.exe\r\n" +
        "\t\t-l (-list) [<Файл1.xsd>,<Файл2.xsd>,...] - перечень XSD файлов\r\n" +
        "\t\t-f (-file) <ФайлСоСписком> - файл со списком файлов XSD")]
    public class GenerateCSharpClass : CommandProcessor
    {
        public override string Name
        {
            get { return "generate-csharp-class"; }
        }

        protected override string ProcessInternal()
        {
            var xsdToolPath = Parser.GetValues("-xsd").FirstOrDefault();
            if (xsdToolPath == null)
            {
                xsdToolPath = SearchForXsdToolAndGetPath();
                if (xsdToolPath == null) throw new Exception("Утилита xsd.exe не найдена в системе.");
            }
            else
            {
                xsdToolPath = Path.Combine(xsdToolPath, "xsd.exe");
                if (!File.Exists(xsdToolPath)) throw new Exception("Утилита xsd.exe не найдена по указанному пути.");
            }

            using (var tempDir = CreateTemporaryDirectory())
            {
                List<string> files = new List<string>();
                if (Parser.ParameterExists("l", "list"))
                {
                    files.AddRange(Parser.GetValues("l", "list"));
                }
                if (Parser.ParameterExists("f", "file"))
                {
                    foreach (var file in Parser.GetValues("f", "file"))
                    {
                        if (!File.Exists(file)) continue;
                        files.AddRange(File.ReadAllLines(file));
                    }
                }
            }

            throw new Exception();
        }

        private string SearchForXsdToolAndGetPath()
        {
            var locations = new string[]
            {"%programfiles%\\Microsoft SDKs\\Windows", "%programfiles(x86)%\\Microsoft SDKs\\Windows"};

            foreach (var loc in locations.Select(Environment.ExpandEnvironmentVariables))
            {
                if (!Directory.Exists(loc)) continue;
                var dirs = Directory.GetDirectories(loc).ToDictionary(k => k.Split('\\').Last(), v => v);
                foreach (var path in new SortedList(dirs).GetValueList().Cast<string>().Reverse())
                {
                    var xsdTool = SearchInDirectory(path, "xsd.exe", false);
                    if (xsdTool != null) return xsdTool;
                }
            }

            return null;
        }
    }
}
