/*
    SQL Server operations runbook.
    Replace every $(...) token outside source control before execution.
    Run under a dedicated SQL Agent proxy/credential, never the web login.
*/
USE [master];
GO

DECLARE @database sysname = N'$(APP_DATABASE)';
DECLARE @backupFile nvarchar(4000) = N'$(BACKUP_DIRECTORY)\$(APP_DATABASE)_full.bak';

BACKUP DATABASE [$(APP_DATABASE)]
    TO DISK = @backupFile
    WITH COPY_ONLY, COMPRESSION, CHECKSUM, INIT, STATS = 10;
GO

RESTORE VERIFYONLY
    FROM DISK = N'$(BACKUP_DIRECTORY)\$(APP_DATABASE)_full.bak'
    WITH CHECKSUM;
GO

/*
    Restore drill (use an isolated database and approved logical file names):

    RESTORE DATABASE [$(RESTORE_DATABASE)]
      FROM DISK = N'$(BACKUP_DIRECTORY)\$(APP_DATABASE)_full.bak'
      WITH MOVE N'$(APP_DATABASE)' TO N'$(RESTORE_DATA_FILE)',
           MOVE N'$(APP_DATABASE)_log' TO N'$(RESTORE_LOG_FILE)',
           RECOVERY, REPLACE, STATS = 10;

    DBCC CHECKDB (N'$(RESTORE_DATABASE)') WITH NO_INFOMSGS, ALL_ERRORMSGS;
    Record restore time, CHECKDB output, backup checksum result and certificate
    availability in the change/ticket system. Do not restore over production
    as part of a drill.
*/
