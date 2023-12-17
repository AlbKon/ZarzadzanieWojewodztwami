using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class Gmina
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public int PowiatId { get; set; }
        public ICollection<Gmina> listaMiast { get; set; } = new List<Gmina>();
        [Display(Name = "Stolica")]
        public int? MiastoId { get; set; }
    }
}
