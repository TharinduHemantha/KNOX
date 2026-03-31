/*
    SecureMultiTenantApiTemplate - SQL Server Database Schema
    Matches the generated starter solution structure for:
      - .NET 10 REST API
      - Clean Architecture + CQRS
      - ASP.NET Core Identity with GUID keys
      - Single database multi-tenancy using TenantId
      - EF Core writes + Dapper reads

    Notes:
      1. This script creates the core tables required by the current starter solution.
      2. It also includes two recommended extension tables:
           - RefreshTokens   (because the API issues refresh tokens)
           - AuditLogs       (because audit logging is a stated requirement)
      3. The starter code currently has not yet implemented persistence for refresh tokens or audit logs,
         but these tables are included so your database is ready for those next steps.
      4. The script is idempotent enough for first-time setup. Run in a new database.
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* =========================================================
   OPTIONAL: CREATE DATABASE
   Uncomment and edit if needed.
   =========================================================
-- CREATE DATABASE [SecureMultiTenantDb];
-- GO
-- USE [SecureMultiTenantDb];
-- GO
*/

/* =========================================================
   TENANTS
   ========================================================= */
IF OBJECT_ID(N'[dbo].[Tenants]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Tenants]
    (
        [Id]                UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Tenants] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        [Name]              NVARCHAR(200)    NOT NULL,
        [Subdomain]         NVARCHAR(100)    NOT NULL,
        [IsActive]          BIT              NOT NULL CONSTRAINT [DF_Tenants_IsActive] DEFAULT (1),
        [CreatedAtUtc]      DATETIMEOFFSET(7) NOT NULL CONSTRAINT [DF_Tenants_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CreatedBy]         NVARCHAR(450)    NULL,
        [LastModifiedAtUtc] DATETIMEOFFSET(7) NULL,
        [LastModifiedBy]    NVARCHAR(450)    NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT [DF_Tenants_IsDeleted] DEFAULT (0),
        [RowVersion]        ROWVERSION       NOT NULL
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Tenants_Subdomain' AND object_id = OBJECT_ID(N'[dbo].[Tenants]'))
BEGIN
    CREATE UNIQUE INDEX [UX_Tenants_Subdomain] ON [dbo].[Tenants]([Subdomain]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Tenants_IsDeleted' AND object_id = OBJECT_ID(N'[dbo].[Tenants]'))
BEGIN
    CREATE INDEX [IX_Tenants_IsDeleted] ON [dbo].[Tenants]([IsDeleted]);
END
GO

/* =========================================================
   ASP.NET CORE IDENTITY TABLES
   Customized names from AppDbContext:
     - Users
     - Roles
   Standard names retained for join/claim/login/token tables.
   ========================================================= */
IF OBJECT_ID(N'[dbo].[Roles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Roles]
    (
        [Id]                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Roles] PRIMARY KEY,
        [Name]               NVARCHAR(256)    NULL,
        [NormalizedName]     NVARCHAR(256)    NULL,
        [ConcurrencyStamp]   NVARCHAR(MAX)    NULL,
        [TenantId]           UNIQUEIDENTIFIER NULL,
        [IsSystemRole]       BIT              NOT NULL CONSTRAINT [DF_Roles_IsSystemRole] DEFAULT (0),
        CONSTRAINT [FK_Roles_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'RoleNameIndex' AND object_id = OBJECT_ID(N'[dbo].[Roles]'))
BEGIN
    CREATE UNIQUE INDEX [RoleNameIndex] ON [dbo].[Roles]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Roles_TenantId_Name' AND object_id = OBJECT_ID(N'[dbo].[Roles]'))
BEGIN
    CREATE INDEX [IX_Roles_TenantId_Name] ON [dbo].[Roles]([TenantId], [Name]);
END
GO

IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users]
    (
        [Id]                     UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Users] PRIMARY KEY,
        [UserName]               NVARCHAR(256)    NULL,
        [NormalizedUserName]     NVARCHAR(256)    NULL,
        [Email]                  NVARCHAR(256)    NULL,
        [NormalizedEmail]        NVARCHAR(256)    NULL,
        [EmailConfirmed]         BIT              NOT NULL,
        [PasswordHash]           NVARCHAR(MAX)    NULL,
        [SecurityStamp]          NVARCHAR(MAX)    NULL,
        [ConcurrencyStamp]       NVARCHAR(MAX)    NULL,
        [PhoneNumber]            NVARCHAR(MAX)    NULL,
        [PhoneNumberConfirmed]   BIT              NOT NULL,
        [TwoFactorEnabled]       BIT              NOT NULL,
        [LockoutEnd]             DATETIMEOFFSET(7) NULL,
        [LockoutEnabled]         BIT              NOT NULL,
        [AccessFailedCount]      INT              NOT NULL,

        [TenantId]               UNIQUEIDENTIFIER NOT NULL,
        [IsActive]               BIT              NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT (1),
        [IsSoftDeleted]          BIT              NOT NULL CONSTRAINT [DF_Users_IsSoftDeleted] DEFAULT (0),
        [AuthenticationSource]   NVARCHAR(50)     NOT NULL CONSTRAINT [DF_Users_AuthenticationSource] DEFAULT (N'Local'),
        [DisplayName]            NVARCHAR(256)    NULL,

        CONSTRAINT [FK_Users_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UserNameIndex' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE UNIQUE INDEX [UserNameIndex] ON [dbo].[Users]([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'EmailIndex' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE INDEX [EmailIndex] ON [dbo].[Users]([NormalizedEmail]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_TenantId_Email' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE INDEX [IX_Users_TenantId_Email] ON [dbo].[Users]([TenantId], [Email]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_TenantId_NormalizedUserName' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE INDEX [IX_Users_TenantId_NormalizedUserName] ON [dbo].[Users]([TenantId], [NormalizedUserName]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Users_TenantId_IsSoftDeleted' AND object_id = OBJECT_ID(N'[dbo].[Users]'))
BEGIN
    CREATE INDEX [IX_Users_TenantId_IsSoftDeleted] ON [dbo].[Users]([TenantId], [IsSoftDeleted]);
END
GO

IF OBJECT_ID(N'[dbo].[AspNetRoleClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetRoleClaims]
    (
        [Id]         INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY,
        [RoleId]     UNIQUEIDENTIFIER  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)     NULL,
        [ClaimValue] NVARCHAR(MAX)     NULL,
        CONSTRAINT [FK_AspNetRoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetRoleClaims_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetRoleClaims]'))
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [dbo].[AspNetRoleClaims]([RoleId]);
END
GO

IF OBJECT_ID(N'[dbo].[AspNetUserClaims]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserClaims]
    (
        [Id]         INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY,
        [UserId]     UNIQUEIDENTIFIER  NOT NULL,
        [ClaimType]  NVARCHAR(MAX)     NULL,
        [ClaimValue] NVARCHAR(MAX)     NULL,
        CONSTRAINT [FK_AspNetUserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserClaims_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserClaims]'))
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [dbo].[AspNetUserClaims]([UserId]);
END
GO

IF OBJECT_ID(N'[dbo].[AspNetUserLogins]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserLogins]
    (
        [LoginProvider]       NVARCHAR(450)    NOT NULL,
        [ProviderKey]         NVARCHAR(450)    NOT NULL,
        [ProviderDisplayName] NVARCHAR(MAX)    NULL,
        [UserId]              UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserLogins_UserId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserLogins]'))
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [dbo].[AspNetUserLogins]([UserId]);
END
GO

IF OBJECT_ID(N'[dbo].[AspNetUserRoles]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserRoles]
    (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID(N'[dbo].[AspNetUserRoles]'))
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [dbo].[AspNetUserRoles]([RoleId]);
END
GO

IF OBJECT_ID(N'[dbo].[AspNetUserTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AspNetUserTokens]
    (
        [UserId]        UNIQUEIDENTIFIER NOT NULL,
        [LoginProvider] NVARCHAR(450)    NOT NULL,
        [Name]          NVARCHAR(450)    NOT NULL,
        [Value]         NVARCHAR(MAX)    NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );
END
GO

/* =========================================================
   DOMAIN TABLES
   ========================================================= */
IF OBJECT_ID(N'[dbo].[Projects]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Projects]
    (
        [Id]                UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_Projects] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        [TenantId]          UNIQUEIDENTIFIER NOT NULL,
        [Name]              NVARCHAR(200)    NOT NULL,
        [Code]              NVARCHAR(50)     NOT NULL,
        [Description]       NVARCHAR(2000)   NULL,
        [IsArchived]        BIT              NOT NULL CONSTRAINT [DF_Projects_IsArchived] DEFAULT (0),
        [CreatedAtUtc]      DATETIMEOFFSET(7) NOT NULL CONSTRAINT [DF_Projects_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CreatedBy]         NVARCHAR(450)    NULL,
        [LastModifiedAtUtc] DATETIMEOFFSET(7) NULL,
        [LastModifiedBy]    NVARCHAR(450)    NULL,
        [IsDeleted]         BIT              NOT NULL CONSTRAINT [DF_Projects_IsDeleted] DEFAULT (0),
        [RowVersion]        ROWVERSION       NOT NULL,
        CONSTRAINT [FK_Projects_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Projects_TenantId_Code' AND object_id = OBJECT_ID(N'[dbo].[Projects]'))
BEGIN
    CREATE UNIQUE INDEX [UX_Projects_TenantId_Code] ON [dbo].[Projects]([TenantId], [Code]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_TenantId_IsDeleted' AND object_id = OBJECT_ID(N'[dbo].[Projects]'))
BEGIN
    CREATE INDEX [IX_Projects_TenantId_IsDeleted] ON [dbo].[Projects]([TenantId], [IsDeleted]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Projects_TenantId_IsArchived' AND object_id = OBJECT_ID(N'[dbo].[Projects]'))
BEGIN
    CREATE INDEX [IX_Projects_TenantId_IsArchived] ON [dbo].[Projects]([TenantId], [IsArchived]);
END
GO

/* =========================================================
   RECOMMENDED EXTENSION: REFRESH TOKENS
   The starter code returns refresh tokens, but persistence is not yet implemented.
   Add this table now so the next implementation step is straightforward.
   ========================================================= */
IF OBJECT_ID(N'[dbo].[RefreshTokens]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RefreshTokens]
    (
        [Id]                 UNIQUEIDENTIFIER  NOT NULL CONSTRAINT [PK_RefreshTokens] PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        [UserId]             UNIQUEIDENTIFIER  NOT NULL,
        [TenantId]           UNIQUEIDENTIFIER  NOT NULL,
        [TokenHash]          NVARCHAR(512)     NOT NULL,
        [ExpiresAtUtc]       DATETIMEOFFSET(7) NOT NULL,
        [CreatedAtUtc]       DATETIMEOFFSET(7) NOT NULL CONSTRAINT [DF_RefreshTokens_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CreatedByIp]        NVARCHAR(64)      NULL,
        [RevokedAtUtc]       DATETIMEOFFSET(7) NULL,
        [RevokedByIp]        NVARCHAR(64)      NULL,
        [ReplacedByTokenHash] NVARCHAR(512)    NULL,
        [ReasonRevoked]      NVARCHAR(256)     NULL,
        [IsUsed]             BIT               NOT NULL CONSTRAINT [DF_RefreshTokens_IsUsed] DEFAULT (0),
        [RowVersion]         ROWVERSION        NOT NULL,
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id]),
        CONSTRAINT [FK_RefreshTokens_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshTokens_UserId_TenantId' AND object_id = OBJECT_ID(N'[dbo].[RefreshTokens]'))
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId_TenantId] ON [dbo].[RefreshTokens]([UserId], [TenantId]);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_RefreshTokens_TokenHash' AND object_id = OBJECT_ID(N'[dbo].[RefreshTokens]'))
BEGIN
    CREATE UNIQUE INDEX [UX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens]([TokenHash]);
END
GO

/* =========================================================
   RECOMMENDED EXTENSION: AUDIT LOGS
   Matches your stated non-functional requirement for audit logging.
   ========================================================= */
IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLogs]
    (
        [Id]               BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AuditLogs] PRIMARY KEY,
        [TenantId]         UNIQUEIDENTIFIER     NULL,
        [UserId]           UNIQUEIDENTIFIER     NULL,
        [CorrelationId]    NVARCHAR(100)        NULL,
        [TraceIdentifier]  NVARCHAR(100)        NULL,
        [Action]           NVARCHAR(200)        NOT NULL,
        [EntityName]       NVARCHAR(200)        NULL,
        [EntityId]         NVARCHAR(100)        NULL,
        [Succeeded]        BIT                  NOT NULL,
        [DetailsJson]      NVARCHAR(MAX)        NULL,
        [CreatedAtUtc]     DATETIMEOFFSET(7)    NOT NULL CONSTRAINT [DF_AuditLogs_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CreatedByIp]      NVARCHAR(64)         NULL,
        CONSTRAINT [FK_AuditLogs_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_TenantId_CreatedAtUtc' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
BEGIN
    CREATE INDEX [IX_AuditLogs_TenantId_CreatedAtUtc] ON [dbo].[AuditLogs]([TenantId], [CreatedAtUtc] DESC);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLogs_UserId_CreatedAtUtc' AND object_id = OBJECT_ID(N'[dbo].[AuditLogs]'))
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId_CreatedAtUtc] ON [dbo].[AuditLogs]([UserId], [CreatedAtUtc] DESC);
END
GO

/* =========================================================
   SEED DATA - TENANT + ROLES + SUPER ADMIN USER
   IMPORTANT:
   - Replace the password hash with a real ASP.NET Core Identity password hash from your app.
   - Replace the email values before running in any real environment.
   - The seed uses a platform tenant for the initial SuperAdmin account.
   ========================================================= */
DECLARE @PlatformTenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @TenantAdminRoleId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @ApplicationUserRoleId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';
DECLARE @SuperAdminUserId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] = @PlatformTenantId)
BEGIN
    INSERT INTO [dbo].[Tenants]
    (
        [Id], [Name], [Subdomain], [IsActive], [CreatedAtUtc], [CreatedBy], [IsDeleted]
    )
    VALUES
    (
        @PlatformTenantId,
        N'Platform Tenant',
        N'platform',
        1,
        SYSUTCDATETIME(),
        N'system',
        0
    );
END
GO

DECLARE @PlatformTenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @TenantAdminRoleId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @ApplicationUserRoleId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Id] = @SuperAdminRoleId)
BEGIN
    INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [TenantId], [IsSystemRole])
    VALUES (@SuperAdminRoleId, N'SuperAdmin', N'SUPERADMIN', NEWID(), NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Id] = @TenantAdminRoleId)
BEGIN
    INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [TenantId], [IsSystemRole])
    VALUES (@TenantAdminRoleId, N'TenantAdmin', N'TENANTADMIN', NEWID(), NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [Id] = @ApplicationUserRoleId)
BEGIN
    INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp], [TenantId], [IsSystemRole])
    VALUES (@ApplicationUserRoleId, N'ApplicationUser', N'APPLICATIONUSER', NEWID(), NULL, 1);
END;
GO

DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';
DECLARE @TenantAdminRoleId UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333333';
DECLARE @ApplicationUserRoleId UNIQUEIDENTIFIER = '44444444-4444-4444-4444-444444444444';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'tenants.manage')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'tenants.manage');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'users.manage')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'users.manage');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'roles.manage')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'roles.manage');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.read')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'projects.read');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.write')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'projects.write');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @SuperAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.read.sensitive')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@SuperAdminRoleId, N'permission', N'projects.read.sensitive');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @TenantAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'users.manage')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@TenantAdminRoleId, N'permission', N'users.manage');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @TenantAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'roles.manage')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@TenantAdminRoleId, N'permission', N'roles.manage');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @TenantAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.read')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@TenantAdminRoleId, N'permission', N'projects.read');
IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @TenantAdminRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.write')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@TenantAdminRoleId, N'permission', N'projects.write');

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetRoleClaims] WHERE [RoleId] = @ApplicationUserRoleId AND [ClaimType] = N'permission' AND [ClaimValue] = N'projects.read')
    INSERT INTO [dbo].[AspNetRoleClaims] ([RoleId], [ClaimType], [ClaimValue]) VALUES (@ApplicationUserRoleId, N'permission', N'projects.read');
GO

/*
    IMPORTANT:
    Replace [PasswordHash] below with a real ASP.NET Core Identity password hash.
    Do not store plain text passwords in SQL scripts.

    Example way to generate the hash in a temporary console/app:
        var hasher = new PasswordHasher<ApplicationUser>();
        var hash = hasher.HashPassword(user, "YourStrongPasswordHere!");
*/
DECLARE @PlatformTenantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @SuperAdminUserId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Id] = @SuperAdminUserId)
BEGIN
    INSERT INTO [dbo].[Users]
    (
        [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail],
        [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp],
        [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount],
        [TenantId], [IsActive], [IsSoftDeleted], [AuthenticationSource], [DisplayName]
    )
    VALUES
    (
        @SuperAdminUserId,
        N'superadmin@platform.local',
        N'SUPERADMIN@PLATFORM.LOCAL',
        N'superadmin@platform.local',
        N'SUPERADMIN@PLATFORM.LOCAL',
        1,
        N'REPLACE_WITH_REAL_ASPNET_IDENTITY_PASSWORD_HASH',
        CONVERT(NVARCHAR(36), NEWID()),
        CONVERT(NVARCHAR(36), NEWID()),
        NULL,
        0,
        1,
        NULL,
        1,
        0,
        @PlatformTenantId,
        1,
        0,
        N'Local',
        N'Platform Super Admin'
    );
END
GO

DECLARE @SuperAdminUserId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = '22222222-2222-2222-2222-222222222222';

IF NOT EXISTS (SELECT 1 FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @SuperAdminUserId AND [RoleId] = @SuperAdminRoleId)
BEGIN
    INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId])
    VALUES (@SuperAdminUserId, @SuperAdminRoleId);
END
GO

/* =========================================================
   SAMPLE TENANT + SAMPLE PROJECT
   Useful for smoke testing the API after first startup.
   ========================================================= */
DECLARE @SampleTenantId UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';
DECLARE @SampleProjectId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] = @SampleTenantId)
BEGIN
    INSERT INTO [dbo].[Tenants]
    (
        [Id], [Name], [Subdomain], [IsActive], [CreatedAtUtc], [CreatedBy], [IsDeleted]
    )
    VALUES
    (
        @SampleTenantId,
        N'Contoso Tenant',
        N'contoso',
        1,
        SYSUTCDATETIME(),
        N'system',
        0
    );
END
GO

DECLARE @SampleTenantId UNIQUEIDENTIFIER = '66666666-6666-6666-6666-666666666666';
DECLARE @SampleProjectId UNIQUEIDENTIFIER = '77777777-7777-7777-7777-777777777777';

IF NOT EXISTS (SELECT 1 FROM [dbo].[Projects] WHERE [Id] = @SampleProjectId)
BEGIN
    INSERT INTO [dbo].[Projects]
    (
        [Id], [TenantId], [Name], [Code], [Description], [IsArchived],
        [CreatedAtUtc], [CreatedBy], [IsDeleted]
    )
    VALUES
    (
        @SampleProjectId,
        @SampleTenantId,
        N'Initial Sample Project',
        N'PRJ-001',
        N'Used to validate tenant-aware reads and writes.',
        0,
        SYSUTCDATETIME(),
        N'system',
        0
    );
END
GO

/* =========================================================
   POST-CREATION CHECKS
   ========================================================= */
SELECT 'Tenants' AS [TableName], COUNT(*) AS [RowCount] FROM [dbo].[Tenants]
UNION ALL
SELECT 'Roles', COUNT(*) FROM [dbo].[Roles]
UNION ALL
SELECT 'Users', COUNT(*) FROM [dbo].[Users]
UNION ALL
SELECT 'Projects', COUNT(*) FROM [dbo].[Projects]
UNION ALL
SELECT 'AspNetRoleClaims', COUNT(*) FROM [dbo].[AspNetRoleClaims];
GO


-- Entra JIT provisioning alignment patch
ALTER TABLE [dbo].[Users] ADD [EntraObjectId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Users] ADD [EntraTenantId] NVARCHAR(100) NULL;
ALTER TABLE [dbo].[Users] ADD [ExternalSubject] NVARCHAR(200) NULL;
ALTER TABLE [dbo].[Users] ADD [LastLoginAtUtc] DATETIMEOFFSET NULL;
GO
CREATE UNIQUE INDEX [IX_Users_TenantId_EntraObjectId]
    ON [dbo].[Users]([TenantId], [EntraObjectId])
    WHERE [EntraObjectId] IS NOT NULL;
GO


-- Invitation-based Entra onboarding
IF OBJECT_ID(N'dbo.EntraInvitations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.EntraInvitations
    (
        Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EntraInvitations PRIMARY KEY,
        TenantId UNIQUEIDENTIFIER NOT NULL,
        Email NVARCHAR(256) NOT NULL,
        NormalizedEmail NVARCHAR(256) NOT NULL,
        EntraTenantId NVARCHAR(128) NULL,
        RoleName NVARCHAR(256) NULL,
        InvitationCodeHash NVARCHAR(128) NOT NULL,
        InvitedByUserId UNIQUEIDENTIFIER NOT NULL,
        ExpiresAtUtc DATETIMEOFFSET NOT NULL,
        AcceptedAtUtc DATETIMEOFFSET NULL,
        AcceptedEntraObjectId NVARCHAR(128) NULL,
        IsRevoked BIT NOT NULL CONSTRAINT DF_EntraInvitations_IsRevoked DEFAULT(0),
        CreatedAtUtc DATETIMEOFFSET NOT NULL CONSTRAINT DF_EntraInvitations_CreatedAtUtc DEFAULT(SYSDATETIMEOFFSET())
    );

    CREATE INDEX IX_EntraInvitations_Tenant_Email_Status
        ON dbo.EntraInvitations (TenantId, NormalizedEmail, IsRevoked, AcceptedAtUtc);

    CREATE INDEX IX_EntraInvitations_ExpiresAtUtc
        ON dbo.EntraInvitations (ExpiresAtUtc);
END;
