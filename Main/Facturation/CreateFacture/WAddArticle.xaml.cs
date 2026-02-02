using Superete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Reflection;

namespace GestionComerce.Main.Facturation.CreateFacture
{
    public partial class WAddArticle : Window
    {
        private CMainFa mainFa;
        private List<Article> stockArticles;
        private List<Article> allStockArticles;
        private Article selectedArticle;
        private bool isStockMode = false;
        private bool isExpeditionMode = false;

        public WAddArticle(CMainFa mainFa)
        {
            InitializeComponent();
            this.mainFa = mainFa;

            // Check if we're in Expedition mode
            isExpeditionMode = mainFa?.InvoiceType == "Expedition";

            LoadStockArticles();

            // Show/hide expedition fields based on invoice type
            UpdateExpeditionFieldVisibility();

            // Add TextChanged handlers for real-time validation
            txtStockTVA.TextChanged += TxtTVA_TextChanged;
            txtCustomTVA.TextChanged += TxtTVA_TextChanged;
            txtStockExpedition.TextChanged += TxtStockExpedition_TextChanged;
            txtCustomExpedition.TextChanged += TxtCustomExpedition_TextChanged;

            // Add paste handlers to prevent pasting invalid values
            DataObject.AddPastingHandler(txtStockTVA, OnPaste);
            DataObject.AddPastingHandler(txtCustomTVA, OnPaste);
            DataObject.AddPastingHandler(txtStockPrice, OnPaste);
            DataObject.AddPastingHandler(txtCustomPrice, OnPaste);
            DataObject.AddPastingHandler(txtStockQuantity, OnPaste);
            DataObject.AddPastingHandler(txtCustomQuantity, OnPaste);
            DataObject.AddPastingHandler(txtStockExpedition, OnPaste);
            DataObject.AddPastingHandler(txtCustomExpedition, OnPaste);
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsValidDecimalInput(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsValidDecimalInput(string text)
        {
            return text.All(c => char.IsDigit(c) || c == '.' || c == ',');
        }

        private void TxtTVA_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            decimal tva = ParseDecimal(textBox.Text);

            if (tva > 100)
            {
                textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                textBox.ToolTip = "La TVA ne peut pas dépasser 100%";
            }
            else
            {
                textBox.ClearValue(TextBox.BorderBrushProperty);
                textBox.ToolTip = null;
            }
        }

        private void TxtStockExpedition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isExpeditionMode) return;

            decimal expeditionQty = ParseDecimal(txtStockExpedition.Text);
            decimal quantity = ParseDecimal(txtStockQuantity.Text);

            if (expeditionQty > quantity)
            {
                txtStockExpedition.BorderBrush = System.Windows.Media.Brushes.Red;
                txtStockExpedition.ToolTip = $"Ne peut pas dépasser la quantité commandée ({quantity})";
            }
            else
            {
                txtStockExpedition.ClearValue(TextBox.BorderBrushProperty);
                txtStockExpedition.ToolTip = null;
            }
        }

        private void TxtCustomExpedition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isExpeditionMode) return;

            decimal expeditionQty = ParseDecimal(txtCustomExpedition.Text);
            decimal quantity = ParseDecimal(txtCustomQuantity.Text);

            if (expeditionQty > quantity)
            {
                txtCustomExpedition.BorderBrush = System.Windows.Media.Brushes.Red;
                txtCustomExpedition.ToolTip = $"Ne peut pas dépasser la quantité commandée ({quantity})";
            }
            else
            {
                txtCustomExpedition.ClearValue(TextBox.BorderBrushProperty);
                txtCustomExpedition.ToolTip = null;
            }
        }

        private void UpdateExpeditionFieldVisibility()
        {
            if (isExpeditionMode)
            {
                StockExpeditionPanel.Visibility = Visibility.Visible;
                CustomExpeditionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StockExpeditionPanel.Visibility = Visibility.Collapsed;
                CustomExpeditionPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadStockArticles()
        {
            try
            {
                allStockArticles = mainFa?.main?.la;

                if (allStockArticles != null && allStockArticles.Count > 0)
                {
                    stockArticles = new List<Article>(allStockArticles);
                    PopulateArticleComboBox(stockArticles);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des articles: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateArticleComboBox(List<Article> articles)
        {
            cmbStockArticles.Items.Clear();

            foreach (var article in articles)
            {
                ComboBoxItem item = new ComboBoxItem
                {
                    Content = $"{article.ArticleName} - {article.PrixVente:0.00} DH",
                    Tag = article
                };
                cmbStockArticles.Items.Add(item);
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            text = text.Replace(",", ".");

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return 0;
        }

        private void txtSearchArticle_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchText = txtSearchArticle.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    PopulateArticleComboBox(allStockArticles ?? new List<Article>());
                    cmbStockArticles.IsDropDownOpen = false;
                    return;
                }

                if (allStockArticles != null && allStockArticles.Count > 0)
                {
                    var filteredArticles = allStockArticles.Where(a =>
                        (a.ArticleName != null && a.ArticleName.ToLower().Contains(searchText.ToLower())) ||
                        a.PrixVente.ToString().Contains(searchText)
                    ).ToList();

                    PopulateArticleComboBox(filteredArticles);

                    if (filteredArticles.Count == 0)
                    {
                        ComboBoxItem noResultItem = new ComboBoxItem
                        {
                            Content = "Aucun article trouvé",
                            IsEnabled = false
                        };
                        cmbStockArticles.Items.Add(noResultItem);
                    }

                    cmbStockArticles.IsDropDownOpen = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
            }
        }

        private void btnStockArticle_Click(object sender, RoutedEventArgs e)
        {
            isStockMode = true;
            InitialPanel.Visibility = Visibility.Collapsed;
            StockArticlePanel.Visibility = Visibility.Visible;
            CustomArticlePanel.Visibility = Visibility.Collapsed;
            btnAdd.IsEnabled = false;
        }

        private void btnCustomArticle_Click(object sender, RoutedEventArgs e)
        {
            isStockMode = false;
            InitialPanel.Visibility = Visibility.Collapsed;
            StockArticlePanel.Visibility = Visibility.Collapsed;
            CustomArticlePanel.Visibility = Visibility.Visible;

            if (isExpeditionMode)
            {
                txtCustomExpedition.Text = "1";
            }

            btnAdd.IsEnabled = true;
        }

        private void cmbStockArticles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStockArticles.SelectedItem is ComboBoxItem item && item.Tag is Article article)
            {
                selectedArticle = article;

                txtStockPrice.Text = article.PrixVente.ToString("0.00");
                txtStockTVA.Text = article.tva.ToString("0.00");
                txtStockQuantity.Text = "1";

                if (isExpeditionMode)
                {
                    txtStockExpedition.Text = "1";
                }

                decimal quantity = GetArticleQuantity(article);
                txtAvailableStock.Text = $"{quantity} unités disponibles";

                btnAdd.IsEnabled = true;
            }
        }

        private decimal GetArticleQuantity(Article article)
        {
            var properties = article.GetType().GetProperties();

            foreach (var prop in properties)
            {
                string propName = prop.Name.ToLower();
                if (propName == "qte" || propName == "quantity" || propName == "quantite" ||
                    propName == "stock" || propName == "qtearticle")
                {
                    var value = prop.GetValue(article);
                    if (value != null)
                    {
                        try
                        {
                            if (value is int intValue)
                                return (decimal)intValue;
                            else if (value is decimal decValue)
                                return decValue;
                            else if (decimal.TryParse(value.ToString(), out decimal qty))
                                return qty;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            return 0;
        }

        private void SetArticleQuantity(Article article, decimal newQuantity)
        {
            var properties = article.GetType().GetProperties();

            foreach (var prop in properties)
            {
                string propName = prop.Name.ToLower();
                if (propName == "qte" || propName == "quantity" || propName == "quantite" ||
                    propName == "stock" || propName == "qtearticle")
                {
                    if (prop.CanWrite)
                    {
                        try
                        {
                            if (prop.PropertyType == typeof(int))
                                prop.SetValue(article, (int)newQuantity);
                            else if (prop.PropertyType == typeof(decimal))
                                prop.SetValue(article, newQuantity);
                            else if (prop.PropertyType == typeof(double))
                                prop.SetValue(article, (double)newQuantity);
                            else if (prop.PropertyType == typeof(float))
                                prop.SetValue(article, (float)newQuantity);
                            return;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (isStockMode)
                {
                    await AddStockArticle();
                }
                else
                {
                    AddCustomArticle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task AddStockArticle()
        {
            if (selectedArticle == null)
            {
                MessageBox.Show("Veuillez sélectionner un article.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal quantity = ParseDecimal(txtStockQuantity.Text);
            if (quantity <= 0)
            {
                MessageBox.Show("Veuillez entrer une quantité valide supérieure à 0.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockQuantity.Focus();
                return;
            }

            decimal price = ParseDecimal(txtStockPrice.Text);
            if (price <= 0)
            {
                MessageBox.Show("Veuillez entrer un prix valide supérieur à 0.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockPrice.Focus();
                return;
            }

            decimal tva = ParseDecimal(txtStockTVA.Text);

            if (tva > 100)
            {
                MessageBox.Show("La TVA ne peut pas dépasser 100%.\n\nVeuillez entrer une valeur entre 0 et 100.",
                    "TVA Invalide",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtStockTVA.Focus();
                txtStockTVA.SelectAll();
                return;
            }

            if (tva < 0)
            {
                MessageBox.Show("La TVA ne peut pas être négative.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStockTVA.Focus();
                txtStockTVA.SelectAll();
                return;
            }

            decimal expeditionQty = quantity;
            if (isExpeditionMode)
            {
                expeditionQty = ParseDecimal(txtStockExpedition.Text);

                if (expeditionQty < 0)
                {
                    MessageBox.Show("La quantité expédiée ne peut pas être négative.", "Attention",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtStockExpedition.Focus();
                    txtStockExpedition.SelectAll();
                    return;
                }

                if (expeditionQty > quantity)
                {
                    MessageBox.Show($"La quantité expédiée ({expeditionQty}) ne peut pas dépasser la quantité commandée ({quantity}).\n\nVeuillez ajuster la quantité expédiée.",
                        "Quantité Invalide",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtStockExpedition.Focus();
                    txtStockExpedition.SelectAll();
                    return;
                }
            }

            // Get current stock quantity
            decimal currentStock = GetArticleQuantity(selectedArticle);

            // ✓ DETERMINE IF STOCK SHOULD BE REDUCED
            bool shouldReduceStock = (currentStock > 0);

            // Check if stock is zero or insufficient
            if (currentStock == 0)
            {
                var result = MessageBox.Show(
                    $"La quantité en stock est 0 pour cet article.\n\nVoulez-vous l'ajouter quand même?",
                    "Stock épuisé",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                shouldReduceStock = false; // Don't reduce if stock is 0
            }
            else if (quantity > currentStock)
            {
                var result = MessageBox.Show(
                    $"Quantité insuffisante en stock. Disponible: {currentStock}\n\nVoulez-vous l'ajouter quand même?",
                    "Stock insuffisant",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }

                shouldReduceStock = false; // Don't reduce if quantity exceeds available
            }

            int intQuantity = (int)Math.Round(quantity);
            int intExpeditionQty = isExpeditionMode ? (int)Math.Round(expeditionQty) : intQuantity;

            // Create invoice article with ReduceStock flag
            InvoiceArticle invoiceArticle = new InvoiceArticle
            {
                OperationID = 0,
                ArticleID = selectedArticle.ArticleID,
                ArticleName = selectedArticle.ArticleName,
                Prix = price,
                Quantite = intQuantity,
                TVA = tva,
                Reversed = false,
                InitialQuantity = intExpeditionQty,
                ExpeditionTotal = intExpeditionQty,
                ReduceStock = shouldReduceStock // ✓ SET REDUCE STOCK FLAG
            };

            System.Diagnostics.Debug.WriteLine($"Adding Stock Article:");
            System.Diagnostics.Debug.WriteLine($"  Quantite: {intQuantity}");
            System.Diagnostics.Debug.WriteLine($"  InitialQuantity: {intExpeditionQty}");
            System.Diagnostics.Debug.WriteLine($"  TVA: {tva}%");
            System.Diagnostics.Debug.WriteLine($"  ReduceStock: {shouldReduceStock}");
            System.Diagnostics.Debug.WriteLine($"  CurrentStock: {currentStock}");

            mainFa.AddManualArticle(invoiceArticle);

            ResetStockArticleForm();
        }

        private void ResetStockArticleForm()
        {
            txtSearchArticle.Text = "";
            cmbStockArticles.SelectedIndex = -1;
            txtStockPrice.Text = "";
            txtStockQuantity.Text = "";
            txtStockTVA.Text = "";
            txtAvailableStock.Text = "";

            if (isExpeditionMode)
            {
                txtStockExpedition.Text = "";
            }

            selectedArticle = null;
            btnAdd.IsEnabled = false;
            LoadStockArticles();
        }

        private void AddCustomArticle()
        {
            if (string.IsNullOrWhiteSpace(txtCustomName.Text))
            {
                MessageBox.Show("Veuillez entrer un nom d'article.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomName.Focus();
                return;
            }

            decimal quantity = ParseDecimal(txtCustomQuantity.Text);
            if (quantity <= 0)
            {
                MessageBox.Show("Veuillez entrer une quantité valide supérieure à 0.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomQuantity.Focus();
                return;
            }

            decimal price = ParseDecimal(txtCustomPrice.Text);
            if (price <= 0)
            {
                MessageBox.Show("Veuillez entrer un prix valide supérieur à 0.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomPrice.Focus();
                return;
            }

            decimal tva = ParseDecimal(txtCustomTVA.Text);

            if (tva > 100)
            {
                MessageBox.Show("La TVA ne peut pas dépasser 100%.\n\nVeuillez entrer une valeur entre 0 et 100.",
                    "TVA Invalide",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                txtCustomTVA.Focus();
                txtCustomTVA.SelectAll();
                return;
            }

            if (tva < 0)
            {
                MessageBox.Show("La TVA ne peut pas être négative.", "Attention",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCustomTVA.Focus();
                txtCustomTVA.SelectAll();
                return;
            }

            decimal expeditionQty = quantity;
            if (isExpeditionMode)
            {
                expeditionQty = ParseDecimal(txtCustomExpedition.Text);

                if (expeditionQty < 0)
                {
                    MessageBox.Show("La quantité expédiée ne peut pas être négative.", "Attention",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCustomExpedition.Focus();
                    txtCustomExpedition.SelectAll();
                    return;
                }

                if (expeditionQty > quantity)
                {
                    MessageBox.Show($"La quantité expédiée ({expeditionQty}) ne peut pas dépasser la quantité commandée ({quantity}).\n\nVeuillez ajuster la quantité expédiée.",
                        "Quantité Invalide",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtCustomExpedition.Focus();
                    txtCustomExpedition.SelectAll();
                    return;
                }
            }

            int intQuantity = (int)Math.Round(quantity);
            int intExpeditionQty = isExpeditionMode ? (int)Math.Round(expeditionQty) : intQuantity;

            // Create invoice article - custom articles never reduce stock
            InvoiceArticle invoiceArticle = new InvoiceArticle
            {
                OperationID = 0,
                ArticleID = -DateTime.Now.Ticks.GetHashCode(),
                ArticleName = txtCustomName.Text,
                Prix = price,
                Quantite = intQuantity,
                TVA = tva,
                Reversed = false,
                InitialQuantity = intExpeditionQty,
                ExpeditionTotal = intExpeditionQty,
                ReduceStock = false // ✓ Custom articles never reduce stock
            };

            System.Diagnostics.Debug.WriteLine($"Adding Custom Article:");
            System.Diagnostics.Debug.WriteLine($"  Quantite: {intQuantity}");
            System.Diagnostics.Debug.WriteLine($"  InitialQuantity: {intExpeditionQty}");
            System.Diagnostics.Debug.WriteLine($"  TVA: {tva}%");
            System.Diagnostics.Debug.WriteLine($"  ReduceStock: false (custom)");

            mainFa.AddManualArticle(invoiceArticle);

            ResetCustomArticleForm();
        }

        private void ResetCustomArticleForm()
        {
            txtCustomName.Text = "";
            txtCustomPrice.Text = "";
            txtCustomQuantity.Text = "";
            txtCustomTVA.Text = "";

            if (isExpeditionMode)
            {
                txtCustomExpedition.Text = "";
            }

            txtCustomName.Focus();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DecimalTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null)
            {
                e.Handled = true;
                return;
            }

            if (e.Text == "." || e.Text == ",")
            {
                e.Handled = textBox.Text.Contains(".") || textBox.Text.Contains(",");
            }
            else
            {
                e.Handled = !e.Text.All(char.IsDigit);
            }
        }
    }
}