FROM mono:5.10

ENV LANG ru_RU.UTF-8

ARG binaries=artifact/net461/debian-x64

ENV BINPREFIX=/var/osp.net
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

ADD ${binaries} $BINPREFIX/

EXPOSE 5000

VOLUME [ "/app" ]
WORKDIR /app

ENTRYPOINT mono $BINPREFIX/OneScript.WebHost.exe