FROM ubuntu:latest
RUN apt update && apt install -y dhcp-helper
EXPOSE 67 67/udp
ENTRYPOINT ["dhcp-helper", "-n"]
