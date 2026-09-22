using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;

namespace MMagnetic.UsersService.Front;

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

    public async Task<List<Cliente>> ListarClientesAsync()
        => await _http.GetFromJsonAsync<List<Cliente>>("api/clientes") ?? new();

    public async Task<Cliente?> ObtenerClienteAsync(Guid clienteId)
        => await _http.GetFromJsonAsync<Cliente>($"api/clientes/{clienteId}");

    public Task<ResultadoApi<Cliente>> CrearClienteAsync(ClienteDto dto)
        => EnviarAsync<Cliente>(HttpMethod.Post, "api/clientes", dto);

    public Task<ResultadoApi<Cliente>> ActualizarClienteAsync(Guid clienteId, ClienteDto dto)
        => EnviarAsync<Cliente>(HttpMethod.Put, $"api/clientes/{clienteId}", dto);

    public async Task<bool> DesactivarClienteAsync(Guid clienteId)
    {
        var respuesta = await _http.DeleteAsync($"api/clientes/{clienteId}");
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarClientesMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/clientes/carga-masiva", archivo);

    // ---------------- Cotitulares ----------------

    public async Task<List<Cotitular>> ListarCotitularesAsync(Guid clienteId)
        => await _http.GetFromJsonAsync<List<Cotitular>>($"api/cotitulares/cliente/{clienteId}") ?? new();

    public Task<ResultadoApi<Cotitular>> CrearCotitularAsync(CotitularDto dto)
        => EnviarAsync<Cotitular>(HttpMethod.Post, "api/cotitulares", dto);

    public Task<ResultadoApi<Cotitular>> ActualizarCotitularAsync(Guid cotitularId, CotitularDto dto)
        => EnviarAsync<Cotitular>(HttpMethod.Put, $"api/cotitulares/{cotitularId}", dto);

    public async Task<bool> EliminarCotitularAsync(Guid cotitularId)
    {
        var respuesta = await _http.DeleteAsync($"api/cotitulares/{cotitularId}");
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarCotitularesMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/cotitulares/carga-masiva", archivo);

    // ---------------- Datos financieros ----------------

    public async Task<List<DatoFinanciero>> ListarDatosFinancierosAsync(Guid clienteId)
        => await _http.GetFromJsonAsync<List<DatoFinanciero>>($"api/datosfinancieros/cliente/{clienteId}") ?? new();

    public Task<ResultadoApi<DatoFinanciero>> CrearDatoFinancieroAsync(DatoFinancieroDto dto)
        => EnviarAsync<DatoFinanciero>(HttpMethod.Post, "api/datosfinancieros", dto);

    public Task<ResultadoApi<DatoFinanciero>> ActualizarDatoFinancieroAsync(Guid datoFinancieroId, DatoFinancieroDto dto)
        => EnviarAsync<DatoFinanciero>(HttpMethod.Put, $"api/datosfinancieros/{datoFinancieroId}", dto);

    public async Task<bool> EliminarDatoFinancieroAsync(Guid datoFinancieroId)
    {
        var respuesta = await _http.DeleteAsync($"api/datosfinancieros/{datoFinancieroId}");
        return respuesta.IsSuccessStatusCode;
    }

    public Task<ResultadoCargaMasiva?> CargarDatosFinancierosMasivoAsync(IBrowserFile archivo)
        => SubirArchivoAsync("api/datosfinancieros/carga-masiva", archivo);

    // ---------------- Formato 1019 ----------------

    public async Task<ResumenClasificacion?> ClasificarAsync(int periodoAno)
    {
        var respuesta = await _http.PostAsync($"api/formato1019/clasificar/{periodoAno}", content: null);
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResumenClasificacion>();
    }

    public async Task<List<Formato1019ConceptoDto>> ObtenerStagingAsync(int periodoAno)
        => await _http.GetFromJsonAsync<List<Formato1019ConceptoDto>>($"api/formato1019/{periodoAno}") ?? new();

    public async Task<List<ErrorFormato1019Dto>> ObtenerErroresAsync(int periodoAno)
        => await _http.GetFromJsonAsync<List<ErrorFormato1019Dto>>($"api/formato1019/errores/{periodoAno}") ?? new();

    public async Task<(byte[] Contenido, string NombreArchivo)> DescargarErroresAsync(int periodoAno)
    {
        var respuesta = await _http.GetAsync($"api/formato1019/errores/{periodoAno}/exportar");
        respuesta.EnsureSuccessStatusCode();

        var contenido = await respuesta.Content.ReadAsByteArrayAsync();
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"errores_formato1019_{periodoAno}.xlsx";
        return (contenido, nombreArchivo);
    }

    public async Task<List<Formato1019ConceptoDto>> ObtenerDefinitivoAsync(int periodoAno)
        => await _http.GetFromJsonAsync<List<Formato1019ConceptoDto>>($"api/formato1019/definitivo/{periodoAno}") ?? new();

    public async Task<ResultadoApi<(byte[] Contenido, string NombreArchivo)>> ExportarAsync(
        int periodoAno, int numEnvio, int codCpt, DateTime fecInicial, DateTime fecFinal)
    {
        var ruta = $"api/formato1019/exportar/{periodoAno}" +
                   $"?numEnvio={numEnvio}&codCpt={codCpt}" +
                   $"&fecInicial={fecInicial:yyyy-MM-dd}&fecFinal={fecFinal:yyyy-MM-dd}";

        var respuesta = await _http.GetAsync(ruta);
        if (!respuesta.IsSuccessStatusCode)
        {
            var errores = await respuesta.Content.ReadFromJsonAsync<List<Dictionary<string, string>>>();
            var mensajes = errores?.Select(e => e.GetValueOrDefault("mensaje", "Error desconocido")).ToList() ?? new();
            return new ResultadoApi<(byte[], string)> { Exitoso = false, Errores = mensajes };
        }

        var contenido = await respuesta.Content.ReadAsByteArrayAsync();
        var nombreArchivo = respuesta.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? $"formato1019_{periodoAno}.xml";
        return new ResultadoApi<(byte[], string)> { Exitoso = true, Dato = (contenido, nombreArchivo) };
    }

    // ---------------- Helpers ----------------

    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpMethod metodo, string ruta, object cuerpo)
    {
        var request = new HttpRequestMessage(metodo, ruta) { Content = JsonContent.Create(cuerpo) };
        var respuesta = await _http.SendAsync(request);

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
        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResultadoCargaMasiva>();
    }
}
