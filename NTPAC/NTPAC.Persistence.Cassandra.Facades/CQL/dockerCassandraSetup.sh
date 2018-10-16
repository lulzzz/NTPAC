#!/bin/sh

if [ "$#" -eq 0 ]
then
	echo "./setupCassandra EXPECTED_NODES"
	exit 1
fi
EXPECTED_NODES=$1

echo "Dropping keyspace"
echo "DROP KEYSPACE IF EXISTS ntpac;" | docker exec -i  `docker ps -q -f name=cassandra-seed-node` cqlsh 

date
while : ;
do
	UP_NODES="$(docker exec -i `docker ps -q -f name=cassandra-seed-node` nodetool status | grep '^UN' | wc -l)"
    [ "$UP_NODES" -eq "$EXPECTED_NODES" ] && break
    echo -en "\033[0KWaiting for $EXPECTED_NODES nodes ($UP_NODES UP)\r"
    sleep 1
done
echo "All Cassandra nodes UP"
date


CQL=`cat << EOF
CREATE KEYSPACE ntpac WITH replication = {'class':'SimpleStrategy', 'replication_factor' : 1};

CREATE TYPE ntpac.l7pduentity (
    Direction tinyint,
    FirstSeenTicks bigint,
    LastSeenTicks bigint,
    Payload blob,
);
CREATE TYPE ntpac.ipendpointentity (
    address inet,
    port int,
);

CREATE TABLE ntpac.captureentity (
    id uuid PRIMARY KEY,
    firstseen timestamp,
    l7conversationcount int,
    lastseen timestamp,
    processed timestamp,
    reassembleraddress text,
    uri text
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE TABLE ntpac.l7conversationentity (
    id uuid PRIMARY KEY,
    captureid uuid,
    destinationendpoint ipendpointentity,
    firstseen timestamp,
    lastseen timestamp,
    pdus frozen<list<frozen<l7pduentity>>>,
    protocoltype int,
    sourceendpoint ipendpointentity
) WITH bloom_filter_fp_chance = 0.01
    AND caching = {'keys': 'ALL', 'rows_per_partition': 'NONE'}
    AND comment = ''
    AND compaction = {'class': 'org.apache.cassandra.db.compaction.SizeTieredCompactionStrategy', 'max_threshold': '32', 'min_threshold': '4'}
    AND compression = {'chunk_length_in_kb': '64', 'class': 'org.apache.cassandra.io.compress.LZ4Compressor'}
    AND crc_check_chance = 1.0
    AND default_time_to_live = 0
    AND gc_grace_seconds = 864000
    AND max_index_interval = 2048
    AND memtable_flush_period_in_ms = 0
    AND min_index_interval = 128
    AND read_repair_chance = 0.0
    AND speculative_retry = '99PERCENTILE';

CREATE INDEX l7conversationentity_captureid_idx ON ntpac.l7conversationentity (captureid);
EOF`

echo "Creating keyspace and schema"
echo $CQL | docker exec -i  `docker ps -q -f name=cassandra-seed-node` cqlsh 

echo "Done"