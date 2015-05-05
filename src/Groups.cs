using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Groups
{
    const int consonantMask = (1 << Russian.ConsonantCount) - 1;
    const int vowelMask = (1 << Russian.VowelCount) - 1;

    static readonly int[] consonantId = new int[1 << Russian.ConsonantCount];
    static readonly int[] vowelId = new int[1 << Russian.VowelCount];
    static readonly int[] consonantGroups;
    static readonly int[] vowelGroups;

    static void generateIds(List<int> groups, int bits, int lo, int hi, int p = 0)
    {
        if (hi < 0)
            return;
        if (bits == 0)
        {
            if (lo <= 0)
                groups.Add(p);
            return;
        }
        --bits;
        generateIds(groups, bits, lo, hi, p);
        generateIds(groups, bits, lo - 1, hi - 1, p | 1 << bits);
    }

    static Groups()
    {
        for (int i = 0; i < consonantId.Length; ++i)
            consonantId[i] = -1;
        for (int i = 0; i < vowelId.Length; ++i)
            vowelId[i] = -1;
        var groups = new List<int>();
        generateIds(groups, Russian.VowelCount, 1, 2);
        vowelGroups = groups.ToArray();
        for (int i = 0; i < vowelGroups.Length; ++i)
            vowelId[vowelGroups[i]] = i;
        groups.Clear();
        generateIds(groups, Russian.ConsonantCount, 3, 4);
        consonantGroups = groups.ToArray();
        for (int i = 0; i < consonantGroups.Length; ++i)
            consonantId[consonantGroups[i]] = i;
    }

    public static int ConsonantId(int p) { return consonantId[p]; }

    public static int VowelId(int p) { return vowelId[p]; }

    public static int[] Consonant { get { return consonantGroups; } }

    public static int[] Vowel { get { return vowelGroups; } }

    public static int SyllableCount { get { return Consonant.Length * Vowel.Length; } }

    public static IEnumerable<int> Syllable
    {
        get
        {
            for (int i = 0; i < consonantGroups.Length; ++i)
            {
                int p = consonantGroups[i] << Russian.VowelCount;
                for (int j = 0; j < vowelGroups.Length; ++j)
                    yield return p | vowelGroups[j];
            }
        }
    }

    public static int SyllableGroupId(int group)
    {
        return consonantId[group >> Russian.VowelCount] * vowelGroups.Length + vowelId[group & vowelMask];
    }
}