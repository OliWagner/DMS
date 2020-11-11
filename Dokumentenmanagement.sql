

USE Dokumentenmanagement;
GO
ALTER DATABASE Dokumentenmanagement
    SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
GO
USE Dokumentenmanagement;
GO




CREATE TABLE [dbo].[OkoEinstellungen](
	[id] [int] NOT NULL,
	[Ordner] [nvarchar](50) NULL,
	[DatenbearbeitungEinAus] [nvarchar](50) NULL
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoTabellenfeldtypen](
	[OkoTabellenfeldtypenId] [int] IDENTITY(1,1) NOT NULL,
	[Tabellenname] [varchar](50) NOT NULL,
	[CsvWertetypen] [varchar](max) NOT NULL,
	[CsvFeldnamen] [varchar](max) NOT NULL,
 CONSTRAINT [PK_OkoTabellenfeldtypen] PRIMARY KEY CLUSTERED 
(
	[OkoTabellenfeldtypenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoDokumentenTyp](
	[OkoDokumentenTypId] [int] IDENTITY(1,1) NOT NULL,
	[Titel] [varchar](50) NULL,
	[Text] [varchar](50) NULL,
	[Geburtstag] [datetime] NULL,
 CONSTRAINT [PK_OkoDokumentenTyp] PRIMARY KEY CLUSTERED 
(
	[OkoDokumentenTypId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoDokumenteDaten](
	[OkoDokumenteDatenId] [int] IDENTITY(1,1) NOT NULL,
	[OkoDokumenteId] [int] NOT NULL,
	[IdInTabelle] [int] NOT NULL,
	[ErfasstAm] [date] NULL,
	[Dateiname] [nvarchar](100) NULL,
 CONSTRAINT [PK_DokumenteDaten] PRIMARY KEY CLUSTERED 
(
	[OkoDokumenteDatenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoDokumente](
	[OkoDokumenteId] [int] IDENTITY(1,1) NOT NULL,
	[Dokument] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_Dokumente] PRIMARY KEY CLUSTERED 
(
	[OkoDokumenteId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoDokTypTabellenfeldtypen](
	[OkoDokTypTabellenfeldtypenId] [int] NOT NULL,
	[Tabellenname] [varchar](50) NOT NULL,
	[CsvWertetypen] [varchar](max) NOT NULL,
	[CsvFeldnamen] [varchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoAnwendungen](
	[OkoAnwendungenId] [int] IDENTITY(1,1) NOT NULL,
	[Dateiendung] [varchar](50) NULL,
	[Anwendung] [varchar](255) NULL,
 CONSTRAINT [PK_OkoAnwendungen] PRIMARY KEY CLUSTERED 
(
	[OkoAnwendungenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO