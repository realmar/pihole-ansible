#!/usr/bin/env python3

import argparse
import re
import subprocess
import sys
import time


def trytimeout(f, timeout=360):
    start = time.time()
    end = time.time()

    while (end - start) < timeout:
        result = f()

        if result == 0:
            break

        print(f'Retrying {f}', file=sys.stderr)

        time.sleep(1)
        end = time.time()

    if result > 0:
        print(f'Failed to execute {f}', file=sys.stderr)

    return result


def is_mounted():
    result = subprocess.run(['mount'], stdout=subprocess.PIPE)
    if result.returncode == 0 and result.stdout.decode('utf-8').find('{{ data_mountpoint }}') > -1:
        return True

    return False


def mount_ceph():
    def f():
        if is_mounted():
            print('already mounted')
            return 0

        result = subprocess.run([
            'mount', '-t', 'ceph',
            '-o', 'name=fs,secretfile={{ ceph_fs.secret_path }}',
            ':/', '{{ data_mountpoint }}'], stdout=subprocess.PIPE)

        if result.returncode == 0:
            return 0
        else:
            return 1

    return trytimeout(f)


def umount_ceph():
    if is_mounted() == False:
        print("not mounted")
        return 0

    result = subprocess.run(
        ['umount', '{{ data_mountpoint }}'], stdout=subprocess.PIPE)
    return result.returncode


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--mount', action='store_true', default=False)
    parser.add_argument('--umount', action='store_true', default=False)
    args = parser.parse_args()

    if args.mount:
        mount_f = mount_ceph
    elif args.umount:
        mount_f = umount_ceph
    else:
        print(f'You either need to specify mount or umount', file=sys.stderr)
        sys.exit(2)

    def f():
        result = subprocess.run(['ceph', 'status'], stdout=subprocess.PIPE)
        output = result.stdout.decode('utf-8')

        if output.find('HEALTH_OK') > -1 or output.find('HEALTH_WARN') > -1:
            return mount_f()
        else:
            return 1

    sys.exit(trytimeout(f))
