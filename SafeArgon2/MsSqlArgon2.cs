using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.SqlServer.Server;

public static class MsSqlArgon2
{
    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static void Argon2id_hash(string password, out string output)
    {
        var settings = Argon2Common.GetSettings();
        var raw_salt = Argon2Common.GenerateSalt(18);
        var raw_hash = perform_hash(settings, raw_salt, password);

        output = Argon2Common.FormatHash(settings, raw_salt, raw_hash);
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static void Argon2id_hash_custom(string password, byte parallel, short memory, byte iterations, byte bc, out string output)
    {
        var settings = new Argon2Common.settings { parallel = parallel,  memory = memory, iterations = iterations, bc = bc };
        var raw_salt = Argon2Common.GenerateSalt(18);
        var raw_hash = perform_hash(settings, raw_salt, password);

        output = Argon2Common.FormatHash(settings, raw_salt, raw_hash);
    }

    [SqlFunction(IsDeterministic = true, IsPrecise = true)]
    public static int Argon2id_verify(string password, string hash)
    {
        var group = Argon2Common.ParseFormattedHash(hash);
        var raw_salt = Convert.FromBase64String(group.salt);
        var raw_original_hash = Convert.FromBase64String(group.hash);
        var raw_password_hash = perform_hash(group.settings, raw_salt, password);

        return Argon2Common.CompareRawHash(raw_password_hash, raw_original_hash) ? 1 : 0;
    }

    private static byte[] perform_hash(Argon2Common.settings settings, byte[] raw_salt, string password)
    {
        var algo = new SafeArgon2.Argon2id(Encoding.UTF8.GetBytes(password));

        algo.AssociatedData = null;
        algo.DegreeOfParallelism = settings.parallel;
        algo.Iterations = settings.iterations;
        algo.KnownSecret = null;
        algo.MemorySize = 1024 * settings.memory;
        algo.Salt = raw_salt;

        return algo.GetBytes(settings.bc);
    }
}
