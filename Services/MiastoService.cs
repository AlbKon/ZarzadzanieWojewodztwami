using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public class MiastoService
    {
        private readonly AppDbContext _dbContext;

        public MiastoService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Miasto> GetAll()
        {
            var listaMiast = _dbContext.listaMiast.ToList();
            return listaMiast;
        }

        public void Create(Miasto miasto)
        {
            var gmina = _dbContext.listaGmin.FirstOrDefault(g => g.Id == miasto.GminaId);
            if (gmina == null) { throw new Exception("Nie znaleziono takiej gminy."); }

            _dbContext.listaMiast.Add(miasto);

            var powiat = _dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);

            gmina.Population += miasto.Population;
            powiat.Population += miasto.Population;
            wojewodztwo.Population += miasto.Population;

            _dbContext.SaveChangesAsync();
        }

        public void Delete(int idMiasta)
        {
            var miasto = _dbContext.listaMiast.FirstOrDefault(m => m.Id == idMiasta);

            if (miasto == null) { throw new Exception("Nie znaleziono takiego miasta!"); }

            _dbContext.Remove(miasto);

            _dbContext.SaveChangesAsync();
        }
    }
}
