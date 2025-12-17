using System.ComponentModel.DataAnnotations;

namespace TestWithADO.Models
{
    public class Utilisateur
    {
        public int UtilisateurID { get; set; }

        [Required] public string Nom { get; set; }
        [Required] public string Prenom { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string MotDePasse { get; set; }
        [Required] public string Role { get; set; }
    }
}