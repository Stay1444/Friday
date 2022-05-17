CREATE TABLE IF NOT EXISTS `bak_role_cooldown` (
    `id` bigint(64) NOT NULL UNIQUE,
    `date` DATETIME NOT NULL,
    `count` bigint(64) NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE TABLE IF NOT EXISTS `bak_backups` (
    `id` bigint(64) NOT NULL UNIQUE,
    `code` varchar(64) NOT NULL,
    `data` text NOT NULL,
    `owner` bigint(64) NOT NULL
);

CREATE TABLE IF NOT EXISTS `bak_guild_settings` (
    `id` bigint(64) NOT NULL UNIQUE,
    `admins_can_backup` tinyint(1) NOT NULL DEFAULT 0,
    `admins_can_restore` tinyint(1) NOT NULL DEFAULT 0,
    `interval` bigint(64) NOT NULL DEFAULT 0,
    `max_backups` bigint(64) NOT NULL DEFAULT 5,
    PRIMARY KEY (`id`)
);

