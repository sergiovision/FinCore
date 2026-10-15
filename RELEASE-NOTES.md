# Release notes

## 0.9.20 — maintenance refresh (2026-10)

Library updates, security fixes and cleanup. No database schema or data changes.

### Platform
- Backend retargeted from .NET 8 to **.NET 10** (`net10.0`); SDK pinned in `global.json`; Dockerfile uses `sdk:10.0`.
- Frontend stays on Angular 14, now with **Node 18** pinned (`.nvmrc`, `engines`); Docker client stage uses `node:18-alpine` and `npm ci`.

### Backend
- **AutoMapper removed** (unpatched high-severity alert); replaced by hand-written `BusinessLogic/EntityMapper.cs`.
- New `UnitTests` xUnit project: 33 tests covering every entity mapping.
- Updated: NHibernate 5.7.0, FluentNHibernate 3.5.0, log4net 3.5.0, Microsoft.* 10.0.13, Autofac 8.4.0, Quartz 3.22.6,
  MySql.Data 8.4.0, HtmlAgilityPack 1.13.1, Newtonsoft.Json 13.0.4, SQLitePCLRaw 2.1.14, NHibernate.Extensions.Sqlite 10.0.5.
- Pinned patched transitive packages: System.Drawing.Common 4.7.3, BouncyCastle.Cryptography 2.7.0.
- Deleted dead `BusinessLogic/packages.config` (.NET Framework leftover).
- Fixed: account-state list failed to map (`AccountId` now taken from the account).
- Removed FTX exchange settings (FTX shut down in 2022).

### Frontend
- Angular 14.3.0, CLI 14.2.13, CoreUI Angular 2.14.1, DevExtreme 23.2.15, TypeScript 4.8.4, RxJS 7.8.2, zone.js 0.11.8,
  Bootstrap 5.3.8, angular-calendar 0.30.1 and other same-major bumps.
- Removed unused packages: protractor/e2e, wallaby, mocha, node-gyp, tslint, rxjs-compat, popper, acorn, ts-helpers,
  mutationobserver-shim, caniuse-lite, fsevents; removed non-runnable generated spec files.
- Lint migrated from TSLint to **ESLint + angular-eslint 14**; all lint errors fixed (incl. a duplicated `case` in Capital).
- Production and Docker builds take the server host from `window.location` instead of hard-coded `127.0.0.1`.
- `package-lock.json` is now committed for reproducible `npm ci`.

### Security
- `npm audit`: 137 → 74 issues, **0 critical** (dead packages removed + targeted `overrides`).
- `dotnet list package --vulnerable`: no vulnerable packages.
- Remaining Angular 14 alerts documented with dismiss reasons in [SECURITY-NOTES.md](SECURITY-NOTES.md).

### Tooling and docs
- New `scripts/` folder: `build.sh`, `restore.sh`, `test.sh`, `run.sh`, `docker.sh` (+ existing `build.bat`, `restore.bat`).
- GitHub Actions CI (`.github/workflows/build.yml`): client lint/build/audit, server build and tests.
- Rider/VS launch profiles fixed to run from the repo `bin/` folder (no more Windows-only `C:\dev\...` path).
- `.gitignore` / `.dockerignore` cover `bin/`, `obj/`, `.angular/`, `dist/`, `node_modules/`.
- README updated: requirements, versions table, Docker quick start, scripts, local debugging.
