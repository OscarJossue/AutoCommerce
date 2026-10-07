using System.Data;
using Commerce.Api.Data;
using Commerce.Api.Models;
using Microsoft.Data.SqlClient;

namespace Commerce.Api.Repositories;

public class CommerceRepository
{
    private readonly DatabaseConnection _database;

    public CommerceRepository(DatabaseConnection database)
    {
        _database = database;
    }

    public async Task InsertAsync(CommerceCsvRow row)
    {
        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.sp_create_commerce",
            connection
        );

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@pc_processdate",
            SqlDbType.Date
        ).Value = row.PcProcessdate.Date;

        command.Parameters.Add(
            "@pc_nomcomred",
            SqlDbType.NVarChar,
            150
        ).Value = (object?)row.PcNomcomred ?? DBNull.Value;

        command.Parameters.Add(
            "@pc_numdoc",
            SqlDbType.VarChar,
            20
        ).Value = (object?)row.PcNumdoc ?? DBNull.Value;

        command.Parameters.Add(
            "@nom_vehiculo",
            SqlDbType.NVarChar,
            120
        ).Value = (object?)row.NomVehiculo ?? DBNull.Value;

        command.Parameters.Add(
            "@marca",
            SqlDbType.NVarChar,
            60
        ).Value = (object?)row.Marca ?? DBNull.Value;

        command.Parameters.Add(
            "@modelo",
            SqlDbType.NVarChar,
            60
        ).Value = (object?)row.Modelo ?? DBNull.Value;

        command.Parameters.Add(
            "@anio",
            SqlDbType.SmallInt
        ).Value = (object?)row.Anio ?? DBNull.Value;

        command.Parameters.Add(
            "@placa",
            SqlDbType.VarChar,
            10
        ).Value = (object?)row.Placa ?? DBNull.Value;

        command.Parameters.Add(
            "@color",
            SqlDbType.NVarChar,
            40
        ).Value = (object?)row.Color ?? DBNull.Value;

        command.Parameters.Add(
            "@tipo_vehiculo",
            SqlDbType.NVarChar,
            40
        ).Value = (object?)row.TipoVehiculo ?? DBNull.Value;

        command.Parameters.Add(
            "@kilometraje",
            SqlDbType.Int
        ).Value = (object?)row.Kilometraje ?? DBNull.Value;

        var precioParameter = command.Parameters.Add(
            "@precio",
            SqlDbType.Decimal
        );

        precioParameter.Precision = 12;
        precioParameter.Scale = 2;
        precioParameter.Value =
            (object?)row.Precio ?? DBNull.Value;

        command.Parameters.Add(
            "@estado_vehiculo",
            SqlDbType.VarChar,
            20
        ).Value = (object?)row.EstadoVehiculo ?? DBNull.Value;

        await command.ExecuteNonQueryAsync();
    }




    public async Task<int> ProcessAsync(DateTime processDate)
    {
        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(
            "dbo.sp_process_commerce",
            connection
        );

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(
            "@pc_processdate",
            SqlDbType.Date
        ).Value = processDate.Date;

        var result = await command.ExecuteScalarAsync();

        return result == null || result == DBNull.Value
            ? 0
            : Convert.ToInt32(result);
    }




    public async Task<List<CommerceQuarantineRow>> GetQuarantineAsync()
    {
        var records = new List<CommerceQuarantineRow>();

        await using var connection = _database.CreateConnection();
        await connection.OpenAsync();

        const string sql = """
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
            motivo
        FROM dbo.commerce_quarantine
        ORDER BY pc_processdate DESC, pc_nomcomred;
        """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            records.Add(new CommerceQuarantineRow
            {
                PcProcessdate = reader.GetDateTime(
                    reader.GetOrdinal("pc_processdate")
                ),

                PcNomcomred = reader["pc_nomcomred"] as string,

                PcNumdoc = reader["pc_numdoc"] as string,

                NomVehiculo = reader["nom_vehiculo"] as string,

                Marca = reader["marca"] as string,

                Modelo = reader["modelo"] as string,

                Anio = reader["anio"] == DBNull.Value
                    ? null
                    : Convert.ToInt16(reader["anio"]),

                Placa = reader["placa"] as string,

                Color = reader["color"] as string,

                TipoVehiculo = reader["tipo_vehiculo"] as string,

                Kilometraje = reader["kilometraje"] == DBNull.Value
                    ? null
                    : Convert.ToInt32(reader["kilometraje"]),

                Precio = reader["precio"] == DBNull.Value
                    ? null
                    : Convert.ToDecimal(reader["precio"]),

                EstadoVehiculo =
                    reader["estado_vehiculo"] as string,

                Motivo =
                    reader["motivo"]?.ToString() ?? string.Empty
            });
        }

        return records;
    }

}