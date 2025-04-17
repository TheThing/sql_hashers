using System;
using System.Text;
using Microsoft.SqlServer.Server;

namespace SafeArgon2
{
    public static class PasswordHasher
    {
        const int hashLengthMin = 4; // Bytes

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static string SayHello()
        {
            return "Hello!";
        }

        [SqlFunction(IsDeterministic = true, IsPrecise = true)]
        public static string Argon2idHash(
            string password,
            string salt,
            string secret,
            string associatedData,
            int parallelism,
            int iterations,
            int memorySize,
            int hashLength)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (string.IsNullOrWhiteSpace(salt))
            {
                throw new ArgumentNullException(nameof(salt));
            }

            if (hashLength < hashLengthMin)
            {
                throw new ArgumentOutOfRangeException(nameof(hashLength));
            }

            // Other parameter checks are performed within the algorithm implementation. 

            Argon2id algo = new Argon2id(Encoding.UTF8.GetBytes(password));

            algo.AssociatedData = string.IsNullOrWhiteSpace(associatedData) ? null : Encoding.UTF8.GetBytes(associatedData);
            algo.DegreeOfParallelism = parallelism;
            algo.Iterations = iterations;
            algo.KnownSecret = string.IsNullOrWhiteSpace(secret) ? null : Encoding.UTF8.GetBytes(secret);
            algo.MemorySize = memorySize;
            algo.Salt = Encoding.UTF8.GetBytes(salt);

            var hash = algo.GetBytes(hashLength);

            return BitConverter.ToString(hash).Replace("-","");
        }
    }
}
