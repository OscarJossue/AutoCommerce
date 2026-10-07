USE CommerceDb;
GO

CREATE OR ALTER PROCEDURE dbo.sp_create_commerce
    @pc_processdate DATE,
    @pc_nomcomred NVARCHAR(150),
    @pc_numdoc VARCHAR(20),
    @nom_vehiculo NVARCHAR(120),
    @marca NVARCHAR(60),
    @modelo NVARCHAR(60),
    @anio SMALLINT,
    @placa VARCHAR(10),
    @color NVARCHAR(40),
    @tipo_vehiculo NVARCHAR(40),
    @kilometraje INT,
    @precio DECIMAL(12,2),
    @estado_vehiculo VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.commerce
    (
        pc_processdate,
        pc_nomcomred,
        pc_numdoc,
        nom_vehiculo,
        marca,
        modelo,
        anio,
        placa,
        color,
        tipo_vehiculo,
        kilometraje,
        precio,
        estado_vehiculo
    )
    VALUES
    (
        @pc_processdate,
        @pc_nomcomred,
        @pc_numdoc,
        @nom_vehiculo,
        @marca,
        @modelo,
        @anio,
        @placa,
        @color,
        @tipo_vehiculo,
        @kilometraje,
        @precio,
        @estado_vehiculo
    );
END;
GO


CREATE OR ALTER PROCEDURE dbo.sp_process_commerce
    @pc_processdate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.commerce_quarantine
        (
            pc_processdate,
            pc_nomcomred,
            pc_numdoc,
            nom_vehiculo,
            marca,
            modelo,
            anio,
            placa,
            color,
            tipo_vehiculo,
            kilometraje,
            precio,
            estado_vehiculo,
            motivo
        )
        SELECT
            pc_processdate,
            pc_nomcomred,
            pc_numdoc,
            nom_vehiculo,
            marca,
            modelo,
            anio,
            placa,
            color,
            tipo_vehiculo,
            kilometraje,
            precio,
            estado_vehiculo,

            CONCAT(
                CASE
                    WHEN NULLIF(LTRIM(RTRIM(pc_nomcomred)), '') IS NULL
                    THEN 'El nombre del comercio (nomcomred) se encuentra vacio. '
                    ELSE ''
                END,

                CASE
                    WHEN NULLIF(LTRIM(RTRIM(pc_numdoc)), '') IS NULL
                    THEN 'El numero de documento (numdoc) se encuentra vacio. '

                    WHEN pc_numdoc LIKE '%[^0-9]%'
                    THEN 'El numero de documento (numdoc) contiene letras o caracteres especiales. '

                    ELSE ''
                END
            )
        FROM dbo.commerce
        WHERE pc_processdate = @pc_processdate
          AND
          (
              NULLIF(LTRIM(RTRIM(pc_nomcomred)), '') IS NULL
              OR NULLIF(LTRIM(RTRIM(pc_numdoc)), '') IS NULL
              OR pc_numdoc LIKE '%[^0-9]%'
          );

        DECLARE @registros_cuarentena INT = @@ROWCOUNT;

        DELETE FROM dbo.commerce
        WHERE pc_processdate = @pc_processdate
          AND
          (
              NULLIF(LTRIM(RTRIM(pc_nomcomred)), '') IS NULL
              OR NULLIF(LTRIM(RTRIM(pc_numdoc)), '') IS NULL
              OR pc_numdoc LIKE '%[^0-9]%'
          );

        COMMIT TRANSACTION;

        SELECT @registros_cuarentena AS registros_cuarentena;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO