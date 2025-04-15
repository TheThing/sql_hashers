using System;
using System.IO;

namespace SafeArgon2
{
    public class BLAKE2b
    {
        public BLAKE2b(int hashSize)
        {
            if ((hashSize % 8) > 0)
            {
                throw new ArgumentException("Hash Size must be byte aligned", nameof(hashSize));
            }

            if (hashSize < 8 || hashSize > 512)
            {
                throw new ArgumentException("Hash Size must be between 8 and 512", nameof(hashSize));
            }

            _hashSize = hashSize;

            _createImpl = CreateImplementation;

            Key = Array.Empty<byte>();
        }

        public BLAKE2b(byte[] keyData, int hashSize) : this(hashSize)
        {
            if (keyData == null)
            {
                keyData = Array.Empty<byte>();
            }

            if (keyData.Length > 64)
            {
                throw new ArgumentException("Key needs to be between 0 and 64 bytes", nameof(keyData));
            }

            Key = keyData;
        }

        internal BLAKE2b(byte[] keyData, int hashSize, Func<BLAKE2bBase> baseCreator) : this(keyData, hashSize)
        {
            _createImpl = baseCreator;
        }

        public int HashSize
        {
            get
            {
                return (int)_hashSize;
            }
        }

        public byte[] Key { get; set; }

        public byte[] ComputeHash(byte[] data)
        {
            Initialize();

            HashCore(data, 0, data.Length);
            
            return HashFinal();
        }

        public byte[] ComputeHash(Stream inputStream)
        {
            // TODO: Check this implementation. It may be wrong.
            Initialize();

            byte[] buffer = new byte[4096]; // 4 KB buffer

            int bytesRead;

            while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                HashCore(buffer, 0, bytesRead);
            }

            return HashFinal();
        }

        public void Initialize()
        {
            _implementation = _createImpl();

            _implementation.Initialize(Key);
        }

        protected void HashCore(byte[] data, int offset, int size)
        {
            if (_implementation == null)
            {
                Initialize();
            }

            _implementation.Update(data, offset, size);
        }

        protected byte[] HashFinal()
        {
            return _implementation.Final();
        }

        private BLAKE2bBase CreateImplementation()
        {
            return new BLAKE2bNormal(_hashSize / 8);
        }

        BLAKE2bBase _implementation;

        private readonly int _hashSize;

        private readonly Func<BLAKE2bBase> _createImpl;
    }
}
