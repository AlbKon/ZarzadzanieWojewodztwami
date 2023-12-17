using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class Powiat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public int WojewodztwoId { get; set; }
        public ICollection<Gmina> listaGmin { get; set; } = new List<Gmina>();
        [Display(Name = "Stolica")]
        public int? MiastoId { get; set; }
    }
}
