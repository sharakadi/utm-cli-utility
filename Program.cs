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
            Console.WriteLine();
            Console.WriteLine("ОБЩИЕ ПАРАМЕТРЫ:");
            Console.WriteLine("\t-url (-u) <АдрессУТМ> - задает веб-адрес размещения УТМ и его порт");
            Console.WriteLine("\t-output (-o) [console]|[stdout]|[file <ИмяФайла>] - задает размещение\r\nвывода данных, куда исполняемая команда возращает перечень обработанных\r\nобъектов или иную информацию");
            Console.ReadKey();
        }
    }
}
