using GestionComerce;
using GestionComerce.Main.Facturation.CreateFacture;
using GestionComerce.Main.Facturation.FacturesEnregistrees;
using GestionComerce.Main.Facturation.HistoriqueFacture;
using GestionComerce.Main.Facturation.VerifierHistorique;
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

namespace GestionComerce.Main.Facturation
{
    /// <summary>
    /// Interaction logic for CMainI.xaml
    /// </summary>
    public partial class CMainIn : UserControl
    {
        MainWindow main;
        User user;
        Operation operation;

        public CMainIn(User u, MainWindow main, Operation op)
        {
            InitializeComponent();
            this.main = main;
            this.user = u;
            this.operation = op;
            ContentContainer.Children.Clear();
            CMainFa loginPage = new CMainFa(u, main, this, null);
            loginPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            loginPage.VerticalAlignment = VerticalAlignment.Stretch;
            loginPage.Margin = new Thickness(0);
            ContentContainer.Children.Add(loginPage);
        }

        private void CreeFacture_Click(object sender, RoutedEventArgs e)
        {
            // Reset all button styles
            ResetButtonStyles();

            // Set active style for this button
            CreeFacture.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            CreeFacture.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));

            ContentContainer.Children.Clear();
            CMainFa loginPage = new CMainFa(user, main, this, null);
            Grid.SetRow(loginPage, 0);
            Grid.SetColumn(loginPage, 0);
            loginPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            loginPage.VerticalAlignment = VerticalAlignment.Stretch;
            ContentContainer.Children.Add(loginPage);
        }

        private void HistoriqueFacture_Click(object sender, RoutedEventArgs e)
        {
            // Reset all button styles
            ResetButtonStyles();

            // Set active style for this button
            HistoriqueFacture.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            HistoriqueFacture.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));

            ContentContainer.Children.Clear();
            CMainHf loginPage = new CMainHf(user, main);
            loginPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            loginPage.VerticalAlignment = VerticalAlignment.Stretch;
            loginPage.Margin = new Thickness(0);
            ContentContainer.Children.Add(loginPage);
        }

        private void VerifierHistorique_Click(object sender, RoutedEventArgs e)
        {
            // Reset all button styles
            ResetButtonStyles();

            // Set active style for this button
            VerifierHistorique.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            VerifierHistorique.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));

            ContentContainer.Children.Clear();
            // TODO: Add your UserControl here when ready
            CMainVerifier verifierPage = new CMainVerifier(user, main);
            verifierPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            verifierPage.VerticalAlignment = VerticalAlignment.Stretch;
            verifierPage.Margin = new Thickness(0);
            ContentContainer.Children.Add(verifierPage);
        }

        private void FacturesEnregistrees_Click(object sender, RoutedEventArgs e)
        {
            // Reset all button styles
            ResetButtonStyles();

            // Set active style for this button
            FacturesEnregistrees.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
            FacturesEnregistrees.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));

            ContentContainer.Children.Clear();
            // TODO: Add your UserControl here when ready
            CMainEnregistrees enregistreesPage = new CMainEnregistrees(user, main);
            enregistreesPage.HorizontalAlignment = HorizontalAlignment.Stretch;
            enregistreesPage.VerticalAlignment = VerticalAlignment.Stretch;
            enregistreesPage.Margin = new Thickness(0);
            ContentContainer.Children.Add(enregistreesPage);
        }

        private void RetourButton_Click(object sender, RoutedEventArgs e)
        {
            main.load_main(user);
        }

        // Helper method to reset all button styles to inactive state
        private void ResetButtonStyles()
        {
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
            var transparentBrush = new SolidColorBrush(Colors.Transparent);

            CreeFacture.Foreground = inactiveColor;
            CreeFacture.BorderBrush = transparentBrush;

            HistoriqueFacture.Foreground = inactiveColor;
            HistoriqueFacture.BorderBrush = transparentBrush;

            VerifierHistorique.Foreground = inactiveColor;
            VerifierHistorique.BorderBrush = transparentBrush;

            FacturesEnregistrees.Foreground = inactiveColor;
            FacturesEnregistrees.BorderBrush = transparentBrush;
        }
    }
}