using System;

namespace SafeArgon2
{
    public static class ArrayExtensions
    {
        // TODO: Check this method implementation. Compare with original version.
        public static void Blit(this ArraySegment<ulong> toBlit, ArraySegment<byte> bytes, int destOffset = 0)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (toBlit == null)
            {
                throw new ArgumentNullException(nameof(toBlit));
            }

            int fullUlongs = bytes.Count / sizeof(ulong);

            int remainder = bytes.Count % sizeof(ulong);

            if (fullUlongs > toBlit.Count - destOffset)
            {
                throw new ArgumentException("Cannot write more than remaining space");
            }

            // Convert full 8-byte chunks to ulong.
            for (int i = 0; i < fullUlongs; i++)
            {
                int byteIndex = i * 8;

                toBlit.Array[toBlit.Offset + destOffset + i] =
                    ((ulong)bytes.Array[bytes.Offset + byteIndex]) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 1] << 8) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 2] << 16) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 3] << 24) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 4] << 32) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 5] << 40) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 6] << 48) |
                    ((ulong)bytes.Array[bytes.Offset + byteIndex + 7] << 56);
            }

            // Handle remaining bytes (1–7).
            if (remainder > 0)
            {
                int byteIndex = fullUlongs * 8;

                ulong extra = 0;

                for (int i = 0; i < remainder; i++)
                {
                    extra |= ((ulong)bytes.Array[bytes.Offset + byteIndex + i]) << (8 * i);
                }

                if (destOffset + fullUlongs >= toBlit.Count)
                {
                    throw new ArgumentException("Not enough space for remainder");
                }

                toBlit.Array[toBlit.Offset + destOffset + fullUlongs] = extra;
            }
        }
    }
}
