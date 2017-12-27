FROM evilbeaver/mono-ru:5.4

ENV BINPREFIX=/var/osp.net
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000

ADD bin/Release/PublishOutput $BINPREFIX/
RUN cp $BINPREFIX/runtimes/debian-x64/native/*.so /usr/lib

EXPOSE 5000

WORKDIR $BINPREFIX
ENTRYPOINT mono $BINPREFIX/OneScript.WebHost.exe