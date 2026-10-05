using System.Collections.Generic;

namespace ZKTris.Core
{
    // The upcoming pieces shown in the Next queue, fed by a bag.
    public class PieceQueue
    {
        readonly PieceBag bag;
        readonly List<PieceType> upcoming;

        public PieceQueue(PieceBag bag, int size)
        {
            this.bag = bag;
            upcoming = new List<PieceType>(size);
            for (int i = 0; i < size; i++)
                upcoming.Add(bag.Next());
        }

        public IReadOnlyList<PieceType> Upcoming => upcoming;

        public PieceType Take()
        {
            var piece = upcoming[0];
            upcoming.RemoveAt(0);
            upcoming.Add(bag.Next());
            return piece;
        }
    }
}
