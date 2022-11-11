CREATE TABLE `Config` (
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
