using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static class RubiksCube
{
    //static readonly string[] pairs = {
    //    "UL", "UF", "UR", "UB",
    //    "LU", "LF", "LB", "LD",
    //    "FU", "FL", "FR", "FD",
    //    "RU", "RF", "RB", "RD",
    //    "BU", "BL", "BR", "BD",
    //    "DL", "DF", "DR", "DB"
    //};

    static readonly string[] pairs = {
        "UL", "LU", "UR", "RU", "UF",
        "FL", "LF", "FR", "RF", "FD",
        "DL", "LD", "DR", "RD", "DB",
        "BL", "LB", "BR", "RB", "BU",
        "UB", "BD", "DF", "FU"
    };

    const string cycle = "ULURUFLFRFDLDRDBLBRBUBDFU";

    public static string Sides { get { return "ULFRBD"; } }

    public static string[] Pairs { get { return pairs; } }

    public static string Cycle { get { return cycle; } }

    public static int SideId(char c)
    {
        switch (c)
        {
            case 'U': return 0;
            case 'L': return 1;
            case 'F': return 2;
            case 'R': return 3;
            case 'B': return 4;
            case 'D': return 5;
        }
        return -1;
    }
}