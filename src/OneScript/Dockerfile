FROM evilbeaver/mono-ru:5.4

ARG PREFIX=/var/osp.net

ADD bin/Release/PublishOutput $PREFIX/
RUN cp $PREFIX/runtimes/debian-x64/native/*.so /usr/lib

EXPOSE 5000

ENTRYPOINT mono /var/osp.net/OneScript.WebHost.exe