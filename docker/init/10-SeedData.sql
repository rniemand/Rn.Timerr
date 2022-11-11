INSERT INTO `Config`
	(`Category`,`Key`,`Host`,`Type`,`Value`)
VALUES
	('BackupSatisfactory',  'Source',            'Dev',  'string',  '\\192.168.0.60\appdata\satisfactory-server\saves'),
	('BackupSatisfactory',  'Destination',       'Dev',  'string',  '\\192.168.0.60\Backups\Satisfactory'),
	('BackupSatisfactory',  'BackupFileName',    'Dev',  'string',  'Satisfactory-{yyyy}{mm}{dd}-{hh}'),
	('BackupSatisfactory',  'ManageSaveRx',      'Dev',  'string',  '^Satisfactory-.*'),
	('BackupSatisfactory',  'OverwriteExisting', 'Dev',  'bool',    'true'),
	('BackupSatisfactory',  'ManageSaves',       'Dev',  'bool',    'true'),
	('BackupSatisfactory',  'TickIntervalMin',   'Dev',  'int',     '1');
