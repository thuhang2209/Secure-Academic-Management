/*
    Optional SQL Server controls. Review the tenant/department predicate with a DBA.
    The application remains responsible for authorization; these policies add defense in depth.
*/
USE [$(APP_DATABASE)];
GO

IF COL_LENGTH(N'dbo.SinhViens', N'Email') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.masked_columns
       WHERE object_id = OBJECT_ID(N'dbo.SinhViens') AND name = N'Email')
    ALTER TABLE dbo.SinhViens ALTER COLUMN Email ADD MASKED WITH (FUNCTION = 'email()');
GO

IF COL_LENGTH(N'dbo.SinhViens', N'SoDienThoai') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.masked_columns
       WHERE object_id = OBJECT_ID(N'dbo.SinhViens') AND name = N'SoDienThoai')
    ALTER TABLE dbo.SinhViens ALTER COLUMN SoDienThoai ADD MASKED WITH (FUNCTION = 'partial(0,"******",2)');
GO

IF OBJECT_ID(N'dbo.SinhVienAccessPredicate', N'IF') IS NULL
    EXEC(N'CREATE FUNCTION dbo.SinhVienAccessPredicate(@IdentityUserId nvarchar(450))
          RETURNS TABLE WITH SCHEMABINDING
          AS RETURN SELECT 1 AS allowed
          WHERE @IdentityUserId = CONVERT(nvarchar(450), SESSION_CONTEXT(N''IdentityUserId''))
             OR IS_MEMBER(N''AcademicAppRole'') = 1;');
GO

IF NOT EXISTS (SELECT 1 FROM sys.security_policies WHERE name = N'SinhVienAccessPolicy')
    CREATE SECURITY POLICY dbo.SinhVienAccessPolicy
        ADD FILTER PREDICATE dbo.SinhVienAccessPredicate(IdentityUserId) ON dbo.SinhViens
        WITH (STATE = ON);
GO
