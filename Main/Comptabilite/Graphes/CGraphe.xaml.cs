using GestionComerce;
using GestionComerce.Main.Facturation;
using Superete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Superete.Main.Comptabilite.Graphes
{
    public partial class CGraphe : UserControl
    {
        User u;
        MainWindow main;
        private object selectedEntity;
        private int? selectedEntityId;

        // Data storage
        private List<Article> articles;
        private List<Client> clients;
        private List<Fournisseur> fournisseurs;
        private List<User> users;
        private List<Operation> operations;
        private List<OperationArticle> operationArticles;
        private List<Invoice> invoices;
        private List<FactureEnregistree> facturesEnregistrees;

        // Flag to track if data is loaded
        private bool isDataLoaded = false;

        public CGraphe(User u, MainWindow main)
        {
            InitializeComponent();
            this.u = u;
            this.main = main;

            // Set default dates
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            EndDatePicker.SelectedDate = DateTime.Now;

            // Make sure checkboxes are checked by default
            if (OperationsCheckBox != null)
                OperationsCheckBox.IsChecked = true;

            if (FacturesCheckBox != null)
                FacturesCheckBox.IsChecked = true;

            // Initialize - Load data asynchronously
            this.Loaded += CGraphe_Loaded;
        }

        private async void CGraphe_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isDataLoaded)
            {
                await LoadAllDataAsync();
                UpdateMetricOptions();
                isDataLoaded = true;
            }
        }

        private async System.Threading.Tasks.Task LoadAllDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== STARTING DATA LOAD ===");

                // Load all data
                articles = await new Article().GetAllArticlesAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {articles?.Count ?? 0} articles");

                clients = await new Client().GetClientsAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {clients?.Count ?? 0} clients");

                fournisseurs = await new Fournisseur().GetFournisseursAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {fournisseurs?.Count ?? 0} fournisseurs");

                users = await new User().GetUsersAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {users?.Count ?? 0} users");

                operations = await new Operation().GetOperationsAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {operations?.Count ?? 0} operations");

                operationArticles = await new OperationArticle().GetAllOperationArticlesAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {operationArticles?.Count ?? 0} operation articles");

                // Initialize repository with proper connection string
                string connectionString = "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;";
                var invoiceRepo = new InvoiceRepository(connectionString);

                System.Diagnostics.Debug.WriteLine("=== LOADING INVOICES ===");

                // **FIXED: Load invoices WITH articles**
                var allInvoices = await invoiceRepo.GetAllInvoicesAsync(includeDeleted: false);
                System.Diagnostics.Debug.WriteLine($"GetAllInvoicesAsync returned {allInvoices?.Count ?? 0} invoices");

                invoices = new List<Invoice>();

                if (allInvoices != null && allInvoices.Count > 0)
                {
                    // Load articles for each invoice
                    foreach (var inv in allInvoices)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Loading details for Invoice ID: {inv.InvoiceID}, Number: {inv.InvoiceNumber}");

                        try
                        {
                            // Get full invoice with articles
                            var fullInvoice = await invoiceRepo.GetInvoiceByIdAsync(inv.InvoiceID);

                            if (fullInvoice != null)
                            {
                                invoices.Add(fullInvoice);
                                System.Diagnostics.Debug.WriteLine($"    ✓ Loaded Invoice {fullInvoice.InvoiceNumber} (ID: {fullInvoice.InvoiceID}): {fullInvoice.Articles?.Count ?? 0} articles");

                                // Show article details
                                if (fullInvoice.Articles != null && fullInvoice.Articles.Count > 0)
                                {
                                    foreach (var art in fullInvoice.Articles)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"      - Article: {art.ArticleName}, Qty: {art.Quantite}, Price: {art.PrixUnitaire}");
                                    }
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"    ✗ Failed to load invoice ID: {inv.InvoiceID} - returned null");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"    ✗ ERROR loading invoice {inv.InvoiceID}: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"       Stack: {ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("  No invoices found in database");
                }

                facturesEnregistrees = FactureEnregistree.GetAllInvoices();
                System.Diagnostics.Debug.WriteLine($"Loaded {facturesEnregistrees?.Count ?? 0} factures enregistrées");

                System.Diagnostics.Debug.WriteLine("=== FINAL DATA SUMMARY ===");
                System.Diagnostics.Debug.WriteLine($"Total Invoices with Articles: {invoices?.Count ?? 0}");

                if (invoices != null && invoices.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Sample of first 3 invoices:");
                    foreach (var inv in invoices.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"  Invoice {inv.InvoiceNumber}:");
                        System.Diagnostics.Debug.WriteLine($"    - Date: {inv.InvoiceDate:yyyy-MM-dd}");
                        System.Diagnostics.Debug.WriteLine($"    - Client: {inv.ClientName ?? "N/A"}");
                        System.Diagnostics.Debug.WriteLine($"    - Reference Client: {inv.ReferenceClient ?? "N/A"}");
                        System.Diagnostics.Debug.WriteLine($"    - Articles: {inv.Articles?.Count ?? 0}");
                        System.Diagnostics.Debug.WriteLine($"    - Total TTC: {inv.TotalTTC}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("=== DATA LOAD COMPLETE ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== CRITICAL ERROR IN LoadAllDataAsync ===");
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
                }

                MessageBox.Show(
                    $"Erreur lors du chargement des données:\n\n{ex.Message}\n\nVeuillez vérifier la console de débogage pour plus de détails.",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EntityTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Add null check
            if (SearchPanel == null || MetricPanel == null || EntityTypeComboBox == null)
                return;

            var selectedItem = (EntityTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Show/hide search panel based on selection
            bool showSearch = selectedItem == "Article" ||
                            selectedItem == "Client" ||
                            selectedItem == "Fournisseur" ||
                            selectedItem == "Utilisateur";

            SearchPanel.Visibility = showSearch ? Visibility.Visible : Visibility.Collapsed;

            // Clear search
            if (SearchTextBox != null)
            {
                SearchTextBox.Text = "";
            }

            if (SearchResultsListBox != null)
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
            }

            selectedEntity = null;
            selectedEntityId = null;

            // Update metric options based on entity type
            UpdateMetricOptions();
        }

        private void UpdateMetricOptions()
        {
            // Add null check
            if (MetricComboBox == null || EntityTypeComboBox == null)
                return;

            MetricComboBox.Items.Clear();
            var entityType = (EntityTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            switch (entityType)
            {
                case "Article":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Stock" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Articles Achetés" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Articles Vendus" });
                    break;

                case "Client":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Articles Vendus" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Revenus Générés" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Opérations de Vente" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Factures Générées" });
                    break;

                case "Fournisseur":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Articles Achetés" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Dépenses" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Opérations d'Achat" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Factures Enregistrées" });
                    break;

                case "Utilisateur":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Nombre d'Opérations" });
                    break;

                case "Opération":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Par Type d'Opération" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Opérations Annulées" });
                    break;

                case "Facture":
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Factures Générées" });
                    MetricComboBox.Items.Add(new ComboBoxItem { Content = "Factures Enregistrées" });
                    break;
            }

            if (MetricComboBox.Items.Count > 0)
                MetricComboBox.SelectedIndex = 0;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            var entityType = (EntityTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            List<SearchResult> results = new List<SearchResult>();

            switch (entityType)
            {
                case "Article":
                    results = articles?
                        .Where(a => a.ArticleName.ToLower().Contains(searchText) || a.Code.ToString().Contains(searchText))
                        .Select(a => new SearchResult { Id = a.ArticleID, DisplayName = $"{a.ArticleName} ({a.Code})" })
                        .ToList() ?? new List<SearchResult>();
                    break;

                case "Client":
                    results = clients?
                        .Where(c => c.Nom.ToLower().Contains(searchText))
                        .Select(c => new SearchResult { Id = c.ClientID, DisplayName = c.Nom })
                        .ToList() ?? new List<SearchResult>();
                    break;

                case "Fournisseur":
                    results = fournisseurs?
                        .Where(f => f.Nom.ToLower().Contains(searchText))
                        .Select(f => new SearchResult { Id = f.FournisseurID, DisplayName = f.Nom })
                        .ToList() ?? new List<SearchResult>();
                    break;

                case "Utilisateur":
                    results = users?
                        .Where(u => u.UserName.ToLower().Contains(searchText))
                        .Select(u => new SearchResult { Id = u.UserID, DisplayName = u.UserName })
                        .ToList() ?? new List<SearchResult>();
                    break;
            }

            if (results.Count > 0)
            {
                SearchResultsListBox.ItemsSource = results;
                SearchResultsListBox.DisplayMemberPath = "DisplayName";
                SearchResultsListBox.Visibility = Visibility.Visible;
            }
            else
            {
                SearchResultsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is SearchResult result)
            {
                SearchTextBox.Text = result.DisplayName;
                selectedEntityId = result.Id;
                SearchResultsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void GraphTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Auto-update on change
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("=== APPLY BUTTON CLICKED ===");
            System.Diagnostics.Debug.WriteLine($"Operations Checkbox: {OperationsCheckBox?.IsChecked}");
            System.Diagnostics.Debug.WriteLine($"Factures Checkbox: {FacturesCheckBox?.IsChecked}");
            System.Diagnostics.Debug.WriteLine($"Invoices Count: {invoices?.Count ?? 0}");

            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner les dates de début et de fin.",
                              "Dates manquantes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate > EndDatePicker.SelectedDate)
            {
                MessageBox.Show("La date de début doit être antérieure à la date de fin.",
                              "Dates invalides", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DiagnoseInvoiceIssue();
            LoadGraph();
        }

        private void LoadGraph()
        {
            GraphCanvas.Children.Clear();
            EmptyState.Visibility = Visibility.Collapsed;

            var entityType = (EntityTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var metric = (MetricComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var graphType = (GraphTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            System.Diagnostics.Debug.WriteLine($"=== LOADING GRAPH ===");
            System.Diagnostics.Debug.WriteLine($"Entity Type: {entityType}");
            System.Diagnostics.Debug.WriteLine($"Metric: {metric}");
            System.Diagnostics.Debug.WriteLine($"Graph Type: {graphType}");
            System.Diagnostics.Debug.WriteLine($"Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Update title
            GraphTitle.Text = $"{entityType} - {metric} ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";

            // Generate data based on entity and metric
            var data = GenerateGraphData(entityType, metric, startDate, endDate);

            System.Diagnostics.Debug.WriteLine($"Generated {data?.Count ?? 0} data points");

            if (data == null || data.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                ((TextBlock)EmptyState.Children[1]).Text = "Aucune donnée disponible pour cette sélection";
                return;
            }

            // Draw graph
            switch (graphType)
            {
                case "Ligne":
                    DrawLineGraph(data);
                    break;
                case "Barres":
                    DrawBarGraph(data);
                    break;
                case "Camembert":
                    DrawPieChart(data);
                    break;
                case "Aires":
                    DrawAreaGraph(data);
                    break;
            }
        }

        private List<GraphDataPoint> GenerateGraphData(string entityType, string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            try
            {
                switch (entityType)
                {
                    case "Article":
                        data = GenerateArticleData(metric, startDate, endDate);
                        break;
                    case "Client":
                        data = GenerateClientData(metric, startDate, endDate);
                        break;
                    case "Fournisseur":
                        data = GenerateFournisseurData(metric, startDate, endDate);
                        break;
                    case "Utilisateur":
                        data = GenerateUserData(metric, startDate, endDate);
                        break;
                    case "Opération":
                        data = GenerateOperationData(metric, startDate, endDate);
                        break;
                    case "Facture":
                        data = GenerateFactureData(metric, startDate, endDate);
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in GenerateGraphData: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");

                MessageBox.Show($"Erreur lors de la génération des données: {ex.Message}",
                              "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return data;
        }
        private List<GraphDataPoint> GenerateArticleData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            if (selectedEntityId == null)
            {
                MessageBox.Show("Veuillez sélectionner un article.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return data;
            }

            var articleOps = operationArticles.Where(oa => oa.ArticleID == selectedEntityId.Value).ToList();

            switch (metric)
            {
                case "Stock":
                    // Track stock evolution over time - only from operations
                    if (OperationsCheckBox.IsChecked == true)
                    {
                        var currentDate = startDate;
                        var article = articles.FirstOrDefault(a => a.ArticleID == selectedEntityId.Value);

                        // Calculate initial stock at start date by going backwards from current stock
                        int currentStock = article?.Quantite ?? 0;

                        // Get all operations after start date to calculate backwards
                        var opsAfterStart = articleOps
                            .Where(oa => operations.Any(o => o.OperationID == oa.OperationID &&
                                                             o.DateOperation >= startDate))
                            .ToList();

                        // Calculate stock at start date by reversing operations after start
                        foreach (var op in opsAfterStart)
                        {
                            var operation = operations.FirstOrDefault(o => o.OperationID == op.OperationID);
                            if (operation != null && operation.OperationType != null)
                            {
                                // Reverse the operation effect
                                if (operation.OperationType.StartsWith("V")) // Vente (Sales)
                                    currentStock += op.QteArticle; // Add back what was sold
                                else if (operation.OperationType.StartsWith("A")) // Achat (Purchase)
                                    currentStock -= op.QteArticle; // Remove what was bought
                            }
                        }

                        int stockAtStartDate = currentStock;

                        while (currentDate <= endDate)
                        {
                            // Only process if date is not in the future
                            if (currentDate.Date <= DateTime.Now.Date)
                            {
                                var opsForDate = articleOps
                                    .Where(oa => operations.Any(o => o.OperationID == oa.OperationID &&
                                                                     o.DateOperation.Date == currentDate.Date))
                                    .ToList();

                                foreach (var op in opsForDate)
                                {
                                    var operation = operations.FirstOrDefault(o => o.OperationID == op.OperationID);
                                    if (operation != null && operation.OperationType != null)
                                    {
                                        if (operation.OperationType.StartsWith("V")) // Vente (Sales)
                                            stockAtStartDate -= op.QteArticle;
                                        else if (operation.OperationType.StartsWith("A")) // Achat (Purchase)
                                            stockAtStartDate += op.QteArticle;
                                    }
                                }

                                data.Add(new GraphDataPoint { Date = currentDate, Value = stockAtStartDate });
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    break;

                case "Articles Achetés":
                    // Track QUANTITY of articles purchased (not number of transactions)
                    if (OperationsCheckBox.IsChecked == true)
                    {
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            // Only process if date is not in the future
                            if (currentDate.Date <= DateTime.Now.Date)
                            {
                                var purchasesForDay = articleOps
                                    .Where(oa => operations.Any(o => o.OperationID == oa.OperationID &&
                                                                     o.OperationType != null &&
                                                                     o.OperationType.StartsWith("A") &&
                                                                     o.DateOperation.Date == currentDate.Date))
                                    .Sum(oa => oa.QteArticle);

                                // Add data point even if value is 0
                                data.Add(new GraphDataPoint
                                {
                                    Date = currentDate,
                                    Value = purchasesForDay
                                });
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    break;

                case "Articles Vendus":
                    // Track QUANTITY of articles sold (not number of transactions)
                    if (OperationsCheckBox.IsChecked == true)
                    {
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            // Only process if date is not in the future
                            if (currentDate.Date <= DateTime.Now.Date)
                            {
                                var salesForDay = articleOps
                                    .Where(oa => operations.Any(o => o.OperationID == oa.OperationID &&
                                                                     o.OperationType != null &&
                                                                     o.OperationType.StartsWith("V") &&
                                                                     o.DateOperation.Date == currentDate.Date &&
                                                                     !oa.Reversed))
                                    .Sum(oa => oa.QteArticle);

                                // Add data point even if value is 0
                                data.Add(new GraphDataPoint
                                {
                                    Date = currentDate,
                                    Value = salesForDay
                                });
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    break;
            }

            return data;
        }
        // Replace the GenerateClientData method with this improved version

        private List<GraphDataPoint> GenerateClientData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            if (selectedEntityId == null)
            {
                MessageBox.Show("Veuillez sélectionner un client.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return data;
            }

            var client = clients.FirstOrDefault(c => c.ClientID == selectedEntityId.Value);
            if (client == null)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: Client not found for ID {selectedEntityId.Value}");
                return data;
            }

            // CRITICAL: Trim whitespace
            string clientName = client.Nom?.Trim();
            string clientIdStr = selectedEntityId.Value.ToString();

            var clientOps = operations.Where(o => o.ClientID == selectedEntityId.Value &&
                                                  o.DateOperation >= startDate &&
                                                  o.DateOperation <= endDate).ToList();

            System.Diagnostics.Debug.WriteLine($"=== GenerateClientData for '{clientName}' (ID: {clientIdStr}) ===");
            System.Diagnostics.Debug.WriteLine($"  Date Range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
            System.Diagnostics.Debug.WriteLine($"  Client Operations: {clientOps.Count}");
            System.Diagnostics.Debug.WriteLine($"  Total Generated Invoices: {invoices?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  FacturesCheckBox: {FacturesCheckBox?.IsChecked}");

            switch (metric)
            {
                case "Articles Vendus":
                    {
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            if (currentDate.Date <= DateTime.Now.Date)
                            {
                                double totalSold = 0;

                                // ====== INVOICES: Articles in generated invoices for this client ======
                                if (FacturesCheckBox?.IsChecked == true && invoices != null)
                                {
                                    // Match by ClientName (case-insensitive, trimmed)
                                    var clientInvoicesForDate = invoices.Where(i =>
                                        i.InvoiceDate.Date == currentDate.Date &&
                                        i.Articles != null &&
                                        i.Articles.Count > 0 &&
                                        (
                                            string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                            i.ReferenceClient?.Trim() == clientIdStr
                                        )
                                    ).ToList();

                                    if (clientInvoicesForDate.Count > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Date {currentDate:yyyy-MM-dd}: Found {clientInvoicesForDate.Count} invoices");

                                        foreach (var invoice in clientInvoicesForDate)
                                        {
                                            var invoiceQty = invoice.Articles.Sum(art => (double)art.Quantite);
                                            totalSold += invoiceQty;
                                            System.Diagnostics.Debug.WriteLine($"    Invoice {invoice.InvoiceNumber}: {invoiceQty} articles");
                                        }
                                    }
                                }

                                // ====== OPERATIONS: Only count operations NOT already in invoices ======
                                if (OperationsCheckBox?.IsChecked == true)
                                {
                                    // Get operation IDs that are in invoices (to avoid double-counting)
                                    var operationIdsInInvoices = new HashSet<int>();
                                    if (invoices != null)
                                    {
                                        var clientInvoicesAllDates = invoices.Where(i =>
                                            string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                            i.ReferenceClient?.Trim() == clientIdStr
                                        ).ToList();

                                        foreach (var invoice in clientInvoicesAllDates)
                                        {
                                            if (invoice.Articles != null)
                                            {
                                                foreach (var art in invoice.Articles.Where(a => a.OperationID.HasValue))
                                                {
                                                    operationIdsInInvoices.Add(art.OperationID.Value);
                                                }
                                            }
                                        }
                                    }

                                    var opArticles = clientOps
                                        .Where(o => o.DateOperation.Date == currentDate.Date &&
                                                   !operationIdsInInvoices.Contains(o.OperationID))
                                        .SelectMany(o => operationArticles.Where(oa => oa.OperationID == o.OperationID && !oa.Reversed))
                                        .Sum(oa => oa.QteArticle);

                                    totalSold += opArticles;
                                    if (opArticles > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"    Operations (not in invoices): {opArticles} articles");
                                    }
                                }

                                if (totalSold > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"    ✓ Total Sold: {totalSold}");
                                }
                                data.Add(new GraphDataPoint { Date = currentDate, Value = totalSold });
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    break;

                case "Revenus Générés":
                    {
                        var currentDateRev = startDate;

                        while (currentDateRev <= endDate)
                        {
                            if (currentDateRev.Date <= DateTime.Now.Date)
                            {
                                double totalRevenue = 0;

                                // ====== INVOICES: Revenue from generated invoices ======
                                if (FacturesCheckBox?.IsChecked == true && invoices != null)
                                {
                                    var clientInvoicesForDate = invoices.Where(i =>
                                        i.InvoiceDate.Date == currentDateRev.Date &&
                                        (
                                            string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                            i.ReferenceClient?.Trim() == clientIdStr
                                        )
                                    ).ToList();

                                    var invoiceRevenue = clientInvoicesForDate.Sum(i => (double)i.TotalTTC);
                                    totalRevenue += invoiceRevenue;

                                    if (invoiceRevenue > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"    Invoice Revenue: {invoiceRevenue:F2}");
                                    }
                                }

                                // ====== OPERATIONS: Only operations NOT in invoices ======
                                if (OperationsCheckBox?.IsChecked == true)
                                {
                                    var operationIdsWithInvoices = new HashSet<int>();
                                    if (invoices != null)
                                    {
                                        var clientInvoicesAllDates = invoices.Where(i =>
                                            string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                            i.ReferenceClient?.Trim() == clientIdStr
                                        ).ToList();

                                        foreach (var invoice in clientInvoicesAllDates)
                                        {
                                            if (invoice.Articles != null)
                                            {
                                                foreach (var art in invoice.Articles.Where(a => a.OperationID.HasValue))
                                                {
                                                    operationIdsWithInvoices.Add(art.OperationID.Value);
                                                }
                                            }
                                        }
                                    }

                                    var opRevenue = clientOps
                                        .Where(o => o.DateOperation.Date == currentDateRev.Date &&
                                                   !o.Reversed &&
                                                   !operationIdsWithInvoices.Contains(o.OperationID))
                                        .Sum(o => (double)o.PrixOperation);

                                    totalRevenue += opRevenue;

                                    if (opRevenue > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"    Operations Revenue: {opRevenue:F2}");
                                    }
                                }

                                data.Add(new GraphDataPoint { Date = currentDateRev, Value = totalRevenue });
                            }

                            currentDateRev = currentDateRev.AddDays(1);
                        }
                    }
                    break;

                case "Opérations de Vente":
                    {
                        var currentDateOps = startDate;

                        while (currentDateOps <= endDate)
                        {
                            if (currentDateOps.Date <= DateTime.Now.Date)
                            {
                                int totalOps = 0;

                                if (OperationsCheckBox?.IsChecked == true)
                                {
                                    // Get operation IDs that have invoices
                                    var operationIdsWithInvoices = new HashSet<int>();
                                    if (FacturesCheckBox?.IsChecked == true && invoices != null)
                                    {
                                        var clientInvoicesAllDates = invoices.Where(i =>
                                            string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                            i.ReferenceClient?.Trim() == clientIdStr
                                        ).ToList();

                                        foreach (var invoice in clientInvoicesAllDates)
                                        {
                                            if (invoice.Articles != null)
                                            {
                                                foreach (var art in invoice.Articles.Where(a => a.OperationID.HasValue))
                                                {
                                                    operationIdsWithInvoices.Add(art.OperationID.Value);
                                                }
                                            }
                                        }
                                    }

                                    totalOps = clientOps
                                        .Count(o => o.DateOperation.Date == currentDateOps.Date &&
                                                   !operationIdsWithInvoices.Contains(o.OperationID));
                                }

                                data.Add(new GraphDataPoint { Date = currentDateOps, Value = totalOps });
                            }

                            currentDateOps = currentDateOps.AddDays(1);
                        }
                    }
                    break;

                case "Factures Générées":
                    {
                        var currentDateInv = startDate;

                        while (currentDateInv <= endDate)
                        {
                            if (currentDateInv.Date <= DateTime.Now.Date)
                            {
                                int totalInvoices = 0;

                                if (FacturesCheckBox?.IsChecked == true && invoices != null)
                                {
                                    totalInvoices = invoices
                                        .Count(i =>
                                            i.InvoiceDate.Date == currentDateInv.Date &&
                                            (
                                                string.Equals(i.ClientName?.Trim(), clientName, StringComparison.OrdinalIgnoreCase) ||
                                                i.ReferenceClient?.Trim() == clientIdStr
                                            )
                                        );

                                    if (totalInvoices > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Date {currentDateInv:yyyy-MM-dd}: {totalInvoices} invoices");
                                    }
                                }

                                data.Add(new GraphDataPoint { Date = currentDateInv, Value = totalInvoices });
                            }

                            currentDateInv = currentDateInv.AddDays(1);
                        }
                    }
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"  Total data points: {data.Count}, Non-zero: {data.Count(d => d.Value > 0)}");
            return data;
        }

        private List<GraphDataPoint> GenerateFournisseurData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            if (selectedEntityId == null)
            {
                MessageBox.Show("Veuillez sélectionner un fournisseur.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return data;
            }

            var fournisseurOps = operations.Where(o => o.FournisseurID == selectedEntityId.Value &&
                                                       o.DateOperation >= startDate &&
                                                       o.DateOperation <= endDate).ToList();

            System.Diagnostics.Debug.WriteLine($"=== GenerateFournisseurData for ID {selectedEntityId.Value} ===");
            System.Diagnostics.Debug.WriteLine($"  Operations: {fournisseurOps.Count}");
            System.Diagnostics.Debug.WriteLine($"  Saved Invoices Available: {facturesEnregistrees?.Count ?? 0}");

            switch (metric)
            {
                case "Articles Achetés":
                    if (OperationsCheckBox?.IsChecked == true)
                    {
                        var currentDate = startDate;

                        while (currentDate <= endDate)
                        {
                            if (currentDate.Date <= DateTime.Now.Date)
                            {
                                var articlesForDay = fournisseurOps
                                    .Where(o => o.DateOperation.Date == currentDate.Date)
                                    .SelectMany(o => operationArticles.Where(oa => oa.OperationID == o.OperationID && !oa.Reversed))
                                    .Sum(oa => oa.QteArticle);

                                data.Add(new GraphDataPoint { Date = currentDate, Value = articlesForDay });
                            }

                            currentDate = currentDate.AddDays(1);
                        }
                    }
                    break;

                case "Dépenses":
                    {
                        var currentDateExp = startDate;

                        while (currentDateExp <= endDate)
                        {
                            if (currentDateExp.Date <= DateTime.Now.Date)
                            {
                                double totalExpenses = 0;

                                // ====== OPERATIONS: Purchase operations ======
                                if (OperationsCheckBox?.IsChecked == true)
                                {
                                    var opExpenses = fournisseurOps
                                        .Where(o => o.DateOperation.Date == currentDateExp.Date && !o.Reversed)
                                        .Sum(o => (double)o.PrixOperation);

                                    totalExpenses += opExpenses;

                                    if (opExpenses > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Date {currentDateExp:yyyy-MM-dd}: Operations Expenses = {opExpenses:F2}");
                                    }
                                }

                                // ====== SAVED INVOICES: Match by FournisseurID ======
                                if (FacturesCheckBox?.IsChecked == true && facturesEnregistrees != null)
                                {
                                    var savedInvoicesForDate = facturesEnregistrees
                                        .Where(f => f.FournisseurID == selectedEntityId.Value &&
                                                   f.InvoiceDate.Date == currentDateExp.Date)
                                        .ToList();

                                    var invoiceExpenses = savedInvoicesForDate.Sum(f => (double)f.TotalAmount);
                                    totalExpenses += invoiceExpenses;

                                    if (invoiceExpenses > 0)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"  Date {currentDateExp:yyyy-MM-dd}: Saved Invoices Expenses = {invoiceExpenses:F2} ({savedInvoicesForDate.Count} invoices)");
                                    }
                                }

                                if (totalExpenses > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"    ✓ Total Expenses: {totalExpenses:F2}");
                                }

                                data.Add(new GraphDataPoint { Date = currentDateExp, Value = totalExpenses });
                            }

                            currentDateExp = currentDateExp.AddDays(1);
                        }
                    }
                    break;

                case "Opérations d'Achat":
                    if (OperationsCheckBox?.IsChecked == true)
                    {
                        var currentDateOps = startDate;

                        while (currentDateOps <= endDate)
                        {
                            if (currentDateOps.Date <= DateTime.Now.Date)
                            {
                                var opsCount = fournisseurOps
                                    .Count(o => o.DateOperation.Date == currentDateOps.Date);

                                data.Add(new GraphDataPoint { Date = currentDateOps, Value = opsCount });
                            }

                            currentDateOps = currentDateOps.AddDays(1);
                        }
                    }
                    break;

                case "Factures Enregistrées":
                    if (FacturesCheckBox?.IsChecked == true && facturesEnregistrees != null)
                    {
                        var currentDateInv = startDate;

                        while (currentDateInv <= endDate)
                        {
                            if (currentDateInv.Date <= DateTime.Now.Date)
                            {
                                // Match by FournisseurID
                                var invoicesCount = facturesEnregistrees
                                    .Count(f => f.FournisseurID == selectedEntityId.Value &&
                                               f.InvoiceDate.Date == currentDateInv.Date);

                                if (invoicesCount > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  Date {currentDateInv:yyyy-MM-dd}: {invoicesCount} saved invoices");
                                }

                                data.Add(new GraphDataPoint { Date = currentDateInv, Value = invoicesCount });
                            }

                            currentDateInv = currentDateInv.AddDays(1);
                        }
                    }
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"  Total data points: {data.Count}, Non-zero: {data.Count(d => d.Value > 0)}");
            return data;
        }


        private List<GraphDataPoint> GenerateUserData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            if (selectedEntityId == null)
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur.", "Sélection requise", MessageBoxButton.OK, MessageBoxImage.Warning);
                return data;
            }

            if (OperationsCheckBox?.IsChecked == true)
            {
                var userOps = operations
                    .Where(o => o.UserID == selectedEntityId.Value &&
                               o.DateOperation >= startDate &&
                               o.DateOperation <= endDate)
                    .GroupBy(o => o.DateOperation.Date)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var group in userOps)
                {
                    data.Add(new GraphDataPoint { Date = group.Key, Value = group.Count() });
                }
            }

            return data;
        }

        private List<GraphDataPoint> GenerateOperationData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            if (OperationsCheckBox?.IsChecked != true)
                return data;

            var filteredOps = operations.Where(o => o.DateOperation >= startDate && o.DateOperation <= endDate).ToList();

            switch (metric)
            {
                case "Par Type d'Opération":
                    var opsByType = filteredOps
                        .GroupBy(o => o.OperationType)
                        .ToList();

                    foreach (var group in opsByType)
                    {
                        data.Add(new GraphDataPoint
                        {
                            Label = group.Key ?? "Non défini",
                            Value = group.Count()
                        });
                    }
                    break;

                case "Opérations Annulées":
                    {
                        var currentDateRev = startDate;

                        while (currentDateRev <= endDate)
                        {
                            if (currentDateRev.Date <= DateTime.Now.Date)
                            {
                                var reversedCount = filteredOps
                                    .Count(o => o.Reversed && o.DateOperation.Date == currentDateRev.Date);

                                data.Add(new GraphDataPoint { Date = currentDateRev, Value = reversedCount });
                            }

                            currentDateRev = currentDateRev.AddDays(1);
                        }
                    }
                    break;
            }

            return data;
        }

        private List<GraphDataPoint> GenerateFactureData(string metric, DateTime startDate, DateTime endDate)
        {
            var data = new List<GraphDataPoint>();

            System.Diagnostics.Debug.WriteLine($"=== GenerateFactureData ===");
            System.Diagnostics.Debug.WriteLine($"  Metric: {metric}");
            System.Diagnostics.Debug.WriteLine($"  FacturesCheckBox.IsChecked: {FacturesCheckBox?.IsChecked}");
            System.Diagnostics.Debug.WriteLine($"  Invoices Count: {invoices?.Count ?? 0}");

            switch (metric)
            {
                case "Factures Générées":
                    if (FacturesCheckBox?.IsChecked == true && invoices != null)
                    {
                        var currentDateGen = startDate;

                        while (currentDateGen <= endDate)
                        {
                            if (currentDateGen.Date <= DateTime.Now.Date)
                            {
                                var invoicesCount = invoices
                                    .Count(i => i.InvoiceDate.Date == currentDateGen.Date);

                                data.Add(new GraphDataPoint { Date = currentDateGen, Value = invoicesCount });

                                if (invoicesCount > 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  Date {currentDateGen:yyyy-MM-dd}: {invoicesCount} invoices");
                                }
                            }

                            currentDateGen = currentDateGen.AddDays(1);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("  Checkbox not checked or no invoices available");
                    }
                    break;

                case "Factures Enregistrées":
                    if (FacturesCheckBox?.IsChecked == true && facturesEnregistrees != null)
                    {
                        var currentDateSaved = startDate;

                        while (currentDateSaved <= endDate)
                        {
                            if (currentDateSaved.Date <= DateTime.Now.Date)
                            {
                                var savedCount = facturesEnregistrees
                                    .Count(f => f.InvoiceDate.Date == currentDateSaved.Date);

                                data.Add(new GraphDataPoint { Date = currentDateSaved, Value = savedCount });
                            }

                            currentDateSaved = currentDateSaved.AddDays(1);
                        }
                    }
                    break;
            }

            System.Diagnostics.Debug.WriteLine($"  Generated {data.Count} data points");
            return data;
        }

        private void DrawLineGraph(List<GraphDataPoint> data)
        {
            if (data.Count == 0) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double margin = 60;

            var timeSeriesData = data.Where(d => d.Date != DateTime.MinValue).OrderBy(d => d.Date).ToList();
            if (timeSeriesData.Count == 0) return;

            double maxValue = timeSeriesData.Max(d => d.Value);
            double minValue = timeSeriesData.Min(d => d.Value);
            double valueRange = maxValue - minValue;

            if (valueRange == 0) valueRange = 1;

            double xStep = (width - 2 * margin) / Math.Max(1, (timeSeriesData.Count - 1));
            double yScale = (height - 2 * margin) / valueRange;

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                StrokeThickness = 3,
                Points = new PointCollection()
            };

            for (int i = 0; i < timeSeriesData.Count; i++)
            {
                double x = margin + i * xStep;
                double y = height - margin - ((timeSeriesData[i].Value - minValue) * yScale);
                polyline.Points.Add(new Point(x, y));

                var circle = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Color.FromRgb(102, 126, 234))
                };
                Canvas.SetLeft(circle, x - 4);
                Canvas.SetTop(circle, y - 4);
                GraphCanvas.Children.Add(circle);
            }

            GraphCanvas.Children.Add(polyline);
            DrawAxes(width, height, margin, timeSeriesData, minValue, maxValue);
        }

        private void DrawBarGraph(List<GraphDataPoint> data)
        {
            if (data.Count == 0) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double margin = 60;

            var validData = data.Where(d => d.Date != DateTime.MinValue || !string.IsNullOrEmpty(d.Label))
                               .OrderBy(d => d.Date != DateTime.MinValue ? d.Date : DateTime.MinValue)
                               .ToList();

            if (validData.Count == 0) return;

            double maxValue = validData.Max(d => d.Value);
            if (maxValue == 0) maxValue = 1;

            double barWidth = (width - 2 * margin) / (validData.Count * 1.5);
            double yScale = (height - 2 * margin) / maxValue;

            for (int i = 0; i < validData.Count; i++)
            {
                double x = margin + i * barWidth * 1.5;
                double barHeight = validData[i].Value * yScale;

                var bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(102, 126, 234))
                };

                Canvas.SetLeft(bar, x);
                Canvas.SetTop(bar, height - margin - barHeight);
                GraphCanvas.Children.Add(bar);

                var label = new TextBlock
                {
                    Text = validData[i].Value.ToString("N0"),
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(74, 85, 104))
                };
                Canvas.SetLeft(label, x + barWidth / 2 - 15);
                Canvas.SetTop(label, height - margin - barHeight - 20);
                GraphCanvas.Children.Add(label);
            }

            DrawAxes(width, height, margin, validData, 0, maxValue);
        }

        private void DrawPieChart(List<GraphDataPoint> data)
        {
            if (data.Count == 0) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double centerX = width / 2;
            double centerY = height / 2;
            double radius = Math.Min(width, height) / 3;

            double total = data.Sum(d => d.Value);
            if (total == 0) return;

            double startAngle = -90;

            Color[] colors = new Color[]
            {
                Color.FromRgb(102, 126, 234),
                Color.FromRgb(245, 101, 101),
                Color.FromRgb(72, 187, 120),
                Color.FromRgb(237, 137, 54),
                Color.FromRgb(159, 122, 234),
                Color.FromRgb(49, 151, 149)
            };

            for (int i = 0; i < data.Count; i++)
            {
                double sweepAngle = (data[i].Value / total) * 360;

                var segment = CreatePieSegment(centerX, centerY, radius, startAngle, sweepAngle, colors[i % colors.Length]);
                GraphCanvas.Children.Add(segment);

                double labelAngle = startAngle + sweepAngle / 2;
                double labelX = centerX + (radius * 0.7) * Math.Cos(labelAngle * Math.PI / 180);
                double labelY = centerY + (radius * 0.7) * Math.Sin(labelAngle * Math.PI / 180);

                var label = new TextBlock
                {
                    Text = $"{(data[i].Value / total * 100):F1}%",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(label, labelX - 20);
                Canvas.SetTop(label, labelY - 10);
                GraphCanvas.Children.Add(label);

                startAngle += sweepAngle;
            }

            double legendY = 20;
            for (int i = 0; i < data.Count; i++)
            {
                var legendRect = new Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(colors[i % colors.Length])
                };
                Canvas.SetLeft(legendRect, 20);
                Canvas.SetTop(legendRect, legendY);
                GraphCanvas.Children.Add(legendRect);

                var legendText = new TextBlock
                {
                    Text = string.IsNullOrEmpty(data[i].Label) ? data[i].Date.ToString("dd/MM") : data[i].Label,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(74, 85, 104))
                };
                Canvas.SetLeft(legendText, 50);
                Canvas.SetTop(legendText, legendY + 2);
                GraphCanvas.Children.Add(legendText);

                legendY += 30;
            }
        }
        private void DiagnoseInvoiceIssue()
        {
            System.Diagnostics.Debug.WriteLine("=== INVOICE DIAGNOSIS ===");

            if (invoices == null)
            {
                System.Diagnostics.Debug.WriteLine("ERROR: invoices is NULL");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Total invoices loaded: {invoices.Count}");

            // Show all invoices with their client info
            foreach (var inv in invoices.OrderBy(i => i.InvoiceDate))
            {
                System.Diagnostics.Debug.WriteLine($"\nInvoice: {inv.InvoiceNumber}");
                System.Diagnostics.Debug.WriteLine($"  Date: {inv.InvoiceDate:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"  ClientName: '{inv.ClientName ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"  ReferenceClient: '{inv.ReferenceClient ?? "NULL"}'");
                System.Diagnostics.Debug.WriteLine($"  Articles: {inv.Articles?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"  TotalTTC: {inv.TotalTTC}");
            }

            // Show what we're searching for
            if (selectedEntityId.HasValue)
            {
                var client = clients.FirstOrDefault(c => c.ClientID == selectedEntityId.Value);
                if (client != null)
                {
                    System.Diagnostics.Debug.WriteLine($"\n=== SEARCH CRITERIA ===");
                    System.Diagnostics.Debug.WriteLine($"Client ID: {selectedEntityId.Value}");
                    System.Diagnostics.Debug.WriteLine($"Client Name: '{client.Nom}'");

                    // Test matching
                    var matchingInvoices = invoices.Where(i =>
                        i.ClientName == client.Nom ||
                        i.ReferenceClient == selectedEntityId.Value.ToString()
                    ).ToList();

                    System.Diagnostics.Debug.WriteLine($"\n=== MATCHING RESULTS ===");
                    System.Diagnostics.Debug.WriteLine($"Invoices matching by ClientName: {invoices.Count(i => i.ClientName == client.Nom)}");
                    System.Diagnostics.Debug.WriteLine($"Invoices matching by ReferenceClient: {invoices.Count(i => i.ReferenceClient == selectedEntityId.Value.ToString())}");
                    System.Diagnostics.Debug.WriteLine($"Total matching: {matchingInvoices.Count}");

                    // Check for case sensitivity or whitespace issues
                    System.Diagnostics.Debug.WriteLine($"\n=== CASE INSENSITIVE CHECK ===");
                    var caseInsensitiveMatches = invoices.Count(i =>
                        i.ClientName?.Equals(client.Nom, StringComparison.OrdinalIgnoreCase) == true
                    );
                    System.Diagnostics.Debug.WriteLine($"Case-insensitive matches: {caseInsensitiveMatches}");
                }
            }
        }
        private Path CreatePieSegment(double centerX, double centerY, double radius, double startAngle, double sweepAngle, Color color)
        {
            double startRad = startAngle * Math.PI / 180;
            double endRad = (startAngle + sweepAngle) * Math.PI / 180;

            Point startPoint = new Point(
                centerX + radius * Math.Cos(startRad),
                centerY + radius * Math.Sin(startRad)
            );

            Point endPoint = new Point(
                centerX + radius * Math.Cos(endRad),
                centerY + radius * Math.Sin(endRad)
            );

            var pathFigure = new PathFigure { StartPoint = new Point(centerX, centerY) };
            pathFigure.Segments.Add(new LineSegment(startPoint, true));
            pathFigure.Segments.Add(new ArcSegment
            {
                Point = endPoint,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = sweepAngle > 180
            });
            pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);

            return new Path
            {
                Data = pathGeometry,
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
        }

        private void DrawAreaGraph(List<GraphDataPoint> data)
        {
            if (data.Count == 0) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double margin = 60;

            var timeSeriesData = data.Where(d => d.Date != DateTime.MinValue).OrderBy(d => d.Date).ToList();
            if (timeSeriesData.Count == 0) return;

            double maxValue = timeSeriesData.Max(d => d.Value);
            double minValue = timeSeriesData.Min(d => d.Value);
            double valueRange = maxValue - minValue;

            if (valueRange == 0) valueRange = 1;

            double xStep = (width - 2 * margin) / Math.Max(1, (timeSeriesData.Count - 1));
            double yScale = (height - 2 * margin) / valueRange;

            var polygon = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromArgb(100, 102, 126, 234)),
                Stroke = new SolidColorBrush(Color.FromRgb(102, 126, 234)),
                StrokeThickness = 2,
                Points = new PointCollection()
            };

            polygon.Points.Add(new Point(margin, height - margin));

            for (int i = 0; i < timeSeriesData.Count; i++)
            {
                double x = margin + i * xStep;
                double y = height - margin - ((timeSeriesData[i].Value - minValue) * yScale);
                polygon.Points.Add(new Point(x, y));
            }

            polygon.Points.Add(new Point(margin + (timeSeriesData.Count - 1) * xStep, height - margin));

            GraphCanvas.Children.Add(polygon);
            DrawAxes(width, height, margin, timeSeriesData, minValue, maxValue);
        }

        private void DrawAxes(double width, double height, double margin, List<GraphDataPoint> data, double minValue, double maxValue)
        {
            var xAxis = new Line
            {
                X1 = margin,
                Y1 = height - margin,
                X2 = width - margin,
                Y2 = height - margin,
                Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(xAxis);

            var yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = height - margin,
                Stroke = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                StrokeThickness = 2
            };
            GraphCanvas.Children.Add(yAxis);

            int labelCount = Math.Min(data.Count, 10);
            int step = Math.Max(1, data.Count / labelCount);

            for (int i = 0; i < data.Count; i += step)
            {
                double x = margin + i * ((width - 2 * margin) / Math.Max(1, (data.Count - 1)));
                string labelText = string.IsNullOrEmpty(data[i].Label)
                    ? data[i].Date.ToString("dd/MM")
                    : data[i].Label;

                var label = new TextBlock
                {
                    Text = labelText,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(113, 128, 150))
                };
                Canvas.SetLeft(label, x - 20);
                Canvas.SetTop(label, height - margin + 10);
                GraphCanvas.Children.Add(label);
            }

            int yLabelCount = 5;
            double valueRange = maxValue - minValue;

            if (valueRange == 0)
            {
                valueRange = Math.Max(1, Math.Abs(maxValue * 0.1));
            }

            for (int i = 0; i <= yLabelCount; i++)
            {
                double value = minValue + (valueRange * i / yLabelCount);
                double y = height - margin - ((value - minValue) / valueRange) * (height - 2 * margin);

                if (double.IsNaN(y) || double.IsInfinity(y))
                    continue;

                var label = new TextBlock
                {
                    Text = value.ToString("N0"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(113, 128, 150))
                };
                Canvas.SetLeft(label, margin - 45);
                Canvas.SetTop(label, y - 8);
                GraphCanvas.Children.Add(label);

                var gridLine = new Line
                {
                    X1 = margin,
                    Y1 = y,
                    X2 = width - margin,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromRgb(237, 242, 247)),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 5, 5 }
                };
                GraphCanvas.Children.Add(gridLine);
            }
        }

        private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (GraphCanvas.Children.Count > 0 && EmptyState.Visibility == Visibility.Collapsed)
            {
                LoadGraph();
            }
        }
    }

    public class GraphDataPoint
    {
        public DateTime Date { get; set; } = DateTime.MinValue;
        public double Value { get; set; }
        public string Label { get; set; }
    }

    public class SearchResult
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }

}