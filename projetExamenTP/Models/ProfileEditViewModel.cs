using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TestWithADO.Models
{
    public class ProfileEditViewModel
    {
        public int UtilisateurID { get; set; }

        [Required(ErrorMessage = "Le nom est requis.")]
        [StringLength(100)]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est requis.")]
        [StringLength(100)]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "Format d'email invalide.")]
        [StringLength(255)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nouveau Mot de Passe (Laisser vide pour ne pas changer)")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer Nouveau Mot de Passe")]
        public string ConfirmNewPassword { get; set; }
    }
}