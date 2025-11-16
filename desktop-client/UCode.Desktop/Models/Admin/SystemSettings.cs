using System;

namespace UCode.Desktop.Models.Admin
{
    public class SystemSettings
    {
        // Email Configuration
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        // Security Settings
        public bool RequireTwoFactorForAdmins { get; set; }
        public bool EnablePasswordComplexity { get; set; }
        public int MinPasswordLength { get; set; } = 8;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;

        // Judge Configuration
        public int MaxExecutionTimeSeconds { get; set; } = 5;
        public int MaxMemoryMB { get; set; } = 256;
        public int MaxOutputSizeKB { get; set; } = 1024;
        public int MaxConcurrentJobs { get; set; } = 10;

        // File Upload Configuration
        public int MaxFileUploadSizeMB { get; set; } = 10;
        public string AllowedImageExtensions { get; set; } = ".jpg,.jpeg,.png,.gif";
        public string AllowedDocumentExtensions { get; set; } = ".pdf,.doc,.docx,.txt";

        // System Configuration
        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessage { get; set; }
        public bool EnableRegistration { get; set; } = true;
        public bool RequireEmailVerification { get; set; } = true;
        public int DataRetentionDays { get; set; } = 365;

        // Notification Settings
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnablePushNotifications { get; set; }
        public bool NotifyAdminOnNewUser { get; set; }
        public bool NotifyAdminOnError { get; set; } = true;

        // Assignment Settings
        public int DefaultAssignmentDurationDays { get; set; } = 7;
        public bool AllowLateSubmissions { get; set; } = true;
        public decimal LatePenaltyPercentage { get; set; } = 10;

        // Backup Settings
        public bool EnableAutoBackup { get; set; }
        public int BackupIntervalHours { get; set; } = 24;
        public string BackupPath { get; set; } = string.Empty;
        public int BackupRetentionDays { get; set; } = 30;

        // Last updated
        public DateTime? LastUpdated { get; set; }
        public string? LastUpdatedBy { get; set; }
    }
}
