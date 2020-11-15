#!/usr/bin/env bash

ansible all -i inventory_dev.ini -a "systemctl reboot"
