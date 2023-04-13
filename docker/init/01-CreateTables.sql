CREATE TABLE `Config` (
	`Category` VARCHAR(64) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Key` VARCHAR(64) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Collection` BIT(1) NOT NULL DEFAULT b'0',
	`Host` VARCHAR(32) NOT NULL DEFAULT '*' COLLATE 'utf8mb3_general_ci',
	`Type` VARCHAR(16) NOT NULL DEFAULT 'string' COLLATE 'utf8mb3_general_ci',
	`Value` TEXT NOT NULL DEFAULT '' COLLATE 'utf8mb3_general_ci',
	INDEX `Host` (`Host`) USING BTREE,
	INDEX `Type` (`Type`) USING BTREE,
	INDEX `Category` (`Category`) USING BTREE,
	INDEX `Collection` (`Collection`) USING BTREE
)
COLLATE='utf8mb3_general_ci'
ENGINE=InnoDB;

CREATE TABLE `State` (
	`Category` VARCHAR(64) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Key` VARCHAR(64) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Host` VARCHAR(32) NOT NULL DEFAULT '*' COLLATE 'utf8mb3_general_ci',
	`Type` VARCHAR(16) NOT NULL DEFAULT 'string' COLLATE 'utf8mb3_general_ci',
	`Value` TEXT NOT NULL DEFAULT '' COLLATE 'utf8mb3_general_ci',
	INDEX `Host` (`Host`) USING BTREE,
	INDEX `Type` (`Type`) USING BTREE,
	INDEX `Category` (`Category`) USING BTREE
)
COLLATE='utf8mb3_general_ci'
ENGINE=InnoDB;

CREATE TABLE `Jobs` (
	`JobID` INT(11) NOT NULL AUTO_INCREMENT,
	`Enabled` BIT(1) NOT NULL DEFAULT b'1',
	`Host` VARCHAR(32) NOT NULL DEFAULT 'utc_timestamp(6)' COLLATE 'utf8mb4_general_ci',
	`JobName` VARCHAR(128) NOT NULL COLLATE 'utf8mb4_general_ci',
	`NextRun` DATETIME NOT NULL DEFAULT utc_timestamp(6),
	`LastRun` DATETIME NOT NULL DEFAULT utc_timestamp(6),
	PRIMARY KEY (`JobID`) USING BTREE,
	INDEX `Enabled` (`Enabled`) USING BTREE,
	INDEX `Host` (`Host`) USING BTREE
)
COLLATE='utf8mb4_general_ci'
ENGINE=InnoDB;

CREATE TABLE `Credentials` (
	`Host` VARCHAR(32) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Name` VARCHAR(64) NOT NULL COLLATE 'utf8mb3_general_ci',
	`Deleted` BIT(1) NOT NULL DEFAULT b'0',
	`Credentials` TEXT NOT NULL COLLATE 'utf8mb3_general_ci',
	INDEX `Host` (`Host`) USING BTREE,
	INDEX `Deleted` (`Deleted`) USING BTREE
)
COLLATE='utf8mb3_general_ci'
ENGINE=InnoDB;

CREATE TABLE `SshCommands` (
	`CommandID` INT(11) NOT NULL AUTO_INCREMENT,
	`Enabled` BIT(1) NOT NULL DEFAULT b'0',
	`Host` VARCHAR(32) NOT NULL COLLATE 'utf8mb4_general_ci',
	`CredentialName` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`LastRun` DATETIME NOT NULL DEFAULT utc_timestamp(6),
	`NextRun` DATETIME NOT NULL DEFAULT utc_timestamp(6),
	`JobID` VARCHAR(64) NOT NULL DEFAULT '' COLLATE 'utf8mb4_general_ci',
	`JobName` VARCHAR(256) NOT NULL COLLATE 'utf8mb4_general_ci',
	`ScheduleExpression` VARCHAR(256) NOT NULL DEFAULT '' COLLATE 'utf8mb4_general_ci',
	PRIMARY KEY (`CommandID`) USING BTREE,
	INDEX `Enabled` (`Enabled`) USING BTREE,
	INDEX `Host` (`Host`) USING BTREE
)
COLLATE='utf8mb4_general_ci'
ENGINE=InnoDB;

CREATE TABLE `SshCommandsActions` (
	`ActionID` INT(11) NOT NULL AUTO_INCREMENT,
	`JobID` VARCHAR(64) NOT NULL COLLATE 'utf8mb4_general_ci',
	`RunOrder` INT(11) NOT NULL DEFAULT '0',
	`Host` VARCHAR(32) NOT NULL COLLATE 'utf8mb4_general_ci',
	`StopOnError` BIT(1) NOT NULL DEFAULT b'1',
	`Command` TEXT NOT NULL DEFAULT '' COLLATE 'utf8mb4_general_ci',
	PRIMARY KEY (`ActionID`) USING BTREE,
	INDEX `Host` (`Host`) USING BTREE
)
COLLATE='utf8mb4_general_ci'
ENGINE=InnoDB;
