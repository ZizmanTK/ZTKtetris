using System.Collections.Generic;

namespace ZKTris.Core
{
    // 7-bag randomizer: every run of seven pieces contains each piece once,
    // so there are no long droughts (or floods) of a single shape.
    public class PieceBag
    {
        readonly System.Random random;
        readonly List<PieceType> bag = new List<PieceType>(7);

        public PieceBag(int? seed = null)
        {
            random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        public PieceType Next()
        {
            if (bag.Count == 0) Refill();
            var piece = bag[bag.Count - 1];
            bag.RemoveAt(bag.Count - 1);
            return piece;
        }

        void Refill()
        {
            for (int i = 0; i < 7; i++)
                bag.Add((PieceType)i);
            for (int i = bag.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (bag[i], bag[j]) = (bag[j], bag[i]);
            }
        }
    }
}
