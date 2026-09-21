using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace MMagnetic.UsersService.Front;

/// <summary>
/// Traduce el JWT guardado en localStorage en un ClaimsPrincipal, para que
/// [Authorize]/AuthorizeView/@context.User funcionen sin depender de una llamada
/// al backend en cada navegación.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _localStorage;

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        return string.IsNullOrWhiteSpace(token) ? Anonimo : ConstruirEstado(token);
    }

    public void NotificarSesionIniciada(string token)
        => NotifyAuthenticationStateChanged(Task.FromResult(ConstruirEstado(token)));

    public void NotificarCierreSesion()
        => NotifyAuthenticationStateChanged(Task.FromResult(Anonimo));

    private static AuthenticationState ConstruirEstado(string token)
    {
        var identity = new ClaimsIdentity(ParsearClaims(token), authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    private static IEnumerable<Claim> ParsearClaims(string jwt)
    {
        var partes = jwt.Split('.');
        if (partes.Length < 2)
            return Enumerable.Empty<Claim>();

        var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(partes[1]));
        var valores = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson) ?? new();

        var claims = new List<Claim>();
        foreach (var (clave, valor) in valores)
        {
            if (valor.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in valor.EnumerateArray())
                    claims.Add(new Claim(clave, item.ToString()));
            }
            else
            {
                claims.Add(new Claim(clave, valor.ToString()));
            }
        }

        return claims;
    }

    private static byte[] Base64UrlDecode(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
