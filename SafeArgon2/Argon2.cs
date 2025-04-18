using System;

namespace SafeArgon2
{
    public abstract class Argon2
    {
        private byte[] _password;

        public Argon2(byte[] password)
        {
            if (password == null || password.Length == 0)
            {
                throw new ArgumentException("Argon2 needs a password set.", nameof(password));
            }

            _password = password;
        }

        public byte[] GetBytes(int bc)
        {
            ValidateParameters(bc);

            return GetBytesImpl(bc);
        }

        public byte[] Salt { get; set; }

        public byte[] KnownSecret { get; set; }

        public byte[] AssociatedData { get; set; }

        public int Iterations { get; set; }

        public int MemorySize { get; set; }

        public int DegreeOfParallelism { get; set; }

        internal abstract Argon2Core BuildCore(int bc);

        private void ValidateParameters(int bc)
        {
            if (bc > 1024)
            {
                throw new NotSupportedException("Current implementation of Argon2 only supports generating up to 1024 bytes.");
            }

            if (Iterations < 1)
            {
                throw new InvalidOperationException("Cannot perform an Argon2 Hash with out at least 1 iteration.");
            }

            if (MemorySize < 4)
            {
                throw new InvalidOperationException("Argon2 requires a minimum of 4kB of memory (MemorySize >= 4).");
            }

            if (DegreeOfParallelism < 1)
            {
                throw new InvalidOperationException("Argon2 requires at least 1 thread (DegreeOfParallelism).");
            }
        }

        private byte[] GetBytesImpl(int bc)
        {
            var n = BuildCore(bc);

            n.Salt = Salt;
            n.Secret = KnownSecret;
            n.AssociatedData = AssociatedData;
            n.Iterations = Iterations;
            n.MemorySize = MemorySize;
            n.DegreeOfParallelism = DegreeOfParallelism;

            return n.Hash(_password);
        }
    }
}
