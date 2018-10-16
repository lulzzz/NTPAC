#!/usr/bin/env bash

NTPAC_NODE="${NTPAC_NODE:-`hostname`}"
NTPAC_SEED_NODE="${NTPAC_SEED_NODE:-`hostname`}"

set -x
exec dotnet NTPAC.LoadBalancerCli.dll -h "$NTPAC_NODE" -s "$NTPAC_SEED_NODE" "$@"
