# SafeArgon2

A **safe**, **single-threaded**, and **fully standalone** implementation of the **Argon2id** password hashing algorithm in **pure C#**.

This project is designed for use as a **CLR assembly in Microsoft SQL Server on Linux**, where features like `unsafe` code, threading, and external dependencies are not allowed in assemblies with `PERMISSION_SET = SAFE`.

---

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

## 🧪 Example SQL Usage

Once compiled, the resulting `SafeArgon2.dll` can be registered and used in Microsoft SQL Server on Linux:

```sql
-- Register assembly (path may vary)
CREATE ASSEMBLY SafeArgon2
FROM '/var/opt/mssql/SafeArgon2.dll'
WITH PERMISSION_SET = SAFE;
GO

-- Create SQL function from exported method
CREATE FUNCTION dbo.Argon2idHash (
    @password NVARCHAR(MAX),
    @salt NVARCHAR(MAX),
    @secret NVARCHAR(MAX),
    @associatedData NVARCHAR(MAX),
    @parallelism INT,
    @iterations INT,
    @memorySize INT,
    @hashLength INT
)
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME SafeArgon2.[SafeArgon2.PasswordHasher].HashPassword;
GO

