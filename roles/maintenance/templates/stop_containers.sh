#!/bin/sh

{{ duplicati.volumes.config.mount }}/bh stop -e {{ backup_helper.exclude|join(' ') }}
