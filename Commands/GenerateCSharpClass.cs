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
        "\t\t-f (-file) <ФайлСоСписком> - файл со списком файлов XSD")
    ]
    public class GenerateCSharpClass : CommandProcessor
    {
        public override string Name
        {
            get { return "generate-csharp-class"; }
        }

        protected override string ProcessInternal()
        {
            var xsdToolPath = Parser.ParameterExists("-xsd") ? Parser.GetValues("-xsd").First() : null;
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
                if (Parser.ParameterExists("d", "dir"))
                {
                    foreach (var dir in Parser.GetValues("d", "dir"))
                    {
                        var dirFiles = Directory.GetFiles(dir, "*.xsd");
                        files.AddRange(dirFiles);
                    }
                }
                if (Parser.ParameterExists("x", "exclude"))
                {
                    foreach (
                        var delete in
                            Parser.GetValues("x", "exclude")
                                .Select(x => files.Select(Path.GetFileName).ToList().IndexOf(x))
                                .Where(x => x > -1))
                    {
                        files.RemoveAt(delete);
                    }
                }

                ValidateFiles(files);
                char newNameCursor = '!';
                List<string> newFiles = new List<string>(files.Count);
                foreach (var file in files)
                {
                    while (Path.GetInvalidFileNameChars().Contains(newNameCursor)) newNameCursor++;
                    var newFile = Path.Combine(tempDir.FullName, newNameCursor + ".xsd");
                    File.Copy(file, newFile);
                    newFiles.Add(newFile);
                    newNameCursor++;
                }

                RunXsdTool(xsdToolPath, files.ToArray());

                throw new Exception();
            }
            return null;
        }

        private void RunXsdTool(string toolExePath, string[] filesFullNames)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = toolExePath;
            startInfo.Arguments = "/c " + string.Join(" ", filesFullNames.Select(x => "\"" + x + "\""));
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            Console.WriteLine();
            Console.WriteLine("Вывод xsd.exe:");
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }

        private void ValidateFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    InfoWriteLineFormat("Файл не найден: ", file);
                    throw new Exception("Файл не найден: " + file);
                }
            }
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