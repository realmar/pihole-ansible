#! /usr/bin/env python3

from vcgencmd import *
import sty as cli_color

def bold(text):
    return cli_color.ef.bold + text + cli_color.rs.all

print(bold('Frequencies\n'))

for source in 'arm', 'core':
    value = measure_clock(source)
    print(f'{source}: {value/1000/1000:.2f} MHz')

print(bold('\nVoltages\n'))

for source in voltage_sources():
    value = measure_volts(source)
    print(f'{source}: {value} V')

title = bold('\nTemperature:')
print(f'{title} {measure_temp()} C°')
