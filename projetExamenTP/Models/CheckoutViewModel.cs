using System.ComponentModel.DataAnnotations;

namespace TestWithADO.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "L'adresse d'expédition est requise.")]
        [StringLength(255)]
        [Display(Name = "Adresse d'Expédition")]
        public string AdresseExpedition { get; set; }

        [Required(ErrorMessage = "La ville est requise.")]
        [StringLength(100)]
        [Display(Name = "Ville")]
        public string Ville { get; set; }

        [Required(ErrorMessage = "Le code postal est requis.")]
        [StringLength(20)]
        [Display(Name = "Code Postal")]
        public string CodePostal { get; set; }

        [Required(ErrorMessage = "Le numéro de téléphone est requis.")]
        [Phone]
        [StringLength(50)]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; }

        [Required(ErrorMessage = "La méthode de livraison est requise.")]
        [Display(Name = "Méthode de Livraison")]
        public string MethodeLivraison { get; set; }
    }
}