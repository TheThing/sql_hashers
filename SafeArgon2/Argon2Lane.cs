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

        public ulong[] this[int index]
        {
            get
            {
                if (index < 0 || index > BlockCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                // TODO: Possibly optimization required.
                // Try use ArraySegment here.
                ulong[] slice = new ulong[128];

                Array.Copy(_memory, 128*index, slice, 0, 128);

                return slice;
            }
        }

        public int BlockCount { get; }

        private readonly ulong[] _memory;
    }
}
