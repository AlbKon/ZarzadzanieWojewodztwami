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

var app = builder.Build();

// Dodajemy obs³ugê Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));

// Inicjalizacja bazy danych
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

    // Dodajemy

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

app.MapGet("/api/gminy", (AppDbContext dbContext) =>
{
    var listaGmin = dbContext.listaGmin.ToList();
    return Results.Ok(listaGmin);
});

app.MapGet("/api/miasta", (AppDbContext dbContext) =>
{
    var listaMiast = dbContext.listaMiast.ToList();
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

app.MapPost("/api/gmina", async (AppDbContext dbContext, Gmina gmina) =>
{
    var powiat = dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
    if (powiat == null) { return Results.NotFound("Nie znaleziono takiego powiatu."); }

    dbContext.listaGmin.Add(gmina);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/api/gminy", gmina);
});

app.MapPost("/api/miasto", async (AppDbContext dbContext, Miasto miasto) =>
{
    var gmina = dbContext.listaGmin.FirstOrDefault(g => g.Id == miasto.GminaId);
    if (gmina == null) { return Results.NotFound("Nie znaleziono takiej gminy."); }

    dbContext.listaMiast.Add(miasto);

    var powiat = dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);

    gmina.Population += miasto.Population;
    powiat.Population += miasto.Population;
    wojewodztwo.Population += miasto.Population;

    await dbContext.SaveChangesAsync();
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

app.MapPost("/api/gmina/setcapital", async (AppDbContext dbContext, int gminaId, int miastoId) =>
{
    var gmina = dbContext.listaGmin.FirstOrDefault(g => g.Id == gminaId);
    var miasto = dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

    if (gmina == null) { return Results.BadRequest("Nie ma takiej gminy!"); }
    if (miasto == null) { return Results.BadRequest("Nie ma takiego miasta!"); }
    if (gmina.MiastoId != null) { return Results.BadRequest("Gmina ma ju¿ stolicê!"); }
    if (miasto.isCapital) { return Results.BadRequest("Miasto jest ju¿ stolic¹!"); }

    gmina.MiastoId = miastoId;
    miasto.isCapital = true;

    await dbContext.SaveChangesAsync();
    return Results.Ok($"{miasto.Name} zosta³o stolic¹ {gmina.Name}.");
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

app.MapDelete("/api/gmina", async (AppDbContext dbContext, int idGminy) =>
{
    var gmina = dbContext.listaGmin.FirstOrDefault(g => g.Id == idGminy);

    if (gmina == null) { return Results.NotFound("Nie znaleziono takiej gminy!"); }

    var powiat = dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.Id);

    wojewodztwo.Population -= gmina.Population;
    powiat.Population -= gmina.Population;

    var miastaDoUsuniecia = dbContext.listaMiast.Where(m => m.GminaId == gmina.Id).ToList();

    // Usuwanie miast
    dbContext.listaMiast.RemoveRange(miastaDoUsuniecia);

    // Usuwanie gmin
    dbContext.listaGmin.Remove(gmina);

    await dbContext.SaveChangesAsync();
    return Results.Ok("Poprawnie usuniêto gminê.");
});

app.MapDelete("/api/miasto", async (AppDbContext dbContext, int idMiasta) =>
{
    var miasto = dbContext.listaMiast.FirstOrDefault(m => m.Id == idMiasta);

    if (miasto == null) { return Results.NotFound("Nie znaleziono takiego miasta!"); }

    dbContext.Remove(miasto);

    await dbContext.SaveChangesAsync();
    return Results.Ok("Poprawnie usuniêto miasto.");
});

app.Run();

// Modele
public class Wojewodztwo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Population { get; set; }
    public ICollection<Powiat> listaPowiatow { get; set; } = new List<Powiat>();
    public int? MiastoId { get; set; }
}

public class Powiat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Population { get; set; }
    public int WojewodztwoId { get; set; }
    public ICollection<Gmina> listaGmin { get; set; } = new List<Gmina>();
    public int? MiastoId { get; set; }
}

public class Gmina
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Population { get; set; }
    public int PowiatId { get; set; }
    public ICollection<Gmina> listaMiast { get; set; } = new List<Gmina>();
    public int? MiastoId { get; set; }
}
public class Miasto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Population { get; set; }
    public bool isCapital { get; set; }
    public int GminaId { get; set; }
}

// Kontekst bazy danych
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Wojewodztwo> listaWojewodztw { get; set; }
    public DbSet<Powiat> listaPowiatow { get; set; }
    public DbSet<Gmina> listaGmin { get; set; }
    public DbSet<Miasto> listaMiast { get; set; }
}





//Dodaj serwisy dla gmin i miasta, wywal modele do osobnego folderu