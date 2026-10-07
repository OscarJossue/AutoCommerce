using System.Globalization;
using System.Text.RegularExpressions;
using Commerce.Api.Data;
using Commerce.Api.Models;
using Commerce.Api.Repositories;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Commerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommerceController : ControllerBase
{
    private readonly DatabaseConnection _database;
    private readonly CommerceRepository _repository;

    public CommerceController(
        DatabaseConnection database,
        CommerceRepository repository)
    {
        _database = database;
        _repository = repository;
    }

    // Comprueba la conexion con SQL Server
    [HttpGet("database-status")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            await using var connection = _database.CreateConnection();

            await connection.OpenAsync();

            return Ok(new
            {
                status = "OK",
                message = "Conexion con SQL Server realizada correctamente.",
                server = connection.DataSource,
                database = connection.Database
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                message = ex.Message
            });
        }
    }


    // Carga el archivo commerce_DDMMYYYY.csv
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCsv(
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message = "El archivo CSV se encuentra vacio."
            });
        }

        var fileName = Path.GetFileName(file.FileName);

        // Ejemplo valido:
        // commerce_07102026.csv
        var validFileName =
            Regex.IsMatch(
                fileName,
                @"^commerce_\d{8}\.csv$",
                RegexOptions.IgnoreCase
            );

        if (!validFileName)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message =
                    "Nombre de archivo invalido. " +
                    "Debe utilizar el formato commerce_DDMMYYYY.csv."
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var configuration =
                new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    TrimOptions = TrimOptions.Trim,

                    PrepareHeaderForMatch = args =>
                        args.Header.Trim().ToLowerInvariant()
                };

            using var csv =
                new CsvReader(reader, configuration);

            var records =
                csv.GetRecords<CommerceCsvRow>().ToList();

            if (records.Count == 0)
            {
                return BadRequest(new
                {
                    status = "ERROR",
                    message =
                        "El archivo no contiene registros para procesar."
                });
            }

            foreach (var record in records)
            {
                await _repository.InsertAsync(record);
            }

            return Ok(new
            {
                status = "OK",
                message =
                    "Archivo CSV cargado correctamente.",
                archivo = fileName,
                registrosInsertados = records.Count
            });
        }
        catch (HeaderValidationException ex)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message =
                    "La estructura del CSV no es valida.",
                detalle = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                message =
                    "Ocurrio un error procesando el archivo.",
                detalle = ex.Message
            });
        }
    }







    //POST
    [HttpPost("process")]
    public async Task<IActionResult> ProcessCommerce(
    [FromBody] ProcessCommerceRequest request)
    {
        if (request.ProcessDate == default)
        {
            return BadRequest(new
            {
                status = "ERROR",
                message = "Debe ingresar una fecha de proceso valida."
            });
        }

        try
        {
            var quarantineCount =
                await _repository.ProcessAsync(request.ProcessDate);

            return Ok(new
            {
                status = "OK",
                message = "Procesamiento realizado correctamente.",
                fechaProceso =
                    request.ProcessDate.ToString("yyyy-MM-dd"),
                registrosCuarentena = quarantineCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                message =
                    "Ocurrio un error durante el procesamiento.",
                detalle = ex.Message
            });
        }
    }



    [HttpGet("quarantine")]
    public async Task<IActionResult> GetQuarantine()
    {
        try
        {
            var records =
                await _repository.GetQuarantineAsync();

            return Ok(new
            {
                status = "OK",
                total = records.Count,
                data = records
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "ERROR",
                message =
                    "Ocurrio un error consultando la cuarentena.",
                detalle = ex.Message
            });
        }
    }





}