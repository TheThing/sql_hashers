using System;

namespace SafeArgon2
{
    public class Argon2Lane
    {
        public Argon2Lane(int blockCount)
        {
            _memory = new ulong[128 * blockCount];

            BlockCount = blockCount;
        }

        public ArraySegment<ulong> this[int index]
        {
            get
            {
                if (index < 0 || index >= BlockCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return new ArraySegment<ulong>(_memory, index * 128, 128);
            }
        }

        public int BlockCount { get; }

        private readonly ulong[] _memory;
    }
}
