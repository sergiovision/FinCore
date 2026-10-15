#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 2020 2021 2022
ENV ASPNETCORE_ENVIRONMENT=Production


FROM node:24-alpine AS client
ARG skip_client_build=false
WORKDIR /app
COPY FinCore/ClientApp .
RUN if [ "$skip_client_build" = "true" ]; then mkdir -p dist; else npm ci && npm run docker; fi


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json .
COPY FinCore/FinCore.csproj FinCore/
COPY BusinessLogic/BusinessLogic.csproj BusinessLogic/
COPY BusinessObjects/BusinessObjects.csproj BusinessObjects/
RUN dotnet restore "FinCore/FinCore.csproj"
COPY . .
WORKDIR "/src/FinCore"
RUN dotnet build "FinCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinCore.csproj" -c Release -p:IsDocker=True -o /app/publish

FROM base AS final
WORKDIR /app
# the app writes its log file next to the binaries and the SQLite DB lives in /DB (see appsettings.Docker.json)
COPY --chown=app:app --from=publish /app/publish .
COPY --chown=app:app --from=client /app/dist /app/dist
COPY --chown=app:app --from=build /src/FinCore/appsettings.Docker.json /app/appsettings.json
COPY --chown=app:app DB/ /DB/
RUN chown app:app /app
USER app
ENTRYPOINT ["dotnet", "FinCore.dll"]
