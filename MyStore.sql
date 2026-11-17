USE [master];
GO

-- 1. Tao Database MyStore neu chua ton tai
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MyStore')
BEGIN
    CREATE DATABASE [MyStore];
END
GO

USE [MyStore];
GO

-- 2. Xoa cac rang buoc khoa ngoai (Foreign Keys) neu da ton tai
IF OBJECT_ID(N'[dbo].[FK__OrderDeta__Order__5812160E]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrderDetail] DROP CONSTRAINT [FK__OrderDeta__Order__5812160E];
IF OBJECT_ID(N'[dbo].[FK__OrderDeta__Produ__571DF1D5]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[OrderDetail] DROP CONSTRAINT [FK__OrderDeta__Produ__571DF1D5];
IF OBJECT_ID(N'[dbo].[FK__Order__CustomerI__5441852A]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Order] DROP CONSTRAINT [FK__Order__CustomerI__5441852A];
IF OBJECT_ID(N'[dbo].[FK_Pro_Category]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Product] DROP CONSTRAINT [FK_Pro_Category];
IF OBJECT_ID(N'[dbo].[FK_User_Customer]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Customer] DROP CONSTRAINT [FK_User_Customer];
GO

-- 3. Xoa cac bang neu da ton tai theo thu tu phu thuoc nguoc
IF OBJECT_ID(N'[dbo].[OrderDetail]', 'U') IS NOT NULL DROP TABLE [dbo].[OrderDetail];
IF OBJECT_ID(N'[dbo].[Order]', 'U') IS NOT NULL DROP TABLE [dbo].[Order];
IF OBJECT_ID(N'[dbo].[Product]', 'U') IS NOT NULL DROP TABLE [dbo].[Product];
IF OBJECT_ID(N'[dbo].[Customer]', 'U') IS NOT NULL DROP TABLE [dbo].[Customer];
IF OBJECT_ID(N'[dbo].[Category]', 'U') IS NOT NULL DROP TABLE [dbo].[Category];
IF OBJECT_ID(N'[dbo].[User]', 'U') IS NOT NULL DROP TABLE [dbo].[User];
GO

-- 4. Tao bang [User]: quan ly tai khoan Khach hang va Admin
CREATE TABLE [dbo].[User] (
    [Username] NVARCHAR (255) NOT NULL,
    [Password] NCHAR (50)     NOT NULL,
    [UserRole] NCHAR (1)      NOT NULL, -- A: Admin, C: Customer
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Username] ASC)
);
GO

-- 5. Tao bang Customer: luu thong tin khach hang
CREATE TABLE [dbo].[Customer] (
    [CustomerID]      INT IDENTITY (1, 1) NOT NULL,
    [CustomerName]    NVARCHAR (MAX)      NOT NULL,
    [CustomerPhone]   NVARCHAR (15)       NOT NULL,
    [CustomerEmail]   NVARCHAR (MAX)      NULL,
    [CustomerAddress] NVARCHAR (MAX)      NULL,
    [Username]        NVARCHAR (255)      NOT NULL,
    CONSTRAINT [PK_Customer] PRIMARY KEY CLUSTERED ([CustomerID] ASC),
    CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
);
GO

-- 6. Tao bang Category: luu danh muc san pham
CREATE TABLE [dbo].[Category] (
    [CategoryID]   INT IDENTITY (1, 1) NOT NULL,
    [CategoryName] NVARCHAR (MAX)      NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
GO

-- 7. Tao bang Product: luu thong tin san pham
CREATE TABLE [dbo].[Product] (
    [ProductID]         INT IDENTITY (1, 1) NOT NULL,
    [CategoryID]        INT                 NOT NULL,
    [ProductName]       NVARCHAR (MAX)      NOT NULL,
    [ProductDecription] NVARCHAR (MAX)      NOT NULL,
    [ProductPrice]      DECIMAL (18, 2)     NOT NULL,
    [ProductImage]      NVARCHAR (MAX)      NULL,
    [Quantity]          INT                 NOT NULL DEFAULT (50),
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);
GO

-- 8. Tao bang [Order]: luu thong tin don hang cua khach hang
CREATE TABLE [dbo].[Order] (
    [OrderID]         INT IDENTITY (1, 1) NOT NULL,
    [CustomerID]      INT                 NOT NULL,
    [OrderDate]       DATE                NOT NULL,
    [TotalAmount]     DECIMAL (18, 2)     NOT NULL,
    [PaymentStatus]   NVARCHAR (MAX)      NULL,
    [AddressDelivery] NVARCHAR (MAX)      NOT NULL,
    [DeliveryMethod]  NVARCHAR (MAX)      NULL,
    [PaymentMethod]   NVARCHAR (MAX)      NULL,
    [OrderStatus]     NVARCHAR (50)       NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([OrderID] ASC),
    CONSTRAINT [FK__Order__CustomerI__5441852A] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
);
GO

-- 9. Tao bang OrderDetail: luu chi tiet cac san pham trong don hang
-- TotalPrice la cot tinh toan tu dong (Computed Column) phu hop voi StoreGeneratedPattern="Computed" cua EF
CREATE TABLE [dbo].[OrderDetail] (
    [ID]         INT IDENTITY (1, 1) NOT NULL,
    [ProductID]  INT                 NOT NULL,
    [OrderID]    INT                 NOT NULL,
    [Quantity]   INT                 NOT NULL,
    [UnitPrice]  DECIMAL (18, 2)     NOT NULL,
    [TotalPrice] AS ([Quantity] * [UnitPrice]) PERSISTED,
    CONSTRAINT [PK_OrderDetail] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK__OrderDeta__Produ__571DF1D5] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
    CONSTRAINT [FK__OrderDeta__Order__5812160E] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
);
GO

-- =========================================================================================
-- 10. CHEN DU LIEU KHOI TAO (SEED DATA)
-- Chi khoi tao tai khoan Admin duy nhat va danh muc co ban
-- =========================================================================================

-- Chen tai khoan Admin duy nhat vao bang [User]
INSERT INTO [dbo].[User] ([Username], [Password], [UserRole]) VALUES
(N'admin', N'123456', N'A');
GO

-- Chen danh muc san pham co ban vao bang Category
SET IDENTITY_INSERT [dbo].[Category] ON;
INSERT INTO [dbo].[Category] ([CategoryID], [CategoryName]) VALUES
(1, N'Sách Kỹ Năng - Kinh Doanh'),
(2, N'Sách Văn Học - Tiểu Thuyết'),
(3, N'Sách Công Nghệ - Lập Trình');
SET IDENTITY_INSERT [dbo].[Category] OFF;
GO
