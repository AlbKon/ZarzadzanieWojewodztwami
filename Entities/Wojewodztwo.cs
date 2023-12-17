using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class Wojewodztwo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public ICollection<Powiat> listaPowiatow { get; set; } = new List<Powiat>();
        [Display(Name = "Stolica")]
        public int? MiastoId { get; set; }
    }
}
