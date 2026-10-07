using Microsoft.Data.SqlClient;

namespace Commerce.Api.Data;

public class DatabaseConnection
{
    private readonly IConfiguration _configuration;

    public DatabaseConnection(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection CreateConnection()
    {
        string? connectionString =
            _configuration.GetConnectionString("CommerceDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No se encontró la cadena de conexión CommerceDb."
            );
        }

        return new SqlConnection(connectionString);
    }
}