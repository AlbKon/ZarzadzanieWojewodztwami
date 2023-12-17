using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Dodajemy serwis bazy danych
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("InMemoryDatabase");
});

// Dodajemy serwis dla kontrolera
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapGet("/api/wojewodztwa", (AppDbContext dbContext) =>
{
    var listaWojewodztw = dbContext.listaWojewodztw.Include(m => m.listaPowiatow).ToList();
    return Results.Ok(listaWojewodztw);
});

app.MapGet("/api/powiaty", (AppDbContext dbContext) =>
{
    var listaPowiatow = dbContext.listaPowiatow.Include(m => m.listaGmin).ToList();
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
app.MapPost("/api/wojewodztwo", async (AppDbContext dbContext, Wojewodztwo wojewodztwo) =>
{
    dbContext.listaWojewodztw.Add(wojewodztwo);
    await dbContext.SaveChangesAsync();
    return Results.Created($"Województwo: {wojewodztwo.Name} zosta³o stworzone.", wojewodztwo);
});

app.MapPost("/api/powiat", async (AppDbContext dbContext, Powiat powiat) =>
{
    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);
    if( wojewodztwo == null) { return Results.NotFound("Nie znaleziono takiego województwa."); }

    dbContext.listaPowiatow.Add(powiat);
    await dbContext.SaveChangesAsync();
    return Results.Created($"Powiat: {powiat.Id} zosta³ stworzony.", powiat);
});

app.MapPost("/api/gmina", async (AppDbContext dbContext, Gmina gmina) =>
{
    var powiat = dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
    if (powiat == null) { return Results.NotFound("Nie znaleziono takiego powiatu."); }

    dbContext.listaGmin.Add(gmina);
    await dbContext.SaveChangesAsync();
    return Results.Created($"Gmina: {gmina.Id} zosta³a stworzona.", gmina);
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
    return Results.Created($"Miasto: {miasto.Id} zosta³o stworzone.", miasto);
});

//Posty - ustalanie stolic----------------------------------------------------------------------------------------------------------------------
app.MapPost("/api/wojewodztwo/setcapital", async (AppDbContext dbContext, int wojewodztwoId, int miastoId) =>
{
    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == wojewodztwoId);
    var miasto = dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

    if(wojewodztwo == null) { return Results.BadRequest("Nie ma takiego województwa!"); }
    if(miasto == null) { return Results.BadRequest("Nie ma takiego miasta!"); }
    if(wojewodztwo.MiastoId != null) { return Results.BadRequest("Wojewodztwo ma ju¿ stolicê!"); }
    if(miasto.isCapital) { return Results.BadRequest("Miasto jest ju¿ stolic¹!"); }

    wojewodztwo.MiastoId = miastoId;
    miasto.isCapital = true;

    await dbContext.SaveChangesAsync();
    return Results.Ok($"{miasto.Name} zosta³o stolic¹ {wojewodztwo.Name}.");
});

app.MapPost("/api/powiat/setcapital", async (AppDbContext dbContext, int powiatId, int miastoId) =>
{
    var powiat = dbContext.listaPowiatow.FirstOrDefault(p => p.Id == powiatId);
    var miasto = dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

    if (powiat == null) { return Results.BadRequest("Nie ma takiego powiatu!"); }
    if (miasto == null) { return Results.BadRequest("Nie ma takiego miasta!"); }
    if (powiat.MiastoId != null) { return Results.BadRequest("Powiat ma ju¿ stolicê!"); }
    if (miasto.isCapital) { return Results.BadRequest("Miasto jest ju¿ stolic¹!"); }

    powiat.MiastoId = miastoId;
    miasto.isCapital = true;

    await dbContext.SaveChangesAsync();
    return Results.Ok($"{miasto.Name} zosta³o stolic¹ {powiat.Name}.");
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

app.MapDelete("/api/wojewodztwo", async (AppDbContext dbContext, int idWojewodztwa) =>
{
    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == idWojewodztwa);


    if (wojewodztwo==null) { return Results.NotFound("Nie znaleziono takiego wojewodztwa!"); }
    // Pobieranie wszystkich powiatów, gmin i miast zwi¹zanych z województwem
    var powiatyDoUsuniecia = dbContext.listaPowiatow.Where(p => p.WojewodztwoId == idWojewodztwa).ToList();
    var gminyDoUsuniecia = dbContext.listaGmin
                                    .Where(g => powiatyDoUsuniecia
                                    .Select(p => p.Id)
                                    .Contains(g.PowiatId))
                                    .ToList();
    var miastaDoUsuniecia = dbContext.listaMiast
                                     .Where(m => gminyDoUsuniecia
                                     .Select(m=> m.Id)
                                     .Contains(m.GminaId))
                                     .ToList();

    // Usuwanie miast
    dbContext.listaMiast.RemoveRange(miastaDoUsuniecia);

    // Usuwanie gmin
    dbContext.listaGmin.RemoveRange(gminyDoUsuniecia);

    // Usuwanie powiatów
    dbContext.listaPowiatow.RemoveRange(powiatyDoUsuniecia);

    // Usuwanie województwa
    dbContext.listaWojewodztw.Remove(wojewodztwo);

    await dbContext.SaveChangesAsync();
    return Results.Ok("Poprawnie usuniêto wojewodztwo.");
});

app.MapDelete("/api/powiat", async (AppDbContext dbContext, int idPowiatu) =>
{
    var powiat = dbContext.listaPowiatow.FirstOrDefault(p=> p.Id == idPowiatu);

    if (powiat == null) { return Results.NotFound("Nie znaleziono takiego powiatu!"); }

    var wojewodztwo = dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);
    wojewodztwo.Population -= powiat.Population;

    var gminyDoUsuniecia = dbContext.listaGmin.Where(p => p.PowiatId == idPowiatu).ToList();
    var miastaDoUsuniecia = dbContext.listaMiast
                                     .Where(m => gminyDoUsuniecia
                                     .Select(m => m.Id)
                                     .Contains(m.GminaId))
                                     .ToList();

    // Usuwanie miast
    dbContext.listaMiast.RemoveRange(miastaDoUsuniecia);

    // Usuwanie gmin
    dbContext.listaGmin.RemoveRange(gminyDoUsuniecia);

    // Usuwanie powiatów
    dbContext.listaPowiatow.Remove(powiat);

    await dbContext.SaveChangesAsync();
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