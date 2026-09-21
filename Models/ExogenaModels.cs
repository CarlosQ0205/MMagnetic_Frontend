namespace MMagnetic.UsersService.Front;

// Espejo de los DTOs/entidades de MMagnetic.ExogenaService, del lado del frontend.

public class ClienteDto
{
    public string? TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? CodigoPais { get; set; }
    public string? CodigoDepartamento { get; set; }
    public string? CodigoMunicipio { get; set; }
    public int? DigitoVerificacion { get; set; }
}

public class Cliente
{
    public Guid ClienteId { get; set; }
    public string? TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? PrimerNombre { get; set; }
    public string? SegundoNombre { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public int? PaisId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? MunicipioId { get; set; }
    public bool EsActivo { get; set; }
}

public class CotitularDto
{
    public string NumeroDocumentoCliente { get; set; } = string.Empty;
    public string? TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public decimal? Participacion { get; set; }
    public string? RelacionConCliente { get; set; }
}

public class Cotitular
{
    public Guid CotitularId { get; set; }
    public Guid ClienteId { get; set; }
    public string? TipoDocumento { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public decimal? Participacion { get; set; }
    public string? RelacionConCliente { get; set; }
}

public class DatoFinancieroDto
{
    public string NumeroDocumentoCliente { get; set; } = string.Empty;
    public string? TipoProducto { get; set; }
    public string? Entidad { get; set; }
    public string? Observaciones { get; set; }
    public decimal? Saldo { get; set; }
    public decimal? IngresosAnuales { get; set; }
    public decimal? EgresosAnuales { get; set; }
    public decimal? Activos { get; set; }
    public decimal? Pasivos { get; set; }
    public decimal? Patrimonio { get; set; }
    public int? PeriodoAno { get; set; }
    public string? NumeroCuenta { get; set; }
    public byte? TipoCuenta { get; set; }
    public byte? CodigoExencion { get; set; }
    public decimal? PromedioSaldoFinal { get; set; }
    public decimal? MedianaSaldoDiario { get; set; }
    public decimal? SaldoMaximo { get; set; }
    public decimal? SaldoMinimo { get; set; }
    public decimal? ValorMovCredito { get; set; }
    public int? NumMovCredito { get; set; }
    public decimal? PromedioMovCredito { get; set; }
    public decimal? MedianaMovCredito { get; set; }
    public decimal? ValorMovDebito { get; set; }
    public int? NumMovDebito { get; set; }
    public decimal? PromedioMovDebito { get; set; }
}

public class DatoFinanciero : DatoFinancieroDto
{
    public Guid DatoFinancieroId { get; set; }
    public Guid ClienteId { get; set; }
}

public record ResultadoCargaMasiva(int TotalFilas, int Exitosas, int ConErrores, List<FilaErrorCarga> Errores);
public record FilaErrorCarga(int Fila, string Mensaje);

public record ResumenClasificacion(int TotalRegistros, int Validos, int ConErrores);

public record ErrorFormato1019Dto(
    Guid ErrorId, Guid? ClienteId, string? NumeroDocumentoCliente,
    string CodigoError, string? DescripcionError, string? ValorInvalido, string? Nivel, DateTime FechaError);

public record Formato1019DefinitivoDto(
    Guid Formato1019DefinitivoId, Guid? ClienteId, int PeriodoAno, byte? PeriodoMes,
    string CodigoConcepto, decimal Valor, string? Descripcion, string? Linea, DateTime FechaGeneracion);

/// <summary>Resultado genérico de una operación de alta/edición: o hay dato, o hay lista de errores.</summary>
public class ResultadoApi<T>
{
    public bool Exitoso { get; init; }
    public T? Dato { get; init; }
    public List<string> Errores { get; init; } = new();
}
