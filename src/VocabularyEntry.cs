using System;

public struct VocabularyEntry : IComparable<VocabularyEntry>
{
    public string Word { get; set; }
    public int Frequency { get; set; }

    public int CompareTo(VocabularyEntry other)
    {
        int x = other.Frequency.CompareTo(Frequency);
        return x != 0 ? x : Word.CompareTo(other.Word);
    }
}
