using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct WordInfo : IComparable<WordInfo>
{
    public string Word { get; set; }
    public int Frequency { get; set; }

    public int CompareTo(WordInfo other)
    {
        int x = other.Frequency.CompareTo(Frequency);
        return x != 0 ? x : Word.CompareTo(other.Word);
    }
}
