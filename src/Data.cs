using System.Collections.Generic;
using System.IO;

static class Data
{
    const string VOCABULARY_PATH = "list.txt";

    public static VocabularyEntry[] Vocabulary { get; private set; }

    static void LoadVocabulary()
    {
        var vocabulary = new List<VocabularyEntry>();
        using (var reader = new StreamReader(File.OpenRead(VOCABULARY_PATH)))
        {
            for (string line; (line = reader.ReadLine()) != null; )
            {
                var tokens = line.Split(' ');
                vocabulary.Add(new VocabularyEntry
                {
                    Word = tokens[0],
                    Frequency = int.Parse(tokens[1])
                });
            }
        }
        vocabulary.Sort();
        Vocabulary = vocabulary.ToArray();
    }

    static Data()
    {
        LoadVocabulary();
    }
}
