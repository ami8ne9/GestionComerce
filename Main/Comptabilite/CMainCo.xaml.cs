using GestionComerce;
using Superete.Main.Comptabilite.Graphes;
using GestionComerce.Main.ProjectManagment;
using System;
using System.Collections.Generic;
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

namespace Superete.Main.Comptabilite
{
    /// <summary>
    /// Interaction logic for CMainCo.xaml
    /// </summary>
    public partial class CMainCo : UserControl
    {
        public CMainCo(User u, MainWindow main)
        {
            InitializeComponent();
            this.u = u;
            this.main = main;

            // Set initial selection
            SetSelectedButtonStyle(GraphesButton);
            LoadGraphes();
        }

        User u;
        MainWindow main;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            main.load_main(u);
        }

        private void Graphes_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            SetSelectedButtonStyle((Button)sender);
            LoadGraphes();
        }

        private void RapportStats_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            SetSelectedButtonStyle((Button)sender);
            LoadRapportStats();
        }

        private void LoadGraphes()
        {
            MainContentArea.Content = new CGraphe(u, main);
        }

        private void LoadRapportStats()
        {
            MainContentArea.Content = new CMainR(u, main);
        }

        private void ResetButtonStyles()
        {
            // Reset all menu buttons to default transparent style
            ResetButton(GraphesButton);
            ResetButton(SalaireButton);
            ResetButton(RapportButton);
            ResetButton(ExpencesButton);
            ResetButton(ExpectationButton);
            // AI button keeps its gradient style
        }

        private void ResetButton(Button button)
        {
            button.Background = Brushes.Transparent;
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748"));
            button.FontWeight = FontWeights.Medium;
        }

        private void SetSelectedButtonStyle(Button button)
        {
            // Don't change AI button style
            if (button == AIButton) return;

            // Simple, clean selected style - light gray background with darker text
            button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
            button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            button.FontWeight = FontWeights.SemiBold;
        }
    }
}