using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(
        Name="send-act",
        Description="Отправляет акт для ТТН",
        Usage="")]
    public class SendAct : CommandProcessor
    {
        public override string Name
        {
            get { return "send-act"; }
        }

        protected override string ProcessInternal()
        {
            return null;
        }
    }
}
