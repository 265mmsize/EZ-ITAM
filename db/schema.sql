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

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AssetCategories] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(10) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DepreciationMonths] int NOT NULL,
        CONSTRAINT [PK_AssetCategories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [Locations] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Building] nvarchar(100) NULL,
        [Floor] nvarchar(20) NULL,
        [Room] nvarchar(50) NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [Vendors] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ContactName] nvarchar(100) NULL,
        [Phone] nvarchar(30) NULL,
        [Email] nvarchar(200) NULL,
        [Notes] nvarchar(500) NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_AssetId] ON [Agents] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Agents_DeviceUuid] ON [Agents] ([DeviceUuid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_HostName] ON [Agents] ([HostName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_SerialNumber] ON [Agents] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Agents_TokenHash] ON [Agents] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AgentSoftware_AgentId_Name] ON [AgentSoftware] ([AgentId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetAssignments_AssetId_ReturnedAt] ON [AssetAssignments] ([AssetId], [ReturnedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AssetAssignments_EmployeeId] ON [AssetAssignments] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AssetCategories_Code] ON [AssetCategories] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Assets_AssetTag] ON [Assets] ([AssetTag]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_CategoryId] ON [Assets] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_CurrentEmployeeId] ON [Assets] ([CurrentEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_LocationId] ON [Assets] ([LocationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_SerialNumber] ON [Assets] ([SerialNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_Status] ON [Assets] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Assets_VendorId] ON [Assets] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityName_EntityKey] ON [AuditLogs] ([EntityName], [EntityKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_Timestamp] ON [AuditLogs] ([Timestamp]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_Code] ON [Departments] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_EmployeeNo] ON [Employees] ([EmployeeNo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LicenseAssignments_AssetId] ON [LicenseAssignments] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LicenseAssignments_EmployeeId] ON [LicenseAssignments] ([EmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LicenseAssignments_SoftwareLicenseId_ReleasedAt] ON [LicenseAssignments] ([SoftwareLicenseId], [ReleasedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRecords_AssetId] ON [MaintenanceRecords] ([AssetId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MaintenanceRecords_VendorId] ON [MaintenanceRecords] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SoftwareLicenses_Name] ON [SoftwareLicenses] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SoftwareLicenses_VendorId] ON [SoftwareLicenses] ([VendorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261011023111_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261011023111_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

