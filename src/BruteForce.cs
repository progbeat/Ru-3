using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class BruteForce
{
    readonly int n, m;
    readonly int[,] rank;
    readonly bool[,] adj = GenerateAdj();
    readonly int[] adjMsk = new int[6];

    int[] consonants = new int[6];
    int[] vowels;
    List<int[]> vowelCandidGroups = new List<int[]>();

    int[] low = new int[24];
    int[] bestVowels;
    int[] bestConsonants = new int[6];

    public int[] Vovels { get { return bestVowels; } }
    public int[] Consonants { get { return bestVowels != null ? bestConsonants : null; } }

    static bool[,] GenerateAdj()
    {
        bool[,] adj = new bool[6, 6];
        foreach (var pair in RubiksCube.Pairs)
        {
            int i = RubiksCube.SideId(pair[0]);
            int j = RubiksCube.SideId(pair[1]);
            adj[i, j] = true;
        }
        return adj;
    }

    public BruteForce(int[,] rank)
    {
        this.rank = rank;
        n = rank.GetLength(0);
        m = rank.GetLength(1);
        //low[0] = 380000;
        for (int i = 0; i < 6; ++i)
            for (int j = 0; j < 6; ++j)
                if (adj[i, j])
                    adjMsk[i] |= 1 << j;
        Debug.Assert(n == Groups.Consonant.Length);
        Debug.Assert(m == Groups.Vowel.Length);
    }

    void VowelDfs(int[] v, int i, int mask)
    {
        switch (i)
        {
            case 2:
                if (v[1] < v[0])
                    return;
                break;
            case 3:
                if (v[2] < v[0] || v[2] < v[1])
                    return;
                break;
            case 4:
                if (v[3] < v[0] || v[3] < v[1])
                    return;
                break;
            case 5:
                if (v[4] < v[0] || v[4] < v[1] || v[4] < v[2])
                    return;
                break;
            case 6:
                if (mask != 0 || v[5] < v[0])
                    return;
                vowelCandidGroups.Add(v.Clone() as int[]);
                return;
        }
        for (int j = 0; j < m; ++j)
        {
            int q = Groups.Vowel[j];
            if ((q & ~mask) != 0)
                continue;
            v[i] = j;
            VowelDfs(v, i + 1, mask ^ q);
        }
    }

    static void Shuffle(List<int[]> list)
    {
        var rnd = new Random();
        for (int k = 0; k < 3; ++k)
        {
            for (int i = 1; i < list.Count; ++i)
            {
                int j = rnd.Next(i + 1);
                var t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
            for (int i = 0, j = list.Count; i < --j; ++i)
            {
                var t = list[i];
                list[i] = list[j];
                list[j] = t;
            }
        }
    }

    void GenerateVowelCombinations()
    {
        int[] v = new int[6];
        VowelDfs(v, 0, (1 << Russian.VowelCount) - 1);
    }

    struct ConsonantCandidate
    {
        public int Id { get; set; }
        public int Mask { get; set; }
        public int Popcount { get; set; }
        public int Score { get; set; }
    }

    List<ConsonantCandidate>[] consonantCandidates = new List<ConsonantCandidate>[6];
    int[][] dp = new int[1 << Russian.ConsonantCount][];
    int[] time = new int[1 << Russian.ConsonantCount];
    int now;

    static int Compare(int[] p, int[] q)
    {
        Debug.Assert(p.Length == q.Length);
        for (int i = 0; i < p.Length; ++i)
            if (p[i] != q[i])
                return p[i].CompareTo(q[i]);
        return 0;
    }

    static int[] infs = Infs();

    static int[] Infs()
    {
        var r = new int[24];
        for (int i = 0; i < 24; ++i)
            r[i] = int.MaxValue;
        return r;
    }

    static void PutBackAndSort(int[] arr, int value)
    {
        int j = arr.Length - 1;
        if (arr[j] <= value)
            return;
        for (; ; --j)
        {
            if (j <= 0 || arr[j - 1] <= value)
            {
                arr[j] = value;
                break;
            }
            arr[j] = arr[j - 1];
        }
    }

    static void Copy(int[] src, int[] dst)
    {
        Debug.Assert(src.Length == dst.Length);
        Array.Copy(src, dst, dst.Length);
    }

    int[] Dp(int i, int mask, int popcount)
    {
        Debug.Assert(Russian.ConsonantCount == Popcount(mask) + popcount);
        if (popcount - i * 3 > Russian.ConsonantCount - 6 * 3 ||
            popcount - i * 4 < Russian.ConsonantCount - 6 * 4)
            return new int[24];
        if (i == 6)
            return infs;
        if (time[mask] == now)
            return dp[mask];
        var res = dp[mask];
        if (res == null)
            res = new int[24];
        if (i == 5)
        {
            int id = Groups.ConsonantId(mask);
            Debug.Assert(id >= 0);
            Copy(Dp(6, 0, Russian.ConsonantCount), res);
            for (int j = 0; j < 6; ++j)
                if (adj[i, j])
                    PutBackAndSort(res, rank[id, vowels[j]]);
            if (Compare(res, low) < 0)
                Array.Clear(res, 0, 24);
        }
        else
        {
            Array.Clear(res, 0, 24);
            int[] t = new int[24];
            foreach (var c in consonantCandidates[i])
            {
                if (c.Score < res[0])
                    break;
                if ((~mask & c.Mask) != 0)
                    continue;
                Copy(Dp(i + 1, mask ^ c.Mask, popcount + c.Popcount), t);
                if (Compare(t, res) <= 0)
                    continue;
                for (int j = 0; j < 6; ++j)
                    if (adj[i, j])
                        PutBackAndSort(t, rank[c.Id, vowels[j]]);
                if (Compare(t, res) > 0)
                    Copy(t, res);
            }
        }
        dp[mask] = res;
        time[mask] = now;
        return res;
    }

    void Restore(int i, int mask)
    {
        if (i == 6)
            return;
        int popcount = Russian.ConsonantCount - Popcount(mask);
        var res = Dp(i, mask, popcount);
        int[] t = new int[24];
        foreach (var c in consonantCandidates[i])
        {
            if (c.Score < res[0] || (~mask & c.Mask) != 0)
                continue;
            Copy(Dp(i + 1, mask ^ c.Mask, popcount + c.Popcount), t);
            for (int j = 0; j < 6; ++j)
                if (adj[i, j])
                    PutBackAndSort(t, rank[c.Id, vowels[j]]);
            if (Compare(t, res) == 0)
            {
                bestConsonants[i] = c.Id;
                Restore(i + 1, mask ^ c.Mask);
                return;
            }
        }
        Debug.Assert(false);
    }

    static int Popcount(int mask)
    {
        mask -= ((mask >> 1) & 0x55555555);
        mask = (mask & 0x33333333) + ((mask >> 2) & 0x33333333);
        return (((mask + (mask >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }

    void Solve()
    {
        for (int i = 0; i < 6; ++i)
            consonantCandidates[i] = new List<ConsonantCandidate>();
        for (int k = 0; k < n; ++k)
        {
            int badMsk = 0;
            for (int j = 0; j < 6; ++j)
                if (rank[k, vowels[j]] < low[0])
                    badMsk |= 1 << j;
            for (int i = 0; i < 6; ++i)
            {
                if ((adjMsk[i] & badMsk) != 0)
                    continue;
                int score = int.MaxValue;
                for (int j = 0; j < 6; ++j)
                    if (adj[i, j])
                        score = Math.Min(score, rank[k, vowels[j]]);
                Debug.Assert(score >= low[0]);
                consonantCandidates[i].Add(new ConsonantCandidate
                {
                    Id = k,
                    Mask = Groups.Consonant[k],
                    Popcount = Popcount(Groups.Consonant[k]),
                    Score = score
                });
            }
        }
        for (int i = 0; i < 6; ++i)
        {
            consonantCandidates[i].Sort((p, q) => q.Score.CompareTo(p.Score));
        }
        ++now;
        var t = Dp(0, (1 << Russian.ConsonantCount) - 1, 0);
        if (Compare(t, low) > 0)
        {
            Copy(t, low);
            bestVowels = vowels;
            Restore(0, (1 << Russian.ConsonantCount) - 1);
        }
    }

    public void Run()
    {
        GenerateVowelCombinations();
        Shuffle(vowelCandidGroups);
        for (int k = 0; k < vowelCandidGroups.Count; ++k)
        {
            vowels = vowelCandidGroups[k];
            Solve();
            if (bestVowels != null)
            {
                Console.WriteLine();
                Console.Write("Vowels = {{");
                for (int i = 0; i < 6; ++i)
                {
                    if (i > 0)
                        Console.Write(' ');
                    Console.Write(vowels[i]);
                }
                Console.WriteLine("}");
                for (int i = 0; i < 6; ++i)
                {
                    Console.Write(RubiksCube.Sides[i]);
                    Console.Write(' ');
                    int q = Groups.Consonant[bestConsonants[i]];
                    for (int j = 0; j < Russian.ConsonantCount; ++j)
                    {
                        if ((q & 1 << j) != 0)
                            Console.Write(Russian.Consonants[j]);
                    }
                    Console.Write(' ');
                    q = Groups.Vowel[bestVowels[i]];
                    for (int j = 0; j < Russian.VowelCount; ++j)
                    {
                        if ((q & 1 << j) != 0)
                            Console.Write(Russian.Vowels[j]);
                    }
                    Console.WriteLine();
                }
                Console.Write("Low = {{", low);
                for (int i = 0; i < low.Length; ++i)
                {
                    if (i > 0)
                        Console.Write(' ');
                    Console.Write(low[i]);
                }
                Console.WriteLine("}");
            }
            Console.WriteLine("Progress = {0:0.000}%", (k + 1) * 1e2 / vowelCandidGroups.Count);
        }
    }
}