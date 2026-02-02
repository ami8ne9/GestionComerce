using GestionComerce;
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

namespace Superete.Main.Comptabilite.Graphes
{
    /// <summary>
    /// Interaction logic for CGraphe.xaml
    /// </summary>
    public partial class CGraphe : UserControl
    {
        public CGraphe(User u,MainWindow main)
        {
            InitializeComponent();
        }

        private void GraphTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
