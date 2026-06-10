# Claude Code Notes

## Common Issues & Fixes

### "This site can't be reached" Error

The servers aren't running. Start them with these commands:

**Quick Fix (run both):**
```bash
# Start API backend (from project root)
cd src/PropertyViewerAccounting.Api && dotnet run

# Start React frontend (from project root, in separate terminal)
cd src/web && npm run dev
```

**For background execution on Windows (Git Bash):**
```bash
# API (run_in_background works)
cd src/PropertyViewerAccounting.Api && dotnet run

# Frontend (use cmd start for Windows)
cd src/web && cmd //c "start /b npm run dev"
```

**Verify servers are running:**
```bash
netstat -ano | grep -E "5141|5173"
```

**URLs:**
- Frontend: http://localhost:5173
- API: http://localhost:5141

---

### "Invalid email or password" Login Error

This error occurs regularly. Here's how to fix it:

**Quick Fix:** Restart the API backend. The seeder runs on startup and will:
1. Check if the admin user exists
2. If missing, create it
3. If exists, reset the password to the default

**Default Credentials:**
- Email: `admin@demo.com`
- Password: `admin123`

**Why This Happens:**
- Database was reset/recreated without restarting the API
- Migrations ran but seeding didn't complete
- Password got corrupted somehow

**If Restarting Doesn't Work:**
1. Check the database connection in `src/PropertyViewerAccounting.Api/appsettings.json`
2. Verify PostgreSQL is running: `Host=localhost;Port=5432;Database=propertyviewer_accounting`
3. Check API console for seeding errors on startup
4. Manually verify user exists: query `Users` table for `admin@demo.com`

**Relevant Files:**
- `src/PropertyViewerAccounting.Infrastructure/Data/DbSeeder.cs` - Creates/resets admin user (lines 18-46)
- `src/PropertyViewerAccounting.Api/Program.cs` - Runs seeding on startup (line 93)
- `src/PropertyViewerAccounting.Api/Controllers/AuthController.cs` - Login endpoint

## Project Structure

- **API:** `src/PropertyViewerAccounting.Api/` - .NET backend (port 5141)
- **Web:** `src/web/` - React frontend (Vite)
- **Core:** `src/PropertyViewerAccounting.Core/` - Domain entities
- **Infrastructure:** `src/PropertyViewerAccounting.Infrastructure/` - EF Core, data access
