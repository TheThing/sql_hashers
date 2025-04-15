using System;

namespace SafeArgon2
{
    public class Argon2iCore : Argon2Core
    {
        private readonly static ulong[] _zeroBlock = new ulong[128];

        internal class PseudoRands : IArgon2PseudoRands
        {
            private ulong[] _rands;

            public PseudoRands(ulong[] rands)
            {
                _rands = rands;
            }

            public ulong PseudoRand(int segment, int prevLane, int prevOffset)
            {
                return _rands[segment];
            }
        }

        public Argon2iCore(int hashSize) : base(hashSize) {}

        public override int Type
        {
            get
            {
                return 1;
            }
        }

        internal override IArgon2PseudoRands GenerateState(Argon2Lane[] lanes, int segmentLength, int pass, int lane, int slice)
        {
            var rands = new ulong[segmentLength];

            var ulongRaw = new ulong[384];

            var inputBlock = new ArraySegment<ulong>(ulongRaw, 0, 128);
            var addressBlock = new ArraySegment<ulong>(ulongRaw, 128, 128);
            var tmpBlock = new ArraySegment<ulong>(ulongRaw, 256, 128);

            inputBlock.Array[inputBlock.Offset + 0] = (ulong)pass;
            inputBlock.Array[inputBlock.Offset + 1] = (ulong)lane;
            inputBlock.Array[inputBlock.Offset + 2] = (ulong)slice;
            inputBlock.Array[inputBlock.Offset + 3] = (ulong)MemorySize;
            inputBlock.Array[inputBlock.Offset + 4] = (ulong)Iterations;
            inputBlock.Array[inputBlock.Offset + 5] = (ulong)Type;

            for (var i = 0; i < segmentLength; i++)
            {
                var ival = i % 128;

                if (ival == 0)
                {
                    inputBlock.Array[inputBlock.Offset + 6]++;

                    Array.Clear(tmpBlock.Array, tmpBlock.Offset, tmpBlock.Count);
                    Array.Clear(addressBlock.Array, addressBlock.Offset, addressBlock.Count);

                    Compress(tmpBlock, inputBlock, new ArraySegment<ulong>(_zeroBlock, 0, 128));
                    Compress(addressBlock, tmpBlock, new ArraySegment<ulong>(_zeroBlock, 0, 128));
                }

                rands[i] = addressBlock.Array[addressBlock.Offset + ival];
            }

            return new PseudoRands(rands);
        }
    }
}
