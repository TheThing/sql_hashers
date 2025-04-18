using System;
using System.Linq;

namespace SafeArgon2
{
    internal abstract class Argon2Core
    {
        public Argon2Core(int hashSize)
        {
            _tagLine = hashSize;
        }

        public int DegreeOfParallelism { get; set; }

        public int MemorySize { get; set; }

        public int Iterations { get; set; }

        public abstract int Type { get; }

        public byte[] AssociatedData { get; set; }

        public byte[] Salt { get; set; }

        public byte[] Secret { get; set; }

        internal byte[] Hash(byte[] password)
        {
            var lanes = InitializeLanes(password);

            var start = 2;

            for (int i = 0; i < Iterations; ++i)
            {
                for (int s = 0; s < 4; s++)
                {
                    for (int l = 0; l < lanes.Length; l++)
                    {
                        var lane = lanes[l];
                        var segmentLength = lane.BlockCount / 4;
                        var curOffset = s * segmentLength + start;

                        var prevLane = l;
                        var prevOffset = curOffset - 1;

                        if (curOffset == 0)
                        {
                            prevOffset = lane.BlockCount - 1;
                        }

                        var state = GenerateState(lanes, segmentLength, i, l, s);

                        for (var c = start; c < segmentLength; ++c, curOffset++)
                        {
                            var pseudoRand = state.PseudoRand(c, prevLane, prevOffset);

                            var refLane = (uint)(pseudoRand >> 32) % lanes.Length;

                            if (i == 0 && s == 0)
                            {
                                refLane = l;
                            }

                            var refIndex = IndexAlpha(l == refLane, (uint)pseudoRand, lane.BlockCount, segmentLength, i, s, c);

                            var refBlock = lanes[refLane][refIndex];

                            var curBlock = lane[curOffset];

                            Compress(curBlock, refBlock, lanes[prevLane][prevOffset]);

                            prevOffset = curOffset;
                        }
                    }

                    start = 0;
                }
            }

            return Finalize(lanes);
        }

        private static void XorLanes(Argon2Lane[] lanes)
        {
            var data = lanes[0][lanes[0].BlockCount - 1];

            for (int i = 1; i < lanes.Length; i++)
            {
                var lane = lanes[i];

                var block = lane[lane.BlockCount - 1];

                for (var b = 0; b < 128; ++b)
                {
                    if (!BitConverter.IsLittleEndian)
                    {
                        block.Array[block.Offset + b] =
                            (block.Array[block.Offset + b] >> 56) ^
                            ((block.Array[block.Offset + b] >> 40) & 0xff00UL) ^
                            ((block.Array[block.Offset + b] >> 24) & 0xff0000UL) ^
                            ((block.Array[block.Offset + b] >> 8) & 0xff000000UL) ^
                            ((block.Array[block.Offset + b] << 8) & 0xff00000000UL) ^
                            ((block.Array[block.Offset + b] << 24) & 0xff0000000000UL) ^
                            ((block.Array[block.Offset + b] << 40) & 0xff000000000000UL) ^
                            ((block.Array[block.Offset + b] << 56) & 0xff00000000000000UL);
                    }

                    data.Array[data.Offset + b] ^= block.Array[block.Offset + b];
                }
            }
        }

        private byte[] Finalize(Argon2Lane[] lanes)
        {
            XorLanes(lanes);

            var ds = new LittleEndianActiveStream();
            
            ds.Expose(lanes[0][lanes[0].BlockCount - 1]);

            ModifiedBLAKE2.Blake2Prime(lanes[0][1], ds, _tagLine);
            
            var result = new byte[_tagLine];

            ArraySegment<ulong> source = lanes[0][1];

            int maxUlongBytes = Math.Min(result.Length, source.Count * 8);

            // Convert each ulong into 8 bytes (little-endian).
            for (int i = 0; i < maxUlongBytes; i++)
            {
                int ulongIndex = i / sizeof(ulong);
                int byteOffset = i % sizeof(ulong);

                // Calculate shift value in bits: byteOffset * 8
                result[i] = (byte)(source.Array[source.Offset + ulongIndex] >> (byteOffset * 8));
            }

            return result;
        }

        internal static void Compress(ArraySegment<ulong> dest, ArraySegment<ulong> refb, ArraySegment<ulong> prev)
        {
            // TODO: Think if it's possible to improve performance here.
            var tmpblock = new ulong[dest.Count];

            for (var n = 0; n < 128; ++n)
            {
                tmpblock[n] = refb.Array[refb.Offset + n] ^ prev.Array[prev.Offset + n];

                dest.Array[dest.Offset + n] ^= tmpblock[n];
            }

            for (var i = 0; i < 8; ++i)
            {
                ModifiedBLAKE2.DoRoundColumns(new ArraySegment<ulong>(tmpblock), i);
            }

            for (var i = 0; i < 8; ++i)
            {
                ModifiedBLAKE2.DoRoundRows(new ArraySegment<ulong>(tmpblock), i);
            }

            for (var n = 0; n < 128; ++n)
            {
                dest.Array[dest.Offset + n] ^= tmpblock[n];
            }
        }

        internal abstract IArgon2PseudoRands GenerateState(Argon2Lane[] lanes, int segmentLength, int pass, int lane, int slice);

        // Single-threaded implementation on purpose.
        internal Argon2Lane[] InitializeLanes(byte[] password)
        {
            var blockHash = Initialize(password);

            var lanes = new Argon2Lane[DegreeOfParallelism];

            // Adjust memory size if needed so that each segment has an even size.
            var segmentLength = MemorySize / (lanes.Length * 4);

            MemorySize = segmentLength * 4 * lanes.Length;

            var blocksPerLane = MemorySize / lanes.Length;

            if (blocksPerLane < 4)
            {
                throw new InvalidOperationException($"Memory should be enough to provide at least 4 blocks per {nameof(DegreeOfParallelism)}.");
            }

            for (var i = 0; i < lanes.Length; ++i)
            {
                lanes[i] = new Argon2Lane(blocksPerLane);

                int iClosure = i;

                var stream = new LittleEndianActiveStream();

                stream.Expose(blockHash);
                stream.Expose(0);
                stream.Expose(iClosure);

                ModifiedBLAKE2.Blake2Prime(lanes[iClosure][0], stream);

                stream = new LittleEndianActiveStream();

                stream.Expose(blockHash);
                stream.Expose(1);
                stream.Expose(iClosure);

                ModifiedBLAKE2.Blake2Prime(lanes[iClosure][1], stream);
            }

            Array.Clear(blockHash, 0, blockHash.Length);

            return lanes;
        }

        internal byte[] Initialize(byte[] password)
        {
            // Initialize the lanes.
            var blake2 = new BLAKE2b(512);

            var dataStream = new LittleEndianActiveStream();

            dataStream.Expose(DegreeOfParallelism);
            dataStream.Expose(_tagLine);
            dataStream.Expose(MemorySize);
            dataStream.Expose(Iterations);
            dataStream.Expose((uint)0x13);
            dataStream.Expose((uint)Type);
            dataStream.Expose(password.Length);
            dataStream.Expose(password);
            dataStream.Expose(Salt?.Length ?? 0);
            dataStream.Expose(Salt);
            dataStream.Expose(Secret?.Length ?? 0);
            dataStream.Expose(Secret);
            dataStream.Expose(AssociatedData?.Length ?? 0);
            dataStream.Expose(AssociatedData);

            blake2.Initialize();

            byte[] blockhash = blake2.ComputeHash(dataStream);

            dataStream.ClearBuffer();

            return blockhash;
        }

        private static int IndexAlpha(bool sameLane, uint pseudoRand, int laneLength, int segmentLength, int pass, int slice, int index)
        {
            uint refAreaSize;

            if (pass == 0)
            {
                if (slice == 0)
                {
                    refAreaSize = (uint)index - 1;
                }
                else if (sameLane)
                {
                    refAreaSize = (uint)(slice * segmentLength) + (uint)index - 1;
                }
                else
                {
                    refAreaSize = (uint)(slice * segmentLength) - ((index == 0) ? 1U : 0);
                }
            }
            else if (sameLane)
            {
                refAreaSize = (uint)laneLength - (uint)segmentLength + (uint)index - 1;
            }
            else
            {
                refAreaSize = (uint)laneLength - (uint)segmentLength - ((index == 0) ? 1U : 0);
            }

            ulong relativePos = pseudoRand;

            relativePos = relativePos * relativePos >> 32;

            relativePos = refAreaSize - 1 - (refAreaSize * relativePos >> 32);

            uint startPos = 0;

            if (pass != 0)
            {
                startPos = (slice == 3) ? 0 : ((uint)slice + 1U) * (uint)segmentLength;
            }

            return (int)(((ulong)startPos + relativePos) % (ulong)laneLength);
        }

        private readonly int _tagLine;
    }
}
