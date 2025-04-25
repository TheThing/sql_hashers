using System;
using System.Security.Cryptography;

public class Argon2Common
{
    public struct settings
    {
        public byte parallel;
        public short memory;
        public byte iterations;
        public byte bc;
        public byte version;

        public override string ToString()
        {
            return string.Format("v={0};{1}", this.version, FormatComplexity(this));
        }
    }

    public struct group
    {
        public settings settings;
        public string salt;
        public string hash;
    }

    static readonly settings[] all_versions = new settings[]
    {
        new settings { version = 1 }, // Custom
        new settings { parallel = 4, memory = 64, iterations = 3, bc = 33, version = 2 }, // Bitwarden defaults
    };

    const int use_version = 2;

    private static void AssertValidVersion(short version)
    {
        if (version <= 0 || version > all_versions.Length)
        {
            throw new ArgumentOutOfRangeException(string.Format("Unsupported version {0}. Minimum allowed is {1} and maximum is {2}", version, 1, all_versions.Length));
        }
    }

    public static string FormatHash(settings settings, byte[] raw_salt, byte[] raw_hash)
    {
        var salt = Convert.ToBase64String(raw_salt);
        var hash = Convert.ToBase64String(raw_hash);
        if (settings.version > 1)
        {
            return FormatCommonHash(settings.version, salt, hash);
        }
        return FormatCustomHash(settings, salt, hash);
    }

    public static string FormatCommonHash(short version, string salt, string hash)
    {
        AssertValidVersion(version);
        return string.Format("{0}${1}${2}", version, salt, hash);
    }

    public static string FormatCustomHash(settings complexity, string salt, string hash)
    {
        return String.Format("{0}${1}${2}${3}", 1, FormatComplexity(complexity), salt, hash);
    }

    public static settings GetSettings()
    {
        return GetSettings(use_version);
    }

    public static settings GetSettings(short version)
    {
        return all_versions[version - 1];
    }

    public static byte[] GenerateSalt(short length)
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] raw_salt = new byte[length];
            rng.GetBytes(raw_salt);
            return raw_salt;
        }
    }

    public static bool CompareRawHash(byte[] raw_pass_hash, byte[] raw_original_hash)
    {
        // Even though timing attack is a concern below, this should not be a problem for this comparison check
        // as we're only comparing that the settings were identical in byte length.
        if (raw_pass_hash.Length != raw_original_hash.Length)
        {
            throw new Exception("Something went wrong, bc was incompatible in hash.");
        }

        int accum = 0;

        for (int i = 0; i < raw_pass_hash.Length; i++)
            accum |= (raw_pass_hash[i] ^ raw_original_hash[i]);

        return accum == 0;
    }

    public static group ParseFormattedHash(string formattedHash)
    {
        settings s;
        string salt;
        string hashed;

        string[] split = formattedHash.Split('$');
        if (split.Length == 0)
        {
            throw new Exception("Invalid or unknown Argon2id hash");
        }

        short version = Convert.ToInt16(split[0]);
        AssertValidVersion(version);

        if (version == 1)
        {
            if (split.Length != 4) // version$settings$salt$hash
            {
                throw new Exception("Invalid argon2id hash format: Custom argon2 settings with version 1 has to have 4 items.");
            }
            s = ParseComplexity(split[1]);
            salt = split[2];
            hashed = split[3];
        }
        else
        {
            if (split.Length != 3) // version$salt$hash
            {
                throw new Exception("Invalid argon2id hash format: Common argon2 settings with version >1 has to have 3 items.");
            }
            s = all_versions[version - 1];
            salt = split[1];
            hashed = split[2];
        }

        return new group { settings = s, hash = hashed, salt = salt };
    }

    private static string FormatComplexity(settings complexity)
    {
        return String.Format("p={0};m={1};i={2};bc={3}", complexity.parallel, complexity.memory, complexity.iterations, complexity.bc);
    }

    private static settings ParseComplexity(string raw_complexity)
    {
        settings s = new settings();

        s.version = 1;

        foreach (string item in raw_complexity.Split(';'))
        {
            string[] key_value = item.Split('=');
            if (key_value.Length != 2) { continue; }

            if (key_value[0] == "p") { s.parallel = Convert.ToByte(key_value[1]); }
            if (key_value[0] == "m") { s.memory = Convert.ToInt16(key_value[1]); }
            if (key_value[0] == "i") { s.iterations = Convert.ToByte(key_value[1]); }
            if (key_value[0] == "bc") { s.bc = Convert.ToByte(key_value[1]); }
        }

        if (s.parallel == 0 || s.memory == 0 || s.iterations == 0 || s.bc == 0)
        {
            throw new Exception("Invalid or missing Argon2id hash settings");
        }
        return s;
    }
}
