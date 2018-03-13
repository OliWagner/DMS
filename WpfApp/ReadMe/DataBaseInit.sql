CREATE TABLE [dbo].[OkoDokumentengruppen](
	[OkoDokumentengruppenId] [int] IDENTITY(1,1) NOT NULL,
	[Bezeichnung] [varchar](50) NOT NULL,
	[Beschreibung] [varchar](255) NULL,
 CONSTRAINT [PK_Dokumentengruppen] PRIMARY KEY CLUSTERED 
(
	[OkoDokumentengruppenId] ASC
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

CREATE TABLE [dbo].[OkoOcrImagesDokTypen](
	[OkoOcrImagesDokTypenId] [int] IDENTITY(1,1) NOT NULL,
	[ImageDatei] [varbinary](max) NULL,
	[DokumententypId] [int] NULL,
 CONSTRAINT [PK_OkoOcrImagesDokTypen] PRIMARY KEY CLUSTERED 
(
	[OkoOcrImagesDokTypenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoOcrInformationen](
	[OkoOcrInformationenId] [int] IDENTITY(1,1) NOT NULL,
	[RecX] [decimal](18, 0) NULL,
	[RecY] [decimal](18, 0) NULL,
	[RecW] [decimal](18, 0) NULL,
	[RecH] [decimal](18, 0) NULL,
	[DokumententypId] [int] NULL,
	[Feld] [varchar](50) NULL,
 CONSTRAINT [PK_OkoOcrInformationen] PRIMARY KEY CLUSTERED 
(
	[OkoOcrInformationenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[OkoDokumenteDaten](
	[OkoDokumenteDatenId] [int] IDENTITY(1,1) NOT NULL,
	[OkoDokumenteId] [int] NOT NULL,
	[OkoDokumentenTypId] [int] NOT NULL,
	[Tabelle] [varchar](50) NOT NULL,
	[IdInTabelle] [int] NOT NULL,
	[Titel] [varchar](50) NOT NULL,
	[Beschreibung] [varchar](255) NULL,
	[Dateiname] [varchar](50) NOT NULL,
	[ErfasstAm] [date] NULL,
 CONSTRAINT [PK_DokumenteDaten] PRIMARY KEY CLUSTERED 
(
	[OkoDokumenteDatenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[OkoDokumententyp](
	[OkoDokumententypId] [int] IDENTITY(1,1) NOT NULL,
	[Bezeichnung] [varchar](50) NOT NULL,
	[Beschreibung] [varchar](255) NULL,
	[OkoDokumentengruppenId] [int] NOT NULL,
 CONSTRAINT [PK_OkoDokumententyp] PRIMARY KEY CLUSTERED 
(
	[OkoDokumententypId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
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

CREATE TABLE [dbo].[OkoDokTypTabellenfeldtypen](
	[OkoDokTypTabellenfeldtypenId] [int] IDENTITY(1,1) NOT NULL,
	[Tabellenname] [varchar](50) NOT NULL,
	[CsvWertetypen] [varchar](max) NOT NULL,
	[CsvFeldnamen] [varchar](max) NOT NULL,
 CONSTRAINT [PK_OkoDokTypTabellenfeldtypen] PRIMARY KEY CLUSTERED 
(
	[OkoDokTypTabellenfeldtypenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
