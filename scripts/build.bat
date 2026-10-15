@echo off
rem Publishes the server (and builds the Angular client) into .\bin
cd /d "%~dp0.."
dotnet publish FinCore\FinCore.csproj -c Release -o .\bin
