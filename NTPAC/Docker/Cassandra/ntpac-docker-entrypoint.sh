#!/usr/bin/env bash

if [ ! -z "$CASSANDRA_SEEDS" ]
then
	# Try to resolve seed host(s)
	RESOLVED_CASSANDRA_SEEDS=$(getent hosts $(echo $CASSANDRA_SEEDS |  tr ',' ' ')) && export CASSANDRA_SEEDS=$(echo "$RESOLVED_CASSANDRA_SEEDS" | awk '{print $1}' | paste -sd "," -)
fi

/docker-entrypoint.sh "$@"