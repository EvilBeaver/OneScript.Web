FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ENV LANG ru_RU.UTF-8

ENV BINPREFIX=/var/osp.net
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

WORKDIR /app
COPY . /var/osp.net
ENTRYPOINT ["/var/osp.net/OneScript.WebHost"]

EXPOSE 5000