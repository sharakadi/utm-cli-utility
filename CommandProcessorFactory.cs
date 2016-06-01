using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UtmCliUtility.Commands;

namespace UtmCliUtility
{
    public static class CommandProcessorFactory
    {
        private class CommandProcessorInfoItem
        {
            public string Description { get; set; }
            public string Usage { get; set; }
            public Type Type { get; set; }
        }

        static readonly IDictionary<string, CommandProcessorInfoItem> Commands;

        static CommandProcessorFactory()
        {
            Commands =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Where(x => x.GetCustomAttributes(typeof (CommandProcessorInfoAttribute), false).Any())
                    .Select(
                        x =>
                            new
                            {
                                Attr =
                                    (CommandProcessorInfoAttribute)
                                        x.GetCustomAttributes(typeof (CommandProcessorInfoAttribute), false).First(),
                                Type = x
                            })
                    .ToDictionary(k => k.Attr.Name,
                        v =>
                            new CommandProcessorInfoItem()
                            {
                                Description = v.Attr.Description,
                                Usage = v.Attr.Usage,
                                Type = v.Type
                            });
        }

        public static ICommandProcessor Create(string commandName, TransportWrapper transport, Action<string> infoWriter, string[] arguments)
        {
            var wd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!Commands.ContainsKey(commandName)) throw new Exception("Неизвестная команда: " + commandName);
            var inst = (ICommandProcessor) Activator.CreateInstance(Commands[commandName].Type);
            inst.Arguments = arguments;
            inst.InfoWriter = infoWriter;
            inst.TransportWrapper = transport;
            inst.WorkingDirectory = new DirectoryInfo(wd);
            return inst;
        }

        public static CommandListItem[] GetCommands()
        {
            return
                Commands.Select(
                    x => new CommandListItem {Description = x.Value.Description, Name = x.Key, Usage = x.Value.Usage})
                    .ToArray();
        }
    }
}
