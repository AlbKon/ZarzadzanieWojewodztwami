using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;

namespace WebApplication1.Services
{
    public class WojewodztwoService
    {
        private readonly AppDbContext _dbContext;

        public WojewodztwoService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Wojewodztwo> GetAll()
        {
            var listaWojewodztw = _dbContext.listaWojewodztw.Include(m => m.listaPowiatow).ToList();
            return listaWojewodztw;
        }

        public void Create(Wojewodztwo wojewodztwo)
        {
            _dbContext.listaWojewodztw.Add(wojewodztwo);
            _dbContext.SaveChangesAsync();
        }

        public void SetCapital(int wojewodztwoId, int miastoId)
        {
            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == wojewodztwoId);
            var miasto = _dbContext.listaMiast.FirstOrDefault(m => m.Id == miastoId);

            if (wojewodztwo == null) { throw new Exception("Nie ma takiego województwa!"); }
            if (miasto == null) { throw new Exception("Nie ma takiego miasta!"); }
            if (wojewodztwo.MiastoId != null) { throw new Exception("Wojewodztwo ma już stolicę!"); }
            if (miasto.isCapital) { throw new Exception("Miasto jest już stolicą!"); }

            wojewodztwo.MiastoId = miastoId;
            miasto.isCapital = true;

            _dbContext.SaveChangesAsync();
        }

        public void Delete (int idWojewodztwa)
        {
            var wojewodztwo = _dbContext.listaWojewodztw.FirstOrDefault(w => w.Id == idWojewodztwa);


            if (wojewodztwo == null) { throw new Exception("Nie znaleziono takiego wojewodztwa!"); }
            // Pobieranie wszystkich powiatów, gmin i miast związanych z województwem
            var powiatyDoUsuniecia = _dbContext.listaPowiatow.Where(p => p.WojewodztwoId == idWojewodztwa).ToList();
            var gminyDoUsuniecia = _dbContext.listaGmin
                                            .Where(g => powiatyDoUsuniecia
                                            .Select(p => p.Id)
                                            .Contains(g.PowiatId))
                                            .ToList();
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
            _dbContext.listaPowiatow.RemoveRange(powiatyDoUsuniecia);

            // Usuwanie województwa
            _dbContext.listaWojewodztw.Remove(wojewodztwo);

            _dbContext.SaveChangesAsync();
        }
    }
}
