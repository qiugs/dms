CREATE TABLE [dbo].[DCE_Templates] (
    [FileID]             UNIQUEIDENTIFIER NOT NULL,
    [Name]               NVARCHAR (200)   NOT NULL,
    [DocumentType]       NVARCHAR (200)   NOT NULL,
    [VirtualPath]        NVARCHAR (200)   NOT NULL,
    [DateTimeUploaded]   DATETIME         NOT NULL,
    [Description]        NVARCHAR (500)   NULL,
    [UserID]             UNIQUEIDENTIFIER NOT NULL,
    [DocumentOCRContent] TEXT             NULL,
    [SkipPages]          NVARCHAR (50)    NULL,
    CONSTRAINT [PK_DCETemplates] PRIMARY KEY CLUSTERED ([FileID] ASC),
    CONSTRAINT [FK_DCETemplates_Files] FOREIGN KEY ([FileID]) REFERENCES [dbo].[Files] ([ID])
);



CREATE TABLE [dbo].[DCE_Keywords] (
    [FileID]  UNIQUEIDENTIFIER NOT NULL,
    [UserID]  UNIQUEIDENTIFIER NOT NULL,
    [Keyword] NVARCHAR (200)   NOT NULL,
    [Rank]    REAL   NOT NULL,
    CONSTRAINT [PK_DCEKeywords] PRIMARY KEY CLUSTERED ([FileID] ASC, [UserID] ASC, [Keyword] ASC),
    CONSTRAINT [FK_DCEKeywords_DCETemplates] FOREIGN KEY ([FileID]) REFERENCES [dbo].[DCE_Templates] ([FileID])
);

CREATE TABLE [dbo].[FilesDownloadAuditTrails] (
    [FileID]             UNIQUEIDENTIFIER NOT NULL,
    [FileName]           NVARCHAR (50)    NOT NULL,
    [UserID]             UNIQUEIDENTIFIER NOT NULL,
    [UserName]           NVARCHAR (256)   NOT NULL,
    [DateTimeDownloaded] DATETIME         NOT NULL,
    CONSTRAINT [PK_FilesDownloadAuditTrails] PRIMARY KEY CLUSTERED ([FileID] ASC, [UserID] ASC, [DateTimeDownloaded] ASC)
);


CREATE TABLE [dbo].[UserLoginAuditTrails] (
    [UserID]         UNIQUEIDENTIFIER NOT NULL,
	[UserName]       NVARCHAR (256)   NOT NULL,
    [DateTimeLogged] DATETIME         NOT NULL,
    CONSTRAINT [PK_UserLoginAuditTrails] PRIMARY KEY CLUSTERED ([UserID] ASC, [DateTimeLogged] ASC)
);

CREATE TABLE [dbo].[FileBeenOCR] (
	[FileID]			UNIQUEIDENTIFIER NOT NULL,
	[DocumentType]       NVARCHAR (200)   NULL,
	[DateTimeRecognised] DATETIME         NOT NULL,
	CONSTRAINT [PK_FileBeenOCR] PRIMARY KEY CLUSTERED ([FileID] ASC),
	CONSTRAINT [FK_FileBeenOCR_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[Files] ([ID])
);

CREATE TABLE [dbo].[OCRFileExtension](
	[FileExtension]		NVARCHAR (10)   NOT NULL,
	[Remark]			NVARCHAR (10)   NULL,
	[DateTimeCreated] DATETIME         NOT NULL,
	CONSTRAINT [PK_OCRFileExtension] PRIMARY KEY CLUSTERED ([FileExtension] ASC)
);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.pdf','PDF Files', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.tif','TIF File', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.tiff','TIFF File', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.gif','GIF File', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.jpeg','JPEG File', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.jpg','JPG File', CURRENT_TIMESTAMP);

INSERT INTO [dbo].[OCRFileExtension] (FileExtension, Remark, DateTimeCreated)
VALUES ('.png','PNG File', CURRENT_TIMESTAMP);

CREATE TABLE [dbo].[RoleProfile](
	[UserId]					UNIQUEIDENTIFIER NOT NULL,
	[RoleId]					UNIQUEIDENTIFIER NOT NULL,
	[PropertyNames]				NTEXT            NOT NULL,
	[PropertyValuesString]      NTEXT            NOT NULL,
	[LastUpdateDate]            DATETIME         NOT NULL,
	CONSTRAINT [PK_RoleProfile] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
	CONSTRAINT [FK_RoleProfile_AspnetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]),
	CONSTRAINT [FK_RoleProfile_AspnetRoles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[aspnet_Roles] ([RoleId])
);

CREATE TABLE [dbo].[FeatureProfile] (
	[FeatureId]					UNIQUEIDENTIFIER NOT NULL,
	[FeatureName]				NVARCHAR (200)   NOT NULL,
	[FeatureRemarks]      		NTEXT            NULL,
	[LastUpdateDate]            DATETIME         NOT NULL,
	CONSTRAINT [PK_FeatureProfile] PRIMARY KEY CLUSTERED ([FeatureId] ASC)
);

CREATE TABLE [dbo].[FeatureAccessProfile](
	[FeatureAccessProfileId]	UNIQUEIDENTIFIER NOT NULL,
	[UserId]					UNIQUEIDENTIFIER NULL,
	[RoleId]					UNIQUEIDENTIFIER NULL,
	[FeatureId]					UNIQUEIDENTIFIER NULL,
	[LastUpdateDate]            DATETIME         NOT NULL,
	CONSTRAINT [PK_FeatureAccessProfile] PRIMARY KEY CLUSTERED ([FeatureAccessProfileId] ASC),
	CONSTRAINT [FK_FeatureAccessProfile_AspnetUsers] FOREIGN KEY ([UserId]) REFERENCES [dbo].[aspnet_Users] ([UserId]),
	CONSTRAINT [FK_FeatureAccessProfile_AspnetRoles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[aspnet_Roles] ([RoleId]),
	CONSTRAINT [FK_FeatureAccessProfile_FeatureProfile] FOREIGN KEY ([FeatureId]) REFERENCES [dbo].[FeatureProfile] ([FeatureId])
);

CREATE TABLE [dbo].[TagsOnFile] (
	[TagID]					UNIQUEIDENTIFIER NOT NULL,
	[FileID]				UNIQUEIDENTIFIER NOT NULL,
	[TagName]       		NVARCHAR (200)   NOT NULL,
	[LastModifiedDateTime] 	DATETIME         NOT NULL,
	CONSTRAINT [PK_TagsOnFile] PRIMARY KEY CLUSTERED ([TagID] ASC),
	CONSTRAINT [FK_TagsOnFile_File] FOREIGN KEY ([FileID]) REFERENCES [dbo].[Files] ([ID])
);











