#!/usr/bin/env bash

NTPAC_NODE="${NTPAC_NODE:-`hostname`}"
NTPAC_SEED_NODE="${NTPAC_SEED_NODE:-`hostname`}"

set -x
# Give seed node a head start
sleep 2
exec dotnet NTPAC.ReassemblerCli.dll -h "$NTPAC_NODE" -s "$NTPAC_SEED_NODE" "$@"
