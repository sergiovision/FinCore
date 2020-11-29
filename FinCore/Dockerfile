#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 2020 2021
ENV ASPNETCORE_ENVIRONMENT Production

RUN  apt-get update \
  && apt-get install -y wget \
  && apt-get install -y mc \
  && apt-get install -y htop \
  && apt-get install -y nano


FROM node:13.13.0-alpine AS client
ARG skip_client_build=false
WORKDIR /app
COPY FinCore/ClientApp .
RUN [[ ${skip_client_build} = true ]] && echo "Skipping npm install" || npm i -f
RUN [[ ${skip_client_build} = true ]] && mkdir dist || npm run docker


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /
COPY DB/ DB/
WORKDIR /src
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
COPY --from=publish /app/publish .
COPY --from=client /app/dist /app/dist
COPY --from=build /src/FinCore/appsettings.Docker.json /app/appsettings.json
ENTRYPOINT ["dotnet", "FinCore.dll"]
