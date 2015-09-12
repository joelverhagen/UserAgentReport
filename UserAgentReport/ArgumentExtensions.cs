using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Knapcode.UserAgentReport
{
    public static class ArgumentExtensions
    {
        private const string OptionPrefix = "-";
        private static readonly Regex AllOptionPrefixes = new Regex(@"^\s*[\-\/]+");

        public static bool HasOption(this string[] args, string option)
        {
            return args.GetOption(option, string.Empty, null) == string.Empty;
        }

        public static string GetOption(this string[] args, string option, string defaultValue)
        {
            return args.GetOption(option, defaultValue, defaultValue);
        }

        public static string GetOption(this string[] args, string option, string defaultValue, string valueIfNotFound)
        {
            var match = args
                .SkipWhile(a => !AllOptionPrefixes.Replace(a, OptionPrefix).Equals(OptionPrefix + option, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (!match.Any())
            {
                return valueIfNotFound;
            }

            return match
                .Skip(1)
                .TakeWhile(a => !AllOptionPrefixes.IsMatch(a))
                .DefaultIfEmpty(defaultValue)
                .First();
        }
    }
}