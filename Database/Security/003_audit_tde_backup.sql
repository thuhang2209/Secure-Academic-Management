/*
    DBA runbook. Paths, certificates and retention must be managed outside source control.
    Do not run with production values copied into the repository.
*/
USE [master];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_audits WHERE name = N'AcademicSecurityAudit')
    CREATE SERVER AUDIT [AcademicSecurityAudit]
    TO FILE (FILEPATH = N'$(AUDIT_DIRECTORY)', MAXSIZE = 512 MB, MAX_ROLLOVER_FILES = 20)
    WITH (ON_FAILURE = FAIL_OPERATION);
ALTER SERVER AUDIT [AcademicSecurityAudit] WITH (STATE = ON);
GO

USE [$(APP_DATABASE)];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_audit_specifications WHERE name = N'AcademicDatabaseAudit')
    CREATE DATABASE AUDIT SPECIFICATION [AcademicDatabaseAudit]
    FOR SERVER AUDIT [AcademicSecurityAudit]
    ADD (SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.SinhViens BY [public]),
    ADD (SELECT, INSERT, UPDATE, DELETE ON OBJECT::dbo.LichSuTrangThais BY [public]);
ALTER DATABASE AUDIT SPECIFICATION [AcademicDatabaseAudit] WITH (STATE = ON);
GO

-- TDE and backup encryption require a separately protected certificate/key.
-- Execute only after the certificate backup procedure has been approved:
-- CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = AES_256
--     ENCRYPTION BY SERVER CERTIFICATE [$(TDE_CERTIFICATE)];
-- ALTER DATABASE [$(APP_DATABASE)] SET ENCRYPTION ON;
--
-- Example backup schedule (run by SQL Agent, not by the web process):
-- BACKUP DATABASE [$(APP_DATABASE)] TO DISK = N'$(BACKUP_FILE)'
--     WITH COMPRESSION, CHECKSUM, INIT;
