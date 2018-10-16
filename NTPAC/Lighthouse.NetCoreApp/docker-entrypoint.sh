#!/usr/bin/env bash

NTPAC_NODE="${NTPAC_NODE:-`hostname`}"

set -x 
exec dotnet Lighthouse.NetCoreApp.dll -h "$NTPAC_NODE" "$@"
