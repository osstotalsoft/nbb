FROM mono:5.14 AS docfx-image
ENV DOCFX_VER 2.37.2
#fix https://github.com/mono/docker/issues/73
RUN apt-get update -y; exit 0
RUN apt-get install unzip wget git -y && \
    wget -q -P /tmp https://github.com/dotnet/docfx/releases/download/v${DOCFX_VER}/docfx.zip && \
    mkdir -p /opt/docfx && \
    unzip /tmp/docfx.zip -d /opt/docfx && \
    echo '#!/bin/bash\nmono /opt/docfx/docfx.exe $@' > /usr/bin/docfx && \
    chmod +x /usr/bin/docfx && \
    rm -f /tmp/*

FROM docfx-image AS builder
WORKDIR /build/src
COPY src .
WORKDIR /build/docs/docfx
COPY docs/DocFx .
RUN docfx --build

FROM node:8
COPY --from=builder /build/docs/docfx/_site ./build
RUN npm install -g serve@6
EXPOSE 80
CMD serve -s build -p 80
