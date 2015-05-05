using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

static class Program
{
    static readonly string topWordsPath = "top-words.txt";
    static readonly string ruschemePath = "ruscheme.txt";
    static readonly string associationsPath = "associations.txt";

    static string booksDir;
    static string vocabularyPath;
    static Dictionary<string, int> vocabulary;
    static Encoding encoding = Encoding.GetEncoding("Windows-1251");
    static WordInfo[] topWords;
    static int[,] syllableFreq1;
    static int[, , ,] syllableFreq2;
    static int[,] rank;
    static int[] letterToSide;
    
    static void LoadConfig()
    {
        var config = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines("xblind.cfg"))
        {
            var tokens = line.Split(new[] { '=' }, 2);
            if (tokens.Length != 2)
                continue;
            var key = tokens[0].Trim();
            var value = tokens[1].Trim();
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            config.Add(key, value);
        }
        config.TryGetValue("books", out booksDir);
        config.TryGetValue("vocabulary", out vocabularyPath);
    }

    static void LoadVocabulary()
    {
        vocabulary = new Dictionary<string, int>();
        foreach (var line in File.ReadAllLines(vocabularyPath, encoding))
        {
            string word = line.Trim().ToLower();
            vocabulary[word] = 0;
        }
    }

    static void ProcessWord(Dictionary<string, int> dict, string word)
    {
        char prev = (char)0;
        int syllables = 0;
        foreach (char c in word)
        {
            if (!Russian.IsLower(c))
                return;
            char t = Russian.LetterType(c);
            if (prev == 'c' && t == 'v')
                ++syllables;
            prev = t != (char)0 ? t : prev;
        }
        if (syllables < 2)
            return;
        int count = 0;
        dict.TryGetValue(word, out count);
        dict[word] = count + 1;
    }

    static WordInfo[] ProcessAllBooks()
    {
        var dict = new Dictionary<string, int>();
        foreach (var path in Directory.EnumerateFiles(booksDir, "*.txt", SearchOption.AllDirectories))
        {
            using (var stream = new StreamReader(File.OpenRead(path), encoding))
            {
                StringBuilder word = new StringBuilder();
                for (string line; (line = stream.ReadLine()) != null; )
                {
                    foreach (char c in line)
                    {
                        if (c == '-')
                            continue;
                        if (char.IsLetter(c))
                        {
                            word.Append(char.ToLower(c));
                        }
                        else if (word.Length > 0)
                        {
                            ProcessWord(dict, word.ToString());
                            word.Clear();
                        }
                    }
                }
                if (word.Length > 0)
                    ProcessWord(dict, word.ToString());
            }
        }
        var words = new List<WordInfo>();
        var kicks = new List<string>();
        foreach (var kv in dict)
            if (kv.Value > 4)
                words.Add(new WordInfo { Word = kv.Key, Frequency = kv.Value });
        words.Sort();
        return words.ToArray();
    }

    static void ComputeTopWords()
    {
        if (!File.Exists(topWordsPath))
        {
            topWords = ProcessAllBooks();
            using (var writer = new StreamWriter(File.OpenWrite(topWordsPath)))
            {
                foreach (var word in topWords)
                    writer.WriteLine("{0} {1}", word.Word, word.Frequency);
            }
        }
        var words = new List<WordInfo>();
        using (var reader = new StreamReader(File.OpenRead(topWordsPath)))
        {
            for (string line; (line = reader.ReadLine()) != null; )
            {
                var tokens = line.Split(' ');
                words.Add(new WordInfo
                {
                    Word = tokens[0],
                    Frequency = int.Parse(tokens[1])
                });
            }
        }
        words.Sort();
        topWords = words.ToArray();
    }

    static string ExtractSyllables(string word)
    {
        var letters = new StringBuilder();
        char prevLetter = (char)0, prevType = (char)0;
        foreach (char letter in word)
        {
            var type = Russian.LetterType(letter);
            switch (type)
            {
                case 'c':
                    if (prevType != 'c')
                    {
                        prevLetter = letter;
                        prevType = 'c';
                    }
                    break;
                case 'v':
                    if (prevType == 'c')
                    {
                        letters.Append(prevLetter);
                        letters.Append(letter);
                        prevType = (char)0;
                    }
                    break;
            }
        }
        return letters.ToString();
    }

    static string ExtractMappedSyllables(string word)
    {
        var s = ExtractSyllables(word);
        var sb = new StringBuilder();
        for (int i = 0; i < s.Length; i += 2)
        {
            if (letterToSide[Russian.LetterIndex(s[i + 0])] < 0)
                continue;
            if (letterToSide[Russian.LetterIndex(s[i + 1])] < 0)
                continue;
            sb.Append(s[i + 0]);
            sb.Append(s[i + 1]);
        }
        return sb.ToString();
    }

    static void ComputeSyllableFreq2()
    {
        syllableFreq2 = new int[Russian.ConsonantCount, Russian.VowelCount, Russian.ConsonantCount, Russian.VowelCount];
        foreach (var node in topWords)
        {
            string key = ExtractSyllables(node.Word).Substring(0, 4);
            Debug.Assert(key.Length == 4);
            syllableFreq2[Russian.ConsonantIndex(key[0]),
                          Russian.VowelIndex(key[1]),
                          Russian.ConsonantIndex(key[2]),
                          Russian.VowelIndex(key[3])] += node.Frequency;
        }
    }

    static void ComputeSyllableFreq1()
    {
        syllableFreq1 = new int[Russian.ConsonantCount, Russian.VowelCount];
        for (int a = 0; a < Russian.ConsonantCount; ++a)
            for (int b = 0; b < Russian.VowelCount; ++b)
                for (int c = 0; c < Russian.ConsonantCount; ++c)
                    for (int d = 0; d < Russian.VowelCount; ++d)
                    {
                        syllableFreq1[a, b] += syllableFreq2[a, b, c, d];
                        syllableFreq1[c, d] += syllableFreq2[a, b, c, d];
                    }
    }

    static void ComputeSyllableFrequencies()
    {
        ComputeSyllableFreq2();
        ComputeSyllableFreq1();
    }

    static void RankSyllables()
    {
        rank = new int[Groups.Consonant.Length, Groups.Vowel.Length];
        for (int i = 0; i < Groups.Consonant.Length; ++i)
        {
            int p = Groups.Consonant[i];
            int[] costs = new int[Russian.VowelCount];
            for (int j = 0; j < Russian.ConsonantCount; ++j)
            {
                if ((p & 1 << j) != 0)
                {
                    for (int k = 0; k < Russian.VowelCount; ++k)
                        costs[k] += syllableFreq1[j, k];
                }
            }
            p <<= Russian.VowelCount;
            for (int j = 0; j < Groups.Vowel.Length; ++j)
            {
                int q = Groups.Vowel[j];
                int cost = 0;
                for (int k = 0; k < Russian.VowelCount; ++k)
                    if ((q & 1 << k) != 0)
                        cost += costs[k];
                rank[i, j] = cost;
            }
        }
    }

    static void ComputeRuscheme()
    {
        letterToSide = new int[Russian.LookupSize];
        for (int i = 0; i < Russian.LookupSize; ++i)
            letterToSide[i] = -1;
        try
        {
            if (File.Exists(ruschemePath))
            {
                var lines = File.ReadAllLines(ruschemePath);
                if (lines.Length != 6)
                    throw new Exception();
                for (int k = 0; k < 6; ++k)
                {
                    var parts = lines[k].Split(' ');
                    if (parts.Length != 3 || parts[0].Length != 1 || RubiksCube.SideId(parts[0][0]) != k)
                        throw new Exception();
                    for (int i = 1; i < parts.Length; ++i)
                        foreach (char c in parts[i])
                            letterToSide[Russian.LetterIndex(c)] = k;
                }
                return;
            }
        }
        catch { }
        for (int i = 0; i < Russian.LookupSize; ++i)
            letterToSide[i] = -1;
        var bruteForce = new BruteForce(rank);
        bruteForce.Run();
        using (var writer = new StreamWriter(File.OpenWrite(ruschemePath)))
        {
            for (int side = 0; side < 6; ++side)
            {
                writer.Write("{0} ", RubiksCube.Sides[side]);
                int cp = Groups.Consonant[bruteForce.Consonants[side]];
                int vp = Groups.Vowel[bruteForce.Vovels[side]];
                for (int i = 0; i < Russian.ConsonantCount; ++i)
                    if ((cp & 1 << i) != 0)
                    {
                        char c = Russian.Consonants[i];
                        letterToSide[Russian.LetterIndex(c)] = side;
                        writer.Write(c);
                    }
                writer.Write(' ');
                for (int i = 0; i < Russian.VowelCount; ++i)
                    if ((vp & 1 << i) != 0)
                    {
                        char c = Russian.Vowels[i];
                        letterToSide[Russian.LetterIndex(c)] = side;
                        writer.Write(c);
                    }
                writer.WriteLine();
            }
        }
    }

    static void PrintRusheme()
    {
        var cs = new StringBuilder[6];
        var vs = new StringBuilder[6];
        for (int i = 0; i < 6; ++i)
        {
            cs[i] = new StringBuilder();
            vs[i] = new StringBuilder();
        }
        for (int i = 0; i < letterToSide.Length; ++i)
            if (letterToSide[i] >= 0)
            {
                char c = Russian.Letter(i);
                var dst = Russian.IsVowel(c) ? vs : cs;
                dst[letterToSide[i]].Append(c);
            }
        for (int i = 0; i < 6; ++i)
            Console.WriteLine("{0} {1} {2}", RubiksCube.Sides[i], cs[i], vs[i]);
    }

    static void GenerateAssociations()
    {
        var groups = new List<string>[6, 6, 6, 6];
        var idx = new int[4];
        foreach (var node in topWords)
        {
            string key = ExtractMappedSyllables(node.Word);
            if (key.Length < 4)
                continue;
            for (int i = 0; i < 4; ++i)
                idx[i] = letterToSide[Russian.LetterIndex(key[i])];
            var group = groups[idx[0], idx[1], idx[2], idx[3]];
            if (group == null)
            {
                group = new List<string>();
                groups[idx[0], idx[1], idx[2], idx[3]] = group;
            }
            group.Add(key.Substring(0, 4) + ' ' + node.Word);
        }
        using (var writer = new StreamWriter(File.OpenWrite(associationsPath)))
        {
            for (int a = 0; a < 6; ++a)
                for (int b = 0; b < 6; ++b)
                    for (int c = 0; c < 6; ++c)
                        for (int d = 0; d < 6; ++d)
                        {
                            var group = groups[a, b, c, d];
                            string groupKey = string.Format("[{0}{1}{2}{3}]", RubiksCube.Sides[a], RubiksCube.Sides[b], RubiksCube.Sides[c], RubiksCube.Sides[d]);
                            if (group == null)
                            {
                                Console.WriteLine("Warning: Group {0} is empty.", groupKey);
                                continue;
                            }
                            writer.WriteLine(groupKey);
                            foreach (var word in group)
                            {
                                writer.WriteLine("{0}", word);
                            }
                            writer.WriteLine("---");
                        }
        }
    }

    static void Main(string[] args)
    {
        LoadConfig();
        ComputeTopWords();
        ComputeSyllableFrequencies();
        RankSyllables();
        ComputeRuscheme();
        PrintRusheme();
        GenerateAssociations();
    }
}
