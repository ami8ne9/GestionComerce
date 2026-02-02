using GestionComerce;
using Superete.Main.Comptabilite.Graphes;
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
        public CMainCo(User u,MainWindow main)
        {
            InitializeComponent();
            this.u = u;
            this.main = main;
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
            LoadGraphes();
        }
        private void LoadGraphes()
        {
            MainContentArea.Content = new CGraphe(u, main);
        }
    }
}
