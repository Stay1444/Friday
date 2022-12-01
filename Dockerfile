FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

COPY . /tmp/friday

RUN dotnet restore /tmp/friday/Friday.sln
RUN dotnet build --configuration Release /tmp/friday/Friday/Friday.csproj

FROM mcr.microsoft.com/dotnet/runtime:6.0

ENV APTLIST="nano"

RUN apt-get -yqq update && \
    apt-get -yqq install $APTLIST && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/* 

VOLUME ["/var/lib/friday/"]

RUN mkdir -p /bin/friday

COPY --from=builder /tmp/friday/Friday/bin/Release/net6.0/ /bin/friday/

RUN chmod +x /bin/friday/Friday

WORKDIR /var/lib/friday/

ENTRYPOINT [ "/bin/friday/Friday" ]
