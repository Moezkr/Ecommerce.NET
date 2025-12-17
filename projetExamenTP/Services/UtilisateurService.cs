using TestWithADO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using BCrypt.Net;

namespace TestWithADO.Services
{
    public class UtilisateurService
    {
        private readonly string _connectionString;

        public UtilisateurService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Utilisateur ValidateUser(string email, string password)
        {
            Utilisateur user = null;
            string userQuery = "SELECT UtilisateurID, Nom, Prenom, Email, Role, MotDePasse FROM Utilisateur WHERE Email = @Email";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(userQuery, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString(5);

                        if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                        {
                            user = new Utilisateur
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
            return user;
        }

        public Utilisateur GetUtilisateurById(int userId)
        {
            Utilisateur user = null;
            string query = "SELECT UtilisateurID, Nom, Prenom, Email, Role FROM Utilisateur WHERE UtilisateurID = @UtilisateurID";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UtilisateurID", userId);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        user = new Utilisateur
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
            return user;
        }

        public bool AddUtilisateur(Utilisateur user)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.MotDePasse);

            string query = @"
                INSERT INTO Utilisateur (Nom, Prenom, Email, MotDePasse, Role)
                VALUES (@Nom, @Prenom, @Email, @MotDePasse, @Role)";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", user.Nom);
                command.Parameters.AddWithValue("@Prenom", user.Prenom);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@MotDePasse", hashedPassword);
                command.Parameters.AddWithValue("@Role", "CLIENT");

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (SqlException ex) when (ex.Number == 2627)
                {
                    return false;
                }
            }
        }

        public bool UpdateUtilisateur(Utilisateur user)
        {
            string query;
            string hashedPassword = null;

            if (string.IsNullOrEmpty(user.MotDePasse))
            {
                query = @"
                    UPDATE Utilisateur
                    SET Nom = @Nom,
                        Prenom = @Prenom,
                        Email = @Email
                    WHERE UtilisateurID = @UtilisateurID";
            }
            else
            {
                hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.MotDePasse);

                query = @"
                    UPDATE Utilisateur
                    SET Nom = @Nom,
                        Prenom = @Prenom,
                        Email = @Email,
                        MotDePasse = @MotDePasse
                    WHERE UtilisateurID = @UtilisateurID";
            }

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Nom", user.Nom);
                command.Parameters.AddWithValue("@Prenom", user.Prenom);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@UtilisateurID", user.UtilisateurID);

                if (!string.IsNullOrEmpty(hashedPassword))
                {
                    command.Parameters.AddWithValue("@MotDePasse", hashedPassword);
                }

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}