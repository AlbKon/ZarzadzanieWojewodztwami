using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Entities
{
    public class Miasto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Population { get; set; }
        public bool isCapital { get; set; }
        [Display(Name = "Stolica")]
        public int GminaId { get; set; }
    }
}
