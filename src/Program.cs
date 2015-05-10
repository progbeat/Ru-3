using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Globalization;

static class Program
{
    static void Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Directory.SetCurrentDirectory(Config.WorkingDir);
        for (int i = 0; i < Data.Vocabulary.Length && i < 10; ++i) {
            var entry = Data.Vocabulary[i];
            Console.WriteLine("{0} {1}", entry.Word, entry.Frequency);
        }
    }
}
