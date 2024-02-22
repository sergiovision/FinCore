<img src="/FinCore/ClientApp/src/assets/img/brand/logo.png" width="100"/>

# About FinCore
<p id="doc_about">
</p>
  New version 0.9.18 released. 
  FinCore is a cool and free cross-platform personal finances manager written in .NET 8 with Angular 14 frontend. <br>

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

  **This version is free of charge but if you want a better and secured version you can write me to hi@sergego.com we can talk about improving/installation/support and agree about $.**
  <br>

## How to build:
1. Clone this repository
2. Build Client 

App. 
  Go to FinCore/ClientApp folder 
  Run: `npm install`
  For UI Debug run: : `npm run start` and use URL http://127.0.0.1:4200 for running debug version of UI

3. Build Whole application 
  To build as a Windows Service or Console app:
  Run from command line: `build.bat`
  To build as a docker image:
  Run: `docker.sh`

4. If you fail to build or do not want to build on your machine then there is an option to get docker image from my docker hub. Run this commands:

`git clone https://github.com/sergiovision/FinCore.git`

`docker pull dockersergio/fincore:latest`

`docker-compose up`

To make build succeeded the following apps should be installed: Visual Studio 2019, Visual Studio 2019 Build tools, latest NPM from http://nodejs.org.
Applications need to be installed to run server properly: 

1. Metatrader 5 Terminal
2. Optionally MySQL Server version 5 or later.
3. .NET Core 8
4. Framework 4.8 (for dlls in Metatrader5) should be installed
  

SQLite database located in `/DB` folder. By default SQLite DB file used, but MySQL also supported, you can switch to MySQL in `/FinCore/appSettings.json` file.

For configuring crypto module setup Api Keys for KuCoin and/or FTX providers:

   For KuCoin exchange set the following properties in `/FinCore/appSettings.json`:
  `KuCoinAPIKey`  - KuCoin Main API Key
  `KuCoinAPISecret` - API Secret
  `KuCoinPassPhrase` - Pass Phrase
  
  `KuCoinFutureAPIKey` - KuCoin Futures API Key
  `KuCoinFutureAPISecret` - API Secret
  `KuCoinFuturePassPhrase` - Futures Pass Phrase

   For FTX Exchange:
  `FTXAPIKey` - FTX API Key
  `FTXAPISecret` - FTX API Secret


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
