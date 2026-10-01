USE CompanyManagement;
GO
SET XACT_ABORT ON;
BEGIN TRAN;
DELETE FROM Ordenes; DELETE FROM Clientes;
DBCC CHECKIDENT ('Clientes', RESEED, 0); DBCC CHECKIDENT ('Ordenes', RESEED, 0);
INSERT INTO Clientes (Nombre,Email,FechaRegistro) VALUES
(N'Ana Martínez','ana@empresa.test',DATEADD(day,-90,GETUTCDATE())),(N'Bruno López','bruno@empresa.test',DATEADD(day,-75,GETUTCDATE())),(N'Carla Rivera','carla@empresa.test',DATEADD(day,-60,GETUTCDATE())),(N'Diego Flores','diego@empresa.test',DATEADD(day,-45,GETUTCDATE())),(N'Elena Cruz','elena@empresa.test',DATEADD(day,-20,GETUTCDATE())),(N'Fernando Reyes','fernando@empresa.test',DATEADD(day,-10,GETUTCDATE())),(N'Gabriela Soto','gabriela@empresa.test',DATEADD(day,-5,GETUTCDATE())),(N'Hugo Molina','hugo@empresa.test',DATEADD(day,-110,GETUTCDATE())),(N'Isabel Vega','isabel@empresa.test',DATEADD(day,-12,GETUTCDATE())),(N'Jorge Castro','jorge@empresa.test',DATEADD(day,-35,GETUTCDATE())),(N'Karla Díaz','karla@empresa.test',DATEADD(day,-8,GETUTCDATE())),(N'Luis Pérez','luis@empresa.test',DATEADD(day,-130,GETUTCDATE())),(N'María Torres','maria@empresa.test',DATEADD(day,-15,GETUTCDATE())),(N'Nicolás Ramos','nicolas@empresa.test',DATEADD(day,-100,GETUTCDATE())),(N'Olga Silva','olga@empresa.test',DATEADD(day,-4,GETUTCDATE()));
INSERT INTO Ordenes (ClienteId,FechaOrden,MontoTotal) SELECT ((n-1)%10)+1, DATEADD(day,-n,GETUTCDATE()), CAST(100+n*17.35 AS decimal(18,2)) FROM (SELECT TOP 40 ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) n FROM sys.all_objects) q;
COMMIT;
GO
