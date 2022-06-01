CREATE TABLE IF NOT EXISTS `mod_user_bans` (
    `user_id` bigint(64) NOT NULL,
    `guild_id` bigint(64) NOT NULL,
    `banned_by` bigint(64) NOT NULL,
    `reason` TEXT NOT NULL,
    `banned_at` DATETIME NOT NULL,
    `expires_at` DATETIME NOT NULL
)