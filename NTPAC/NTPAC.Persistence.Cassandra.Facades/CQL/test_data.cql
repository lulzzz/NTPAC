INSERT INTO ntpac.captureentity (id, uri) VALUES (f0c671fc-2c09-4b54-ae19-0dcfab744c73, 'blah.pcap');
INSERT INTO ntpac.captureentity (id, uri) VALUES (bcaf356e-ade3-429c-99c7-7d12541c1311, 'blah2.pcap');

-- INSERT INTO ntpac.l7conversationentity (id) VALUES (uuid());

INSERT INTO ntpac.l7conversationentity (id, captureId, sourceEndPoint, destinationEndPoint, protocolType, firstSeen, lastSeen, pdus) VALUES (79355c7e-3a86-422f-ac81-ea63e08441a1, f0c671fc-2c09-4b54-ae19-0dcfab744c73, {address: '192.168.0.123', port: 1234}, {address: '192.168.0.1', port: 80}, 6, '2018-08-16 09:00:00', '2018-08-16 09:30:00', [
	{directionInt: 1, firstSeenTicks: 636700068000000000, payload: textAsBlob('ABCD')},
	{directionInt: 2, firstSeenTicks: 636700077000000000, payload: textAsBlob('EFGH')},
	{directionInt: 1, firstSeenTicks: 636700086000000000, payload: textAsBlob('IJK')}
	]);

