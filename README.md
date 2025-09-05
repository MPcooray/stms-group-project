# STMS — Shared Database Guide (Azure SQL)

This README explains how to connect to and use our **single shared database** for the project.

---

## Database (fixed)

- **Server:** `fffff.database.windows.net`  _(note: **five** f’s)_
- **Database:** `dbstms`
- **Authentication:** SQL (username/password)
- **App/Login:** `stms_app`  
  - Roles: `db_datareader`, `db_datawriter` (no DDL by default)  
  - Default schema: `dbo`

> 🔒 Ask the owner for the `stms_app` password privately. **Never** commit secrets to Git.

---

## Access (one-time per teammate)

1. **Whitelist your IP (firewall)**  
   - Send your **public IP** to the owner (`curl ifconfig.me`).  
   - Owner adds it: **Azure Portal → SQL server (fffff) → Networking → + Add client IP → Save**.  
   - If your IP changes later, ask to update it.

2. **Install tools / drivers**  
   See the **Mac** and **Windows** quick-starts at the bottom of this doc.

---

## Quick Connectivity Test

### Using `sqlcmd`
```bash
sqlcmd -S fffff.database.windows.net -d dbstms -U stms_app -Q "SELECT DB_NAME(), CURRENT_USER;"
# you'll be prompted for the password
