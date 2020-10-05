#!/bin/sh

# configure route for VPN
/sbin/ip route add {{ openvpn.conf.server.v4.subnet }} via {{ openvpn.pihole_ipv4 }}

# start pihole
/s6-init
