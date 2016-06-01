using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(
        Name = "doc-version", 
        Description ="Управление версиями докуменоборота в ЕГАИС",
        Usage ="")]
    public class DocumentVersion :  CommandProcessor
    {
        public override string Name
        {
            get { return "doc-version"; }
        }

        protected override string ProcessInternal()
        {
            if (Parser.ParameterExists("set"))
            {
                var maxVer = Parser.GetValues("set").ToArray();
                if (!maxVer.Any())
                    throw new Exception("Следует указать максимальную поддерживаемую версию документооборота.");
                switch (maxVer.First())
                {
                    case "v1":
                        return ChangeDocVersion("WayBill");
                    case "v2":
                        return ChangeDocVersion("WayBill_v2");
                    default:
                        throw new Exception("Неизвестная версия документооборота - " + maxVer.First());
                }
            }
            throw new Exception("Параметры не распознаны.");
        }

        private string ChangeDocVersion(string version)
        {
            InfoWriteLineFormat("Переход на версию документооборота: {0}", version);
            throw new NotImplementedException(); // TODO: !!! нужны версии схем 1.0.13
        }
    }
}
