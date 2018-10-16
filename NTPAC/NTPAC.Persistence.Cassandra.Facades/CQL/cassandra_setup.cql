CREATE KEYSPACE IF NOT EXISTS ntpac WITH replication = {'class':'SimpleStrategy', 'replication_factor' : 1};
USE ntpac;
CREATE TYPE IF NOT EXISTS l7pduentity (
    Direction tinyint,
    FirstSeenTicks bigint,
    LastSeenTicks bigint,
    Payload blob,
);
CREATE TYPE IF NOT EXISTS ipendpointentity (
    address inet,
    port int,
);