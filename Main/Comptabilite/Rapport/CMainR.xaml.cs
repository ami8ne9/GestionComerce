using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GestionComerce.Main.Facturation;

namespace GestionComerce.Main.ProjectManagment
{
    public partial class CMainR : UserControl
    {
        bool isInitialized = false;

        public CMainR(User u, MainWindow main)
        {
            InitializeComponent();

            this.Loaded += (s, e) => isInitialized = true;
            this.u = u;
            this.main = main;

            index3 = 10;
            index4 = 10;

            LOperation = new List<Operation>();
            LOperationArticle = new List<OperationArticle>();

            selectedbtn = "day";
            SetSelectedButton(DayButton);
            ShowView(DayView);
            PopulateYearComboBox(MonthYearComboBox);
            PopulateYearComboBox(YearComboBox);

            ResetStatistics();
            LoadInvoices(); // Load invoices on initialization
        }

        public User u;
        public MainWindow main;
        public int index3 = 10;
        public int index4 = 10;
        public string selectedbtn = "day";
        List<Operation> LOperation = new List<Operation>();
        List<OperationArticle> LOperationArticle = new List<OperationArticle>();
        List<Invoice> LInvoices = new List<Invoice>();
        private DateTime? _previousStartDate;
        private DateTime? _previousEndDate;

        private async void LoadInvoices()
        {
            try
            {
                var invoiceRepo = new InvoiceRepository("");
                LInvoices = await invoiceRepo.GetAllInvoicesAsync(false);
                
                // Load invoice articles for each invoice
                foreach (var invoice in LInvoices)
                {
                    if (invoice.Articles == null || invoice.Articles.Count == 0)
                    {
                        // Articles should already be loaded, but just in case
                        var fullInvoice = await invoiceRepo.GetInvoiceByIdAsync(invoice.InvoiceID);
                        if (fullInvoice != null)
                        {
                            invoice.Articles = fullInvoice.Articles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des factures: {ex.Message}");
                LInvoices = new List<Invoice>();
            }
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            selectedbtn = "day";
            index3 = 10;
            index4 = 10;

            DayDatePicker.SelectedDate = null;
            ResetStatistics();
            SetSelectedButton(DayButton);
            ShowView(DayView);
        }

        private void MonthButton_Click(object sender, RoutedEventArgs e)
        {
            selectedbtn = "month";
            index3 = 10;
            index4 = 10;

            ResetStatistics();
            SetSelectedButton(MonthButton);
            ShowView(MonthView);
            PopulateYearComboBox(MonthYearComboBox);
        }

        private void YearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedbtn = "year";
            index3 = 10;
            index4 = 10;

            ResetStatistics();
            SetSelectedButton(YearButton);
            ShowView(YearView);
            PopulateYearComboBox(YearComboBox);
        }

        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            selectedbtn = "personalized";
            index3 = 10;
            index4 = 10;

            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            ResetStatistics();
            SetSelectedButton(CustomButton);
            ShowView(CustomView);
        }

        private void ResetStatistics()
        {
            LOperation.Clear();
            LOperationArticle.Clear();

            BoughtText.Text = "0.00 DH";
            RevenueText.Text = "0.00 DH";
            SoldText.Text = "0.00 DH";
            DifferenceText.Text = "0.00 DH";
            ArticlesSoldText.Text = "0";
            ArticlesBoughtText.Text = "0";
            SoldOpsText.Text = "0";
            BoughtOpsText.Text = "0";

            TopClientsContainer.Children.Clear();
            TopSuppliersContainer.Children.Clear();
            TopArticlesContainer.Children.Clear();
            TopSalariesContainer.Children.Clear();
            RevenueOperationsContainer.Children.Clear();
            RevenueArticlesContainer.Children.Clear();
            
            SeeMoreContainer2.Visibility = Visibility.Collapsed;
            SeeMoreContainer3.Visibility = Visibility.Collapsed;
        }

        private void SetSelectedButton(Button selected)
        {
            ResetButtonStyle(DayButton);
            ResetButtonStyle(MonthButton);
            ResetButtonStyle(YearButton);
            ResetButtonStyle(CustomButton);
            SetActiveButtonStyle(selected);
        }

        private void ResetButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#64748B"));
        }

        private void SetActiveButtonStyle(Button button)
        {
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F46E5"));
            button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4F46E5"));
            button.Foreground = new SolidColorBrush(Colors.White);
        }

        private void ShowView(FrameworkElement viewToShow)
        {
            DayView.Visibility = Visibility.Collapsed;
            MonthView.Visibility = Visibility.Collapsed;
            YearView.Visibility = Visibility.Collapsed;
            CustomView.Visibility = Visibility.Collapsed;
            viewToShow.Visibility = Visibility.Visible;
        }

        private void PopulateYearComboBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear; i >= currentYear - 10; i--)
            {
                comboBox.Items.Add(i);
            }
            comboBox.SelectedIndex = 0;
        }

        private void DatePicker_Changed(object sender, EventArgs e)
        {
            if (!isInitialized)
                return;
            LoadStatistics();
        }

        private void DatePicker_Changed1(object sender, EventArgs e)
        {
            if (!isInitialized)
                return;

            if (StartDatePicker == null || EndDatePicker == null)
                return;

            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                if (sender == StartDatePicker && StartDatePicker.SelectedDate.HasValue)
                {
                    _previousStartDate = StartDatePicker.SelectedDate;
                }
                else if (sender == EndDatePicker && EndDatePicker.SelectedDate.HasValue)
                {
                    _previousEndDate = EndDatePicker.SelectedDate;
                }
                return;
            }

            DateTime startDate = StartDatePicker.SelectedDate.Value;
            DateTime endDate = EndDatePicker.SelectedDate.Value;

            if (endDate < startDate)
            {
                MessageBox.Show("La date de fin ne peut pas être antérieure à la date de début.",
                    "Dates invalides", MessageBoxButton.OK, MessageBoxImage.Warning);

                if (sender == StartDatePicker)
                {
                    StartDatePicker.SelectedDateChanged -= DatePicker_Changed1;
                    StartDatePicker.SelectedDate = _previousStartDate;
                    StartDatePicker.SelectedDateChanged += DatePicker_Changed1;
                }
                else if (sender == EndDatePicker)
                {
                    EndDatePicker.SelectedDateChanged -= DatePicker_Changed1;
                    EndDatePicker.SelectedDate = _previousEndDate;
                    EndDatePicker.SelectedDateChanged += DatePicker_Changed1;
                }
                return;
            }

            _previousStartDate = startDate;
            _previousEndDate = endDate;
            LOperation.Clear();
            LOperationArticle.Clear();
            LoadStatistics();
        }

        public void LoadStatistics()
        {
            LOperation.Clear();
            LOperationArticle.Clear();

            Decimal revenue = 0;
            Decimal achete = 0;
            Decimal vendus = 0;
            Decimal reverse = 0;
            int articleVendus = 0;
            int articleAchete = 0;
            int OperationVente = 0;
            int OperationAchete = 0;

            // Dictionaries for tracking top performers
            Dictionary<string, decimal> clientRevenue = new Dictionary<string, decimal>();
            Dictionary<int, decimal> supplierPurchases = new Dictionary<int, decimal>();
            Dictionary<int, decimal> articleRevenue = new Dictionary<int, decimal>();

            DateTime? filterStartDate = null;
            DateTime? filterEndDate = null;

            // Determine date range based on selected filter
            if (selectedbtn == "day" && DayDatePicker.SelectedDate.HasValue)
            {
                filterStartDate = DayDatePicker.SelectedDate.Value.Date;
                filterEndDate = filterStartDate.Value.AddDays(1);
            }
            else if (selectedbtn == "month" && MonthComboBox.SelectedItem != null && MonthYearComboBox.SelectedItem != null)
            {
                int selectedMonth = MonthComboBox.SelectedIndex + 1;
                int selectedYear = int.Parse(MonthYearComboBox.SelectedItem.ToString());
                filterStartDate = new DateTime(selectedYear, selectedMonth, 1);
                filterEndDate = filterStartDate.Value.AddMonths(1);
            }
            else if (selectedbtn == "year" && YearComboBox.SelectedItem != null)
            {
                int selectedYear = int.Parse(YearComboBox.SelectedItem.ToString());
                filterStartDate = new DateTime(selectedYear, 1, 1);
                filterEndDate = filterStartDate.Value.AddYears(1);
            }
            else if (selectedbtn == "personalized" && StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                filterStartDate = StartDatePicker.SelectedDate.Value.Date;
                filterEndDate = EndDatePicker.SelectedDate.Value.Date.AddDays(1);
            }

            if (filterStartDate.HasValue && filterEndDate.HasValue)
            {
                // Process Operations
                foreach (Operation o in main.lo)
                {
                    if (o.DateOperation >= filterStartDate && o.DateOperation < filterEndDate)
                    {
                        LOperation.Add(o);

                        if (o.Reversed)
                        {
                            reverse += o.PrixOperation;
                        }
                        else
                        {
                            bool isVente = o.OperationType != null && o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase);
                            bool isAchat = o.OperationType != null && o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase);

                            if (isVente)
                            {
                                OperationVente++;
                                vendus += o.PrixOperation;

                                if (o.ClientID.HasValue)
                                {
                                    var client = main.lc?.FirstOrDefault(c => c.ClientID == o.ClientID.Value);
                                    if (client != null)
                                    {
                                        string clientKey = client.Nom ?? "Client Inconnu";
                                        if (!clientRevenue.ContainsKey(clientKey))
                                            clientRevenue[clientKey] = 0;
                                        clientRevenue[clientKey] += o.PrixOperation;
                                    }
                                }
                            }
                            else if (isAchat)
                            {
                                OperationAchete++;
                                achete += o.PrixOperation;

                                if (o.FournisseurID.HasValue)
                                {
                                    if (!supplierPurchases.ContainsKey(o.FournisseurID.Value))
                                        supplierPurchases[o.FournisseurID.Value] = 0;
                                    supplierPurchases[o.FournisseurID.Value] += o.PrixOperation;
                                }
                            }
                        }

                        // Track articles from operations
                        foreach (OperationArticle oa in main.loa.Where(x => x.OperationID == o.OperationID))
                        {
                            LOperationArticle.Add(oa);

                            Article article = main.la?.FirstOrDefault(a => a.ArticleID == oa.ArticleID);
                            if (article != null && !oa.Reversed)
                            {
                                bool isVente = o.OperationType != null && o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase);
                                bool isAchat = o.OperationType != null && o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase);

                                if (isVente)
                                {
                                    articleVendus += oa.QteArticle;

                                    if (!articleRevenue.ContainsKey(oa.ArticleID))
                                        articleRevenue[oa.ArticleID] = 0;
                                    articleRevenue[oa.ArticleID] += article.PrixVente * oa.QteArticle;
                                }
                                else if (isAchat)
                                {
                                    articleAchete += oa.QteArticle;
                                }
                            }
                        }
                    }
                }

                // Process Invoices (Factures)
                // Process Invoices (Factures)
                foreach (Invoice invoice in LInvoices)
                {
                    if (invoice.InvoiceDate >= filterStartDate && invoice.InvoiceDate < filterEndDate && !invoice.IsDeleted)
                    {
                        if (invoice.IsReversed)
                        {
                            reverse += invoice.TotalTTC;
                        }
                        else
                        {
                            // Determine if invoice is sales or purchase
                            // Typically invoices are sales documents
                            bool isVente = true;
                            bool isAchat = false;

                            // If you have a way to distinguish purchase invoices, add logic here
                            // For example:
                            if (invoice.InvoiceType != null)
                            {
                                string typeLower = invoice.InvoiceType.ToLower(); // FIXED: Correct variable declaration
                                isAchat = typeLower.Contains("achat") || typeLower.Contains("purchase");
                                isVente = !isAchat; // If it's not purchase, it's sale
                            }

                            if (isVente)
                            {
                                OperationVente++;
                                vendus += invoice.TotalTTC;

                                // Track client revenue from invoices
                                if (!string.IsNullOrEmpty(invoice.ClientName))
                                {
                                    string clientKey = invoice.ClientName;
                                    if (!clientRevenue.ContainsKey(clientKey))
                                        clientRevenue[clientKey] = 0;
                                    clientRevenue[clientKey] += invoice.TotalTTC;
                                }
                            }
                            else if (isAchat)
                            {
                                OperationAchete++;
                                achete += invoice.TotalTTC;
                            }

                            // Track invoice articles
                            if (invoice.Articles != null)
                            {
                                foreach (var invoiceArticle in invoice.Articles.Where(a => !a.IsDeleted && !a.IsReversed))
                                {
                                    if (isVente)
                                    {
                                        articleVendus += (int)invoiceArticle.Quantite;

                                        if (!articleRevenue.ContainsKey(invoiceArticle.ArticleID))
                                            articleRevenue[invoiceArticle.ArticleID] = 0;
                                        articleRevenue[invoiceArticle.ArticleID] += invoiceArticle.TotalTTC;
                                    }
                                    else if (isAchat)
                                    {
                                        articleAchete += (int)invoiceArticle.Quantite;
                                    }
                                }
                            }
                        }
                    }
                }

                revenue = vendus - achete;
            }

            // Update UI
            BoughtText.Text = achete.ToString("N2") + " DH";
            RevenueText.Text = revenue.ToString("N2") + " DH";
            SoldText.Text = vendus.ToString("N2") + " DH";
            DifferenceText.Text = reverse.ToString("N2") + " DH";
            ArticlesSoldText.Text = articleVendus.ToString();
            ArticlesBoughtText.Text = articleAchete.ToString();
            SoldOpsText.Text = OperationVente.ToString();
            BoughtOpsText.Text = OperationAchete.ToString();

            // Load Top 5 lists
            LoadTopClients(clientRevenue);
            LoadTopSuppliers(supplierPurchases);
            LoadTopArticles(articleRevenue);
            LoadTopSalaries(filterStartDate, filterEndDate);
            
            // Load operations and articles lists
            LoadOpeerationsMouvment(LOperation);
            LoadOpeerationsArticleMouvment(LOperationArticle);
        }

        private void LoadTopClients(Dictionary<string, decimal> clientRevenue)
        {
            TopClientsContainer.Children.Clear();

            var topClients = clientRevenue
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            if (topClients.Count == 0)
            {
                AddEmptyState(TopClientsContainer, "Aucun client trouvé");
                return;
            }

            int rank = 1;
            foreach (var item in topClients)
            {
                AddTopItem(TopClientsContainer, rank, item.Key, item.Value, GetRankColor(rank));
                rank++;
            }
        }

        private void LoadTopSuppliers(Dictionary<int, decimal> supplierPurchases)
        {
            TopSuppliersContainer.Children.Clear();

            var topSuppliers = supplierPurchases
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            if (topSuppliers.Count == 0)
            {
                AddEmptyState(TopSuppliersContainer, "Aucun fournisseur trouvé");
                return;
            }

            int rank = 1;
            foreach (var item in topSuppliers)
            {
                var supplier = main.lfo?.FirstOrDefault(f => f.FournisseurID == item.Key);
                if (supplier != null)
                {
                    AddTopItem(TopSuppliersContainer, rank, supplier.Nom, item.Value, GetRankColor(rank));
                    rank++;
                }
            }
        }

        private void LoadTopArticles(Dictionary<int, decimal> articleRevenue)
        {
            TopArticlesContainer.Children.Clear();

            var topArticles = articleRevenue
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToList();

            if (topArticles.Count == 0)
            {
                AddEmptyState(TopArticlesContainer, "Aucun article trouvé");
                return;
            }

            int rank = 1;
            foreach (var item in topArticles)
            {
                var article = main.la?.FirstOrDefault(a => a.ArticleID == item.Key);
                if (article != null)
                {
                    AddTopItem(TopArticlesContainer, rank, article.ArticleName, item.Value, GetRankColor(rank));
                    rank++;
                }
            }
        }

        private void LoadTopSalaries(DateTime? startDate, DateTime? endDate)
        {
            TopSalariesContainer.Children.Clear();

            try
            {
                var salaryHelper = new SalaryReportHelper();
                var topSalaries = salaryHelper.GetTopSalaries(startDate, endDate);

                if (topSalaries.Count == 0)
                {
                    AddEmptyState(TopSalariesContainer, "Aucun salaire trouvé");
                    return;
                }

                int rank = 1;
                foreach (var item in topSalaries)
                {
                    AddTopItem(TopSalariesContainer, rank, item.EmployeeName, item.TotalSalary, GetRankColor(rank));
                    rank++;
                }
            }
            catch (Exception ex)
            {
                AddEmptyState(TopSalariesContainer, "Erreur lors du chargement");
            }
        }

        public void LoadOpeerationsMouvment(List<Operation> lo)
        {
            int i = 1;
            RevenueOperationsContainer.Children.Clear();
            foreach (Operation operation in lo)
            {
                if (i > index3) break;
                i++;
                CSingleOperation wSingleOperation = new CSingleOperation(this, operation);
                RevenueOperationsContainer.Children.Add(wSingleOperation);
            }

            SeeMoreContainer2.Visibility = lo.Count > index3 ? Visibility.Visible : Visibility.Collapsed;
        }

        public void LoadOpeerationsArticleMouvment(List<OperationArticle> loa)
        {
            int i = 1;
            RevenueArticlesContainer.Children.Clear();
            foreach (OperationArticle operationA in loa)
            {
                if (i > index4) break;
                i++;
                CSingleMouvment wSingleMouvment = new CSingleMouvment(this, operationA);
                RevenueArticlesContainer.Children.Add(wSingleMouvment);
            }

            SeeMoreContainer3.Visibility = loa.Count > index4 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            index3 = index3 + 10;
            LoadOpeerationsMouvment(LOperation);
        }

        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            index4 = index4 + 10;
            LoadOpeerationsArticleMouvment(LOperationArticle);
        }

        private void AddTopItem(StackPanel container, int rank, string name, decimal amount, string rankColor)
        {
            var itemBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 12),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"))
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Rank badge
            var rankBadge = new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(rankColor))
            };

            var rankText = new TextBlock
            {
                Text = rank.ToString(),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            rankBadge.Child = rankText;
            Grid.SetColumn(rankBadge, 0);
            grid.Children.Add(rankBadge);

            // Name
            var nameText = new TextBlock
            {
                Text = name ?? "N/A",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B")),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(nameText, 2);
            grid.Children.Add(nameText);

            // Amount
            var amountText = new TextBlock
            {
                Text = amount.ToString("N2") + " DH",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(amountText, 3);
            grid.Children.Add(amountText);

            itemBorder.Child = grid;
            container.Children.Add(itemBorder);
        }

        private void AddEmptyState(StackPanel container, string message)
        {
            var emptyBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC")),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(24),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"))
            };

            var emptyText = new TextBlock
            {
                Text = message,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            emptyBorder.Child = emptyText;
            container.Children.Add(emptyBorder);
        }

        private string GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return "#6366F1"; // Indigo
                case 2: return "#8B5CF6"; // Purple
                case 3: return "#EC4899"; // Pink
                case 4: return "#F59E0B"; // Amber
                case 5: return "#10B981"; // Green
                default: return "#94A3B8"; // Gray
            }
        }
    }
}