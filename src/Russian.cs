using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class Russian
{
    public const int ConsonantCount = 20;
    public const int VowelCount = 10;
    public const int LookupSize = 40;

    const char russianA = 'а';
    const string consonants = "бвгджзклмнпрстфхцчшщ";
    const string vowels = "аеёиоуэыюя";

    static char[] lookup = new char[LookupSize];
    static int[] consonantIndex = new int[LookupSize];
    static int[] vowelIndex = new int[LookupSize];

    static Russian()
    {
        Debug.Assert(consonants.Length == ConsonantCount);
        Debug.Assert(vowels.Length == VowelCount);
        for (int i = 0; i < LookupSize; ++i)
        {
            consonantIndex[i] = -1;
            vowelIndex[i] = -1;
        }
        int idx = 0;
        foreach (char c in consonants)
        {
            lookup[LetterIndex(c)] = 'c';
            consonantIndex[LetterIndex(c)] = idx++;
        }
        idx = 0;
        foreach (char c in vowels)
        {
            lookup[LetterIndex(c)] = 'v';
            vowelIndex[LetterIndex(c)] = idx++;
        }
    }

    public static bool IsLower(char c)
    {
        return 'а' <= c && c <= 'ё';
    }

    public static bool IsConsonant(char c)
    {
        return IsLower(c) && lookup[LetterIndex(c)] == 'c';
    }

    public static bool IsVowel(char c)
    {
        return IsLower(c) && lookup[LetterIndex(c)] == 'v';
    }

    public static char LetterType(char c)
    {
        return IsLower(c) ? lookup[LetterIndex(c)] : (char)0;
    }

    public static int LetterIndex(char c)
    {
        return (int)(c - russianA);
    }

    public static char Letter(int index)
    {
        return (char)(russianA + index);
    }

    public static int ConsonantIndex(char c)
    {
        return IsLower(c) ? consonantIndex[LetterIndex(c)] : -1;
    }

    public static int VowelIndex(char c)
    {
        return IsLower(c) ? vowelIndex[LetterIndex(c)] : -1;
    }

    public static string Consonants { get { return consonants; } }

    public static string Vowels { get { return vowels; } }
}
