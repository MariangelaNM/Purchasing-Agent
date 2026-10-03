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

IF OBJECT_ID(N'dbo.Stock', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Stock
    (
        ProductId INT NOT NULL CONSTRAINT PK_Stock PRIMARY KEY,
        Quantity INT NOT NULL CONSTRAINT DF_Stock_Quantity DEFAULT 0,
        CONSTRAINT FK_Stock_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Stock_Quantity CHECK (Quantity >= 0)
    );
END;
GO

INSERT INTO dbo.Products (Name, Description, Price)
SELECT data.Name, data.Description, data.Price
FROM (VALUES
    (N'Laptop Lenovo ThinkPad E14', N'Laptop empresarial, Intel Core i5, 16 GB RAM y SSD de 512 GB', 899.99),
    (N'Laptop HP Pavilion 15', N'Laptop para oficina y estudio, Intel Core i7, 16 GB RAM y SSD de 1 TB', 1099.99),
    (N'Laptop ASUS TUF Gaming F15', N'Laptop gamer, Intel Core i7, 16 GB RAM, SSD de 512 GB y GPU RTX 4060', 1399.99),
    (N'PC Dell OptiPlex 7010', N'Computadora de escritorio, Intel Core i5, 16 GB RAM y SSD de 512 GB', 749.99),
    (N'Monitor LG UltraGear 27', N'Monitor gamer de 27 pulgadas, resolución QHD y frecuencia de 144 Hz', 329.99),
    (N'Monitor Dell P2422H', N'Monitor profesional de 24 pulgadas, resolución Full HD y panel IPS', 219.99),
    (N'Teclado mecánico Logitech G413', N'Teclado mecánico USB con retroiluminación para oficina y gaming', 79.99),
    (N'Mouse Logitech MX Master 3S', N'Mouse inalámbrico ergonómico con sensor de alta precisión', 99.99),
    (N'Memoria RAM Kingston 16 GB DDR4', N'Módulo de memoria DDR4 a 3200 MHz para computadora de escritorio', 44.99),
    (N'SSD Samsung 980 1 TB', N'Unidad SSD NVMe M.2 de 1 TB para actualización de equipos', 89.99),
    (N'Disco externo Seagate 2 TB', N'Disco duro externo USB 3.0 de 2 TB para respaldos', 64.99),
    (N'Webcam Logitech C920', N'Cámara web Full HD con micrófono integrado para videoconferencias', 69.99)
) AS data(Name, Description, Price)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products product WHERE product.Name = data.Name);
GO

INSERT INTO dbo.Stock (ProductId, Quantity)
SELECT product.Id, CASE product.Name
    WHEN N'Laptop Lenovo ThinkPad E14' THEN 8
    WHEN N'Laptop HP Pavilion 15' THEN 4
    WHEN N'Laptop ASUS TUF Gaming F15' THEN 2
    WHEN N'PC Dell OptiPlex 7010' THEN 11
    WHEN N'Monitor LG UltraGear 27' THEN 6
    WHEN N'Monitor Dell P2422H' THEN 3
    WHEN N'Teclado mecánico Logitech G413' THEN 18
    WHEN N'Mouse Logitech MX Master 3S' THEN 1
    WHEN N'Memoria RAM Kingston 16 GB DDR4' THEN 24
    WHEN N'SSD Samsung 980 1 TB' THEN 9
    WHEN N'Disco externo Seagate 2 TB' THEN 0
    WHEN N'Webcam Logitech C920' THEN 5
    ELSE 0
END
FROM dbo.Products product
WHERE NOT EXISTS (SELECT 1 FROM dbo.Stock stock WHERE stock.ProductId = product.Id);
GO
