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

            // TODO: Check this.
            var ulongRaw = new ulong[384];

            //// TODO: Try replace Array.Copy with ArraySegment here and futher.
            var inputBlock = new ulong[128];
            var addressBlock = new ulong[128];
            var tmpBlock = new ulong[128];

            Array.Copy(ulongRaw, 0, inputBlock, 0, 128);
            Array.Copy(ulongRaw, 128, addressBlock, 0, 128);
            Array.Copy(ulongRaw, 256, tmpBlock, 0, 128);
            ////

            inputBlock[0] = (ulong)pass;
            inputBlock[1] = (ulong)lane;
            inputBlock[2] = (ulong)slice;
            inputBlock[3] = (ulong)MemorySize;
            inputBlock[4] = (ulong)Iterations;
            inputBlock[5] = (ulong)Type;

            for (var i = 0; i < segmentLength; i++)
            {
                var ival = i % 128;

                if (ival == 0)
                {
                    inputBlock[6]++;

                    //// TODO: Ensure memore block zeroing is correct here.
                    //tmpBlock.Fill(0);
                    //addressBlock.Fill(0);

                    Array.Clear(tmpBlock, 0, tmpBlock.Length);
                    Array.Clear(addressBlock, 0, addressBlock.Length);
                    ////

                    Compress(tmpBlock, inputBlock, _zeroBlock);
                    Compress(addressBlock, tmpBlock, _zeroBlock);
                }

                rands[i] = addressBlock[ival];
            }

            return new PseudoRands(rands);
        }
    }
}
