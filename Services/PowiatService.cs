using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public class PowiatService
    {
        private readonly AppDbContext _dbContext;
        public PowiatService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Powiat> GetAll()
        {
            var listaPowiatow = _dbContext.listaPowiatow.Include(m => m.listaGmin).ToList();
            return listaPowiatow;
        }

        public void Create(Powiat powiat)
        {
            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);
            if (wojewodztwo == null) { throw new Exception("Nie znaleziono takiego województwa."); }

            _dbContext.listaPowiatow.Add(powiat);
            _dbContext.SaveChangesAsync();
        }

        public void SetCapital(int powiatId, int miastoId)
        {
            var powiat = _dbContext.listaPowiatow.FirstOrDefault(p => p.Id == powiatId);
            var miasto = _dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

            if (powiat == null) { throw new Exception("Nie ma takiego powiatu!"); }
            if (miasto == null) { throw new Exception("Nie ma takiego miasta!"); }
            if (powiat.MiastoId != null) { throw new Exception("Powiat ma już stolicę!"); }
            if (miasto.isCapital) { throw new Exception("Miasto jest już stolicą!"); }

            powiat.MiastoId = miastoId;
            miasto.isCapital = true;

            _dbContext.SaveChangesAsync();
        }

        public void Delete(int idPowiatu)
        {
            var powiat = _dbContext.listaPowiatow.FirstOrDefault(p => p.Id == idPowiatu);

            if (powiat == null) { throw new Exception("Nie znaleziono takiego powiatu!"); }

            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == powiat.WojewodztwoId);
            wojewodztwo.Population -= powiat.Population;

            var gminyDoUsuniecia = _dbContext.listaGmin.Where(p => p.PowiatId == idPowiatu).ToList();
            var miastaDoUsuniecia = _dbContext.listaMiast
                                             .Where(m => gminyDoUsuniecia
                                             .Select(m => m.Id)
                                             .Contains(m.GminaId))
                                             .ToList();

            // Usuwanie miast
            _dbContext.listaMiast.RemoveRange(miastaDoUsuniecia);

            // Usuwanie gmin
            _dbContext.listaGmin.RemoveRange(gminyDoUsuniecia);

            // Usuwanie powiatów
            _dbContext.listaPowiatow.Remove(powiat);

            _dbContext.SaveChangesAsync();
        }
    }
}
