using ManejoPresupuesto.Interface;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));
});
builder.Services.AddTransient<ITipoCuentasReposritory, TipoCuentaRepository>();
builder.Services.AddTransient<IServicioUsuario, ServicioUsuario>();
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<ICuentaRepository, CuentaRepository>();
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<ITransaccionesRepository, TransaccionesRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IReportesRepository, ReportesRepository>();
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>().AddErrorDescriber<MensajesDeErrorIdentity>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;

}).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/usuario/login";
});

builder.Services.AddAutoMapper(_ => { }, typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
