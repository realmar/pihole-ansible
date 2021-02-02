#!/usr/bin/env python3

import argparse
import importlib.util
import sys


keepass_install_dir = '/mnt/c/Program Files (x86)/KeePass Password Safe 2'
keepass_py = keepass_install_dir + '/KeePassEntry.py'


def get_vault_id():
    parser = argparse.ArgumentParser(
        description='Retrieves the password for a given vault-id from keepass2 using KeePassCommander')

    parser.add_argument('--vault-id ', dest='vault_id', required=True,
                        help='The vault-id for which the password is retrieved')

    args = parser.parse_args()
    return args.vault_id


def get_keepass_entry_function():
    spec = importlib.util.spec_from_file_location(
        'keepass_entry_module', keepass_py)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)

    return module.KeePassEntry


if __name__ == '__main__':
    vault_id = get_vault_id()
    keepass_entry = get_keepass_entry_function()

    entry = keepass_entry(f'ansible-vault-{vault_id}')
    password = entry.get('password')

    if not (password and password.strip()):
        sys.exit(f'Cannot find password for vault-id {vault_id}')

    sys.stdout.write(password)
