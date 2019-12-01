#! /usr/bin/env python3

from vcgencmd import *
import sty as cli_color

import psutil as ps
import os
import sys
from time import sleep
import builtins

process = ps.Process(os.getpid())
interval = 1


def print(*args, **kwargs):
    kwargs['end'] = ''
    builtins.print(*args, **kwargs)
    builtins.print('\033[K')


def unhide_cursor():
    print('\e[?25h', end='')


def hide_cursor():
    print('\e[?25l', end='')


def move_cursor_to_origin():
    print('\033[;H', end='')


def bold(text):
    return cli_color.ef.bold + text + cli_color.rs.all


def clear():
    os.system('clear')


def main():
    hide_cursor()
    clear()

    while True:
        move_cursor_to_origin()
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

        print(
            bold('\nThis Process:'),
            f'CPU: {process.cpu_percent()}%',
            f'Memory: {process.memory_info().rss/1024/1024:.2f} MB')
        sleep(interval)


if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print('exiting ...')
        unhide_cursor()
