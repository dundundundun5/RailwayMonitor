-- Create alarm_trace table
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