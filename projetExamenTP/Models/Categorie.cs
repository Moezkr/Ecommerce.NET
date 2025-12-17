using System.ComponentModel.DataAnnotations;

namespace TestWithADO.Models
{
    public class Categorie
    {
        public int CategorieID { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est requis.")]
        [StringLength(100)]
        public string Nom { get; set; }
    }
}