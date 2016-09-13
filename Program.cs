using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UtmCliUtility
{
    internal class Program
    {
        private const string DEFAULT_UTM_URL = "http://127.0.0.1:8080";

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
            TransportWrapper transport = !string.IsNullOrEmpty(transportUri)
                ? new TransportWrapper(transportUri)
                : new TransportWrapper(DEFAULT_UTM_URL);
            Console.WriteLine("Адрес УТМ: {0}", transport.Address);
            var commandName = args[0];
            var command = CommandProcessorFactory.Create(commandName, transport, Console.WriteLine, args);
            var result = command.Process();
            if (!result.Success)
            {
                Console.WriteLine("Произошла неустранимая ошибка при обработке команды {0}: {1}", commandName, result.Error);
                Console.Error.WriteLine(">>>ERROR>>>");
                Console.Error.WriteLine("ERROR:" + result.Error);
                Console.Error.WriteLine("STACK TRACE:" + result.StackTrace);
                Console.Error.WriteLine("<<<ERROR<<<");
                //Console.ReadKey();
                return;
            }

            string[] output = parameters.ParameterExists("output", "o")
                ? parameters.GetValues("output", "o").ToArray()
                : new[] {"console"};
            switch (output[0])
            {
                case "stdout": Console.Out.WriteLine(GetHashedOutput(result.ResultTextData));
                    break;
                case "file":
                    var bytes = Encoding.Default.GetBytes(result.ResultTextData);
                    var filename = output.Length > 1
                        ? output[1]
                        : ".\\" + GetHash(bytes) + ".out";
                    File.WriteAllText(filename, result.ResultTextData);
                    Console.WriteLine("Вывод записан в файл: {0} ({1} байт)", filename, bytes.Length);
                    break;
                case "console":
                default: Console.WriteLine(result.ResultTextData);
                    break;
            }
        }

        private static string GetHash(byte[] input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(input);
            return string.Join(string.Empty, hash.Select(x => x.ToString("x2")).ToArray());
        }

        private static string GetHashedOutput(string input)
        {
            var hashString = GetHash(Encoding.Default.GetBytes(input));
            var sb = new StringBuilder(input.Length + 100);
            sb.AppendLine(">>>OUT:[" + hashString + "]>>>");
            sb.AppendLine(input);
            sb.AppendLine("<<<OUT:[" + hashString + "]<<<");
            return sb.ToString();
        }

        private static void WriteProgramHeader()
        {
            Console.WriteLine("********************************************************");
            Console.WriteLine("* UtmCliUtility - утилита для извлечения данных из УТМ *");
            Console.WriteLine("********************************************************");
            Console.WriteLine("Copyright (c) 2016 Sharkadi Andrey (sharkadi.a@gmail.com)");
            Console.WriteLine();
        }

        private static void WriteHelp()
        {
            Console.WriteLine("Для использования утилиты необходимо указать желаемую команду. Она идет всегда\r\nпервой в аргументах командой строки. Затем следуют параметры - специфические\r\nдля данной команды, а также общие параметры.");
            Console.WriteLine();
            Console.WriteLine("КОМАНДЫ:");
            foreach (var c in CommandProcessorFactory.GetCommands())
            {
                Console.WriteLine("\t{0} - {1}", c.Name, c.Description);
                Console.WriteLine(c.Usage);
                Console.WriteLine();
            }
            Console.WriteLine("ОБЩИЕ ПАРАМЕТРЫ:");
            Console.WriteLine("\t-url (-u) <АдрессУТМ> - задает веб-адрес размещения УТМ и его порт");
            Console.WriteLine("\t-output (-o) [console]|[stdout]|[file <ИмяФайла>] - задает размещение\r\nвывода данных, куда исполняемая команда возращает перечень обработанных\r\nобъектов или иную информацию");
            //Console.ReadKey();
        }
    }
}
