namespace Commerce.Api.Models;

public class CommerceQuarantineRow
{
    public DateTime PcProcessdate { get; set; }

    public string? PcNomcomred { get; set; }

    public string? PcNumdoc { get; set; }

    public string? NomVehiculo { get; set; }

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public short? Anio { get; set; }

    public string? Placa { get; set; }

    public string? Color { get; set; }

    public string? TipoVehiculo { get; set; }

    public int? Kilometraje { get; set; }

    public decimal? Precio { get; set; }

    public string? EstadoVehiculo { get; set; }

    public string Motivo { get; set; } = string.Empty;
}