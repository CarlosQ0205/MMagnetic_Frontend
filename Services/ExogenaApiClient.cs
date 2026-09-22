using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace MMagnetic.UsersService.Front;

/// <summary>Se lanza cuando el backend responde 401: el JWT expiró o no es válido. Las páginas la capturan para mostrar "vuelve a iniciar sesión" en vez del error crudo de HttpClient.</summary>
public class SesionExpiradaException : Exception
{
    public SesionExpiradaException() : base("Tu sesión expiró. Por favor inicia sesión de nuevo.")
    {
    }
}

/// <summary>Cliente HTTP hacia MMagnetic.ExogenaService (Clientes, Cotitulares, Datos_Financieros, Formato 1019).</summary>
public class ExogenaApiClient
{
    private const long TamanoMaximoArchivo = 20_000_000;

    private readonly HttpClient _http;

    public ExogenaApiClient(HttpClient http)
    {
        _http = http;
    }

    // ---------------- Clientes ----------------

    public Task<List<Cliente>> ListarClientesAsync()
        => ObtenerAsync<List<Cliente>>("api/clientes", new());

    public Task<Cliente?> ObtenerClienteAsync(Guid clienteId)
        => ObtenerAsync<Cliente?>($"api/clientes/{clienteId}", null);

    public Task<ResultadoApi<Cliente>> CrearClienteAsync(ClienteDto dto)
        => EnviarAsync<Cliente>(HttpMethod.Post, "api/clientes", dto);

    public Task<ResultadoApi<Cliente>> ActualizarClienteAsync(Guid clienteId, ClienteDto dto)
        => EnviarAsync<Cliente>(HttpMethod.Put, $"api/clientes/{clienteId}", dto);

    public async Task<bool> DesactivarClienteAsync(Guid clienteId)
    {
        var respuesta = await _http.DeleteAsync($"api/clientes/{clienteId}");
        await LanzarSiSesionExpiroAsync(respuesta);
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarClientesMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/clientes/carga-masiva", archivo);

    // ---------------- Cotitulares ----------------

    public Task<List<Cotitular>> ListarCotitularesAsync(Guid clienteId)
        => ObtenerAsync<List<Cotitular>>($"api/cotitulares/cliente/{clienteId}", new());

    public Task<ResultadoApi<Cotitular>> CrearCotitularAsync(CotitularDto dto)
        => EnviarAsync<Cotitular>(HttpMethod.Post, "api/cotitulares", dto);

    public Task<ResultadoApi<Cotitular>> ActualizarCotitularAsync(Guid cotitularId, CotitularDto dto)
        => EnviarAsync<Cotitular>(HttpMethod.Put, $"api/cotitulares/{cotitularId}", dto);

    public async Task<bool> EliminarCotitularAsync(Guid cotitularId)
    {
        var respuesta = await _http.DeleteAsync($"api/cotitulares/{cotitularId}");
        await LanzarSiSesionExpiroAsync(respuesta);
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarCotitularesMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/cotitulares/carga-masiva", archivo);

    // ---------------- Datos financieros ----------------

    public Task<List<DatoFinanciero>> ListarDatosFinancierosAsync(Guid clienteId)
        => ObtenerAsync<List<DatoFinanciero>>($"api/datosfinancieros/cliente/{clienteId}", new());

    public Task<ResultadoApi<DatoFinanciero>> CrearDatoFinancieroAsync(DatoFinancieroDto dto)
        => EnviarAsync<DatoFinanciero>(HttpMethod.Post, "api/datosfinancieros", dto);

    public Task<ResultadoApi<DatoFinanciero>> ActualizarDatoFinancieroAsync(Guid datoFinancieroId, DatoFinancieroDto dto)
        => EnviarAsync<DatoFinanciero>(HttpMethod.Put, $"api/datosfinancieros/{datoFinancieroId}", dto);

    public async Task<bool> EliminarDatoFinancieroAsync(Guid datoFinancieroId)
    {
        var respuesta = await _http.DeleteAsync($"api/datosfinancieros/{datoFinancieroId}");
        await LanzarSiSesionExpiroAsync(respuesta);
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarDatosFinancierosMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/datosfinancieros/carga-masiva", archivo);

    // ---------------- Formato 1019 ----------------

    public async Task<ResumenClasificacion?> ClasificarAsync(int periodoAno)
    {
        var respuesta = await _http.PostAsync($"api/formato1019/clasificar/{periodoAno}", content: null);
        await LanzarSiSesionExpiroAsync(respuesta);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResumenClasificacion>();
    }

    public Task<List<Formato1019ConceptoDto>> ObtenerStagingAsync(int periodoAno)
        => ObtenerAsync<List<Formato1019ConceptoDto>>($"api/formato1019/{periodoAno}", new());

    public Task<(byte[] Contenido, string NombreArchivo)> DescargarStagingAsync(int periodoAno)
        => DescargarArchivoAsync($"api/formato1019/{periodoAno}/exportar", $"formato1019_{periodoAno}.xlsx");

    public Task<List<ErrorFormato1019Dto>> ObtenerErroresAsync(int periodoAno)
        => ObtenerAsync<List<ErrorFormato1019Dto>>($"api/formato1019/errores/{periodoAno}", new());

    public Task<(byte[] Contenido, string NombreArchivo)> DescargarErroresAsync(int periodoAno)
        => DescargarArchivoAsync($"api/formato1019/errores/{periodoAno}/exportar", $"errores_formato1019_{periodoAno}.xlsx");

    public Task<List<Formato1019ConceptoDto>> ObtenerDefinitivoAsync(int periodoAno)
        => ObtenerAsync<List<Formato1019ConceptoDto>>($"api/formato1019/definitivo/{periodoAno}", new());

    public Task<(byte[] Contenido, string NombreArchivo)> DescargarDefinitivoAsync(int periodoAno)
        => DescargarArchivoAsync($"api/formato1019/definitivo/{periodoAno}/exportar", $"f1019_definitivo_{periodoAno}.xlsx");

    public async Task LimpiarPeriodoAsync(int periodoAno)
    {
        var respuesta = await _http.DeleteAsync($"api/formato1019/{periodoAno}");
        await LanzarSiSesionExpiroAsync(respuesta);
        respuesta.EnsureSuccessStatusCode();
    }

    public Task<ResultadoCargaMasiva?> CargarCorregidosF1019Async(IBrowserFile archivo)
        => SubirArchivoAsync("api/formato1019/corregidos", archivo);

    /// <summary>
    /// Descarga F_1019_Definitivo del período como uno o varios .xml (máximo 5000 "movcta"
    /// cada uno, comprimidos en un .zip), listos para pasar por el pre-validador de la DIAN.
    /// </summary>
    public Task<(byte[] Contenido, string NombreArchivo)> DescargarLotesXmlAsync(int periodoAno)
        => DescargarArchivoAsync($"api/formato1019/definitivo/{periodoAno}/lotes/xml", $"f1019_definitivo_{periodoAno}_xml.zip");

    /// <summary>Igual que <see cref="DescargarLotesXmlAsync"/> pero en Excel.</summary>
    public Task<(byte[] Contenido, string NombreArchivo)> DescargarLotesExcelAsync(int periodoAno)
        => DescargarArchivoAsync($"api/formato1019/definitivo/{periodoAno}/lotes/excel", $"f1019_definitivo_{periodoAno}_excel.zip");

    // ---------------- Helpers ----------------

    private async Task<T> ObtenerAsync<T>(string ruta, T valorPorDefecto)
    {
        var respuesta = await _http.GetAsync(ruta);
        await LanzarSiSesionExpiroAsync(respuesta);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<T>() ?? valorPorDefecto;
    }

    private async Task<(byte[] Contenido, string NombreArchivo)> DescargarArchivoAsync(string ruta, string nombrePorDefecto)
    {
        var respuesta = await _http.GetAsync(ruta);
        await LanzarSiSesionExpiroAsync(respuesta);
        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content.ReadAsByteArrayAsync();
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? nombrePorDefecto;
        return (contenido, nombreArchivo);
    }

    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object cuerpo)
    {
        var request = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(cuerpo) };
        var respuesta = await _http.SendAsync(request);
        await LanzarSiSesionExpiroAsync(respuesta);

        if (respuesta.IsSuccessStatusCode)
        {
            var dato = await respuesta.Content.ReadFromJsonAsync<T>();
            return new ResultadoApi<T> { Exitoso = true, Dato = dato };
        }

        var errores = await respuesta.Content.ReadFromJsonAsync<List<string>>() ?? new() { await respuesta.Content.ReadAsStringAsync() };
        return new ResultadoApi<T> { Exitoso = false, Errores = errores };
    }

    private async Task<ResultadoCargaMasiva?> SubirArchivoAsync(string ruta, IBrowserFile archivo)
    {
        using var contenido = new MultipartFormDataContent();
        await using var stream = archivo.OpenReadStream(TamanoMaximoArchivo);
        using var streamContent = new StreamContent(stream);
        contenido.Add(streamContent, "archivo", archivo.Name);

        var respuesta = await _http.PostAsync(ruta, contenido);
        await LanzarSiSesionExpiroAsync(respuesta);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResultadoCargaMasiva>();
    }

    private static Task LanzarSiSesionExpiroAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            throw new SesionExpiradaException();

        return Task.CompletedTask;
    }
}
