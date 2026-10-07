using CsvHelper.Configuration.Attributes;

namespace Commerce.Api.Models;

public class CommerceCsvRow
{
    [Name("pc_processdate")]
    public DateTime PcProcessdate { get; set; }

    [Name("pc_nomcomred")]
    public string? PcNomcomred { get; set; }

    [Name("pc_numdoc")]
    public string? PcNumdoc { get; set; }

    [Name("nom_vehiculo")]
    public string? NomVehiculo { get; set; }

    [Name("marca")]
    public string? Marca { get; set; }

    [Name("modelo")]
    public string? Modelo { get; set; }

    [Name("anio")]
    public short? Anio { get; set; }

    [Name("placa")]
    public string? Placa { get; set; }

    [Name("color")]
    public string? Color { get; set; }

    [Name("tipo_vehiculo")]
    public string? TipoVehiculo { get; set; }

    [Name("kilometraje")]
    public int? Kilometraje { get; set; }

    [Name("precio")]
    public decimal? Precio { get; set; }

    [Name("estado_vehiculo")]
    public string? EstadoVehiculo { get; set; }
}