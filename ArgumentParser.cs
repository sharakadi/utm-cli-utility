using System;
using System.Collections.Generic;
using System.Linq;

namespace UtmCliUtility
{
    public sealed class ArgumentParser
    {
        private readonly string[] _arguments;

        public ArgumentParser(string[] arguments)
        {
            _arguments = arguments;
        }

        public bool ParameterExists(string name)
        {
            return _arguments.Any(x => x == "-" + name);
        }

        public IEnumerable<string> GetValues(params string[] parameterNameAliases)
        {
            if (!parameterNameAliases.Select(ParameterExists).Any()) throw new Exception();
            //var idx = _arguments.FirstIndexOf(x => x == "-" + parameterName);
            var idx = int.MinValue;
            foreach (var p in parameterNameAliases)
            {
                if (_arguments.FirstIndexOf(x => x == "-" + p, out idx)) break;
            }
            if (idx == int.MinValue) throw new Exception();
            for (int i = idx+1; i < _arguments.Length; i++)
            {
                if (_arguments[i].StartsWith("-")) yield break;
                yield return _arguments[i];
            }
        }
    }
}
