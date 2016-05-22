using System;
using System.Collections.Generic;
using System.Linq;

namespace UtmCliUtility
{
    public sealed class ArgumentParser
    {
        private readonly string[] _arguments;
        public char ParameterPrefixChar { get; private set; }

        public ArgumentParser(string[] arguments, char parameterPreficChar)
        {
            _arguments = arguments;
            ParameterPrefixChar = parameterPreficChar;
        }

        public ArgumentParser(string[] arguments) : this(arguments, '-') { }

        public bool ParameterExists(params string[] parameterNameAliases)
        {
            return parameterNameAliases.Any(x => _arguments.Contains("-" + x));
        }

        public IEnumerable<string> GetValues(params string[] parameterNameAliases)
        {
            if (!ParameterExists(parameterNameAliases)) throw new Exception();
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
