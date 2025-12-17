using System;
using System.Collections.Generic;

namespace TestWithADO.Models
{
    public class Commande
    {
        public int CommandeID { get; set; }
        public int UtilisateurID { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal Total { get; set; }
        public string Etat { get; set; }
        public string AdresseExpedition { get; set; }
        public string Ville { get; set; }
        public string CodePostal { get; set; }
        public string Telephone { get; set; }
        public string MethodeLivraison { get; set; }
        public List<LigneCommande> Lignes { get; set; } = new List<LigneCommande>();
    }
}