using TestWithADO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System;
using System.Data;

namespace TestWithADO.Services
{
    public class CommandeService
    {
        private readonly string _connectionString;
        private readonly ProduitService _produitService;

        public CommandeService(IConfiguration configuration, ProduitService produitService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _produitService = produitService;
        }

        public bool SaveOrder(int userId, decimal total, CheckoutViewModel model, List<CartItem> cartItems)
        {
            const string defaultStatus = "Processing";
            using var connection = new SqlConnection(_connectionString);

            try
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    string orderSql = @"
                        INSERT INTO Commande 
                        (UtilisateurID, Total, AdresseExpedition, Ville, CodePostal, 
                         Telephone, MethodeLivraison, Etat, DateCommande)
                        VALUES 
                        (@UtilisateurID, @Total, @AdresseExpedition, @Ville, @CodePostal, 
                         @Telephone, @MethodeLivraison, @Etat, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    using var orderCmd = new SqlCommand(orderSql, connection, transaction);
                    orderCmd.Parameters.AddWithValue("@UtilisateurID", userId);
                    orderCmd.Parameters.AddWithValue("@Total", total);
                    orderCmd.Parameters.AddWithValue("@AdresseExpedition", model.AdresseExpedition ?? "");
                    orderCmd.Parameters.AddWithValue("@Ville", model.Ville ?? "");
                    orderCmd.Parameters.AddWithValue("@CodePostal", model.CodePostal ?? "");
                    orderCmd.Parameters.AddWithValue("@Telephone", model.Telephone ?? "");
                    orderCmd.Parameters.AddWithValue("@MethodeLivraison", model.MethodeLivraison ?? "Standard");
                    orderCmd.Parameters.AddWithValue("@Etat", defaultStatus);

                    int commandeId = Convert.ToInt32(orderCmd.ExecuteScalar());

                    foreach (var item in cartItems)
                    {
                        string detailSql = @"
                            INSERT INTO LigneCommande 
                            (CommandeID, ProduitID, Quantite, PrixUnitaire)
                            VALUES 
                            (@CommandeID, @ProduitID, @Quantite, @PrixUnitaire)";

                        using var detailCmd = new SqlCommand(detailSql, connection, transaction);
                        detailCmd.Parameters.AddWithValue("@CommandeID", commandeId);
                        detailCmd.Parameters.AddWithValue("@ProduitID", item.ProduitID);
                        detailCmd.Parameters.AddWithValue("@Quantite", item.Quantite);
                        detailCmd.Parameters.AddWithValue("@PrixUnitaire", item.Prix);
                        detailCmd.ExecuteNonQuery();
                    }

                    foreach (var item in cartItems)
                    {
                        string updateSql = @"
                            UPDATE Produit 
                            SET Capacite = Capacite - @Quantite 
                            WHERE ProduitID = @ProduitID";

                        using var updateCmd = new SqlCommand(updateSql, connection, transaction);
                        updateCmd.Parameters.AddWithValue("@Quantite", item.Quantite);
                        updateCmd.Parameters.AddWithValue("@ProduitID", item.ProduitID);
                        updateCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Commande> GetOrdersByUserId(int userId)
        {
            var orders = new List<Commande>();

            string orderQuery = @"
                SELECT CommandeID, DateCommande, Total, AdresseExpedition, Ville, CodePostal, Telephone, MethodeLivraison, Etat
                FROM Commande
                WHERE UtilisateurID = @UtilisateurID
                ORDER BY DateCommande DESC";

            string lineQuery = @"
                SELECT lc.LigneCommandeID, lc.ProduitID, p.Titre, lc.Quantite, lc.PrixUnitaire
                FROM LigneCommande lc
                JOIN Produit p ON lc.ProduitID = p.ProduitID
                WHERE lc.CommandeID = @CommandeID";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var orderCommand = new SqlCommand(orderQuery, connection))
                {
                    orderCommand.Parameters.AddWithValue("@UtilisateurID", userId);

                    using (var orderReader = orderCommand.ExecuteReader())
                    {
                        while (orderReader.Read())
                        {
                            orders.Add(new Commande
                            {
                                CommandeID = orderReader.GetInt32(0),
                                UtilisateurID = userId,
                                DateCommande = orderReader.GetDateTime(1),
                                Total = orderReader.GetDecimal(2),
                                AdresseExpedition = orderReader.GetString(3),
                                Ville = orderReader.GetString(4),
                                CodePostal = orderReader.GetString(5),
                                Telephone = orderReader.IsDBNull(6) ? null : orderReader.GetString(6),
                                MethodeLivraison = orderReader.IsDBNull(7) ? null : orderReader.GetString(7),
                                Etat = orderReader.GetString(8),
                                Lignes = new List<LigneCommande>()
                            });
                        }
                    }
                }

                foreach (var order in orders)
                {
                    using (var lineCommand = new SqlCommand(lineQuery, connection))
                    {
                        lineCommand.Parameters.AddWithValue("@CommandeID", order.CommandeID);

                        using (var lineReader = lineCommand.ExecuteReader())
                        {
                            while (lineReader.Read())
                            {
                                order.Lignes.Add(new LigneCommande
                                {
                                    LigneCommandeID = lineReader.GetInt32(0),
                                    ProduitID = lineReader.GetInt32(1),
                                    ProduitTitre = lineReader.GetString(2),
                                    Quantite = lineReader.GetInt32(3),
                                    PrixUnitaire = lineReader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }

            return orders;
        }

        public List<Commande> GetAllOrders()
        {
            var orders = new List<Commande>();

            string orderQuery = @"
                SELECT CommandeID, UtilisateurID, DateCommande, Total, AdresseExpedition, Ville,
                       CodePostal, Telephone, MethodeLivraison, Etat
                FROM Commande
                ORDER BY DateCommande DESC";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var orderCommand = new SqlCommand(orderQuery, connection))
                {
                    using (var orderReader = orderCommand.ExecuteReader())
                    {
                        while (orderReader.Read())
                        {
                            orders.Add(new Commande
                            {
                                CommandeID = orderReader.GetInt32(0),
                                UtilisateurID = orderReader.GetInt32(1),
                                DateCommande = orderReader.GetDateTime(2),
                                Total = orderReader.GetDecimal(3),
                                AdresseExpedition = orderReader.GetString(4),
                                Ville = orderReader.GetString(5),
                                CodePostal = orderReader.GetString(6),
                                Telephone = orderReader.GetString(7),
                                MethodeLivraison = orderReader.GetString(8),
                                Etat = orderReader.GetString(9)
                            });
                        }
                    }
                }
            }
            return orders;
        }

        public (Commande Order, Utilisateur Customer) GetFullOrderDetailsForAdmin(int orderId)
        {
            Commande order = null;
            Utilisateur customer = null;

            string orderQuery = @"
                SELECT CommandeID, UtilisateurID, DateCommande, Total, AdresseExpedition, Ville, CodePostal, Telephone, MethodeLivraison, Etat
                FROM Commande
                WHERE CommandeID = @CommandeID";

            string lineQuery = @"
                SELECT lc.LigneCommandeID, lc.ProduitID, p.Titre, lc.Quantite, lc.PrixUnitaire
                FROM LigneCommande lc
                JOIN Produit p ON lc.ProduitID = p.ProduitID
                WHERE lc.CommandeID = @CommandeID";

            string userQuery = @"
                SELECT UtilisateurID, Nom, Prenom, Email, Role
                FROM Utilisateur
                WHERE UtilisateurID = @UtilisateurID";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                int userId = 0;

                using (var orderCmd = new SqlCommand(orderQuery, connection))
                {
                    orderCmd.Parameters.AddWithValue("@CommandeID", orderId);
                    using (var reader = orderCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new Commande
                            {
                                CommandeID = reader.GetInt32(0),
                                UtilisateurID = reader.GetInt32(1),
                                DateCommande = reader.GetDateTime(2),
                                Total = reader.GetDecimal(3),
                                AdresseExpedition = reader.GetString(4),
                                Ville = reader.GetString(5),
                                CodePostal = reader.GetString(6),
                                Telephone = reader.GetString(7),
                                MethodeLivraison = reader.GetString(8),
                                Etat = reader.GetString(9),
                                Lignes = new List<LigneCommande>()
                            };
                            userId = order.UtilisateurID;
                        }
                    }
                }

                if (order == null) return (null, null);

                using (var lineCmd = new SqlCommand(lineQuery, connection))
                {
                    lineCmd.Parameters.AddWithValue("@CommandeID", orderId);
                    using (var reader = lineCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            order.Lignes.Add(new LigneCommande
                            {
                                LigneCommandeID = reader.GetInt32(0),
                                ProduitID = reader.GetInt32(1),
                                ProduitTitre = reader.GetString(2),
                                Quantite = reader.GetInt32(3),
                                PrixUnitaire = reader.GetDecimal(4)
                            });
                        }
                    }
                }

                using (var userCmd = new SqlCommand(userQuery, connection))
                {
                    userCmd.Parameters.AddWithValue("@UtilisateurID", userId);
                    using (var reader = userCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            customer = new Utilisateur
                            {
                                UtilisateurID = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Prenom = reader.GetString(2),
                                Email = reader.GetString(3),
                                Role = reader.GetString(4)
                            };
                        }
                    }
                }
            }

            return (order, customer);
        }

        public Commande GetOrderById(int orderId)
        {
            Commande order = null;

            string orderQuery = @"
                SELECT CommandeID, UtilisateurID, DateCommande, Total, AdresseExpedition, Ville, CodePostal, Telephone, MethodeLivraison
                FROM Commande
                WHERE CommandeID = @CommandeID";

            string lineQuery = @"
                SELECT lc.LigneCommandeID, lc.ProduitID, p.Titre, lc.Quantite, lc.PrixUnitaire
                FROM LigneCommande lc
                JOIN Produit p ON lc.ProduitID = p.ProduitID
                WHERE lc.CommandeID = @CommandeID";

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var orderCommand = new SqlCommand(orderQuery, connection))
                {
                    orderCommand.Parameters.AddWithValue("@CommandeID", orderId);

                    using (var orderReader = orderCommand.ExecuteReader())
                    {
                        if (orderReader.Read())
                        {
                            order = new Commande
                            {
                                CommandeID = orderReader.GetInt32(0),
                                UtilisateurID = orderReader.GetInt32(1),
                                DateCommande = orderReader.GetDateTime(2),
                                Total = orderReader.GetDecimal(3),
                                AdresseExpedition = orderReader.GetString(4),
                                Ville = orderReader.GetString(5),
                                CodePostal = orderReader.GetString(6),
                                Telephone = orderReader.IsDBNull(7) ? null : orderReader.GetString(7),
                                MethodeLivraison = orderReader.IsDBNull(8) ? null : orderReader.GetString(8),
                                Lignes = new List<LigneCommande>()
                            };
                        }
                    }
                }

                if (order != null)
                {
                    using (var lineCommand = new SqlCommand(lineQuery, connection))
                    {
                        lineCommand.Parameters.AddWithValue("@CommandeID", order.CommandeID);

                        using (var lineReader = lineCommand.ExecuteReader())
                        {
                            while (lineReader.Read())
                            {
                                order.Lignes.Add(new LigneCommande
                                {
                                    LigneCommandeID = lineReader.GetInt32(0),
                                    ProduitID = lineReader.GetInt32(1),
                                    ProduitTitre = lineReader.GetString(2),
                                    Quantite = lineReader.GetInt32(3),
                                    PrixUnitaire = lineReader.GetDecimal(4)
                                });
                            }
                        }
                    }
                }
            }

            return order;
        }

        public bool UpdateOrderStatus(int commandeId, string newStatus)
        {
            string updateSql = @"
                UPDATE Commande 
                SET Etat = @NewStatus 
                WHERE CommandeID = @CommandeID";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var cmd = new SqlCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                        cmd.Parameters.AddWithValue("@CommandeID", commandeId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}