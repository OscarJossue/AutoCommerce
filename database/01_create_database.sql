IF DB_ID('CommerceDb') IS NULL
BEGIN
    CREATE DATABASE CommerceDb;
END;
GO

USE CommerceDb;
GO

IF OBJECT_ID('dbo.commerce_quarantine', 'U') IS NOT NULL
    DROP TABLE dbo.commerce_quarantine;
GO

IF OBJECT_ID('dbo.commerce', 'U') IS NOT NULL
    DROP TABLE dbo.commerce;
GO

CREATE TABLE dbo.commerce
(
    pc_processdate DATE NOT NULL,
    pc_nomcomred NVARCHAR(150) NULL,
    pc_numdoc VARCHAR(20) NULL,
    nom_vehiculo NVARCHAR(120) NULL,
    marca NVARCHAR(60) NULL,
    modelo NVARCHAR(60) NULL,
    anio SMALLINT NULL,
    placa VARCHAR(10) NULL,
    color NVARCHAR(40) NULL,
    tipo_vehiculo NVARCHAR(40) NULL,
    kilometraje INT NULL,
    precio DECIMAL(12,2) NULL,
    estado_vehiculo VARCHAR(20) NULL
);
GO

CREATE TABLE dbo.commerce_quarantine
(
    pc_processdate DATE NOT NULL,
    pc_nomcomred NVARCHAR(150) NULL,
    pc_numdoc VARCHAR(20) NULL,
    nom_vehiculo NVARCHAR(120) NULL,
    marca NVARCHAR(60) NULL,
    modelo NVARCHAR(60) NULL,
    anio SMALLINT NULL,
    placa VARCHAR(10) NULL,
    color NVARCHAR(40) NULL,
    tipo_vehiculo NVARCHAR(40) NULL,
    kilometraje INT NULL,
    precio DECIMAL(12,2) NULL,
    estado_vehiculo VARCHAR(20) NULL,
    motivo NVARCHAR(500) NOT NULL
);
GO