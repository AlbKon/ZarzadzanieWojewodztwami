using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using System.Collections.Generic;
using System.Drawing;
using WebApplication1.Entities;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Dodajemy serwis bazy danych
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("InMemoryDatabase");
});

// Dodajemy serwis dla kontrolera
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<WojewodztwoService>();
builder.Services.AddScoped<PowiatService>();
builder.Services.AddScoped<GminaService>();
builder.Services.AddScoped<MiastoService>();

var app = builder.Build();

// Dodajemy obs³ugê Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zarzadzanie Wojewodztwami API v.1.WSB"));

// Inicjalizacja bazy danych
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

//Gety - pobieranie list
app.MapGet("/api/wojewodztwa", (WojewodztwoService wojewodztwoService) =>
    {
        var listaWojewodztw = wojewodztwoService.GetAll();
        return Results.Ok(listaWojewodztw);
    });

app.MapGet("/api/powiaty", (PowiatService powiatService) =>
{
    var listaPowiatow = powiatService.GetAll();
    return Results.Ok(listaPowiatow);
});

app.MapGet("/api/gminy", (GminaService gminaService) =>
{
    var listaGmin = gminaService.GetAll();
    return Results.Ok(listaGmin);
});

app.MapGet("/api/miasta", (MiastoService miastoService) =>
{
    var listaMiast = miastoService.GetAll();
    return Results.Ok(listaMiast);
});

//Posty - nowe obiekty
app.MapPost("/api/wojewodztwo", async (WojewodztwoService wojewodztwoService, Wojewodztwo wojewodztwo) =>
{
    wojewodztwoService.Create(wojewodztwo);
    return Results.Created($"/api/wojewodztwa", wojewodztwo);
});

app.MapPost("/api/powiat", async (PowiatService powiatService, Powiat powiat) =>
{
    powiatService.Create(powiat);
    return Results.Created($"/api/powiaty/", powiat);
});

app.MapPost("/api/gmina", async (GminaService gminaService, Gmina gmina) =>
{
    gminaService.Create(gmina);
    return Results.Created($"/api/gminy", gmina);
});

app.MapPost("/api/miasto", async (MiastoService miastoService, Miasto miasto) =>
{
    miastoService.Create(miasto);
    return Results.Created($"/api/miasta", miasto);
});

//Posty - ustalanie stolic
app.MapPost("/api/wojewodztwo/setcapital", async (WojewodztwoService wojewodztwoService, AppDbContext dbContext, int wojewodztwoId, int miastoId) =>
{
    wojewodztwoService.SetCapital(wojewodztwoId, miastoId);
    return Results.Ok($"Ustawiono stolicê.");
});

app.MapPost("/api/powiat/setcapital", async (PowiatService powiatService, int powiatId, int miastoId) =>
{
    powiatService.SetCapital(powiatId, miastoId);
    return Results.Ok($"Ustawiono stolicê.");
});

app.MapPost("/api/gmina/setcapital", async (GminaService gminaService, int gminaId, int miastoId) =>
{
    gminaService.SetCapital(gminaId, miastoId);
    return Results.Ok($"Ustawiono stolicê.");
});

//Delety - usuwamy obiekty

app.MapDelete("/api/wojewodztwo", async (WojewodztwoService wojewodztwoService, AppDbContext dbContext, int idWojewodztwa) =>
{
    wojewodztwoService.Delete(idWojewodztwa);
    return Results.Ok("Poprawnie usuniêto wojewodztwo.");
});

app.MapDelete("/api/powiat", async (PowiatService powiatService, int idPowiatu) =>
{
    powiatService.Delete(idPowiatu);
    return Results.Ok("Poprawnie usuniêto powiat.");
});

app.MapDelete("/api/gmina", async (GminaService gminaService, int idGminy) =>
{
    gminaService.Delete(idGminy);
    return Results.Ok("Poprawnie usuniêto gminê.");
});

app.MapDelete("/api/miasto", async (MiastoService miastoService, int idMiasta) =>
{
    miastoService.Delete(idMiasta);
    return Results.Ok("Poprawnie usuniêto miasto.");
});

app.Run();