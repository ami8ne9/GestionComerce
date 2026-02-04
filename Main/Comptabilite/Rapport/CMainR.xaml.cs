using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestionComerce.Main.ProjectManagment
{
    /// <summary>
    /// Interaction logic for CMainR.xaml
    /// </summary>
    public partial class CMainR : UserControl
    {
        bool isInitialized = false;

        public CMainR(User u, MainWindow main)
        {
            InitializeComponent();

            this.Loaded += (s, e) => isInitialized = true;
            this.u = u;
            this.main = main;

            // Initialize indices
            index3 = 10;
            index4 = 10;

            // Initialize lists
            LOperation = new List<Operation>();
            LOperationArticle = new List<OperationArticle>();

            // Set default button selection
            selectedbtn = "day";
            SetSelectedButton(DayButton);
            ShowView(DayView);
            PopulateYearComboBox(MonthYearComboBox);
            PopulateYearComboBox(YearComboBox);

            // Reset statistics
            ResetStatistics();
        }

        public User u;
        public MainWindow main;
        public int index3 = 10;
        public int index4 = 10;
        public string selectedbtn = "day";
        List<Operation> LOperation = new List<Operation>();
        List<OperationArticle> LOperationArticle = new List<OperationArticle>();
        private DateTime? _previousStartDate;
        private DateTime? _previousEndDate;

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            main.load_main(u);
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
            RevenueOperationsContainer.Children.Clear();
            RevenueArticlesContainer.Children.Clear();
            LOperation.Clear();
            LOperationArticle.Clear();
            SeeMoreContainer2.Visibility = Visibility.Collapsed;
            SeeMoreContainer3.Visibility = Visibility.Collapsed;

            BoughtText.Text = "0.00 DH";
            RevenueText.Text = "0.00 DH";
            SoldText.Text = "0.00 DH";
            DifferenceText.Text = "0.00 DH";
            ArticlesSoldText.Text = "0";
            ArticlesBoughtText.Text = "0";
            SoldOpsText.Text = "0";
            BoughtOpsText.Text = "0";
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

        public void LoadStatistics()
        {
            LOperation.Clear();
            LOperationArticle.Clear();
            RevenueOperationsContainer.Children.Clear();
            RevenueArticlesContainer.Children.Clear();
            SeeMoreContainer2.Visibility = Visibility.Collapsed;
            SeeMoreContainer3.Visibility = Visibility.Collapsed;

            Decimal revenue = 0;
            Decimal achete = 0;
            Decimal vendus = 0;
            Decimal reverse = 0;
            int articleVendus = 0;
            int articleAchete = 0;
            int OperationVente = 0;
            int OperationAchete = 0;
            int OperationNbr = 0;
            int MouvmentNbr = 0;

            if (selectedbtn == "day")
            {
                if (DayDatePicker.SelectedDate.HasValue)
                {
                    DateTime selectedDate = DayDatePicker.SelectedDate.Value.Date;

                    foreach (Operation o in main.lo)
                    {
                        if (o.DateOperation.Date == selectedDate.Date)
                        {
                            OperationNbr++;
                            LOperation.Add(o);

                            if (o.Reversed)
                            {
                                reverse += o.PrixOperation;
                            }
                            else
                            {
                                if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationVente++;
                                    vendus += o.PrixOperation;
                                }
                                else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationAchete++;
                                    achete += o.PrixOperation;
                                }
                            }

                            foreach (OperationArticle oa in main.loa.Where(x => x.OperationID == o.OperationID))
                            {
                                MouvmentNbr++;
                                LOperationArticle.Add(oa);

                                Article article = main.la.FirstOrDefault(a => a.ArticleID == oa.ArticleID);
                                if (article != null && !oa.Reversed)
                                {
                                    if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleVendus += (int)oa.QteArticle;
                                    }
                                    else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleAchete += (int)oa.QteArticle;
                                    }
                                }
                            }
                        }
                    }
                    revenue = vendus - achete;
                }
            }
            else if (selectedbtn == "month")
            {
                if (MonthComboBox.SelectedItem != null && MonthYearComboBox.SelectedItem != null)
                {
                    int selectedMonth = MonthComboBox.SelectedIndex + 1;
                    int selectedYear = int.Parse(MonthYearComboBox.SelectedItem.ToString());

                    foreach (Operation o in main.lo)
                    {
                        if (o.DateOperation.Month == selectedMonth && o.DateOperation.Year == selectedYear)
                        {
                            OperationNbr++;
                            LOperation.Add(o);

                            if (o.Reversed)
                            {
                                reverse += o.PrixOperation;
                            }
                            else
                            {
                                if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationVente++;
                                    vendus += o.PrixOperation;
                                }
                                else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationAchete++;
                                    achete += o.PrixOperation;
                                }
                            }

                            foreach (OperationArticle oa in main.loa.Where(x => x.OperationID == o.OperationID))
                            {
                                MouvmentNbr++;
                                LOperationArticle.Add(oa);

                                Article article = main.la.FirstOrDefault(a => a.ArticleID == oa.ArticleID);
                                if (article != null && !oa.Reversed)
                                {
                                    if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleVendus += (int)oa.QteArticle;
                                    }
                                    else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleAchete += (int)oa.QteArticle;
                                    }
                                }
                            }
                        }
                    }
                    revenue = vendus - achete;
                }
            }
            else if (selectedbtn == "year")
            {
                if (YearComboBox.SelectedItem != null)
                {
                    int selectedYear = int.Parse(YearComboBox.SelectedItem.ToString());

                    foreach (Operation o in main.lo)
                    {
                        if (o.DateOperation.Year == selectedYear)
                        {
                            OperationNbr++;
                            LOperation.Add(o);

                            if (o.Reversed)
                            {
                                reverse += o.PrixOperation;
                            }
                            else
                            {
                                if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationVente++;
                                    vendus += o.PrixOperation;
                                }
                                else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationAchete++;
                                    achete += o.PrixOperation;
                                }
                            }

                            foreach (OperationArticle oa in main.loa.Where(x => x.OperationID == o.OperationID))
                            {
                                MouvmentNbr++;
                                LOperationArticle.Add(oa);

                                Article article = main.la.FirstOrDefault(a => a.ArticleID == oa.ArticleID);
                                if (article != null && !oa.Reversed)
                                {
                                    if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleVendus += (int)oa.QteArticle;
                                    }
                                    else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleAchete += (int)oa.QteArticle;
                                    }
                                }
                            }
                        }
                    }
                    revenue = vendus - achete;
                }
            }
            else if (selectedbtn == "personalized")
            {
                if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                {
                    DateTime selectedDate = StartDatePicker.SelectedDate.Value.Date;
                    DateTime selectedDate1 = EndDatePicker.SelectedDate.Value.Date;

                    foreach (Operation o in main.lo)
                    {
                        if (o.DateOperation.Date >= selectedDate && o.DateOperation.Date <= selectedDate1)
                        {
                            OperationNbr++;
                            LOperation.Add(o);

                            if (o.Reversed)
                            {
                                reverse += o.PrixOperation;
                            }
                            else
                            {
                                if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationVente++;
                                    vendus += o.PrixOperation;
                                }
                                else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                {
                                    OperationAchete++;
                                    achete += o.PrixOperation;
                                }
                            }

                            foreach (OperationArticle oa in main.loa.Where(x => x.OperationID == o.OperationID))
                            {
                                MouvmentNbr++;
                                LOperationArticle.Add(oa);

                                Article article = main.la.FirstOrDefault(a => a.ArticleID == oa.ArticleID);
                                if (article != null && !oa.Reversed)
                                {
                                    if (o.OperationType.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleVendus += (int)oa.QteArticle;
                                    }
                                    else if (o.OperationType.StartsWith("A", StringComparison.OrdinalIgnoreCase))
                                    {
                                        articleAchete += (int)oa.QteArticle;
                                    }
                                }
                            }
                        }
                    }
                    revenue = vendus - achete;
                }
            }

            LoadOpeerationsMouvment(LOperation);
            LoadOpeerationsArticleMouvment(LOperationArticle);

            if (OperationNbr > 10)
            {
                SeeMoreContainer2.Visibility = Visibility.Visible;
            }
            if (MouvmentNbr > 10)
            {
                SeeMoreContainer3.Visibility = Visibility.Visible;
            }

            BoughtText.Text = achete.ToString("0.00") + " DH";
            RevenueText.Text = revenue.ToString("0.00") + " DH";
            SoldText.Text = vendus.ToString("0.00") + " DH";
            DifferenceText.Text = reverse.ToString("0.00") + " DH";
            ArticlesSoldText.Text = articleVendus.ToString();
            ArticlesBoughtText.Text = articleAchete.ToString();
            SoldOpsText.Text = OperationVente.ToString();
            BoughtOpsText.Text = OperationAchete.ToString();
        }
    }
}