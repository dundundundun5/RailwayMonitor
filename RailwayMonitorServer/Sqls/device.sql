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