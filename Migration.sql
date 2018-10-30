USE Everest
GO

--CREATE TABLE Cartridges (
--	Id int NOT NULL IDENTITY(1, 1),
--	Name nvarchar(max) NULL,
--	Cost float NULL,
--	Type nvarchar(max) NULL,
--	Color nvarchar(max) NULL,
--	Description nvarchar(max) NULL
--)
--GO

--SET Identity_Insert Cartridges ON
--INSERT INTO Cartridges (Id, Name, Cost, Type, Color, Description)
--SELECT N, Caption, Price, Type, Color, Description FROM CARTRIDGE
--SET Identity_Insert Cartridges OFF
--GO

--DROP TABLE CARTRIDGE
--GO

--CREATE TABLE Printers (
--	Id int NOT NULL IDENTITY(1, 1),
--	Name nvarchar(max) NULL,
--	Description nvarchar(max) NULL
--)
--GO

--SET Identity_Insert Printers ON
--INSERT INTO Printers (Id, Name, Description)
--SELECT N, Caption, Description FROM PRINTER
--SET Identity_Insert Printers OFF
--GO

--DROP TABLE PRINTER
--GO

--CREATE TABLE PrintersCartridges (
--	PrinterId int NOT NULL,
--	CartridgeId int NOT NULL
--)
--GO

--INSERT INTO PrintersCartridges (PrinterId, CartridgeId)
--SELECT Printer, Cartridge FROM OFFICE
--GO

--DROP TABLE OFFICE
--GO

--CREATE TABLE Folders (
--	Id int NOT NULL IDENTITY(1, 1),
--	Name nvarchar(max) NULL,
--	FolderId int NOT NULL,
--	Type nvarchar(max) NOT NULL
--)
--GO

--SET Identity_Insert Folders ON
--INSERT INTO Folders (Id, Name, FolderId, Type)
--SELECT G_ID, G_Title, G_Inside, G_Type FROM [GROUP]
--SET Identity_Insert Folders OFF
--GO

--DROP TABLE [GROUP]
--GO

--IF OBJECT_ID('dbo.Writeoffs') IS NOT NULL DROP TABLE Writeoffs;
--CREATE TABLE Writeoffs (
--	Id int IDENTITY(1,1) NOT NULL,
--	Name nvarchar(max) NULL,
--	Type nvarchar(max) NULL,
--	Date datetime NULL,
--	Params nvarchar(max) NULL,
--	Description nvarchar(max) NULL,
--	FolderId int NULL,
--	LastExcel nvarchar(max) NULL,
--	LastExcelDate datetime NULL,
--	CostArticle int NULL,
--)
--GO

--SET Identity_Insert Writeoffs ON
--INSERT INTO Writeoffs (Id, Name, Type, Date, Params, Description, FolderId, LastExcel, LastExcelDate, CostArticle)
--SELECT W_ID, W_Name, W_Type, W_Date, W_Params, W_Description, G_ID, W_Last_Excel, W_Last_Date, W_Cost_Article FROM writeoff
--SET Identity_Insert Writeoffs OFF
--GO

--DROP TABLE writeoff
--GO

--CREATE TABLE Storages (
--	Ncard int NOT NULL,
--	Name nvarchar(max) NULL,
--	Type nvarchar(max) NULL,
--	Cost float NULL,
--	Nall int NOT NULL,
--	Nstorage int NOT NULL,
--	Nrepairs int NOT NULL,
--	Noff int NOT NULL,
--	Date datetime NOT NULL,
--	IsDeleted bit NOT NULL,
--	Account nvarchar(max) NULL,
--	CartridgeId int NULL,
--	FolderId int NULL
--)
--GO

--INSERT INTO Storages (Ncard, Name, Type, Cost, Nall, Nstorage, Nrepairs, Noff, Date, IsDeleted, Account, CartridgeId, FolderId)
--SELECT NCard, Name, class_name, Price, Nadd, Nis, Nuse, Nbreak, Date, CASE WHEN delit = 1 THEN 0 ELSE 1 END AS delit, uchet, ID_cart, G_ID FROM SKLAD
--GO

--DROP TABLE SKLAD
--GO

--sp_rename 'DEVICE.class_device', 'Type', 'COLUMN'
--GO
--sp_rename 'DEVICE.number_device', 'Number', 'COLUMN'
--GO
--sp_rename 'DEVICE.number_comp', 'ComputerNumber', 'COLUMN'
--GO
--sp_rename 'DEVICE.install_date', 'DateInstall', 'COLUMN'
--GO
--sp_rename 'DEVICE.LastRepairDate', 'DateLastRepair', 'COLUMN'
--GO
--sp_rename 'DEVICE.number_serial', 'SerialNumber', 'COLUMN'
--GO
--sp_rename 'DEVICE.number_passport', 'PassportNumber', 'COLUMN'
--GO
--sp_rename 'DEVICE.attribute', 'Location', 'COLUMN'
--GO
--sp_rename 'DEVICE.ID_prn', 'PrinterId', 'COLUMN'
--GO
--sp_rename 'DEVICE.G_Id', 'FolderId', 'COLUMN'
--GO
--sp_rename 'DEVICE.deleted', 'IsDeleted', 'COLUMN'
--GO
--sp_rename 'DEVICE.service_tag', 'ServiceTag', 'COLUMN'
--GO
--sp_rename 'DEVICE.description1C', 'Description1C', 'COLUMN'
--GO
--sp_rename 'DEVICE.used', 'IsOff', 'COLUMN'
--GO

--ALTER TABLE DEVICE
--	ADD Id int IDENTITY(1, 1) NOT NULL,
--		ComputerId int NULL;
--GO

--ALTER TABLE REMONT
--	ADD DeviceId int NULL;
--GO

--UPDATE REMONT SET DeviceId = Id 
--FROM REMONT INNER JOIN DEVICE ON DEVICE.Number = REMONT.ID_D
--GO

--UPDATE Activity SET Activity.Id = Device.Id
--FROM Activity INNER JOIN DEVICE ON DEVICE.Number = Activity.Id
--GO

--UPDATE DEVICE SET DEVICE.ComputerId = Computers.Id
--FROM DEVICE INNER JOIN DEVICE AS Computers ON Computers.Number = DEVICE.ComputerNumber
--GO

--sp_rename 'DEVICE', 'Devices'
--GO

--sp_rename 'REMONT', 'Repairs'
--GO

--ALTER TABLE Devices 
--	DROP COLUMN Number, ComputerNumber
--GO

--sp_rename 'Repairs.ID_U', 'StorageNcard', 'COLUMN'
--GO

--sp_rename 'Repairs.Units', 'Number', 'COLUMN'
--GO

--sp_rename 'Repairs.IfSpis', 'IsOff', 'COLUMN'
--GO

--sp_rename 'Repairs.Virtual', 'IsVirtual', 'COLUMN'
--GO

--sp_rename 'Repairs.W_ID', 'WriteoffId', 'COLUMN'
--GO

--sp_rename 'Repairs.G_ID', 'FolderId', 'COLUMN'
--GO

--sp_rename 'Repairs.INum', 'Id', 'COLUMN'
--GO

--ALTER TABLE Repairs 
--	DROP COLUMN ID_D
--GO

--ALTER TABLE Repairs
--	ALTER COLUMN IsOff bit NOT NULL
--GO

--ALTER TABLE Repairs
--	ALTER COLUMN IsVirtual bit NOT NULL
--GO

--ALTER TABLE Repairs
--	ALTER COLUMN Author nvarchar(max) NULL
--GO

--ALTER TABLE Repairs
--	ALTER COLUMN StorageNcard int NULL
--GO

--ALTER TABLE Devices
--	ALTER COLUMN IsOff bit NOT NULL
--GO

--ALTER TABLE Devices
--	ALTER COLUMN IsDeleted bit NOT NULL
--GO

--sp_rename 'Storages.Ncard', 'Inventory', 'COLUMN'
--GO

--sp_rename 'Repairs.StorageNcard', 'StorageInventory', 'COLUMN'
--GO

--UPDATE Devices SET IsOff = CASE WHEN IsOff = 1 THEN 0 ELSE 1 END;
--GO

--sp_rename 'catalog_device_types', 'TypesDevices'
--GO

--sp_rename 'TypesDevices.T_alias', 'Id', 'COLUMN'
--GO

--sp_rename 'TypesDevices.T_name', 'DeviceText', 'COLUMN'
--GO

--sp_rename 'TypesDevices.T_storages', 'StorageText', 'COLUMN'
--GO

--sp_rename 'TypesDevices.T_icon', 'Icon', 'COLUMN'
--GO

--ALTER TABLE TypesDevices
--	DROP COLUMN T_ID
--GO

--sp_rename 'catalog_os', 'TypesSystems'
--GO

--ALTER TABLE TypesSystems
--	DROP COLUMN T_ID
--GO

--sp_rename 'TypesSystems.T_alias', 'Id', 'COLUMN'
--GO

--sp_rename 'TypesSystems.T_name', 'Name', 'COLUMN'
--GO

--sp_rename 'catalog_cartridge_types', 'TypesCartridges'
--GO

--sp_rename 'TypesCartridges.C_alias', 'Id', 'COLUMN'
--GO

--sp_rename 'TypesCartridges.C_name', 'Name', 'COLUMN'
--GO

--sp_rename 'TypesCartridges.C_type', 'Type', 'COLUMN'
--GO

--sp_rename 'catalog_writeoffs', 'TypesWriteoffs'
--GO

--sp_rename 'TypesWriteoffs.O_Alias', 'Id', 'COLUMN'
--GO

--sp_rename 'TypesWriteoffs.O_Name', 'Name', 'COLUMN'
--GO

--sp_rename 'TypesWriteoffs.O_Data', 'Captions', 'COLUMN'
--GO

--sp_rename 'TypesWriteoffs.O_Count', 'FieldsCount', 'COLUMN'
--GO

--sp_rename 'TypesWriteoffs.O_Excel', 'Template', 'COLUMN'
--GO

--sp_rename 'TypesWriteoffs.O_Template', 'DefaultData', 'COLUMN'
--GO

--ALTER TABLE TypesWriteoffs
--	DROP COLUMN O_ID
--GO