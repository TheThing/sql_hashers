# Safe & External Argon2id

This is a library to implement argon2id password checking in MS SQL Server that is **both** runnable in **SAFE** mode as well as a faster and more compliant **UNSAFE** mode.

It's split into two libraries:

## ExternalArgon2

An argon2id specification compliant implementation for MS SQL Server that requires **UNSAFE** permission in `CREATE ASSEMBLY` and therefore only usable on MS SQL Server running on Windows.

## SafeArgon2

An argon2id specification compliant **(non-parallel)** implementation for MS SQL Sever that is runnable in **SAFE** permission in `CREATE ASSEMBLY` and therefore usable in MS SQL Server in Linux (as well as Windows).

# Installation

## Adding SafeArgon2

```bash
sudo cp /YOUR_PATH/SafeArgon2.dll /var/opt/mssql/data/SafeArgon2.dll
```

```sql
---- Optional: Set trustworthy ON so we can import the assembly
--USE master
--ALTER DATABASE master SET TRUSTWORTHY ON;
--go

CREATE ASSEMBLY ClrArgon2 FROM '/var/opt/mssql/data/SafeArgon2.dll' WITH PERMISSION_SET = SAFE;
GO
```

## Adding ExternalArgon2

Copy over bundled dll for ExternalArgon2 in a folder of your choosing (I'm using `C:\SQL2022\MsArgon2\External`) and then run:

```sql
---- Optional: Set trustworthy ON so we can import the assembly
--USE master
--ALTER DATABASE master SET TRUSTWORTHY ON;
--go

CREATE ASSEMBLY ClrArgon2 FROM 'C:\SQL2022\MsArgon2\External\ExternalArgon2.bundled.dll' WITH PERMISSION_SET = UNSAFE;
GO
```

## Turning trustworthy back off again

You can run the following code to register our clr as safe and turn trustworthy back off

```sql
DECLARE @Hash BINARY(64),
        @ClrName NVARCHAR(4000),
        @AssemblySize INT,
        @MvID UNIQUEIDENTIFIER;

SELECT  @Hash = HASHBYTES(N'SHA2_512', af.[content]),
        @ClrName = CONVERT(NVARCHAR(4000), ASSEMBLYPROPERTY(af.[name],
                N'CLRName')),
        @AssemblySize = DATALENGTH(af.[content]),
        @MvID = CONVERT(UNIQUEIDENTIFIER, ASSEMBLYPROPERTY(af.[name], N'MvID'))
FROM    sys.assembly_files af
  JOIN  sys.assemblies a ON (af.assembly_id = a.assembly_id)
WHERE   a.name = 'ClrArgon2'
AND     af.[file_id] = 1;

SELECT  @ClrName, @AssemblySize, @MvID, @Hash;

EXEC sys.sp_add_trusted_assembly @Hash, @ClrName;
GO

ALTER DATABASE master SET TRUSTWORTHY OFF;
go
```

# API

Both library have consistent API. They both exposes 3 helper functions:
```cs
public static void Argon2id_hash(string password, out string output);
public static void Argon2id_hash_custom(string password, byte parallel, short memory, byte iterations, byte bc, out string output);
public static int Argon2id_verify(string password, string hash);
```

## Register Procedure

```sql
CREATE PROCEDURE dbo.[argon2id_hash] (@password [nvarchar](256), @hash [nvarchar](256) OUT)
AS EXTERNAL NAME [ClrArgon2].[MsSqlArgon2].[Argon2id_hash]
GO
CREATE PROCEDURE dbo.[argon2id_hash_custom] (@password [nvarchar](256), @parallel [tinyint], @memory [smallint], @iterations [tinyint], @bc [tinyint], @output [nvarchar](256) OUT)
AS EXTERNAL NAME [ClrArgon2].[MsSqlArgon2].[Argon2id_hash_custom]
GO
CREATE PROCEDURE dbo.[argon2id_verify] (@i [nvarchar](256), @h [nvarchar](256))
AS EXTERNAL NAME [ClrArgon2].[MsSqlArgon2].[Argon2id_verify]
GO
```

#### `PROCEDURE [argon2id_hash] (@password [nvarchar](256), @hash [nvarchar](256) OUT)`

Generate new salt and create an MsSqlArgon2 formatted string. The string contains within it both the salt and the hash.
This function generates an argon2id using the same defaults that bitwarden uses which is:

```cs
new settings { parallel = 4, memory = 64, iterations = 3, bc = 33 }
```

**Example:**
```sql
DECLARE @Hashed NVARCHAR(256);
exec dbo.[argon2id_hash] 'My password', @Hashed output;
-- @Hashed = '2$GdrGCqszJmUq3kfKLt+gnWCb$AFMqejAkQfh9JaBq0A0XmSVy25Ev8pzfiHCS2ghyGpNv'
```

#### `PROCEDURE [argon2id_hash_custom] (@password [nvarchar](256), @parallel [tinyint], @memory [smallint], @iterations [tinyint], @bc [tinyint], @output [nvarchar](256) OUT)`

Generate new salt using custom argon2id options for parallel, memory, iterations and bc.
Keep in mind, the parameter `@memory` specifies memory usage in **megabytes**.

This will return an MsSqlArgon2 formatted string containing all the options used.

**Example:**
```sql
DECLARE @Hashed NVARCHAR(256);
exec dbo.[argon2id_hash_custom] 'Hello world', 1, 1, 1, 33, @Hashed output;
-- @Hashed = '1$p=1;m=1;i=1;bc=33$w9tFGHy7W1NtNcT/BMKwXVyh$fEWUIwagG0yEVO55pOT5fxRJZOc9xCKzOMgK9JWG2C75'
```

#### `PROCEDURE dbo.[argon2id_verify] (@i [nvarchar](256), @h [nvarchar](256))`

Verifies that the password `@i` matches the MsSqlArgon2 formatted hash `@h`.
Returns `0` for failure and `1` for success.

**Example:**
```sql
DECLARE @Verified AS INT;
exec @Verified = dbo.[argon2id_verify] 'Hello world', '1$p=1;m=1;i=1;bc=33$w9tFGHy7W1NtNcT/BMKwXVyh$fEWUIwagG0yEVO55pOT5fxRJZOc9xCKzOMgK9JWG2C75';
-- @Verified = 1
```


# Performance consideration between SafeArgon2 and ExternalArgon2

Here are some basic execution numbers.

#### ConsoleTest
```
ExternalArgon2 regular     = 00:00:00.1224412
ExternalArgon2 fast        = 00:00:00.0074187
ExternalArgon2 slower      = 00:00:01.4640895
ExternalArgon2 slow        = 00:00:02.7076549
ExternalArgon2 slow parall = 00:00:00.8820149

SafeArgon2 regular         = 00:00:00.2965957
SafeArgon2 fast            = 00:00:00.0066044
SafeArgon2 slower          = 00:00:01.5480459
SafeArgon2 slow            = 00:00:02.8710398
SafeArgon2 slow parallel   = 00:00:02.9125752
```

```
regular:       { parallel = 4, memory =  64, iterations =  3, bc = 33 } (Bitwarden defaults)
fast:          { parallel = 1, memory =   4, iterations =  1, bc = 33 }
slower:        { parallel = 1, memory = 128, iterations =  8, bc = 32 }
slow:          { parallel = 1, memory = 128, iterations = 15, bc = 32 }
slow parallel: { parallel = 4, memory = 128, iterations = 15, bc = 32 }
```

#### Inside an MS SQL Server
```sql
SET STATISTICS TIME ON;
DECLARE @Hashed NVARCHAR(256);
exec dbo.[external_argon2id_hash] 'Hello world', @Hashed output;
exec dbo.[safe_argon2id_hash] 'Hello world', @Hashed output;
```

```
SQL Server parse and compile time: 
   CPU time = 0 ms, elapsed time = 0 ms.

 SQL Server Execution Times:
   CPU time = 0 ms,  elapsed time = 167 ms.

 SQL Server Execution Times:
   CPU time = 328 ms,  elapsed time = 345 ms.
```

---

# Information on SafeArgon2

A **safe**, **single-threaded**, and **fully standalone** implementation of the **Argon2id** password hashing algorithm in **pure C#**.

This project is designed for use as a **CLR assembly in Microsoft SQL Server on Linux**, where features like `unsafe` code, threading, and external dependencies are not allowed in assemblies with `PERMISSION_SET = SAFE`.


## ✅ Features

- Builds into a self-contained `.dll`
- Compatible with SQL Server on Linux
- No unsafe code
- No external dependencies
- No threading or synchronization
- Deterministic and secure hash generation

---

## ⚙️ Background

This implementation is **based on** the original [Konscious.Security.Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) library by [@kmaragon](https://github.com/kmaragon).

The original project is an optimized version using parallelism and `unsafe` memory access. However, SQL Server's Linux CLR host does not support:

- Unsafe code
- External/native dependencies (like `System.Memory`, `System.Numerics.Vectors`, etc.)
- Threading/synchronization

To make this work under those limitations, the implementation was **rewritten** to meet the restrictions.

---

## 🔍 Comparison

| Feature                        | `SafeArgon2`        | `Konscious.Security.Cryptography` |
|-------------------------------|---------------------|------------------------------------|
| SQL Server Linux compatible   | ✅ Yes              | ❌ No                              |
| External dependencies         | ❌ None             | ✔ Yes                              |
| Unsafe code                   | ❌ None             | ✔ Yes                              |
| Threading / parallelism       | ❌ No               | ✔ Yes                              |
| Performance                   | 🐢 Slower           | ⚡ Fast                             |

---

## 🖥️ Target Platform

This library is built for:

- **.NET Framework 4.7.2** or higher
- **Microsoft SQL Server on Linux**
- **CLR Integration with `PERMISSION_SET = SAFE`**

> ⚠️ This project is **not compatible with .NET Core**, .NET 5+, or assemblies requiring `EXTERNAL_ACCESS` or `UNSAFE` permissions.

If you're building this DLL yourself, make sure your project targets **.NET Framework 4.7.2**. You may also try higher version.

---

## 🧪 Example SQL Usage

Once compiled, the resulting `SafeArgon2.dll` can be registered and used in Microsoft SQL Server on Linux.

Copy the DLL to SQL Server Data Directory. Make sure the DLL is accessible to the SQL Server process.

```bash
sudo cp /YOUR_PATH/SafeArgon2.dll /var/opt/mssql/data/SafeArgon2.dll
```

```sql
-- Register assembly (path may vary)
CREATE ASSEMBLY SafeArgon2 FROM '/var/opt/mssql/data/SafeArgon2.dll' WITH PERMISSION_SET = SAFE;
GO

-- Create SQL function from exported method
CREATE FUNCTION dbo.Argon2idHash (@password NVARCHAR(MAX), @salt NVARCHAR(MAX), @secret NVARCHAR(MAX), @associatedData NVARCHAR(MAX), @parallelism INT, @iterations INT, @memorySize INT, @hashLength INT)
RETURNS NVARCHAR(MAX) AS EXTERNAL NAME SafeArgon2.[SafeArgon2.PasswordHasher].Argon2idHash;
GO
```

## Sample Query

```sql
SELECT dbo.Argon2idHash(N'Hello', N'qwerty12345', N'', N'', 16, 15, 4096, 32);
```

---

## ⚠️ Security Warning

This implementation of **Argon2id** is **single-threaded by design** to comply with Microsoft SQL Server on Linux `SAFE` assembly restrictions.

### ❗ Important Limitations:

- ❌ Does **not** use parallelism (multi-threading) — a core feature of the original Argon2id
- ❌ May be **less resistant** to GPU/ASIC attacks
- ❌ Does **not fully comply** with the Argon2id specification ([RFC 9106](https://datatracker.ietf.org/doc/html/rfc9106))
- ❌ Not recommended for use in high-security environments or systems exposed to untrusted users without additional safeguards

### ✅ Still safe for:

- Internal SQL Server use under controlled environments
- Use cases where standard Argon2id implementations are not permitted (e.g., due to SAFE CLR restrictions)
- Scenarios where **usability inside SQL Server outweighs optimal attack resistance**

> 🛡️ For maximum security, prefer a standard, multi-threaded Argon2id implementation when platform restrictions allow it.
