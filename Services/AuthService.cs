using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace MMagnetic.UsersService.Front;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService localStorage, JwtAuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return new LoginResult(false, null, await response.Content.ReadAsStringAsync());

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result is null) return new LoginResult(false, null, "Invalid login response");

        await _localStorage.SetItemAsync("authToken", result.Token);
        await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
        _authStateProvider.NotificarSesionIniciada(result.Token);

        return new LoginResult(true, result.Token, null);
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _authStateProvider.NotificarCierreSesion();
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/usuarios/registrar", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        return string.IsNullOrWhiteSpace(token) ? null : await _http.GetFromJsonAsync<UserInfo>("api/auth/me");
    }

    public record LoginRequest(string TipoDocumento, string NumeroDocumento, string Password);
    public record RegisterRequest(string TipoDocumento, string NumeroDocumento, string PrimerNombre, string PrimerApellido, string CorreoElectronico, string Password);
    public record LoginResponse(string Token, string RefreshToken);
    public record UserInfo(Guid UsuarioId, string TipoDocumento, string NumeroDocumento, string PrimerNombre, string? SegundoNombre, string PrimerApellido, string? SegundoApellido, string? CorreoElectronico, string? Telefono);
    public record LoginResult(bool IsSuccess, string? Token = null, string? Error = null);
}
