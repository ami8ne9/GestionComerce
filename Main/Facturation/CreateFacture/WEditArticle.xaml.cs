using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestionComerce.Main.Facturation.CreateFacture
{
    public partial class WEditArticle : Window
    {
        private CMainFa mainFa;
        private InvoiceArticle article;
        private CSingleArticle articleControl;
        private bool isExpeditionMode = false;

        public WEditArticle(CMainFa mainFa, InvoiceArticle article, CSingleArticle articleControl)
        {
            InitializeComponent();
            this.mainFa = mainFa;
            this.article = article;
            this.articleControl = articleControl;

            // Check if we're in Expedition mode
            isExpeditionMode = mainFa?.InvoiceType == "Expedition";

            LoadArticleData();

            // Show/hide expedition field based on invoice type
            UpdateExpeditionFieldVisibility();

            // Add TextChanged handlers for real-time validation
            txtTVA.TextChanged += TxtTVA_TextChanged;
            txtExpedition.TextChanged += TxtExpedition_TextChanged;

            // Add paste handler to prevent pasting invalid values
            DataObject.AddPastingHandler(txtTVA, OnPaste);
            DataObject.AddPastingHandler(txtPrice, OnPaste);
            DataObject.AddPastingHandler(txtQuantity, OnPaste);
            DataObject.AddPastingHandler(txtExpedition, OnPaste);
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
            // Allow only digits, dot and comma
            return text.All(c => char.IsDigit(c) || c == '.' || c == ',');
        }

        private void TxtTVA_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            decimal tva = ParseDecimal(textBox.Text);

            // Visual feedback for invalid TVA
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

        private void TxtExpedition_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isExpeditionMode) return;

            var textBox = sender as TextBox;
            if (textBox == null) return;

            decimal expeditionQty = ParseDecimal(txtExpedition.Text);
            decimal quantity = ParseDecimal(txtQuantity.Text);

            // Visual feedback for invalid expedition quantity
            if (expeditionQty > quantity)
            {
                textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                textBox.ToolTip = $"Ne peut pas dépasser la quantité commandée ({quantity})";
            }
            else
            {
                textBox.ClearValue(TextBox.BorderBrushProperty);
                textBox.ToolTip = null;
            }
        }

        private void UpdateExpeditionFieldVisibility()
        {
            if (isExpeditionMode)
            {
                ExpeditionPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ExpeditionPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadArticleData()
        {
            txtArticleName.Text = article.ArticleName;
            txtPrice.Text = article.Prix.ToString("0.00");
            txtQuantity.Text = article.Quantite.ToString("0.00");
            txtTVA.Text = article.TVA.ToString("0.00");

            // Load expedition quantity if in expedition mode
            if (isExpeditionMode)
            {
                txtExpedition.Text = article.InitialQuantity.ToString("0.00");
            }
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Replace comma with dot for decimal parsing
            text = text.Replace(",", ".");

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Parse and validate price
                decimal price = ParseDecimal(txtPrice.Text);
                if (price <= 0)
                {
                    MessageBox.Show("Veuillez entrer un prix valide supérieur à 0.", "Attention",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPrice.Focus();
                    return;
                }

                // Parse and validate quantity
                decimal quantity = ParseDecimal(txtQuantity.Text);
                if (quantity <= 0)
                {
                    MessageBox.Show("Veuillez entrer une quantité valide supérieure à 0.", "Attention",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtQuantity.Focus();
                    return;
                }

                // Parse and validate TVA
                decimal tva = ParseDecimal(txtTVA.Text);

                // CRITICAL: TVA cannot exceed 100%
                if (tva > 100)
                {
                    MessageBox.Show("La TVA ne peut pas dépasser 100%.\n\nVeuillez entrer une valeur entre 0 et 100.",
                        "TVA Invalide",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtTVA.Focus();
                    txtTVA.SelectAll();
                    return;
                }

                if (tva < 0)
                {
                    MessageBox.Show("La TVA ne peut pas être négative.", "Attention",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTVA.Focus();
                    txtTVA.SelectAll();
                    return;
                }

                // Validate expedition quantity if in expedition mode
                decimal expeditionQty = quantity;
                if (isExpeditionMode)
                {
                    expeditionQty = ParseDecimal(txtExpedition.Text);

                    if (expeditionQty < 0)
                    {
                        MessageBox.Show("La quantité expédiée ne peut pas être négative.", "Attention",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtExpedition.Focus();
                        txtExpedition.SelectAll();
                        return;
                    }

                    // CRITICAL: Expedition quantity cannot exceed ordered quantity
                    if (expeditionQty > quantity)
                    {
                        MessageBox.Show($"La quantité expédiée ({expeditionQty}) ne peut pas dépasser la quantité commandée ({quantity}).\n\nVeuillez ajuster la quantité expédiée.",
                            "Quantité Invalide",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        txtExpedition.Focus();
                        txtExpedition.SelectAll();
                        return;
                    }
                }

                // Update article properties
                article.Prix = price;
                article.Quantite = quantity;
                article.TVA = tva;

                // Update expedition quantity if in expedition mode
                if (isExpeditionMode)
                {
                    article.InitialQuantity = expeditionQty;
                    article.ExpeditionTotal = expeditionQty;
                }

                System.Diagnostics.Debug.WriteLine($"Updating Article:");
                System.Diagnostics.Debug.WriteLine($"  Quantite (QTY): {quantity}");
                System.Diagnostics.Debug.WriteLine($"  InitialQuantity (QTY EXPIDE): {expeditionQty}");
                System.Diagnostics.Debug.WriteLine($"  TVA: {tva}%");
                System.Diagnostics.Debug.WriteLine($"  IsExpeditionMode: {isExpeditionMode}");

                // Update the visual control
                articleControl.UpdateArticle(article);

                // Recalculate totals
                mainFa.RecalculateTotals();

                MessageBox.Show("Article modifié avec succès!", "Succès",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la modification: {ex.Message}", "Erreur",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            // Allow decimal point or comma
            if (e.Text == "." || e.Text == ",")
            {
                // Only allow one decimal separator
                e.Handled = textBox.Text.Contains(".") || textBox.Text.Contains(",");
            }
            else
            {
                // Only allow digits
                e.Handled = !e.Text.All(char.IsDigit);
            }
        }
    }
}