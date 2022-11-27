FROM mcr.microsoft.com/dotnet/sdk:6.0

ENV APTLIST="wget curl nano git"

VOLUME ["/etc/zge/", "/var/log/zge/"]

RUN apt-get -yqq update && \
    apt-get -yqq install $APTLIST && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/* 

ENTRYPOINT [ "/bin/friday/Friday" ]
