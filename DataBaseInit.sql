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

CREATE TABLE [dbo].[OkoDokumenteDaten](
	[OkoDokumenteDatenId] [int] IDENTITY(1,1) NOT NULL,
	[OkoDokumenteId] [int] NOT NULL,
	[OkoDokumentenTypId] [int] NOT NULL,
	[Tabelle] [varchar](50) NOT NULL,
	[IdInTabelle] [int] NOT NULL,
	[Titel] [varchar](50) NOT NULL,
	[Beschreibung] [varchar](255) NULL,
	[Dateiname] [varchar](50) NOT NULL,
	[Dateigroesse] [int] NULL,
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
	[Tabelle] [varchar](50) NULL,
 CONSTRAINT [PK_OkoDokumententyp] PRIMARY KEY CLUSTERED 
(
	[OkoDokumententypId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[Tabellenfeldtypen](
	[TabellenfeldtypenId] [int] IDENTITY(1,1) NOT NULL,
	[Tabellenname] [varchar](50) NOT NULL,
	[CsvWertetypen] [varchar](max) NOT NULL,
	[CsvFeldnamen] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Tabellenfeldtypen] PRIMARY KEY CLUSTERED 
(
	[TabellenfeldtypenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[DokTypTabellenfeldtypen](
	[DokTypTabellenfeldtypenId] [int] IDENTITY(1,1) NOT NULL,
	[Tabellenname] [varchar](50) NOT NULL,
	[CsvWertetypen] [varchar](max) NOT NULL,
	[CsvFeldnamen] [varchar](max) NOT NULL,
 CONSTRAINT [PK_DokTypTabellenfeldtypen] PRIMARY KEY CLUSTERED 
(
	[DokTypTabellenfeldtypenId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
