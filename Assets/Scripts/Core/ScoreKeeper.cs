using UnityEngine;

namespace ZKTris.Core
{
    public class ScoreKeeper
    {
        static readonly int[] LinePoints = { 0, 100, 300, 500, 800 };

        public const int LinesPerLevel = 10;

        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Holes { get; private set; }
        public int Level => 1 + Lines / LinesPerLevel;

        public void Reset()
        {
            Score = 0;
            Lines = 0;
            Holes = 0;
        }

        // Returns the points awarded. Scored at the level the lines were
        // cleared on, before any level up they cause.
        public int AddLines(int count)
        {
            if (count <= 0) return 0;
            int points = LinePoints[Mathf.Min(count, 4)] * Level;
            Score += points;
            Lines += count;
            return points;
        }

        public void AddSoftDrop(int cells) => Score += cells;
        public void AddHardDrop(int cells) => Score += 2 * cells;
        public void AddHoles(int count) => Holes += count;

        // Seconds per row of gravity. Level 1 matches the original 0.4s and
        // it gets 15% faster each level, never below 0.03s.
        public static float FallInterval(int level) =>
            Mathf.Max(0.03f, 0.4f * Mathf.Pow(0.85f, level - 1));
    }
}
