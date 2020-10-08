#!/usr/bin/env python3

if __name__ == "__main__":
    import os
    import json
    import base64
    import argparse

    parser = argparse.ArgumentParser()
    parser.add_argument('acme', type=str)
    parser.add_argument('dest', type=str)

    args = parser.parse_args()

    acme_path = args.acme
    dest_path = args.dest

    with open(acme_path) as f:
        data = json.loads(f.read())

    bundles = data['lets_encrypt']['Certificates']

    for bundle in bundles:
        domain = bundle['domain']['main']
        cert_b64 = bundle['certificate']
        key_b64 = bundle['key']

        cert = base64.decodebytes(cert_b64.encode('utf-8')).decode('utf-8')
        key = base64.decodebytes(key_b64.encode('utf-8')).decode('utf-8')

        with open(os.path.join(dest_path, domain + '.crt'), 'w') as f:
            f.write(cert)

        with open(os.path.join(dest_path, domain + '.pem'), 'w') as f:
            f.write(key)
