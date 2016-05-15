using System;
using System.Linq;

namespace UtmCliUtility.Commands
{
    [CommandProcessorInfo(
        Name = "generate-csharp-class", 
        Description = "Генерирует класс на языке C#, являющийся моделью XSD",
        Usage="")]
    public class GenerateCSharpClass : CommandProcessor
    {
        public override string Name
        {
            get { return "generate-csharp-class"; }
        }

        protected override string ProcessInternal()
        {
            var xsdTool = Parser.GetValues("-xsd").FirstOrDefault();
            if (xsdTool == null)
            {
                
            }
            throw new NotImplementedException();
        }
    }
}
