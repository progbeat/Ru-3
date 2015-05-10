using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

static class Config
{
    public static string WorkingDir { get; private set; }

    public static double T0 { get; private set; }

    public static double T1 { get; private set; }

    public static TimeSpan Duration { get; private set; }

    static Config()
    {
        var config = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines("Ru^3.cfg"))
        {
            var tokens = line.Split(new[] { '=' }, 2);
            if (tokens.Length != 2)
                continue;
            var key = tokens[0].Trim();
            var value = tokens[1].Trim();
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            var property = typeof(Config).GetProperty(key);
            if (property != null)
            {
                var parse = property.PropertyType.GetMethod("Parse", new[] { typeof(string) });
                var converted = parse != null ? parse.Invoke(null, new[] { value }) : Convert.ChangeType(value, property.PropertyType);
                property.SetValue(null, converted);
            }
        }
    }
}
