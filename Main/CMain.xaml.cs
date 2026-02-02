
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

namespace GestionComerce.Main
{
    /// <summary>
    /// Logique d'interaction pour CMain.xaml
    /// </summary>
    public partial class CMain : UserControl
    {
        // ========================================
        // ADD THIS CODE TO YOUR CMain.xaml.cs CLASS
        // ========================================

        // In the constructor, add permission check for Livraison button
        public CMain(MainWindow main, User u)
        {
            InitializeComponent();
            this.main = main;
            this.u = u;
            Name.Text = u.UserName;

            foreach (Role r in main.lr)
            {
                if (r.RoleID == u.RoleID)
                {
                    if (r.ViewSettings == false)
                    {
                        SettingsBtn.IsEnabled = false;
                    }
                    if (r.ViewProjectManagment == false)
                    {
                        ProjectManagmentBtn.IsEnabled = false;
                    }
                    if (r.ViewVente == false)
                    {
                        VenteBtn.IsEnabled = false;
                    }
                    if (r.ViewInventrory == false)
                    {
                        InventoryBtn.IsEnabled = false;
                    }
                    if (r.ViewClientsPage == false)
                    {
                        ClientBtn.IsEnabled = false;
                    }
                    if (r.ViewFournisseurPage == false)
                    {
                        FournisseurBtn.IsEnabled = false;
                    }
                    // NEW: Add permission check for Livraison
                  
                }
            }
        }
        public MainWindow main; public User u;

        // NEW: Add this method to handle Livraison button click
        private void LivraisonBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_livraison(u);
        }
        


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            main.load_settings(u);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Article a = new Article();
            List<Article> la = await a.GetArticlesAsync();
            main.load_vente(u,la);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            main.load_inventory(u);
        }

        //private void Button_Click_3(object sender, RoutedEventArgs e)
        //{
        //    main.load_fournisseur(u);
        //}

        //private void Button_Click_4(object sender, RoutedEventArgs e)
        //{
        //    main.load_client(u);
        //}

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            main.load_ProjectManagement(u);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            main.load_client(u);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            main.load_fournisseur(u);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Exit exit=new Exit(this,1);
            exit.ShowDialog();
        }

        private void FacturationBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_facturation(u);
        }

        private void AccountabilityBtn_Click(object sender, RoutedEventArgs e)
        {
            main.load_accountibility(u);
        }
    }
}
