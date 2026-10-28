/*
    Run as a DBA in a controlled environment.
    Replace the placeholders before execution. Never commit real passwords.
*/
USE [master];
GO

IF SUSER_ID(N'$(APP_LOGIN)') IS NULL
    CREATE LOGIN [$(APP_LOGIN)] WITH PASSWORD = '$(APP_PASSWORD)', CHECK_POLICY = ON, CHECK_EXPIRATION = ON;
GO

USE [$(APP_DATABASE)];
GO

IF DATABASE_PRINCIPAL_ID(N'AcademicAppRole') IS NULL
    CREATE ROLE [AcademicAppRole];
GO

IF DATABASE_PRINCIPAL_ID(N'$(APP_USER)') IS NULL
    CREATE USER [$(APP_USER)] FOR LOGIN [$(APP_LOGIN)];
GO

ALTER ROLE [AcademicAppRole] ADD MEMBER [$(APP_USER)];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO [AcademicAppRole];
DENY ALTER, CONTROL, TAKE OWNERSHIP ON SCHEMA::dbo TO [AcademicAppRole];
GO
