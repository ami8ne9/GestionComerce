using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace GestionComerce
{
    public class Article
    {
        public int ArticleID { get; set; }
        public int Quantite { get; set; }
        public decimal PrixAchat { get; set; }
        public decimal PrixVente { get; set; }
        public decimal PrixMP { get; set; }
        public int FamillyID { get; set; }
        public long Code { get; set; }
        public string ArticleName { get; set; }
        public bool Etat { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string marque { get; set; }
        public decimal tva { get; set; }
        public string numeroLot { get; set; }
        public string bonlivraison { get; set; }
        public DateTime? DateLivraison { get; set; }
        public int FournisseurID { get; set; }

        // NEW: Image property
        public byte[] ArticleImage { get; set; }

        // NEW: Unlimited stock property
        public bool IsUnlimitedStock { get; set; }

        private static readonly string ConnectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";

        public async Task<List<Article>> GetArticlesAsync()
        {
            var articles = new List<Article>();
            string query = "SELECT * FROM Article where Etat=1";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Article article = new Article
                        {
                            ArticleID = Convert.ToInt32(reader["ArticleID"]),
                            Quantite = Convert.ToInt32(reader["Quantite"]),
                            PrixAchat = Convert.ToDecimal(reader["PrixAchat"]),
                            PrixVente = Convert.ToDecimal(reader["PrixVente"]),
                            PrixMP = Convert.ToDecimal(reader["PrixMP"]),
                            FamillyID = Convert.ToInt32(reader["FamillyID"]),
                            FournisseurID = Convert.ToInt32(reader["FournisseurID"]),
                            Code = reader["Code"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Code"]),
                            ArticleName = reader["ArticleName"] == DBNull.Value ? string.Empty : reader["ArticleName"].ToString(),
                            Date = reader["Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date"]),
                            DateExpiration = reader["DateExpiration"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateExpiration"]),
                            marque = reader["marque"] == DBNull.Value ? string.Empty : reader["marque"].ToString(),
                            tva = reader["tva"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tva"]),
                            numeroLot = reader["numeroLot"] == DBNull.Value ? string.Empty : reader["numeroLot"].ToString(),
                            bonlivraison = reader["bonlivraison"] == DBNull.Value ? string.Empty : reader["bonlivraison"].ToString(),
                            DateLivraison = reader["DateLivraison"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateLivraison"]),
                            ArticleImage = reader["ArticleImage"] == DBNull.Value ? null : (byte[])reader["ArticleImage"],
                            IsUnlimitedStock = reader["IsUnlimitedStock"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsUnlimitedStock"]),
                            Etat = Convert.ToBoolean(reader["Etat"])
                        };
                        articles.Add(article);
                    }
                }
            }
            return articles;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            var articles = new List<Article>();
            string query = "SELECT * FROM Article";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Article article = new Article
                        {
                            ArticleID = Convert.ToInt32(reader["ArticleID"]),
                            Quantite = Convert.ToInt32(reader["Quantite"]),
                            PrixAchat = Convert.ToDecimal(reader["PrixAchat"]),
                            PrixVente = Convert.ToDecimal(reader["PrixVente"]),
                            PrixMP = Convert.ToDecimal(reader["PrixMP"]),
                            FamillyID = Convert.ToInt32(reader["FamillyID"]),
                            FournisseurID = Convert.ToInt32(reader["FournisseurID"]),
                            Code = reader["Code"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Code"]),
                            ArticleName = reader["ArticleName"] == DBNull.Value ? string.Empty : reader["ArticleName"].ToString(),
                            Date = reader["Date"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["Date"]),
                            DateExpiration = reader["DateExpiration"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateExpiration"]),
                            marque = reader["marque"] == DBNull.Value ? string.Empty : reader["marque"].ToString(),
                            tva = reader["tva"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tva"]),
                            numeroLot = reader["numeroLot"] == DBNull.Value ? string.Empty : reader["numeroLot"].ToString(),
                            bonlivraison = reader["bonlivraison"] == DBNull.Value ? string.Empty : reader["bonlivraison"].ToString(),
                            DateLivraison = reader["DateLivraison"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateLivraison"]),
                            ArticleImage = reader["ArticleImage"] == DBNull.Value ? null : (byte[])reader["ArticleImage"],
                            IsUnlimitedStock = reader["IsUnlimitedStock"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsUnlimitedStock"]),
                            Etat = Convert.ToBoolean(reader["Etat"])
                        };
                        articles.Add(article);
                    }
                }
            }
            return articles;
        }

        public async Task<int> InsertArticleAsync()
        {
            string query = "INSERT INTO Article (Quantite, PrixAchat, PrixVente, PrixMP, FamillyID, Code, FournisseurID, ArticleName, Date, DateExpiration, marque, tva, numeroLot, bonlivraison, DateLivraison, ArticleImage, IsUnlimitedStock) " +
                "VALUES (@Quantite, @PrixAchat, @PrixVente, @PrixMP, @FamillyID, @Code, @FournisseurID, @ArticleName, @Date, @DateExpiration, @marque, @tva, @numeroLot, @bonlivraison, @DateLivraison, @ArticleImage, @IsUnlimitedStock); SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Quantite", this.Quantite);
                        cmd.Parameters.AddWithValue("@PrixAchat", this.PrixAchat);
                        cmd.Parameters.AddWithValue("@PrixVente", this.PrixVente);
                        cmd.Parameters.AddWithValue("@PrixMP", this.PrixMP);
                        cmd.Parameters.AddWithValue("@FamillyID", this.FamillyID);
                        cmd.Parameters.AddWithValue("@Code", this.Code);
                        cmd.Parameters.AddWithValue("@FournisseurID", this.FournisseurID);
                        cmd.Parameters.AddWithValue("@ArticleName", this.ArticleName ?? string.Empty);

                        cmd.Parameters.AddWithValue("@Date",
                            this.Date.HasValue ? (object)this.Date.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@DateExpiration",
                            this.DateExpiration.HasValue ? (object)this.DateExpiration.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@marque", this.marque ?? string.Empty);
                        cmd.Parameters.AddWithValue("@tva", this.tva);
                        cmd.Parameters.AddWithValue("@numeroLot", this.numeroLot ?? string.Empty);
                        cmd.Parameters.AddWithValue("@bonlivraison", this.bonlivraison ?? string.Empty);

                        cmd.Parameters.AddWithValue("@DateLivraison",
                            this.DateLivraison.HasValue ? (object)this.DateLivraison.Value : DBNull.Value);

                        // FIXED: Explicitly specify SqlDbType for the image parameter
                        if (this.ArticleImage != null && this.ArticleImage.Length > 0)
                        {
                            cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = this.ArticleImage;
                        }
                        else
                        {
                            cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                        }

                        // NEW: Add IsUnlimitedStock parameter
                        cmd.Parameters.AddWithValue("@IsUnlimitedStock", this.IsUnlimitedStock);

                        object result = await cmd.ExecuteScalarAsync();
                        int id = Convert.ToInt32(result);
                        return id;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show($"Article not inserted, error: {err}");
                    return 0;
                }
            }
        }

        public async Task<int> DeleteArticleAsync()
        {
            string query = "UPDATE Article SET Etat=0 WHERE ArticleID=@ArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> BringBackArticleAsync()
        {
            string query = "UPDATE Article SET Etat=1 WHERE ArticleID=@ArticleID";
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not deleted: {err}");
                        return 0;
                    }
                }
            }
        }

        public async Task<int> UpdateArticleAsync()
        {
            string query = "UPDATE Article SET " +
               "Quantite=@Quantite, " +
               "PrixAchat=@PrixAchat, " +
               "PrixVente=@PrixVente, " +
               "PrixMP=@PrixMP, " +
               "FamillyID=@FamillyID, " +
               "Code=@Code, " +
               "ArticleName=@ArticleName, " +
               "Date=@Date, " +
               "DateExpiration=@DateExpiration, " +
               "marque=@marque, " +
               "tva=@tva, " +
               "numeroLot=@numeroLot, " +
               "bonlivraison=@bonlivraison, " +
               "DateLivraison=@DateLivraison, " +
               "ArticleImage=@ArticleImage, " +
               "IsUnlimitedStock=@IsUnlimitedStock " +
               "WHERE ArticleID=@ArticleID";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        cmd.Parameters.AddWithValue("@Quantite", this.Quantite);
                        cmd.Parameters.AddWithValue("@PrixAchat", this.PrixAchat);
                        cmd.Parameters.AddWithValue("@PrixVente", this.PrixVente);
                        cmd.Parameters.AddWithValue("@PrixMP", this.PrixMP);
                        cmd.Parameters.AddWithValue("@FamillyID", this.FamillyID);
                        cmd.Parameters.AddWithValue("@Code", this.Code);
                        cmd.Parameters.AddWithValue("@ArticleName", this.ArticleName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ArticleID", this.ArticleID);

                        cmd.Parameters.AddWithValue("@Date",
                            this.Date.HasValue ? (object)this.Date.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@DateExpiration",
                            this.DateExpiration.HasValue ? (object)this.DateExpiration.Value : DBNull.Value);

                        cmd.Parameters.AddWithValue("@marque", this.marque ?? string.Empty);
                        cmd.Parameters.AddWithValue("@tva", this.tva);
                        cmd.Parameters.AddWithValue("@numeroLot", this.numeroLot ?? string.Empty);
                        cmd.Parameters.AddWithValue("@bonlivraison", this.bonlivraison ?? string.Empty);

                        cmd.Parameters.AddWithValue("@DateLivraison",
                            this.DateLivraison.HasValue ? (object)this.DateLivraison.Value : DBNull.Value);

                        // FIXED: Explicitly specify SqlDbType for the image parameter
                        if (this.ArticleImage != null && this.ArticleImage.Length > 0)
                        {
                            cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = this.ArticleImage;
                        }
                        else
                        {
                            cmd.Parameters.Add("@ArticleImage", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                        }

                        // NEW: Add IsUnlimitedStock parameter
                        cmd.Parameters.AddWithValue("@IsUnlimitedStock", this.IsUnlimitedStock);

                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                    catch (Exception err)
                    {
                        MessageBox.Show($"Article not updated: {err}");
                        return 0;
                    }
                }
            }
        }

        // HELPER METHODS FOR UNLIMITED STOCK

        /// <summary>
        /// Returns a formatted string for displaying the article's stock quantity.
        /// Returns "∞" for unlimited stock, or the actual quantity for limited stock.
        /// </summary>
        public string GetStockDisplayString()
        {
            if (IsUnlimitedStock)
            {
                return "∞";
            }
            return Quantite.ToString();
        }

        /// <summary>
        /// Checks if the article has sufficient stock for a sale.
        /// Always returns true for unlimited stock articles.
        /// </summary>
        public bool HasSufficientStock(int requestedQuantity)
        {
            if (IsUnlimitedStock)
            {
                return true; // Unlimited stock always has sufficient quantity
            }
            return Quantite >= requestedQuantity;
        }

        /// <summary>
        /// Decrements the stock quantity by the specified amount.
        /// Does nothing if the article has unlimited stock.
        /// </summary>
        public void DecrementStock(int quantity)
        {
            if (!IsUnlimitedStock)
            {
                Quantite -= quantity;
            }
            // If unlimited stock, do nothing - quantity remains at 0
        }

        /// <summary>
        /// Increments the stock quantity by the specified amount.
        /// Does nothing if the article has unlimited stock.
        /// </summary>
        public void IncrementStock(int quantity)
        {
            if (!IsUnlimitedStock)
            {
                Quantite += quantity;
            }
            // If unlimited stock, do nothing - quantity remains at 0
        }

        /// <summary>
        /// Gets the effective quantity for calculations.
        /// Returns 0 for unlimited stock to avoid negative calculations.
        /// Use HasSufficientStock() to check availability instead.
        /// </summary>
        public int GetEffectiveQuantity()
        {
            if (IsUnlimitedStock)
            {
                return 0; // Return 0 to avoid negative calculations
            }
            return Quantite;
        }
    }
}