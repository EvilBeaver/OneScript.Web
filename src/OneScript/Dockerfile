FROM mono:5.10

ENV LANG ru_RU.UTF-8

ARG binaries=bin/Release/net461/publish

ENV BINPREFIX=/var/osp.net
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

ADD ${binaries} $BINPREFIX/
RUN cp $BINPREFIX/runtimes/debian-x64/native/*.so /usr/lib

EXPOSE 5000

WORKDIR $BINPREFIX
ENTRYPOINT mono $BINPREFIX/OneScript.WebHost.exe