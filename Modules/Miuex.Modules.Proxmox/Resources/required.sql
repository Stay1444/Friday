CREATE TABLE IF NOT EXISTS `vps` (
                       `id` int NOT NULL AUTO_INCREMENT,
                       `userId` bigint NOT NULL,
                       `nodeId` int DEFAULT NULL,
                       `vmId` int DEFAULT NULL,
                       `name` varchar(64) DEFAULT NULL,
                       PRIMARY KEY (`id`),
                       UNIQUE KEY `vps_id_uindex` (`id`),
                       UNIQUE KEY `name_UNIQUE` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=2;