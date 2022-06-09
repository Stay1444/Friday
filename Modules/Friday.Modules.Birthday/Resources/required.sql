CREATE TABLE IF NOT EXISTS `birthday_dates` (
    `id` bigint(64) NOT NULL UNIQUE,
    `date` date NOT NULl,
    `public` tinyint(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`id`)
);