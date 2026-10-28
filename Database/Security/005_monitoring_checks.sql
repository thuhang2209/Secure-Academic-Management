/*
    Read-only monitoring checks for a DBA/SQL Agent job.
    Schedule outside the web application and send alerts through the approved channel.
*/
USE [master];
GO

SELECT name, is_disabled, is_policy_checked, is_expiration_checked
FROM sys.sql_logins
WHERE name IN (N'sa', N'$(APP_LOGIN)');
GO

SELECT
    DB_NAME(database_id) AS DatabaseName,
    encryption_state,
    encryption_state_desc,
    percent_complete
FROM sys.dm_database_encryption_keys
WHERE DB_NAME(database_id) = N'$(APP_DATABASE)';
GO

-- Review SQL Audit files from a protected audit directory:
-- SELECT event_time, action_id, succeeded, server_principal_name, database_name,
--        schema_name, object_name, statement
-- FROM sys.fn_get_audit_file(N'$(AUDIT_DIRECTORY)\*.sqlaudit', DEFAULT, DEFAULT);
