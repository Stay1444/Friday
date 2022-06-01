CREATE TABLE IF NOT EXISTS `mod_user_bans` (
    `user_id` int(64) NOT NULL,
    `guild_id` int(64) NOT NULL,
    `banned_by` int(64) NOT NULL,
    `reason` TEXT NOT NULL,
    `banned_at` DATETIME NOT NULL,
    `expires_at` DATETIME NOT NULL
)