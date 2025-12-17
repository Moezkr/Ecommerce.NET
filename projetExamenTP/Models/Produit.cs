using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestWithADO.Models
{
    public class Produit
    {
        public int ProduitID { get; set; }

        [Required(ErrorMessage = "Le titre est requis.")]
        [StringLength(255)]
        public string Titre { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Le prix est requis.")]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Le prix doit être positif.")]
        public decimal Prix { get; set; }

        [Required(ErrorMessage = "La capacité est requise.")]
        [Range(0, int.MaxValue, ErrorMessage = "La capacité doit être valide.")]
        public int Capacite { get; set; }

        public string ImagePath { get; set; }

        [NotMapped]
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "La catégorie est requise.")]
        public int CategorieID { get; set; }

        [NotMapped]
        public string CategorieNom { get; set; }

        [NotMapped]
        public bool IsAvailable => Capacite > 0;
    }
}