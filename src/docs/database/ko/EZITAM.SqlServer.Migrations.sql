IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AssetCategories] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(10) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [DepreciationMonths] int NOT NULL,
    CONSTRAINT [PK_AssetCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AuditLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [EntityName] nvarchar(100) NOT NULL,
    [EntityKey] nvarchar(50) NOT NULL,
    [Action] int NOT NULL,
    [Changes] nvarchar(max) NULL,
    [UserName] nvarchar(100) NULL,
    [Timestamp] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(20) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Locations] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Building] nvarchar(100) NULL,
    [Floor] nvarchar(20) NULL,
    [Room] nvarchar(50) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Vendors] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [ContactName] nvarchar(100) NULL,
    [Phone] nvarchar(30) NULL,
    [Email] nvarchar(200) NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [EmployeeNo] nvarchar(30) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NULL,
    [Title] nvarchar(50) NULL,
    [DepartmentId] int NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [SoftwareLicenses] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Publisher] nvarchar(100) NULL,
    [Version] nvarchar(50) NULL,
    [LicenseType] int NOT NULL,
    [LicenseKey] nvarchar(500) NULL,
    [TotalSeats] int NOT NULL,
    [PurchaseDate] datetime2 NULL,
    [ExpiryDate] datetime2 NULL,
    [VendorId] int NULL,
    [Cost] decimal(18,2) NULL,
    [Notes] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SoftwareLicenses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SoftwareLicenses_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Assets] (
    [Id] int NOT NULL IDENTITY,
    [AssetTag] nvarchar(30) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [CategoryId] int NOT NULL,
    [Status] int NOT NULL,
    [Manufacturer] nvarchar(100) NULL,
    [Model] nvarchar(100) NULL,
    [SerialNumber] nvarchar(100) NULL,
    [VendorId] int NULL,
    [PurchaseDate] datetime2 NULL,
    [PurchasePrice] decimal(18,2) NULL,
    [WarrantyExpiry] datetime2 NULL,
    [LocationId] int NULL,
    [CurrentEmployeeId] int NULL,
    [IpAddress] nvarchar(100) NULL,
    [MacAddress] nvarchar(100) NULL,
    [HostName] nvarchar(100) NULL,
    [OperatingSystem] nvarchar(100) NULL,
    [Notes] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    [UpdatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Assets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assets_AssetCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [AssetCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Assets_Employees_CurrentEmployeeId] FOREIGN KEY ([CurrentEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Assets_Locations_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Locations] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Assets_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Agents] (
    [Id] int NOT NULL IDENTITY,
    [DeviceUuid] nvarchar(64) NOT NULL,
    [SerialNumber] nvarchar(100) NULL,
    [HostName] nvarchar(100) NOT NULL,
    [Manufacturer] nvarchar(100) NULL,
    [Model] nvarchar(100) NULL,
    [CpuName] nvarchar(100) NULL,
    [CpuCores] int NULL,
    [TotalMemoryMb] bigint NULL,
    [OsName] nvarchar(100) NULL,
    [OsVersion] nvarchar(100) NULL,
    [OsBuild] nvarchar(20) NULL,
    [LoggedOnUser] nvarchar(100) NULL,
    [MacAddresses] nvarchar(300) NULL,
    [IpAddress] nvarchar(100) NULL,
    [BitLockerOn] bit NULL,
    [DiskTotalGb] bigint NULL,
    [AgentVersion] nvarchar(32) NOT NULL,
    [Status] int NOT NULL,
    [TokenHash] nvarchar(64) NULL,
    [EnrolledAt] datetime2 NOT NULL,
    [LastSeenAt] datetime2 NULL,
    [LastInventoryAt] datetime2 NULL,
    [BootTimeUtc] datetime2 NULL,
    [RebootPending] bit NOT NULL,
    [AssetId] int NULL,
    CONSTRAINT [PK_Agents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Agents_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [AssetAssignments] (
    [Id] int NOT NULL IDENTITY,
    [AssetId] int NOT NULL,
    [EmployeeId] int NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [ReturnedAt] datetime2 NULL,
    [AssignedBy] nvarchar(100) NULL,
    [ReturnedBy] nvarchar(100) NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_AssetAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AssetAssignments_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AssetAssignments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [LicenseAssignments] (
    [Id] int NOT NULL IDENTITY,
    [SoftwareLicenseId] int NOT NULL,
    [AssetId] int NULL,
    [EmployeeId] int NULL,
    [AssignedAt] datetime2 NOT NULL,
    [ReleasedAt] datetime2 NULL,
    [AssignedBy] nvarchar(100) NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_LicenseAssignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LicenseAssignments_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LicenseAssignments_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LicenseAssignments_SoftwareLicenses_SoftwareLicenseId] FOREIGN KEY ([SoftwareLicenseId]) REFERENCES [SoftwareLicenses] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MaintenanceRecords] (
    [Id] int NOT NULL IDENTITY,
    [AssetId] int NOT NULL,
    [Type] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Cost] decimal(18,2) NULL,
    [VendorId] int NULL,
    [CompletedAt] datetime2 NULL,
    [CreatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_MaintenanceRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MaintenanceRecords_Assets_AssetId] FOREIGN KEY ([AssetId]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MaintenanceRecords_Vendors_VendorId] FOREIGN KEY ([VendorId]) REFERENCES [Vendors] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [AgentSoftware] (
    [Id] bigint NOT NULL IDENTITY,
    [AgentId] int NOT NULL,
    [Name] nvarchar(300) NOT NULL,
    [Publisher] nvarchar(200) NULL,
    [Version] nvarchar(100) NULL,
    [InstallDate] nvarchar(20) NULL,
    [Source] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_AgentSoftware] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AgentSoftware_Agents_AgentId] FOREIGN KEY ([AgentId]) REFERENCES [Agents] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Agents_AssetId] ON [Agents] ([AssetId]);
GO

CREATE UNIQUE INDEX [IX_Agents_DeviceUuid] ON [Agents] ([DeviceUuid]);
GO

CREATE INDEX [IX_Agents_HostName] ON [Agents] ([HostName]);
GO

CREATE INDEX [IX_Agents_SerialNumber] ON [Agents] ([SerialNumber]);
GO

CREATE INDEX [IX_Agents_TokenHash] ON [Agents] ([TokenHash]);
GO

CREATE INDEX [IX_AgentSoftware_AgentId_Name] ON [AgentSoftware] ([AgentId], [Name]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_AssetAssignments_AssetId_ReturnedAt] ON [AssetAssignments] ([AssetId], [ReturnedAt]);
GO

CREATE INDEX [IX_AssetAssignments_EmployeeId] ON [AssetAssignments] ([EmployeeId]);
GO

CREATE UNIQUE INDEX [IX_AssetCategories_Code] ON [AssetCategories] ([Code]);
GO

CREATE UNIQUE INDEX [IX_Assets_AssetTag] ON [Assets] ([AssetTag]);
GO

CREATE INDEX [IX_Assets_CategoryId] ON [Assets] ([CategoryId]);
GO

CREATE INDEX [IX_Assets_CurrentEmployeeId] ON [Assets] ([CurrentEmployeeId]);
GO

CREATE INDEX [IX_Assets_LocationId] ON [Assets] ([LocationId]);
GO

CREATE INDEX [IX_Assets_SerialNumber] ON [Assets] ([SerialNumber]);
GO

CREATE INDEX [IX_Assets_Status] ON [Assets] ([Status]);
GO

CREATE INDEX [IX_Assets_VendorId] ON [Assets] ([VendorId]);
GO

CREATE INDEX [IX_AuditLogs_EntityName_EntityKey] ON [AuditLogs] ([EntityName], [EntityKey]);
GO

CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
GO

CREATE UNIQUE INDEX [IX_Departments_Code] ON [Departments] ([Code]);
GO

CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
GO

CREATE UNIQUE INDEX [IX_Employees_EmployeeNo] ON [Employees] ([EmployeeNo]);
GO

CREATE INDEX [IX_LicenseAssignments_AssetId] ON [LicenseAssignments] ([AssetId]);
GO

CREATE INDEX [IX_LicenseAssignments_EmployeeId] ON [LicenseAssignments] ([EmployeeId]);
GO

CREATE INDEX [IX_LicenseAssignments_SoftwareLicenseId_ReleasedAt] ON [LicenseAssignments] ([SoftwareLicenseId], [ReleasedAt]);
GO

CREATE INDEX [IX_MaintenanceRecords_AssetId] ON [MaintenanceRecords] ([AssetId]);
GO

CREATE INDEX [IX_MaintenanceRecords_VendorId] ON [MaintenanceRecords] ([VendorId]);
GO

CREATE INDEX [IX_SoftwareLicenses_Name] ON [SoftwareLicenses] ([Name]);
GO

CREATE INDEX [IX_SoftwareLicenses_VendorId] ON [SoftwareLicenses] ([VendorId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261011023111_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Agents] ADD [Ring] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261011231858_AddAgentRing', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Inventories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [DateFrom] date NOT NULL,
    [DateTo] date NOT NULL,
    [Notes] nvarchar(200) NULL,
    [InventoryStatus] int NOT NULL,
    CONSTRAINT [PK_Inventories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [InventoryDetails] (
    [Id] int NOT NULL IDENTITY,
    [InventoryId] int NOT NULL,
    [IdAsset] int NOT NULL,
    [IdLocation] int NOT NULL,
    [Status] int NOT NULL,
    [IdEmployee] int NOT NULL,
    [IdLocationIV] int NOT NULL,
    [AssetStatusIV] int NOT NULL,
    [IdEmployeeIV] int NOT NULL,
    [Notes] nvarchar(200) NULL,
    [ScanCount] int NOT NULL DEFAULT 0,
    [UpdatedBy] nvarchar(max) NULL,
    [UpdatedTime] datetime2 NOT NULL,
    CONSTRAINT [PK_InventoryDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventoryDetails_Assets_IdAsset] FOREIGN KEY ([IdAsset]) REFERENCES [Assets] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryDetails_Employees_IdEmployee] FOREIGN KEY ([IdEmployee]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryDetails_Employees_IdEmployeeIV] FOREIGN KEY ([IdEmployeeIV]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryDetails_Inventories_InventoryId] FOREIGN KEY ([InventoryId]) REFERENCES [Inventories] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InventoryDetails_Locations_IdLocation] FOREIGN KEY ([IdLocation]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventoryDetails_Locations_IdLocationIV] FOREIGN KEY ([IdLocationIV]) REFERENCES [Locations] ([Id]) ON DELETE NO ACTION
);
GO

CREATE UNIQUE INDEX [IX_Inventories_Name] ON [Inventories] ([Name]);
GO

CREATE INDEX [IX_InventoryDetails_IdAsset] ON [InventoryDetails] ([IdAsset]);
GO

CREATE INDEX [IX_InventoryDetails_IdEmployee] ON [InventoryDetails] ([IdEmployee]);
GO

CREATE INDEX [IX_InventoryDetails_IdEmployeeIV] ON [InventoryDetails] ([IdEmployeeIV]);
GO

CREATE INDEX [IX_InventoryDetails_IdLocation] ON [InventoryDetails] ([IdLocation]);
GO

CREATE INDEX [IX_InventoryDetails_IdLocationIV] ON [InventoryDetails] ([IdLocationIV]);
GO

CREATE UNIQUE INDEX [IX_InventoryDetails_InventoryId_IdAsset] ON [InventoryDetails] ([InventoryId], [IdAsset]);
GO

CREATE INDEX [IX_InventoryDetails_InventoryId_ScanCount] ON [InventoryDetails] ([InventoryId], [ScanCount]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261019020434_addInventory', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryDetails]') AND [c].[name] = N'IdLocationIV');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [InventoryDetails] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [InventoryDetails] ALTER COLUMN [IdLocationIV] int NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryDetails]') AND [c].[name] = N'IdLocation');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [InventoryDetails] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [InventoryDetails] ALTER COLUMN [IdLocation] int NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryDetails]') AND [c].[name] = N'IdEmployeeIV');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [InventoryDetails] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [InventoryDetails] ALTER COLUMN [IdEmployeeIV] int NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InventoryDetails]') AND [c].[name] = N'IdEmployee');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [InventoryDetails] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [InventoryDetails] ALTER COLUMN [IdEmployee] int NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261020060317_fix_inventory_detail_table', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Employees] ADD [Source] nvarchar(30) NULL;
GO

CREATE TABLE [EmployeeTemps] (
    [Id] int NOT NULL IDENTITY,
    [OrgCode] nvarchar(30) NOT NULL,
    [EmployeeNo] nvarchar(30) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(200) NULL,
    [DeptCode] nvarchar(30) NULL,
    [DeptName] nvarchar(100) NULL,
    [DeptLevel] nvarchar(30) NULL,
    [TitleCode] nvarchar(30) NULL,
    [TitleName] nvarchar(100) NULL,
    [PositionCode] nvarchar(30) NULL,
    [PositionName] nvarchar(100) NULL,
    [IsResigned] nvarchar(1) NULL,
    [DateValue1] nvarchar(100) NULL,
    [DateValue2] nvarchar(100) NULL,
    [DateValue3] nvarchar(100) NULL,
    [DateValue4] nvarchar(100) NULL,
    [DateValue5] nvarchar(100) NULL,
    [DateValue6] nvarchar(100) NULL,
    [DateValue7] nvarchar(100) NULL,
    [DateValue8] nvarchar(100) NULL,
    [DateValue9] nvarchar(100) NULL,
    CONSTRAINT [PK_EmployeeTemps] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_EmployeeTemps_OrgCode_EmployeeNo] ON [EmployeeTemps] ([OrgCode], [EmployeeNo]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261027074541_addTableEmployeeTemp', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_AuditLogs_EntityName_EntityKey] ON [AuditLogs];
GO

DROP INDEX [IX_Agents_TokenHash] ON [Agents];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SoftwareLicenses]') AND [c].[name] = N'TotalSeats');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [SoftwareLicenses] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [SoftwareLicenses] ADD DEFAULT 1 FOR [TotalSeats];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SoftwareLicenses]') AND [c].[name] = N'LicenseType');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [SoftwareLicenses] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [SoftwareLicenses] ADD DEFAULT 0 FOR [LicenseType];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Locations]') AND [c].[name] = N'IsActive');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Locations] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Locations] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Inventories]') AND [c].[name] = N'InventoryStatus');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Inventories] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Inventories] ADD DEFAULT 1 FOR [InventoryStatus];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EmployeeTemps]') AND [c].[name] = N'IsResigned');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [EmployeeTemps] DROP CONSTRAINT [' + @var8 + '];');
UPDATE [EmployeeTemps] SET [IsResigned] = N'N' WHERE [IsResigned] IS NULL;
ALTER TABLE [EmployeeTemps] ALTER COLUMN [IsResigned] nvarchar(1) NOT NULL;
ALTER TABLE [EmployeeTemps] ADD DEFAULT N'N' FOR [IsResigned];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'IsActive');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Employees] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Departments]') AND [c].[name] = N'IsActive');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Departments] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Departments] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'Status');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Assets] ADD DEFAULT 0 FOR [Status];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AssetCategories]') AND [c].[name] = N'DepreciationMonths');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [AssetCategories] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [AssetCategories] ADD DEFAULT 48 FOR [DepreciationMonths];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'Status');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Agents] ADD DEFAULT 0 FOR [Status];
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'Ring');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Agents] ADD DEFAULT 0 FOR [Ring];
GO

ALTER TABLE [SoftwareLicenses] ADD CONSTRAINT [CK_SoftwareLicenses_Cost_NonNegative] CHECK ([Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999);
GO

ALTER TABLE [SoftwareLicenses] ADD CONSTRAINT [CK_SoftwareLicenses_LicenseType] CHECK ([LicenseType] IN (0, 1, 2, 3, 4));
GO

ALTER TABLE [SoftwareLicenses] ADD CONSTRAINT [CK_SoftwareLicenses_TotalSeats] CHECK ([TotalSeats] BETWEEN 0 AND 1000000);
GO

CREATE INDEX [IX_MaintenanceRecords_Open_Date_Id] ON [MaintenanceRecords] ([Date], [Id]) WHERE [CompletedAt] IS NULL;
GO

ALTER TABLE [MaintenanceRecords] ADD CONSTRAINT [CK_MaintenanceRecords_Cost_NonNegative] CHECK ([Cost] IS NULL OR [Cost] BETWEEN 0 AND 999999999999);
GO

ALTER TABLE [MaintenanceRecords] ADD CONSTRAINT [CK_MaintenanceRecords_Type] CHECK ([Type] IN (0, 1, 2, 3));
GO

CREATE UNIQUE INDEX [UX_LicenseAssignments_Open_Asset] ON [LicenseAssignments] ([SoftwareLicenseId], [AssetId]) WHERE [ReleasedAt] IS NULL AND [AssetId] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UX_LicenseAssignments_Open_Employee] ON [LicenseAssignments] ([SoftwareLicenseId], [EmployeeId]) WHERE [ReleasedAt] IS NULL AND [EmployeeId] IS NOT NULL;
GO

ALTER TABLE [LicenseAssignments] ADD CONSTRAINT [CK_LicenseAssignments_ExactlyOneAssignee] CHECK (([AssetId] IS NOT NULL AND [EmployeeId] IS NULL) OR ([AssetId] IS NULL AND [EmployeeId] IS NOT NULL));
GO

ALTER TABLE [InventoryDetails] ADD CONSTRAINT [CK_InventoryDetails_AssetStatusIV] CHECK ([AssetStatusIV] IN (0, 1, 2, 3, 4, 5, 6));
GO

ALTER TABLE [InventoryDetails] ADD CONSTRAINT [CK_InventoryDetails_ScanCount] CHECK ([ScanCount] >= 0);
GO

ALTER TABLE [InventoryDetails] ADD CONSTRAINT [CK_InventoryDetails_Status] CHECK ([Status] IN (0, 1, 2, 3, 4, 5, 6));
GO

ALTER TABLE [Inventories] ADD CONSTRAINT [CK_Inventories_DateRange] CHECK ([DateFrom] <= [DateTo]);
GO

ALTER TABLE [Inventories] ADD CONSTRAINT [CK_Inventories_Status] CHECK ([InventoryStatus] IN (1, 2, 3));
GO

ALTER TABLE [EmployeeTemps] ADD CONSTRAINT [CK_EmployeeTemps_IsResigned] CHECK ([IsResigned] IN ('Y', 'N'));
GO

CREATE INDEX [IX_AuditLogs_EntityName_EntityKey_Timestamp] ON [AuditLogs] ([EntityName], [EntityKey], [Timestamp]);
GO

ALTER TABLE [AuditLogs] ADD CONSTRAINT [CK_AuditLogs_Action] CHECK ([Action] IN (0, 1, 2));
GO

ALTER TABLE [Assets] ADD CONSTRAINT [CK_Assets_PurchasePrice_NonNegative] CHECK ([PurchasePrice] IS NULL OR [PurchasePrice] BETWEEN 0 AND 999999999999);
GO

ALTER TABLE [Assets] ADD CONSTRAINT [CK_Assets_Status] CHECK ([Status] IN (0, 1, 2, 3, 4, 5, 6));
GO

ALTER TABLE [AssetCategories] ADD CONSTRAINT [CK_AssetCategories_DepreciationMonths] CHECK ([DepreciationMonths] BETWEEN 0 AND 600);
GO

CREATE UNIQUE INDEX [UX_AssetAssignments_AssetId_Open] ON [AssetAssignments] ([AssetId]) WHERE [ReturnedAt] IS NULL;
GO

CREATE INDEX [IX_Agents_LastSeenAt] ON [Agents] ([LastSeenAt]);
GO

CREATE UNIQUE INDEX [IX_Agents_TokenHash] ON [Agents] ([TokenHash]) WHERE [TokenHash] IS NOT NULL;
GO

ALTER TABLE [Agents] ADD CONSTRAINT [CK_Agents_NonNegativeHardware] CHECK (([CpuCores] IS NULL OR [CpuCores] >= 0) AND ([TotalMemoryMb] IS NULL OR [TotalMemoryMb] >= 0) AND ([DiskTotalGb] IS NULL OR [DiskTotalGb] >= 0));
GO

ALTER TABLE [Agents] ADD CONSTRAINT [CK_Agents_Ring] CHECK ([Ring] IN (0, 1));
GO

ALTER TABLE [Agents] ADD CONSTRAINT [CK_Agents_Status] CHECK ([Status] IN (0, 1, 2));
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261114022006_OptimizeDataIntegrityAndQueryIndexes', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_MaintenanceRecords_Open_Date_Id] ON [MaintenanceRecords];
GO

DROP INDEX [IX_Agents_TokenHash] ON [Agents];
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Vendors]') AND [c].[name] = N'Email');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Vendors] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Vendors] ALTER COLUMN [Email] nvarchar(254) NULL;
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SoftwareLicenses]') AND [c].[name] = N'PurchaseDate');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [SoftwareLicenses] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [SoftwareLicenses] ALTER COLUMN [PurchaseDate] date NULL;
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SoftwareLicenses]') AND [c].[name] = N'ExpiryDate');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [SoftwareLicenses] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [SoftwareLicenses] ALTER COLUMN [ExpiryDate] date NULL;
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SoftwareLicenses]') AND [c].[name] = N'Cost');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [SoftwareLicenses] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [SoftwareLicenses] ALTER COLUMN [Cost] decimal(14,2) NULL;
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MaintenanceRecords]') AND [c].[name] = N'Date');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [MaintenanceRecords] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [MaintenanceRecords] ALTER COLUMN [Date] date NOT NULL;
GO

DECLARE @var20 sysname;
SELECT @var20 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MaintenanceRecords]') AND [c].[name] = N'Cost');
IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [MaintenanceRecords] DROP CONSTRAINT [' + @var20 + '];');
ALTER TABLE [MaintenanceRecords] ALTER COLUMN [Cost] decimal(14,2) NULL;
GO

DECLARE @var21 sysname;
SELECT @var21 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MaintenanceRecords]') AND [c].[name] = N'CompletedAt');
IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [MaintenanceRecords] DROP CONSTRAINT [' + @var21 + '];');
ALTER TABLE [MaintenanceRecords] ALTER COLUMN [CompletedAt] date NULL;
GO

DECLARE @var22 sysname;
SELECT @var22 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[EmployeeTemps]') AND [c].[name] = N'Email');
IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [EmployeeTemps] DROP CONSTRAINT [' + @var22 + '];');
ALTER TABLE [EmployeeTemps] ALTER COLUMN [Email] nvarchar(254) NULL;
GO

DECLARE @var23 sysname;
SELECT @var23 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Employees]') AND [c].[name] = N'Email');
IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Employees] DROP CONSTRAINT [' + @var23 + '];');
ALTER TABLE [Employees] ALTER COLUMN [Email] nvarchar(254) NULL;
GO

DECLARE @var24 sysname;
SELECT @var24 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'WarrantyExpiry');
IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT [' + @var24 + '];');
ALTER TABLE [Assets] ALTER COLUMN [WarrantyExpiry] date NULL;
GO

DECLARE @var25 sysname;
SELECT @var25 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'PurchasePrice');
IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT [' + @var25 + '];');
ALTER TABLE [Assets] ALTER COLUMN [PurchasePrice] decimal(14,2) NULL;
GO

DECLARE @var26 sysname;
SELECT @var26 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'PurchaseDate');
IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT [' + @var26 + '];');
ALTER TABLE [Assets] ALTER COLUMN [PurchaseDate] date NULL;
GO

DECLARE @var27 sysname;
SELECT @var27 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Assets]') AND [c].[name] = N'IpAddress');
IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Assets] DROP CONSTRAINT [' + @var27 + '];');
ALTER TABLE [Assets] ALTER COLUMN [IpAddress] nvarchar(64) NULL;
GO

DECLARE @var28 sysname;
SELECT @var28 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'TokenHash');
IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var28 + '];');
ALTER TABLE [Agents] ALTER COLUMN [TokenHash] char(64) NULL;
GO

DECLARE @var29 sysname;
SELECT @var29 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Agents]') AND [c].[name] = N'IpAddress');
IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Agents] DROP CONSTRAINT [' + @var29 + '];');
ALTER TABLE [Agents] ALTER COLUMN [IpAddress] nvarchar(64) NULL;
GO

CREATE INDEX [IX_MaintenanceRecords_Open_Date_Id] ON [MaintenanceRecords] ([Date], [Id]) WHERE [CompletedAt] IS NULL;
GO

CREATE UNIQUE INDEX [IX_Agents_TokenHash] ON [Agents] ([TokenHash]) WHERE [TokenHash] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261114023249_OptimizeDatabaseColumnTypesAndLengths', N'8.0.11');
GO

COMMIT;
GO

