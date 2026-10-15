# Security notes

Status of the GitHub Dependabot / `npm audit` alerts for this legacy FinCore code base (checked 2026-10-15).

This repository is kept as a demo: the frontend stays on **Angular 14 / CoreUI 2 / DevExtreme 23.2**.
Active development continues in the new FinCore product, which is on Angular 21. **The real fix for every
Angular alert below is that upgrade**; on 14.x the advisories list no patched version.

## Fixed in this refresh

| Area | Change | Alerts closed |
|---|---|---|
| `BusinessLogic/packages.config` | Deleted (dead .NET Framework 4.8 leftover, not referenced by the SDK-style csproj) | SSH.NET (2 high), BouncyCastle (4 moderate), NHibernate 5.2.6 (high) |
| NHibernate | 5.5.0 → 5.7.0 | SQL injection (high, fixed in 5.5.2) |
| log4net | 2.0.15 → 3.5.0 | log4net alert (fixed in 3.3.0) |
| AutoMapper | Removed; replaced by explicit mapping code | DoS via uncontrolled recursion (high, patched only in the commercially licensed 15.1.1+) |
| Frontend dev toolchain | Removed protractor, wallaby, mocha, node-gyp, tslint, rxjs-compat and other unused packages; `npm audit fix`; targeted `overrides` for `tar`, `piscina`, `postcss`, `minimatch@5`, `webpack-dev-middleware@5`, `tmp` | `npm audit`: 137 (11 critical, 55 high) → 74 (**0 critical**, 33 high) |

## Angular alerts that cannot be fixed on Angular 14 — suggested dismiss reasons

Facts checked in `FinCore/ClientApp/src`:
- No SSR, no hydration, no `HttpTransferCache` (these APIs do not exist in Angular 14; the app is a client-only SPA).
- No `i18n` attributes and no `$localize` calls in templates or code (`@angular/localize/init` is only loaded by `polyfills.ts`).
- No `bypassSecurityTrust*`, no `DomSanitizer`, no `[innerHTML]`, no SVG/MathML in templates.
- `HttpClient` URLs are always built from `environment.baseURL` (absolute `http://…` URL); no protocol-relative (`//host`) URLs.
- `DatePipe` formats come from constants in `environment*.ts`; the only `number` pipe uses the literal `'1.2-2'` — no user-controlled format strings.

| Package | Advisory | Severity | Why not exploitable here | Suggested dismiss reason |
|---|---|---|---|---|
| @angular/common | [GHSA-58c5-g7wp-6w37](https://github.com/advisories/GHSA-58c5-g7wp-6w37) XSRF token leakage via protocol-relative URLs | high | No protocol-relative URLs are passed to `HttpClient` | Vulnerable code not used |
| @angular/common | [GHSA-48r7-hpm6-gfxm](https://github.com/advisories/GHSA-48r7-hpm6-gfxm) DoS via OOM in date formatting | high | Date formats are hard-coded constants, not user input | Vulnerable code not used |
| @angular/common | [GHSA-p3vc-36g9-x9gr](https://github.com/advisories/GHSA-p3vc-36g9-x9gr) DoS via OOM in number formatting (digitsInfo) | high | `digitsInfo` is a template literal, not user input | Vulnerable code not used |
| @angular/common | [GHSA-39pv-4j6c-2g6v](https://github.com/advisories/GHSA-39pv-4j6c-2g6v) weak cache-key hashing in `HttpTransferCache` | high | No SSR / `HttpTransferCache` | Vulnerable code not used |
| @angular/common | [GHSA-q6f4-qqrg-jv6x](https://github.com/advisories/GHSA-q6f4-qqrg-jv6x) caching of credentialed requests in `HttpTransferCache` | high | No SSR / `HttpTransferCache` | Vulnerable code not used |
| @angular/common | [GHSA-jhpw-976m-542j](https://github.com/advisories/GHSA-jhpw-976m-542j) cache-key ambiguity in `HttpTransferCache` | high | No SSR / `HttpTransferCache` | Vulnerable code not used |
| @angular/common | [GHSA-p297-fm68-3q8c](https://github.com/advisories/GHSA-p297-fm68-3q8c) `HttpTransferCache` bypass with `withRequestsMadeViaParent` | moderate | No SSR / `HttpTransferCache` | Vulnerable code not used |
| @angular/compiler, @angular/core | [GHSA-jrmj-c5cx-3cw6](https://github.com/advisories/GHSA-jrmj-c5cx-3cw6) XSS via unsanitized SVG script attributes | high | No SVG in templates; templates are compiled at build time from repo sources | Vulnerable code not used |
| @angular/compiler | [GHSA-v4hv-rgfq-gp49](https://github.com/advisories/GHSA-v4hv-rgfq-gp49) stored XSS via SVG animation, SVG URL and MathML attributes | high | No SVG/MathML in templates | Vulnerable code not used |
| @angular/compiler, @angular/core | [GHSA-jj27-h5hq-8x99](https://github.com/advisories/GHSA-jj27-h5hq-8x99) i18n XSS via event-handler attributes | high | No `i18n` attributes / translations | Vulnerable code not used |
| @angular/core | [GHSA-prjf-86w9-mfqv](https://github.com/advisories/GHSA-prjf-86w9-mfqv) i18n XSS | high | No `i18n` attributes / translations | Vulnerable code not used |
| @angular/core | [GHSA-rgjc-h3x7-9mwg](https://github.com/advisories/GHSA-rgjc-h3x7-9mwg) client hydration DOM clobbering & response-cache poisoning | high | No hydration (not available in v14) | Vulnerable code not used |
| @angular/router | [GHSA-ff3f-86qr-9cv3](https://github.com/advisories/GHSA-ff3f-86qr-9cv3) SSR DoS via numeric URL matrix parameters | high | No SSR | Vulnerable code not used |
| @angular/compiler | [GHSA-58w9-8g37-x9v5](https://github.com/advisories/GHSA-58w9-8g37-x9v5) two-way property binding sanitization bypass | moderate | No two-way `[( )]` bindings to security-sensitive properties; the only `[href]` bindings are one-way chart links built by `externChartURL()` and still pass Angular's URL sanitizer | Vulnerable code not used |
| @angular/compiler, @angular/core | [GHSA-f3m7-gqxr-g87x](https://github.com/advisories/GHSA-f3m7-gqxr-g87x) template/attribute namespace sanitization bypass | moderate | No namespaced (SVG/MathML/xlink) attributes in templates | Vulnerable code not used |
| @angular/core | [GHSA-692r-grfm-v8x7](https://github.com/advisories/GHSA-692r-grfm-v8x7) template & dynamic component namespace bypass | moderate | No dynamic namespaced components | Vulnerable code not used |
| @angular/compiler, @angular/core | [GHSA-hh8m-fm6v-7cvg](https://github.com/advisories/GHSA-hh8m-fm6v-7cvg) sanitization bypass via directive host bindings | moderate | No `host:` metadata or `@HostBinding` in the app | Vulnerable code not used |
| @coreui/angular 2.14.1 | Flagged only through its Angular 14 dependency | high | Same as the Angular rows above; 2.14.1 is the last release supporting Angular 14 | Risk is tolerable / fixed by the Angular 21 product |

## Remaining dev-toolchain highs (`npm audit`, not shipped to the browser)

These live only in the build/lint/dev-server toolchain (`@angular-devkit/build-angular` 14, `@angular/cli` 14,
`@angular-eslint` 14, `@typescript-eslint` 5, `karma` 6). They never reach the production bundle. Patches exist only
in major versions that require Angular ≥ 15 or Node ≥ 20.

| Package | Advisory | Why it remains | Exposure |
|---|---|---|---|
| serialize-javascript ≤ 7.0.2 (via copy/terser webpack plugins) | [GHSA-5c6j-r48x-rmvq](https://github.com/advisories/GHSA-5c6j-r48x-rmvq) RCE via RegExp.flags / Date.toISOString | Fix is 7.1.x, which requires Node ≥ 20 | Build time only, serializes our own build config |
| webpack-dev-middleware 5.3.4 | [GHSA-g84c-rxfj-3j2c](https://github.com/advisories/GHSA-g84c-rxfj-3j2c) path traversal via non-slash-terminated publicPath | Fixed only in 7.4.5 (webpack-dev-server 5) | `ng serve` on localhost only |
| webpack-dev-server 4.11, http-proxy-middleware, selfsigned / node-forge ≤ 1.4.0 | dev-server advisories; [GHSA-86w9-cpqp-85rv](https://github.com/advisories/GHSA-86w9-cpqp-85rv) node-forge signature verification | No patched node-forge release; webpack-dev-server 5 needs Angular ≥ 17 | `ng serve` on localhost only |
| braces ≤ 3.0.3 → micromatch, fast-glob, globby, chokidar, sass (build-angular's), stylus-loader, copy-webpack-plugin, @typescript-eslint/* | [GHSA-vfj7-8cjw-p6xm](https://github.com/advisories/GHSA-vfj7-8cjw-p6xm) stack-exhaustion DoS with deeply nested patterns | No patched 3.x release | Glob patterns come from repo config only |
| pacote (via @angular/cli) | [GHSA-w4pp-8pjf-rmxw](https://github.com/advisories/GHSA-w4pp-8pjf-rmxw) DoS via `addGitSha` | Fix only in CLI ≥ 21 | Only used by `ng add` / `ng update` |
| karma 6 | flagged through its dependencies | Test runner, no spec files remain | Not run in CI |

Overrides applied in `FinCore/ClientApp/package.json` (each one checked with `npm run build` and `ng serve`):
`tar ^7.5.22` (was critical), `piscina ^4.9.4` (was critical), `postcss ^8.5.30`, `minimatch@5 ^5.1.9`,
`webpack-dev-middleware@5 ^5.3.4`, `tmp ^0.2.7`. The `ngx-monaco-editor` override only relaxes its Angular 12 peer
range to the installed Angular 14 (no `--legacy-peer-deps` needed). The `tar` 7 override affects only the CLI's
package-install code path (`ng add`/`ng update`), not `ng build`/`ng serve`.

## Backend (.NET)

Backend target is **net10.0** (all Microsoft.AspNetCore.* / Microsoft.Extensions.* / Microsoft.Data.Sqlite packages 10.0.13).

`dotnet list FinCore.sln package --vulnerable --include-transitive --source https://api.nuget.org/v3/index.json`
→ **no vulnerable packages** in FinCore, BusinessLogic, BusinessObjects and UnitTests (2026-10-15).

Two transitive packages pulled in by `MySql.Data` 8.4.0 were flagged after the retarget and are pinned directly
in `BusinessLogic/BusinessLogic.csproj` (same major):

| Package | From → To | Advisory |
|---|---|---|
| System.Drawing.Common (via System.Windows.Extensions) | 4.7.0 → 4.7.3 | [GHSA-rxg9-xrhp-64gj](https://github.com/advisories/GHSA-rxg9-xrhp-64gj) (critical) |
| BouncyCastle.Cryptography | 2.2.1 → 2.7.0 | GHSA-8xfc-gm6g-vgpv, GHSA-m44j-cfrm-g8qc, GHSA-v435-xc8x-wvr9 (moderate) |
