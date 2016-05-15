using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UtmCliUtility
{
    public class CommandProcessorInfoAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
    }
}
