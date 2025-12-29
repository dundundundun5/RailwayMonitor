CREATE TABLE IF NOT EXISTS alarm_trace (
    id BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    device_ip VARCHAR(255) NOT NULL,
    super_brain_channel INT NOT NULL,
    alarm_type INT NOT NULL,
    alarm_date DATETIME NOT NULL,
    image_path VARCHAR(500),
    alarm_status INT NOT NULL,
    create_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    update_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
CREATE TABLE IF NOT EXISTS `device` (
                                        `id` int(11) NOT NULL AUTO_INCREMENT COMMENT '主键ID',
    `name` varchar(100) NOT NULL COMMENT '设备名称',
    `index` int(11) NOT NULL COMMENT '监控顺位',
    `ip` varchar(15) NOT NULL COMMENT 'IP地址',
    `port` int(11) NOT NULL COMMENT '端口号',
    `username` varchar(50) DEFAULT NULL COMMENT '用户名',
    `password` varchar(100) DEFAULT NULL COMMENT '密码',
    `type` int(11) NOT NULL COMMENT '设备类型值',
    `channel` int(11) DEFAULT NULL COMMENT '通道号',
    `enabled` tinyint(1) DEFAULT '1' COMMENT '启用状态：0-禁用，1-启用',
    `create_date` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
    `update_date` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
    PRIMARY KEY (`id`)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='设备信息表';