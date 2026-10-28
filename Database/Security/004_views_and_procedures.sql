/*
    Expose only approved columns to reporting users.
    Run after migrations and review grants with a DBA.
*/
USE [$(APP_DATABASE)];
GO

CREATE OR ALTER VIEW dbo.vw_StudentReport
AS
SELECT
    MaSinhVien,
    HoTen,
    TrangThai,
    NgaySinh
FROM dbo.SinhViens;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetStudentByCode
    @MaSinhVien nvarchar(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MaSinhVien, HoTen, Email, TrangThai, NgaySinh
    FROM dbo.SinhViens
    WHERE MaSinhVien = @MaSinhVien;
END;
GO

IF DATABASE_PRINCIPAL_ID(N'AcademicReportReader') IS NULL
    CREATE ROLE AcademicReportReader;
GRANT SELECT ON dbo.vw_StudentReport TO AcademicReportReader;
GRANT EXECUTE ON dbo.usp_GetStudentByCode TO AcademicReportReader;
DENY SELECT ON dbo.SinhViens TO AcademicReportReader;
GO
