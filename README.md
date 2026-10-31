<img src="/FinCore/ClientApp/src/assets/img/brand/logo.png" width="100"/>

# About FinCore
<p id="doc_about">
</p>
  Version 0.9.20, maintenance refresh 2026-10 (library updates and security fixes, see [RELEASE-NOTES.md](RELEASE-NOTES.md) and [SECURITY-NOTES.md](SECURITY-NOTES.md)).
  FinCore is a cool and free cross-platform personal finances manager written in .NET 10 with Angular 14 frontend. <br>

  > This repository is the legacy FinCore, kept as a demo. Active development continues in the new FinCore product: [capitaltools.app/fincore](https://capitaltools.app/fincore).

## What this project demonstrates

A complete, self-hosted trading and personal-finance back office, built from my experience as a developer and team lead at banks and FOREX brokers and working with individual traders:

- **ASP.NET Core (.NET 10) + Angular SPA**: Web API controllers with JWT auth, the Angular client is served by the same host (Windows Service, console app or Docker).
- **Real-time over WebSockets**: a NetCoreServer-based WebSocket server (`ws`/`wss`, port `2021`) pushes positions, quotes and logs to the dashboard without polling.
- **Background jobs with Quartz.NET**: exchange rates, terminal sync and terminal monitoring jobs scheduled by cron expressions stored in the DB, and controlled from the UI.
- **Persistence with NHibernate / FluentNHibernate**: one domain model over SQLite (default) or MySQL, Autofac for dependency injection.
- **MetaTrader 5 bridge**: MQL5 Expert Advisers plus a .NET Framework 4.8 `MQLConnector.dll` sync terminals, accounts, deals and adviser settings with the server over Web API (port `2020`) and a TCP channel (port `2022`), so several terminals and brokers are managed from one place.
- **Crypto exchange integration**: KuCoin spot and futures positions in the same views.

```
 Browser (Angular 14 SPA)
   │  HTTPS / REST (2020)        ▲ WebSocket push (2021)
   ▼                             │
 ┌──────────────────────── FinCore server (ASP.NET Core, .NET 10) ───────────────────────┐
 │  Web API controllers ── BusinessLogic ── NHibernate ──► SQLite / MySQL                │
 │  Quartz.NET jobs     ── Messaging (WebSocket + TCP servers) ── KuCoin API client      │
 └───────────────────────────────────────────────────────────────────────────────────────┘
   ▲  Web API (2020) + TCP (2022)
   │
 MetaTrader 5 terminals (MQL5 Expert Advisers + MQLConnector.dll), one per broker/account
```

**Why Angular 14?** This repository is a frozen legacy demo. The front end stays on Angular 14 because it depends on CoreUI Angular 2 and the matching DevExtreme release, and upgrading means rewriting the UI layer. That work was done in the new FinCore product (Angular 22). Only dependency and security updates go here, see [SECURITY-NOTES.md](SECURITY-NOTES.md).

## For users

  <p>   This project is a summary of my experience working as a developer and team leader for various financial institutions (banks and FOREX brokers) and individual traders. I made application as simple as possible to use with such complex thing as finances.</p>
  
Program fits for anybody. <br>
  1. For merchant/consumer<br>
  If one lazy to take risks on trading and investing then can stick to <a href="#doc_wallets">Wallets book</a> and <a href="#doc_rates">Realtime exchange rates</a> <br>
  In this case one can control and observe his wallets state in different currencies with current exchange rates and track performance here <a href="#doc_performance">Earning/Spending Performance per day/month</a>.<br>
  2. For trader<br>
  If one wants to remove emotions and use risk restriction and analyze trading statistics then these screens
  <a href="#doc_instruments">Trading Statistics and Risk management</a> and <a href="#doc_dashboard">Realtime Dashboard</a> might help to evolve into profitable trader.<br>
   This will help to make trading less emotional and will help to create trading strategy and find favourite and most profitable instruments.<br>
  3. For Investor <br>
  If one wants to track investment portfolio and observe shares/stock prices then these screens will help  <a href="#doc_dashboard">Realtime Dashboard</a>
  and <a href="#doc_investments">Investments Pie</a> and <a href="#doc_capital">Yearly Capital State</a>.
  <br>
  <p>Using this app one can grow in all 3 directions - on constant usage one can become better and smart consumer/merchant, then better trader and finally wise investor.
  These screens will show and track in time your performance and personal relations with finances <a href="#doc_capital">Yearly Capital State</a> and
  <a href="#doc_performance">Earning/Spending Performance per day/month</a>.</p>

  Consulting / commercial support: [hi@sergego.com](mailto:hi@sergego.com)
  <br>

## Requirements

- Node.js 18 (see `FinCore/ClientApp/.nvmrc`; Angular 14 does not run on Node 20+)
- .NET 10 SDK (pinned in `global.json`)
- Docker (optional, for the quick start below)
- Optional: Metatrader 5 terminal, MySQL Server 5+, .NET Framework 4.8 (for the dlls used inside Metatrader 5)

## Main versions

| Component | Version |
|---|---|
| .NET (FinCore, BusinessLogic, BusinessObjects) | net10.0, ASP.NET Core / Microsoft.Extensions / Microsoft.Data.Sqlite 10.0.13 |
| NHibernate / FluentNHibernate | 5.7.0 / 3.5.0 |
| log4net | 3.5.0 |
| Autofac / Quartz / MySql.Data | 8.4.0 / 3.22.6 / 8.4.0 |
| Object mapping | hand-written `BusinessLogic/EntityMapper.cs` (AutoMapper removed) |
| Angular / Angular CLI | 14.3.0 / 14.2.13 |
| CoreUI Angular | 2.14.1 |
| DevExtreme | 23.2.15 |
| TypeScript / RxJS / zone.js | 4.8.4 / 7.8.2 / 0.11.8 |
| Lint | ESLint 8 + angular-eslint 14 (`npm run lint`) |

Known open security alerts and why they are not exploitable here are listed in [SECURITY-NOTES.md](SECURITY-NOTES.md).

## Quick start with Docker

```
git clone https://github.com/sergiovision/FinCore.git
cd FinCore
scripts/docker.sh          # same as: docker compose build
docker compose up
```

Then open http://localhost:2020/#/login (default login below). Websockets use port `2021`.

## Scripts

All helper scripts live in [`/scripts`](/scripts) and can be run from any folder:

| Script | What it does |
|---|---|
| `scripts/restore.sh` / `scripts/restore.bat` | Restores NuGet packages and runs `npm ci` for the client |
| `scripts/build.sh` / `scripts/build.bat` | Builds the Angular client and publishes the server into `./bin` (Windows Service / console app) |
| `scripts/test.sh` | Same checks as CI: client lint, build and `npm audit --audit-level=critical`; server build and unit tests |
| `scripts/run.sh` | Runs the server locally on http://localhost:2020 (build the client first) |
| `scripts/docker.sh` | Builds the docker image (`docker compose build`) |

## How to build

1. Clone this repository.
2. Build the client app:
   ```
   cd FinCore/ClientApp
   nvm use            # Node 18
   npm ci
   npm run lint
   npm run build
   ```
   For UI debugging run `npm run start` and open http://127.0.0.1:4200.
3. Build and test the server:
   ```
   dotnet build FinCore.sln -c Release
   dotnet test FinCore.sln -c Release
   ```
   Or run `scripts/build.sh` (`scripts\build.bat` on Windows) to build everything and publish into `./bin`.
4. Debug locally (JetBrains Rider / Visual Studio): use the `FinCore` launch profile. It runs from the repo `bin/` folder
   (`workingDirectory: ../bin` in `FinCore/Properties/launchSettings.json`). In `Development` the server proxies the UI to
   the Angular dev server, so also run `npm run start` in `FinCore/ClientApp`, then open http://localhost:2020.

SQLite database located in `/DB` folder. By default SQLite DB file used, but MySQL also supported, you can switch to MySQL in `/FinCore/appSettings.json` file.

For configuring crypto module setup Api Keys for KuCoin:

   For KuCoin exchange set the following properties in `/FinCore/appSettings.json`:
  `KuCoinAPIKey`  - KuCoin Main API Key
  `KuCoinAPISecret` - API Secret
  `KuCoinPassPhrase` - Pass Phrase
  
  `KuCoinFutureAPIKey` - KuCoin Futures API Key
  `KuCoinFutureAPISecret` - API Secret
  `KuCoinFuturePassPhrase` - Futures Pass Phrase


Open `fincore_empty.sqlite` file in any DB editor that works with SQLite ( like Navicat ).

Open Settings screen and set the following variables

`XTrade.TerminalUser` - should be set to windows user login name where trading terminals will be running

`XTrade.InstallDir` - XTrade installation folder.

`Metatrader.CommonFiles` - path to MT5 common files folder

`MQL.Sources` - path to MQL folder where your MQL robots stored

To install application in Windows Service mode, build project under Windows, go to bin folder and run command  (under Administrator privileges): `FinCore.exe install`

If you have problems running check `FinCore.MainServer.log` to see errors.

FinCore folders structure:

[/BusinessLogic](/BusinessLogic) - main app logic

[/BusinessObjects](/BusinessObjects) - shared business objects

[/FinCore](/FinCore) - main server self host and WebAPI controllers

[/ClientApp](/FinCore/ClientApp) - Angular client application

[/MQL5](/MQL5) - MQL5 executables that needs to be installed in Metatrader to be able to synchronize and work with Metatrader.

## Documentation

<ul>
  <li>FinCore Features</li>
    <p></p>
    <p><a href="#doc_about">About FinCore</a></p>
    <p><a href="#doc_dashboard">Realtime Dashboard</a></p>
    <p><a href="#doc_wallets">Wallets book</a></p>
    <p><a href="#doc_mt">Metatrader integration</a></p>
    <p><a href="#doc_advisers">Multiple Terminals/Brokers and Advisers Management</a></p>
    <p><a href="#doc_metasymbols">Metasymbols management</a></p>
    <p><a href="#doc_rates">Realtime exchange rates</a></p>
    <p><a href="#doc_instruments">Trading Statistics and Risk management</a></p>
    <p><a href="#doc_deals">Deals history</a></p>
    <p><a href="#doc_log">Application logs</a></p>
    <p><a href="#doc_investments">Investments Pie</a></p>
    <p><a href="#doc_jobs">Background Jobs</a></p>
    <p><a href="#doc_performance">Earning/Spending Performance per day/month</a></p>
    <p><a href="#doc_capital">Yearly Capital State</a></p>
  <li><a href="#doc_install">Installation</a></li>
</ul>

## Installation

  <p id="doc_install"></p>
  Generally application should be installed on your Virtual Private Server (VPS). Where your trading/investment terminals run.
  FinCore application is completely crossplatform.
  You can biuld it for Windows, MacOS, Linux/Docker and run it there. <br>
  There are 3 options to run FinCore application:<br>
  
  1. As a Windows Service
  2. As an usual console application
  3. As a Docker image in a Docker container

  When application started it becomes accessible by this link: http://localhost:2020/#/login or http://localhost:2020/#/dashboard <br>
  Websockets port uses port `2021`.
  Make sure ports `2020` and `2021` are opened to make FinCore accesible outside of VPS.
  If you run application in docker on a separate address to connect it to Windows where Metatrader terminals live - you should activate ports forwarding in Windows:<br>
  Run this command in windows command line:<br>
  `netsh interface portproxy add v4tov4 listenport=2020 listenaddress=127.0.0.1 connectport=2020 connectaddress='docker container address'`
  `netsh interface portproxy add v4tov4 listenport=2022 listenaddress=127.0.0.1 connectport=2022 connectaddress='docker container address'`

  After that all experts in Metatrader will use be able to synchronize with FinCore app running in Docker or on another machine/OS.
  
  To expose Fincore application through NGINX webserver add the following text in nginx.conf file under server { } section : 
    `location ^~ /fincore/ {

        proxy_pass      http://127.0.0.1:2020/;
        
        proxy_set_header Host $host;
        
        proxy_set_header   X-Real-IP        $remote_addr;
        
        proxy_set_header   X-Forwarded-For  $proxy_add_x_forwarded_for;
        
    }` 

  Default login for fincore_empty file is: <br>mail: `test@test.com`<br>
  password: `test`
  <br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/login.png"/>
  <br>
  <p id="doc_wallets">Wallets book</p>
  
  <img src="/FinCore/ClientApp/src/assets/img/doc/wallets.jpg"/>
  <br>
  <p id="doc_dashboard">Realtime Dashboard</p>
  Dashboard shows current positions and investments performance in realtime. Used high performance websocket driven engine to update instruments in realtime.
  <br>
  Mobile friendly and no need to login to your VPS and focus in terminal apps to see all positions. This is very convenient to have an eye on your assets and trading anytime.
  <br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/dashboard.png"/>
  <p>Settings</p>
  <img src="/FinCore/ClientApp/src/assets/img/doc/settings.png"/>
  <p>Adding Adviser in Metatrader</p>
  To add Adviser on a chart just right click and selet Objective template. FinCore will make all the rest. <br>
  But before adding template make sure you have added this symbol into <a href="#doc_metasymbols">Metasymbols management</a> screen.<br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/add_adviser.png"/>
  <br>
  <p id="doc_mt">Metatrader Settings</p>
  Expert Adviser interacts with app server through WebAPI.
  <br>
  To make Objective Expert Adviser work you need to set the following settings in Metatrader settings:
  <br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/mt5settings.png"/>
  <p>Adviser</p>
  Adviser has a panel in top left corner of the chart. Adviser settings can be edited on this screen <a href="#doc_advisers">Advisers Management</a><br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/adviser.png"/>
  <br>
  <p id="doc_advisers">Multiple Terminals/Brokers and Advisers Management</p>
  <img src="/FinCore/ClientApp/src/assets/img/doc/adviser_properties.png"/>
  <br>
  <p id="doc_capital">Yearly Capital State</p>
  Summary Capital state through the year<br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/capital.png"/>
  <br>
  <p id="doc_instruments">Trading Statistics and Risk management</p>
  Useful screen for trader/investor. Here you can see which instruments perform better and select favourite ones for your next trade/investment.<br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/instruments.png"/>
  <br>
  <p id="doc_investments">Investments Pie</p>
  Observe your investment portfolio pie on this screen.
  <img src="/FinCore/ClientApp/src/assets/img/doc/investments.png"/>
  <br>
  <p id="doc_log">Application logs</p>
    Colorful logging from all trading terminals in one log roll. No need to open each terminal log.<br>
    All problems and events can be seen on this page in FinCore app.<br>
  <br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/logs.png"/>
  <br>
  <p id="doc_performance">Earning/Spending Performance per day/month</p>
  This very useful screen shows how you earn/spend money in life and on market.
  <img src="/FinCore/ClientApp/src/assets/img/doc/performance.png"/>
  <br>  
  <p id="doc_deals">Deals history</p>
  Completed/closed trades history.<br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/deals.png"/>
  <br>
  <p id="doc_jobs">Background Jobs</p>
  View and control Jobs. Here you can call various duty jobs.
  Jobs schedule implemented using Quartz library and set using cron expressions in DB.
  <br>
  <img src="/FinCore/ClientApp/src/assets/img/doc/jobs.png"/>
  <br>
  <p id="doc_metasymbols">Metasymbols management</p>
  To Add new Symbol to FinCore add new MetaSymbol and then Add new Symbol on this screen. Then Adviser can be created for this symbol.
  <img src="/FinCore/ClientApp/src/assets/img/doc/metasymbols.png"/>
  <br>
  <p id="doc_rates">Realtime exchange rates</p>
  To Update Exchange rates open and connect your Metatrader terminal with Fincore and start ExhangeRatesJob in <a href="#doc_jobs">Background Jobs</a>
  <img src="/FinCore/ClientApp/src/assets/img/doc/rates.png"/>
  <br>
