-- ===============================================================================================================================
-- BackupSatisfactory
-- ===============================================================================================================================
INSERT INTO `Config`
	(`Category`,`Key`,`Host`,`Type`,`Value`)
VALUES
	('BackupSatisfactory',  'Source',            'Dev',  'string',  '\\\\192.168.0.60\\appdata\\satisfactory-server\\saves'),
	('BackupSatisfactory',  'Destination',       'Dev',  'string',  '\\\\192.168.0.60\\Backups\\Satisfactory'),
	('BackupSatisfactory',  'BackupFileName',    'Dev',  'string',  'Satisfactory-{yyyy}{mm}{dd}-{hh}'),
	('BackupSatisfactory',  'ManageSaveRx',      'Dev',  'string',  '^Satisfactory-.*'),
	('BackupSatisfactory',  'OverwriteExisting', 'Dev',  'bool',    'true'),
	('BackupSatisfactory',  'ManageSaves',       'Dev',  'bool',    'true'),
	('BackupSatisfactory',  'TickIntervalMin',   'Dev',  'int',     '1');

INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('BackupSatisfactory', 'Dev', 0);

-- ===============================================================================================================================
-- BackupSonarQube
-- ===============================================================================================================================
INSERT INTO `Config`
	(`Category`,`Key`,`Host`,`Type`,`Value`)
VALUES
	('BackupSonarQube', 'SqlConnection', 'Dev', 'string',  'Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;'),
	('BackupSonarQube', 'ssh.creds',     'Dev', 'string',  'SshNas');

INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('BackupSonarQube', 'Dev', 1);

-- ===============================================================================================================================
-- BackupObsidian
-- ===============================================================================================================================
INSERT INTO `Config`
	(`Category`,`Key`,`Host`,`Type`,`Value`)
VALUES
	('BackupObsidian', 'ssh.creds', 'Dev', 'string',  'SshNas');

INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('BackupObsidian', 'Dev', 1);


-- ===============================================================================================================================
-- VerifyMariaDbBackups
-- ===============================================================================================================================
INSERT INTO `Config`
	(`Category`,`Key`,`Host`,`Type`,`Value`)
VALUES
	('VerifyMariaDbBackups', 'configFile',      'Dev', 'string',  '\\\\192.168.0.60\\Backups\\app-data\\rn-timerr\\job-config\\VerifyMariaDbBackups\\config.json'),
	('VerifyMariaDbBackups', 'NextRunTemplate', 'Dev', 'string',  'yyyy-MM-ddT09:00:00.0000000-07:00');

INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('VerifyMariaDbBackups', 'Dev', 1);

-- ===============================================================================================================================
-- BackupAppData
-- ===============================================================================================================================
INSERT INTO `Config`
	(`Category`,`Key`, `Collection`,`Host`,`Type`,`Value`)
VALUES
	('BackupAppData', 'backupDestRoot', 0, 'Dev', 'string',  '/mnt/user/Backups/app-data-backups'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/binhex-nginx'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/dns-updater'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/feed-scraper'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/grafana'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/mosquitto'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/my-home'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/nas-file-indexer'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/nas-landing-page'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/rn-go'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/rn-timerr'),
	('BackupAppData', 'directory',      1, 'Dev', 'string',  '/mnt/user/Backups/app-data/teletgraf'),
	('BackupAppData', 'ssh.creds',      0, 'Dev', 'string',  'SshNas');

INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('BackupAppData', 'Dev', 1);


-- ===============================================================================================================================
-- Run SSH Commands
-- ===============================================================================================================================
INSERT INTO `Jobs` (`JobName`, `Host`, `Enabled`) VALUES ('RunSshCommands', 'Dev', 0);

INSERT INTO `SshCommands`
	(`Enabled`, `Host`, `CredentialName`, `JobID`, `JobName`, `ScheduleExpression`)
VALUES
	(1, 'Dev', 'SshNas', 'chmod-media-12-hours', 'CHMOD Media every 12 hours', 'interval:hours:12@start');

INSERT INTO `SshCommandsActions`
	(`JobID`, `RunOrder`, `Host`, `StopOnError`, `Command`)
VALUES
	('chmod-media-12-hours', 1, 'Dev', 1, 'chmod -R 0777 /mnt/user/Media/');