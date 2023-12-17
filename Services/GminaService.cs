using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public class GminaService
    {
        private readonly AppDbContext _dbContext;

        public GminaService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Gmina> GetAll()
        {
            var listaGmin = _dbContext.listaGmin.Include(m => m.listaMiast).ToList();
            return listaGmin;
        }

        public void Create(Gmina gmina)
        {
            var powiat = _dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
            if (powiat == null) { throw new Exception("Nie znaleziono takiego powiatu."); }

            _dbContext.listaGmin.Add(gmina);
            _dbContext.SaveChangesAsync();
        }

        public void SetCapital(int gminaId, int miastoId)
        {
            var gmina = _dbContext.listaGmin.FirstOrDefault(g => g.Id == gminaId);
            var miasto = _dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

            if (gmina == null) { throw new Exception("Nie ma takiej gminy!"); }
            if (miasto == null) { throw new Exception("Nie ma takiego miasta!"); }
            if (gmina.MiastoId != null) { throw new Exception("Gmina ma już stolicę!"); }
            if (miasto.isCapital) { throw new Exception("Miasto jest już stolicą!"); }

            gmina.MiastoId = miastoId;
            miasto.isCapital = true;

            _dbContext.SaveChangesAsync();
        }

        public void Delete(int idGminy)
        {
            var gmina = _dbContext.listaGmin.FirstOrDefault(g => g.Id == idGminy);

            if (gmina == null) { throw new Exception("Nie znaleziono takiej gminy!"); }

            var powiat = _dbContext.listaPowiatow.FirstOrDefault(p => p.Id == gmina.PowiatId);
            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.Id);

            wojewodztwo.Population -= gmina.Population;
            powiat.Population -= gmina.Population;

            var miastaDoUsuniecia = _dbContext.listaMiast.Where(m => m.GminaId == gmina.Id).ToList();

            // Usuwanie miast
            _dbContext.listaMiast.RemoveRange(miastaDoUsuniecia);

            // Usuwanie gmin
            _dbContext.listaGmin.Remove(gmina);

            _dbContext.SaveChangesAsync();
        }
    }
}
