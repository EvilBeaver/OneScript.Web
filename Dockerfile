FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy everything else and build

COPY . ./
RUN dotnet restore OneScript/OneScriptWeb.csproj
RUN dotnet publish OneScript/OneScriptWeb.csproj -c Release -o /app/out -f netcoreapp3.1 -r debian-x64

# RUNTIME
#FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
FROM ubuntu:18.04

RUN apt update
RUN apt install -y --no-install-recommends liblttng-ust0 libcurl4 libssl1.0.0 libkrb5-3 zlib1g libicu60 && \
    rm -rf /var/lib/apt/lists/*

ENV LANG ru_RU.UTF-8

ENV BINPREFIX=/var/osp.net
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

WORKDIR /app
COPY --from=build-env /app/out /var/osp.net
ENTRYPOINT ["/var/osp.net/OneScript.WebHost"]

EXPOSE 5000