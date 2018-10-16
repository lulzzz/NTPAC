#!/bin/bash

apt update
apt install -y default-jre 

CASSANDRA_HOME="/opt/cassandra"
if [ ! -d "$CASSANDRA_HOME" ]
then
	SAVED_DIR=$(pwd)

	grep -q cassandra /etc/passwd || (echo "Creating user cassandra ..."; useradd cassandra)

	echo "Downloading and installing Cassandra to $CASSANDRA_HOME ..."
	cd /tmp
	wget http://tux.rainside.sk/apache/cassandra/3.11.3/apache-cassandra-3.11.3-bin.tar.gz
	tar -xvzf apache-cassandra-3.11.3-bin.tar.gz
	rm apache-cassandra-3.11.3-bin.tar.gz
	rm -rf  apache-cassandra-3.11.3/javadoc/
	mkdir -p /opt/cassandra
	mv apache-cassandra-3.11.3/* "$CASSANDRA_HOME"
	chown -R cassandra:cassandra "$CASSANDRA_HOME"
	cd "$CASSANDRA_HOME"

	echo "Setting up PATH for user marta ..."
	grep -q "/opt/cassandra/bin" /home/marta/.profile || echo 'PATH="/opt/cassandra/bin:$PATH"' >> /home/marta/.profile 

	echo "Setting up data mnt ..."
	mkdir -p /home/marta/mnt/cassandra
	chown -R cassandra:cassandra /home/marta/mnt/cassandra

	echo "TODO:
1. Copy cassandra.yaml to /opt/cassandra/conf/cassandra.yaml
1b. Configure listen_address in cassandra.yaml
2. Copy cassandra.service to /usr/lib/systemd/system/cassandra.service"

	cd $SAVED_DIR
fi
