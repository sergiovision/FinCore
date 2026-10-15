@echo off
rem Restores NuGet and npm packages
cd /d "%~dp0.."
dotnet restore FinCore.sln
cd FinCore\ClientApp
npm ci
