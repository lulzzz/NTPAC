FROM cassandra:3.11.3 as base
COPY cassandra.yml /etc/cassandra/cassandra.yaml
COPY ntpac-docker-entrypoint.sh /
ENTRYPOINT ["/ntpac-docker-entrypoint.sh"]
