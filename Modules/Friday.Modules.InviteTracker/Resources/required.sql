CREATE TABLE IF NOT EXISTS `invite_tracker` (
    `id` bigint(64) NOT NULL UNIQUE,
    `settings` text NOT NULL,
    PRIMARY KEY (`id`)
);