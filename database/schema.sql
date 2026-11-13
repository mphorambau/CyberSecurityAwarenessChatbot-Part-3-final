CREATE DATABASE IF NOT EXISTS cyber_awareness
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE cyber_awareness;

CREATE TABLE IF NOT EXISTS tasks (
    id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(160) NOT NULL,
    description TEXT NOT NULL,
    reminder_at DATETIME NULL,
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_tasks_status (is_completed),
    INDEX idx_tasks_reminder (reminder_at)
);