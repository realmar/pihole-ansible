#!/usr/bin/env python3

import subprocess, sys

def call(command):
    proc = subprocess.Popen(        \
        command,                    \
        stdout=subprocess.PIPE,     \
        shell = True)
    output = proc.communicate()

    return proc.returncode

def execute(command):
    rc = call(f'iptables -C {command}')
    execute_action = rc {{ '== 0' if action == 'D' else '!= 0' }}

    rc = 0
    if execute_action:
        rc = call(f'iptables -{{ action }} {command}')

    return rc

rules = [ \
    'FORWARD -i {{ host.public_interface }} -o {{ openvpn.tun_device_name }} -s {{ host_net.v4.subnet }} -d {{ openvpn.conf.server.v4.subnet }} -j ACCEPT', \
    'FORWARD -i {{ openvpn.tun_device_name }} -o {{ host.public_interface }} -s {{ openvpn.conf.server.v4.subnet }} -d {{ host_net.v4.subnet }} -j ACCEPT' \
]

rc = 0
for rule in rules:
    rc = rc + execute(rule)

sys.exit(rc)
