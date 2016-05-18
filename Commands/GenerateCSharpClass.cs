using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(
        Name = "generate-csharp-class", 
        Description = "Генерирует класс на языке C# из файлов XSD",
        Usage="\t   -xsd <ПутьКXsd.exe> - путь к утилите xsd.exe\r\n" +
        "\t   -f (-files) [<Файл1.xsd>,<Файл2.xsd>,...] - перечень XSD файлов\r\n" +
        "\t   -l (-lists) [<Файл1>,<Файл2>...] - файлы со списком файлов XSD\r\n" +
        "\t   -d (-dir) [<Папка1>,<Папка2>,...] - перечень папок с файлами XSD\r\n" +
        "\t   -x (-exclude) [<Файл1>,<Файл2>] - перечень исключаемых файлов\r\n" +
        "\t   На выходе - содержимое CS файла (следует указать назначение вывода)")]
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
                InfoWriteLineFormat("Получение списка файлов...");
                if (Parser.ParameterExists("f", "files"))
                {
                    files.AddRange(Parser.GetValues("f", "files"));
                }
                if (Parser.ParameterExists("l", "lists"))
                {
                    foreach (var file in Parser.GetValues("l", "lists"))
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
                        InfoWriteLineFormat("Исключен файл: {0}", files[delete]);
                        files.RemoveAt(delete);
                    }
                }

                ValidateFiles(files);
                InfoWriteLineFormat("Всего файлов найдено: {0}", files.Count);
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

                var xsdOut = RunXsdTool(xsdToolPath, tempDir.FullName, newFiles.ToArray());
                InfoWriteLineFormat(">>>XSD.EXE>>>");
                InfoWriteLineFormat(xsdOut);
                InfoWriteLineFormat("<<<XSD.EXE<<<");
                var outputFileRegex = new Regex(@"'([\w\\\-_:]{1,}?([\w._!$&]{1,}.cs))'");
                if (!outputFileRegex.IsMatch(xsdOut)) throw  new Exception();
                var outputCsFile = outputFileRegex.Match(xsdOut).Groups[1].Captures[0].Value;
                Console.WriteLine("Файл CS успешно сформирован и направлен в вывод.");
                return File.ReadAllText(outputCsFile);
                //throw new Exception();
            }
        }

        private string RunXsdTool(string toolExePath, string outputDir, string[] filesFullNames)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = toolExePath;
            startInfo.Arguments = "/c /o:\"" + outputDir + "\" " + string.Join(" ", filesFullNames.Select(x => "\"" + x + "\""));
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return process.StandardOutput.ReadToEnd();
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