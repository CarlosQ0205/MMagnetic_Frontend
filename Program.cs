using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MMagnetic.UsersService.Front;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var usersApiBaseUrl = builder.Configuration["UsersApiBaseUrl"] ?? "https://localhost:7150/";
var exogenaApiBaseUrl = builder.Configuration["ExogenaApiBaseUrl"] ?? "https://localhost:7266/";

builder.Services.AddBlazoredLocalStorage();

// -------------------------------------------------------
// Autenticación: el JWT se guarda en localStorage y este handler lo adjunta
// automáticamente a toda petición hacia cualquiera de los dos backends.
// -------------------------------------------------------
builder.Services.AddTransient<JwtAuthorizationHandler>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// -------------------------------------------------------
// MMagnetic.UsersService (login, usuarios) y MMagnetic.ExogenaService
// (clientes, cotitulares, datos financieros, Formato 1019) son APIs
// separadas; cada servicio tipado recibe su propio HttpClient ya
// configurado (BaseAddress + JWT automático vía JwtAuthorizationHandler).
// -------------------------------------------------------
builder.Services.AddHttpClient<AuthService>(client => client.BaseAddress = new Uri(usersApiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddHttpClient<ExogenaApiClient>(client => client.BaseAddress = new Uri(exogenaApiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

await builder.Build().RunAsync();
