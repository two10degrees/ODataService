CREATE TABLE [dbo].[Segment](
	[SegmentId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[ConnectionString] [varchar](255) NOT NULL,
	[Query] [varchar](max) NOT NULL,
 CONSTRAINT [PK_Segment] PRIMARY KEY CLUSTERED 
(
	[SegmentId] ASC
))