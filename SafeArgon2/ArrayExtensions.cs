using System;

namespace SafeArgon2
{
    public static class ArrayExtensions
    {
        // TODO: Check this method implementation. Compare with original version.
        public static void Blit(this ulong[] toBlit, byte[] bytes, int destOffset = 0)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            if (toBlit == null)
            {
                throw new ArgumentNullException(nameof(toBlit));
            }

            int fullUlongs = bytes.Length / 8;
            int remainder = bytes.Length % 8;

            if (fullUlongs > toBlit.Length - destOffset)
            {
                throw new ArgumentException("Cannot write more than remaining space");
            }

            // Convert full 8-byte chunks to ulong.
            for (int i = 0; i < fullUlongs; i++)
            {
                int byteIndex = i * 8;

                toBlit[destOffset + i] =
                    ((ulong)bytes[byteIndex]) |
                    ((ulong)bytes[byteIndex + 1] << 8) |
                    ((ulong)bytes[byteIndex + 2] << 16) |
                    ((ulong)bytes[byteIndex + 3] << 24) |
                    ((ulong)bytes[byteIndex + 4] << 32) |
                    ((ulong)bytes[byteIndex + 5] << 40) |
                    ((ulong)bytes[byteIndex + 6] << 48) |
                    ((ulong)bytes[byteIndex + 7] << 56);
            }

            // Handle remaining bytes (1–7).
            if (remainder > 0)
            {
                int byteIndex = fullUlongs * 8;

                ulong extra = 0;

                for (int i = 0; i < remainder; i++)
                {
                    extra |= ((ulong)bytes[byteIndex + i]) << (8 * i);
                }

                if (destOffset + fullUlongs >= toBlit.Length)
                {
                    throw new ArgumentException("Not enough space for remainder");
                }

                toBlit[destOffset + fullUlongs] = extra;
            }
        }
    }
}
