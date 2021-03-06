USE [master]
GO
/****** Object:  Database [lmxLED]    Script Date: 05/07/2016 11:58:51 AM ******/
CREATE DATABASE [lmxLED]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'lmxLED_', FILENAME = N'F:\Database\lmxLED_.mdf' , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'lmxLED__log', FILENAME = N'F:\Database\lmxLED__log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [lmxLED] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [lmxLED].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [lmxLED] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [lmxLED] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [lmxLED] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [lmxLED] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [lmxLED] SET ARITHABORT OFF 
GO
ALTER DATABASE [lmxLED] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [lmxLED] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [lmxLED] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [lmxLED] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [lmxLED] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [lmxLED] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [lmxLED] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [lmxLED] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [lmxLED] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [lmxLED] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [lmxLED] SET  DISABLE_BROKER 
GO
ALTER DATABASE [lmxLED] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [lmxLED] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [lmxLED] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [lmxLED] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [lmxLED] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [lmxLED] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [lmxLED] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [lmxLED] SET RECOVERY FULL 
GO
ALTER DATABASE [lmxLED] SET  MULTI_USER 
GO
ALTER DATABASE [lmxLED] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [lmxLED] SET DB_CHAINING OFF 
GO
ALTER DATABASE [lmxLED] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [lmxLED] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'lmxLED', N'ON'
GO
USE [lmxLED]
GO
/****** Object:  Table [dbo].[Device]    Script Date: 05/07/2016 11:58:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Device](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [nvarchar](max) NOT NULL,
	[PostCode] [nvarchar](50) NULL,
	[AreaId] [nvarchar](max) NULL,
	[UserName] [nvarchar](50) NULL,
	[DeviceName] [nvarchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DisplaySetting]    Script Date: 05/07/2016 11:58:51 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DisplaySetting](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DeviceId] [nvarchar](max) NOT NULL,
	[Height] [nvarchar](50) NULL,
	[Width] [nvarchar](50) NULL,
	[Style] [nvarchar](50) NULL,
	[Animation] [nvarchar](50) NULL,
	[RegionTop] [nvarchar](50) NULL,
	[RegionLeft] [nvarchar](50) NULL,
	[Region2Top] [nvarchar](50) NULL,
	[Region2Left] [nvarchar](50) NULL,
	[ScheduleTime] [varchar](50) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
SET IDENTITY_INSERT [dbo].[Device] ON 

INSERT [dbo].[Device] ([Id], [DeviceId], [PostCode], [AreaId], [UserName], [DeviceName]) VALUES (1, N'SZLCCL0009', N'123456', N'http://180.211.159.172/price/1', N'szlccl', N'LED First Test')
INSERT [dbo].[Device] ([Id], [DeviceId], [PostCode], [AreaId], [UserName], [DeviceName]) VALUES (2, N'SZLCCL0008', N'123456', N'http://180.211.159.172/price/2', N'szlccl', N'LED Sencond Test')
INSERT [dbo].[Device] ([Id], [DeviceId], [PostCode], [AreaId], [UserName], [DeviceName]) VALUES (3, N'10', N'123', N'201', N'atib', N'LED TEst')
INSERT [dbo].[Device] ([Id], [DeviceId], [PostCode], [AreaId], [UserName], [DeviceName]) VALUES (4, N'12', N'369852', N'401', N'Atibur', N'China')
SET IDENTITY_INSERT [dbo].[Device] OFF
SET IDENTITY_INSERT [dbo].[DisplaySetting] ON 

INSERT [dbo].[DisplaySetting] ([Id], [DeviceId], [Height], [Width], [Style], [Animation], [RegionTop], [RegionLeft], [Region2Top], [Region2Left], [ScheduleTime]) VALUES (1, N'1', N'4', N'6', N'', N'', NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[DisplaySetting] ([Id], [DeviceId], [Height], [Width], [Style], [Animation], [RegionTop], [RegionLeft], [Region2Top], [Region2Left], [ScheduleTime]) VALUES (2, N'SZLCCL0008', N'50', N'500', N'', N'', NULL, NULL, NULL, NULL, N'10')
INSERT [dbo].[DisplaySetting] ([Id], [DeviceId], [Height], [Width], [Style], [Animation], [RegionTop], [RegionLeft], [Region2Top], [Region2Left], [ScheduleTime]) VALUES (3, N'SZLCCL0009', N'50', N'500', N'', N'', N'10', N'20', NULL, NULL, N'10')
INSERT [dbo].[DisplaySetting] ([Id], [DeviceId], [Height], [Width], [Style], [Animation], [RegionTop], [RegionLeft], [Region2Top], [Region2Left], [ScheduleTime]) VALUES (4, N'11', N'50', N'500', N'', N'', NULL, NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[DisplaySetting] OFF
USE [master]
GO
ALTER DATABASE [lmxLED] SET  READ_WRITE 
GO
