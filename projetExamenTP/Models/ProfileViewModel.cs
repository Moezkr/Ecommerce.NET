using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestWithADO.Models
{
    public class ProfileViewModel
    {
        public Utilisateur User { get; set; }
        public List<Commande> Orders { get; set; } = new List<Commande>();
    }
}