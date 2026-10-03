IF DB_ID(N'PurchasingDb') IS NULL
BEGIN
    CREATE DATABASE PurchasingDb;
END;
GO

USE PurchasingDb;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NOT NULL CONSTRAINT DF_Products_Description DEFAULT N'',
        Price DECIMAL(18,2) NOT NULL,
        CONSTRAINT CK_Products_Price CHECK (Price > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Stock', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.Stocks', N'U') IS NULL
BEGIN
    EXEC sp_rename N'dbo.Stock', N'Stocks';
END;
GO

IF OBJECT_ID(N'dbo.Stocks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Stocks
    (
        ProductId INT NOT NULL CONSTRAINT PK_Stocks PRIMARY KEY,
        Quantity INT NOT NULL CONSTRAINT DF_Stocks_Quantity DEFAULT 0,
        CONSTRAINT FK_Stocks_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Stocks_Quantity CHECK (Quantity >= 0)
    );
END;
GO

DECLARE @Catalog TABLE
(
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL
);

INSERT INTO @Catalog (Name, Description, Price, Quantity)
VALUES
    (N'Lenovo ThinkPad T14 Gen 5 Intel', N'Ficha oficial: lenovo-thinkpad-t14-gen5-intel-spec.pdf', 1249.99, 8),
    (N'Lenovo ThinkPad T14 Gen 5 AMD', N'Ficha oficial: lenovo-thinkpad-t14-gen5-amd-spec.pdf', 1199.99, 5),
    (N'Lenovo ThinkPad T14s Gen 5', N'Ficha oficial: lenovo-thinkpad-t14s-gen5-spec.pdf', 1399.99, 3),
    (N'Lenovo ThinkPad T14 Gen 6 Intel', N'Ficha oficial: lenovo-thinkpad-t14-gen6-intel-spec.pdf', 1349.99, 6),
    (N'Lenovo ThinkPad T14 Gen 6 AMD', N'Ficha oficial: lenovo-thinkpad-t14-gen6-amd-spec.pdf', 1299.99, 4),
    (N'Lenovo ThinkPad T16 Gen 2 Intel', N'Ficha oficial: lenovo-thinkpad-t16-gen2-intel-spec.pdf', 1449.99, 2),
    (N'Lenovo ThinkPad T16 Gen 2 AMD', N'Ficha oficial: lenovo-thinkpad-t16-gen2-amd-spec.pdf', 1399.99, 1),
    (N'Lenovo ThinkPad X1 Carbon Gen 13', N'Ficha oficial: lenovo-thinkpad-x1-carbon-gen13-spec.pdf', 1799.99, 3),
    (N'Lenovo ThinkPad X1 Carbon Gen 12', N'Ficha oficial: lenovo-thinkpad-x1-carbon-gen12-spec.pdf', 1699.99, 2),
    (N'Lenovo ThinkPad X1 2-in-1 Gen 10', N'Ficha oficial: lenovo-thinkpad-x1-2-in-1-gen10-spec.pdf', 1899.99, 1),
    (N'Lenovo ThinkPad X1 Nano Gen 3', N'Ficha oficial: lenovo-thinkpad-x1-nano-gen3-spec.pdf', 1599.99, 2),
    (N'Lenovo ThinkPad X1 Extreme Gen 5', N'Ficha oficial: lenovo-thinkpad-x1-extreme-gen5-spec.pdf', 2199.99, 1),
    (N'Lenovo ThinkPad X13 Gen 4 Intel', N'Ficha oficial: lenovo-thinkpad-x13-gen4-intel-spec.pdf', 1299.99, 4),
    (N'Lenovo ThinkPad X13 Gen 4 AMD', N'Ficha oficial: lenovo-thinkpad-x13-gen4-amd-spec.pdf', 1249.99, 3),
    (N'Lenovo ThinkPad L14 Gen 5 Intel', N'Ficha oficial: lenovo-thinkpad-l14-gen5-intel-spec.pdf', 999.99, 7),
    (N'Lenovo ThinkPad L14 Gen 5 AMD', N'Ficha oficial: lenovo-thinkpad-l14-gen5-amd-spec.pdf', 949.99, 5),
    (N'Lenovo ThinkPad L16 Gen 2 Intel', N'Ficha oficial: lenovo-thinkpad-l16-gen2-intel-spec.pdf', 1099.99, 4),
    (N'Lenovo ThinkPad L16 Gen 2 AMD', N'Ficha oficial: lenovo-thinkpad-l16-gen2-amd-spec.pdf', 1049.99, 3),
    (N'Lenovo ThinkPad E14 Gen 6 Intel', N'Ficha oficial: lenovo-thinkpad-e14-gen6-intel-spec.pdf', 849.99, 9),
    (N'Lenovo ThinkPad E14 Gen 6 AMD', N'Ficha oficial: lenovo-thinkpad-e14-gen6-amd-spec.pdf', 799.99, 6),
    (N'Lenovo ThinkPad E16 Gen 2 Intel', N'Ficha oficial: lenovo-thinkpad-e16-gen2-intel-spec.pdf', 899.99, 5),
    (N'Lenovo ThinkPad E16 Gen 2 AMD', N'Ficha oficial: lenovo-thinkpad-e16-gen2-amd-spec.pdf', 849.99, 4),
    (N'HP EliteBook 1040 G11', N'Ficha oficial: hp-elitebook-1040-g11-c08908273.pdf', 1749.99, 2),
    (N'HP EliteBook 860 G11', N'Ficha oficial: hp-elitebook-860-g11-c08903319.pdf', 1599.99, 3),
    (N'HP EliteBook 860 G10', N'Ficha oficial: hp-elitebook-860-g10-c08504817.pdf', 1399.99, 2),
    (N'HP EliteBook 840 G11', N'Ficha oficial: hp-elitebook-840-g11-c08903294.pdf', 1499.99, 4),
    (N'HP EliteBook 840 G10', N'Ficha oficial: hp-elitebook-840-g10-c08488438.pdf', 1299.99, 3),
    (N'HP EliteBook 640 G11', N'Ficha oficial: hp-elitebook-640-g11-c08953478.pdf', 1099.99, 5),
    (N'HP EliteBook 645 G11', N'Ficha oficial: hp-elitebook-645-g11-c08953476.pdf', 1049.99, 4),
    (N'HP ProBook 440 G11', N'Ficha oficial: hp-probook-440-g11-c08947328.pdf', 899.99, 6),
    (N'HP ProBook 440 G10', N'Ficha oficial: hp-probook-440-g10-c08480164.pdf', 799.99, 2),
    (N'HP ProBook 450 G10', N'Ficha oficial: hp-probook-450-g10-c08480165.pdf', 849.99, 3),
    (N'Dell Latitude 7x50 Series', N'Ficha oficial: dell-latitude-7x50-spec-sheet.pdf', 999.99, 4),
    (N'Dell Latitude E6430', N'Ficha oficial: dell-latitude-e6430-spec-sheet.pdf', 399.99, 1),
    (N'Dell Latitude 14 E5450', N'Ficha oficial: dell-latitude-14-e5450-spec-sheet.pdf', 449.99, 2),
    (N'Dell Latitude 5280, 5480 y 5580', N'Ficha oficial: dell-latitude-5280-5480-5580-spec-sheet.pdf', 499.99, 3),
    (N'Dell Latitude 7275 2-in-1', N'Ficha oficial: dell-latitude-7275-spec-sheet.pdf', 549.99, 1),
    (N'Dell Latitude 5285 2-in-1', N'Ficha oficial: dell-latitude-5285-2-in-1-spec-sheet.pdf', 599.99, 2),
    (N'Dell Precision 7720', N'Ficha oficial: dell-precision-7720-spec-sheet.pdf', 899.99, 1),
    (N'Dell Precision 5520', N'Ficha oficial: dell-precision-5520-spec-sheet.pdf', 799.99, 2),
    (N'Samsung Galaxy Book Go NP340XLA', N'Ficha oficial: samsung-galaxy-book-go-np340xla-datasheet.pdf', 499.99, 5),
    (N'Samsung Galaxy Book Go NP340XLA-K06BR', N'Ficha oficial: samsung-galaxy-book-go-np340xlak06-spec.pdf', 449.99, 3);

INSERT INTO dbo.Products (Name, Description, Price)
SELECT catalog.Name, catalog.Description, catalog.Price
FROM @Catalog catalog
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products product WHERE product.Name = catalog.Name);

INSERT INTO dbo.Stocks (ProductId, Quantity)
SELECT product.Id, catalog.Quantity
FROM dbo.Products product
JOIN @Catalog catalog ON catalog.Name = product.Name
WHERE NOT EXISTS (SELECT 1 FROM dbo.Stocks stock WHERE stock.ProductId = product.Id);
GO
