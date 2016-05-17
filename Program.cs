using System;
using System.IO;
using System.Linq;

namespace UtmCliUtility
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            WriteProgramHeader();
            if (args.Length == 0)
            {
                WriteHelp();
                return;
            }
            var parameters = new ArgumentParser(args);
            var transportUri = parameters.GetValues("url", "u").FirstOrDefault();
            TransportWrapper transport = !string.IsNullOrEmpty(transportUri) ? new TransportWrapper(transportUri) : null;
            var commandName = args[0];
            var command = CommandProcessorFactory.Create(commandName, transport, Console.WriteLine, args);
            var result = command.Process();
            if (!result.Success)
            {
                Console.WriteLine("Произошла неустранимая ошибка при обработке команды {0}: {1}", commandName, result.Error);
                Console.Error.WriteLine("ERROR:" + result.Error);
                Console.Error.WriteLine("STACK TRACE:" + result.StackTrace);
                Console.ReadKey();
                return;
            }
            var output = parameters.GetValues("output", "o").ToArray();
            if (!output.Any()) return;
            switch (output[0])
            {
                case "console": Console.WriteLine(result.ResultTextData);
                    break;
                case "stdout": Console.Out.WriteLine(result.ResultTextData);
                    break;
                case "file":
                    if (output.Length > 1)
                    {
                        File.WriteAllText(output[1], result.ResultTextData);
                    }
                    break;
            }
        }

        private static void WriteProgramHeader()
        {
            Console.WriteLine("********************************************************");
            Console.WriteLine("* UtmCliUtility - утилита для извлечения данных из УТМ *");
            Console.WriteLine("********************************************************");
            Console.WriteLine("Copyright (c) 2016 messerspiel (messerspiel@hmamail.com)");
            Console.WriteLine();
        }

        private static void WriteHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Для использования утилиты необходимо указать желаемую команду. Она идет всегда\r\nпервой в аргументах командой строки. Затем следуют параметры - специфические\r\nдля данной команды, а также общие параметры.");
            Console.WriteLine();
            Console.WriteLine("КОМАНДЫ:");
            foreach (var c in CommandProcessorFactory.GetCommands())
            {
                Console.WriteLine("\t{0} - {1}", c.Name, c.Description);
                Console.WriteLine(c.Usage);
            }
            //Console.WriteLine("\tget-schemas - получение XSD из УТМ");
            //Console.WriteLine("\t\t-dir <Директория> - указывает директорию для сохранения схем");
            Console.WriteLine();
            Console.WriteLine("ОБЩИЕ ПАРАМЕТРЫ:");
            Console.WriteLine("\t-url (-u) <АдрессУТМ> - задает веб-адрес размещения УТМ и его порт");
            Console.WriteLine("\t-output (-o) [console]|[stdout]|[file <ИмяФайла>] - задает размещение\r\nвывода данных, куда исполняемая команда возращает перечень обработанных\r\nобъектов или иную информацию");
            Console.ReadKey();
        }
        //public static void Main(string[] args)
        //{
        //    string utmUrl = "http://192.168.1.245:8080";
        //    string outputDir = @".\UTM_XSD";

        //    // -------------------------------------
        //    var uri = new Uri(utmUrl);
        //    var dir = new DirectoryInfo(outputDir);
        //    var urlRegex = new Regex(@"(?<url>info\/(\w.?(\/)?)*\.xsd)");
        //    var versionRegex = new Regex(@"version:([0-9]{1,}\.[0-9]{1,}\.[0-9]{1,}){1}");
        //    if (!dir.Exists) dir.Create();

        //    var homePage = WebHelper.GetWebPageContent(uri);
        //    var matches = urlRegex.Matches(homePage);
        //    var version = versionRegex.Match(homePage).Groups[1].Captures[0].Value;
        //    List<Uri> uries = new List<Uri>();
        //    foreach (Match m in matches)
        //    {
        //        var g = m.Groups["url"];
        //        if (g.Captures.Count > 0)
        //        {
        //            var builder = new UriBuilder();
        //            builder.Host = uri.Host;
        //            builder.Port = uri.Port;
        //            builder.Path = g.Captures[0].Value;
        //            uries.Add(builder.Uri);
        //        }
        //    }

        //    foreach (Uri u in uries)
        //    {
        //        var content = WebHelper.GetWebPageContent(u);
        //        int start = content.IndexOf("<pre>");
        //        int end = content.LastIndexOf("</pre>");
        //        if (end <= start) continue;
        //        var xsd = content.Substring(start + 5, end - start - 5);
        //        var file = Path.Combine(dir.FullName, u.PathAndQuery.Split('/').Last());
        //        File.WriteAllText(file, xsd);
        //        Console.WriteLine("Сохранена XSD-схема: {0}", u.PathAndQuery);
        //    }

        //    var wbDocSchema = new FileInfo(Path.Combine(dir.FullName, "WB_DOC_SINGLE_01.xsd"));
        //    if (wbDocSchema.Exists)
        //    {
        //        var docContent = File.ReadAllText(wbDocSchema.FullName);
        //        var schemaFileRegex = new Regex(@"schemaLocation=""(?<xsd>\w{1,}.xsd)""");
        //        var xsdMatches = schemaFileRegex.Matches(docContent);
        //        if (xsdMatches.Count > 0)
        //        {
        //            var sb = new StringBuilder(500);
        //            sb.AppendLine("@echo off");
        //            sb.Append("xsd.exe /c");
        //            char liter = 'a';
        //            foreach (Match m in xsdMatches)
        //            {
        //                var g = m.Groups["xsd"];
        //                if (g.Captures.Count > 0)
        //                {
        //                    var schema = g.Captures[0].Value;
        //                    var newFile = Path.Combine(dir.FullName, liter + ".xsd");
        //                    if (File.Exists(newFile)) File.Delete(newFile);
        //                    File.Move(Path.Combine(dir.FullName, schema), newFile);
        //                    sb.AppendFormat(" {0}", liter + ".xsd");
        //                    liter++;
        //                }
        //            }
        //            File.Move(Path.Combine(dir.FullName, "ActInventoryABInfo.xsd"),
        //                Path.Combine(dir.FullName, liter + ".xsd"));
        //            sb.AppendFormat(" {0}", liter + ".xsd");
        //            //sb.AppendFormat(" ActInventoryABInfo.xsd"); // в оф. схеме ссылки на эту схему нет!
        //            sb.AppendFormat(" {0}", "WB_DOC_SINGLE_01.xsd");

        //            //var verTokens = version.Split('.');
        //            //var outputSchema = string.Format("WB_DOC_SINGLE_01_{0}_{1}_{2}.xsd",verTokens[0], verTokens[1], verTokens[2]);
        //            //sb.AppendFormat(" .\\{0}", outputSchema);
        //            File.WriteAllText(Path.Combine(dir.FullName, "csharp_classes.bat"), sb.ToString());
        //            //File.WriteAllText(Path.Combine(dir.FullName, outputSchema), @"<?xml version=""1.0"" encoding=""utf-8""?>");
        //        }
        //    }
        //}
    }
}
