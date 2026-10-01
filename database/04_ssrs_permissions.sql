USE master;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name=N'NT SERVICE\SQLServerReportingServices') CREATE LOGIN [NT SERVICE\SQLServerReportingServices] FROM WINDOWS;
GO
USE CompanyManagement;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name=N'NT SERVICE\SQLServerReportingServices') CREATE USER [NT SERVICE\SQLServerReportingServices] FOR LOGIN [NT SERVICE\SQLServerReportingServices];
ALTER ROLE db_datareader ADD MEMBER [NT SERVICE\SQLServerReportingServices];
GO
