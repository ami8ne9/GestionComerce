using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GestionComerce.Main.Facturation.CreateFacture
{
    public partial class CSingleArticle : UserControl
    {
        private CMainFa mainFa;
        private InvoiceArticle article;

        public CSingleArticle(CMainFa mainFa, InvoiceArticle article)
        {
            InitializeComponent();
            this.mainFa = mainFa;
            this.article = article;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            txtArticleName.Text = article.ArticleName;

            // Check if in expedition mode
            bool isExpeditionMode = mainFa?.InvoiceType == "Expedition";

            if (isExpeditionMode)
            {
                // Show both ordered quantity and expedition quantity
                txtArticleDetails.Text = $"Qté: {article.Quantite} | Expédié: {article.InitialQuantity} | TVA: {article.TVA}%";

                // Show expedition info
                txtExpeditionInfo.Visibility = Visibility.Visible;
                txtExpeditionInfo.Text = $"📦 {article.InitialQuantity} expédiés";
            }
            else
            {
                // Show only quantity and TVA
                txtArticleDetails.Text = $"Qté: {article.Quantite} | TVA: {article.TVA}%";
                txtExpeditionInfo.Visibility = Visibility.Collapsed;
            }

            txtArticlePrice.Text = article.TotalTTC.ToString("0.00") + " DH";

            // Show type badge
            if (article.ArticleID < 0)
            {
                txtType.Text = "Personnalisé";
            }
            else if (article.OperationID == 0)
            {
                txtType.Text = "Stock";
            }
            else
            {
                txtType.Text = "Opération";
                badgeType.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DCFCE7"));
                txtType.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
                SideColor.Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#10B981"));
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if click was on the edit button
            var clickedElement = e.OriginalSource as FrameworkElement;
            while (clickedElement != null)
            {
                if (clickedElement == btnEdit)
                {
                    e.Handled = true;
                    return;
                }
                clickedElement = System.Windows.Media.VisualTreeHelper.GetParent(clickedElement) as FrameworkElement;
            }

            e.Handled = true;

            // Ask for deletion confirmation
            var result = MessageBox.Show(
                $"Voulez-vous supprimer l'article '{article.ArticleName}' ?",
                "Confirmer la suppression",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RemoveArticle();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            WEditArticle editWindow = new WEditArticle(mainFa, article, this);
            editWindow.ShowDialog();
        }

        private void RemoveArticle()
        {
            // Remove from invoice articles list
            mainFa.InvoiceArticles.Remove(article);

            // Remove this control from container
            var parent = this.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(this);
            }

            // Recalculate totals
            mainFa.RecalculateTotals();
        }

        // Method to update the article and refresh display
        public void UpdateArticle(InvoiceArticle updatedArticle)
        {
            this.article = updatedArticle;
            UpdateDisplay();
        }
    }
}