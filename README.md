# Safe Argon2id

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
