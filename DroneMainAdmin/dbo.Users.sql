CREATE TABLE [dbo].[Users] (
    [UserID]          INT              IDENTITY (1, 1) NOT NULL,
    [FirstName]       VARCHAR (50)     NOT NULL,
    [MiddleName]      VARCHAR (50)     NULL,
    [LastName]        VARCHAR (50)     NOT NULL,
    [EmailID]         VARCHAR (254)    NOT NULL,
    [DateOfBirth]     DATETIME         NOT NULL,
    [Password]        NVARCHAR (52)    NOT NULL,
    [IsEmailVerified] BIT              NOT NULL,
    [ContactNo]       NUMERIC (14)     NOT NULL,
    [ActivationCode]  UNIQUEIDENTIFIER NOT NULL,
    [TeamName]        NVARCHAR (50)    NOT NULL,
    [CountryName]     NVARCHAR (50)    NOT NULL,
	[Decscript]       NVARCHAR (200)   NULL,
    [TermsAccepted ]  BIT              NOT NULL,
    [AdminType]       BIT              NOT NULL,
    [SubEmail]        BIT              NOT NULL,
    [Address1]        NVARCHAR (80)    NOT NULL,
	[Address2]        NVARCHAR (80)    NULL,
	[pincode]         NVARCHAR(20)     NOT NULL,
    PRIMARY KEY CLUSTERED ([UserID] ASC)
);

