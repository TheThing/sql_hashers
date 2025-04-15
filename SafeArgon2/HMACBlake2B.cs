﻿using System;

namespace SafeArgon2
{
    public class HMACBlake2B
    {
        /// <summary>
        /// Construct an HMACBlake2B without a key.
        /// </summary>
        /// <param name="hashSize">the hash size in bits</param>
        public HMACBlake2B(int hashSize)
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

        /// <summary>
        /// Construct an HMACBlake2B
        /// </summary>
        /// <param name="keyData">The key for the HMAC</param>
        /// <param name="hashSize">The hash size in bits</param>
        public HMACBlake2B(byte[] keyData, int hashSize) : this(hashSize)
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

        internal HMACBlake2B(byte[] keyData, int hashSize, Func<BLAKE2bBase> baseCreator) : this(keyData, hashSize)
        {
            _createImpl = baseCreator;
        }

        /// <summary>
        /// Implementation of HashSize <seealso cref="System.Security.Cryptography.HashAlgorithm"/>
        /// </summary>
        /// <returns>The hash</returns>
        public int HashSize
        {
            get
            {
                return (int)_hashSize;
            }
        }

        public byte[] Key { get; set; }

        /// <summary>
        /// Implementation of Initialize - initializes the HMAC buffer
        /// </summary>
        public void Initialize()
        {
            _implementation = _createImpl();

            _implementation.Initialize(Key);
        }

        /// <summary>
        /// Implementation of HashCore
        /// </summary>
        /// <param name="data">The data to hash</param>
        /// <param name="offset">The offset to start hashing from</param>
        /// <param name="size">The amount of data in the hash to consume</param>
        protected void HashCore(byte[] data, int offset, int size)
        {
            if (_implementation == null)
            {
                Initialize();
            }

            _implementation.Update(data, offset, size);
        }

        /// <summary>
        /// Finish hashing and return the final hash
        /// </summary>
        /// <returns>The final hash from HashCore</returns>
        protected byte[] HashFinal()
        {
            return _implementation.Final();
        }

        private BLAKE2bBase CreateImplementation()
        {
            /*if (Vector.IsHardwareAccelerated)
            {
                return new Blake2bSimd(_hashSize / 8);
            }*/

            return new BLAKE2bNormal(_hashSize / 8);
        }

        BLAKE2bBase _implementation;

        private readonly int _hashSize;

        private readonly Func<BLAKE2bBase> _createImpl;
    }
}
