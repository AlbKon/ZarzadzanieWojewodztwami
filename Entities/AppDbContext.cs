using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Entities
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Wojewodztwo> listaWojewodztw { get; set; }
        public DbSet<Powiat> listaPowiatow { get; set; }
        public DbSet<Gmina> listaGmin { get; set; }
        public DbSet<Miasto> listaMiast { get; set; }
    }
}