#!/usr/bin/env bash

{% if is_rpi %}
echo "system,component=cpu temperature=`cat /sys/class/thermal/thermal_zone0/temp`"
echo "system,component=cpu frequency=`cat /sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_cur_freq`"
{% endif %}
