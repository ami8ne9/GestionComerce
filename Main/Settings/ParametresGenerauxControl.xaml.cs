using GestionComerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Superete.Main.Settings
{
    public partial class ParametresGenerauxControl : UserControl
    {
        private string _connectionString;
        private int _currentUserId;
        private ParametresGeneraux _parametresActuels;
        private List<PaymentMethod> _paymentMethods;

        public ParametresGenerauxControl()
        {
            InitializeComponent();
        }

        public ParametresGenerauxControl(int userId, string connectionString) : this()
        {
            _currentUserId = userId;
            _connectionString = connectionString;
            Loaded += ParametresGenerauxControl_Loaded;

            // Ajouter un gestionnaire pour le changement de vue
            CbVueParDefaut.SelectionChanged += CbVueParDefaut_SelectionChanged;
        }

        private async void ParametresGenerauxControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ChargerMethodesPaiement();
            ChargerParametres();
        }

        private async Task ChargerMethodesPaiement()
        {
            try
            {
                PaymentMethod pm = new PaymentMethod();
                _paymentMethods = await pm.GetPaymentMethodsAsync();

                CbMethodePaiementParDefaut.Items.Clear();

                foreach (var method in _paymentMethods)
                {
                    ComboBoxItem item = new ComboBoxItem
                    {
                        Content = method.PaymentMethodName,
                        Tag = method.PaymentMethodID
                    };
                    CbMethodePaiementParDefaut.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des méthodes de paiement : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChargerParametres()
        {
            try
            {
                _parametresActuels = ParametresGeneraux.ObtenirOuCreerParametres(_currentUserId, _connectionString);

                // SI LES PARAMETRES VIENNENT D'ÊTRE CRÉÉS, FORCER LES BONNES VALEURS PAR DÉFAUT
                if (_parametresActuels != null)
                {
                    bool needsUpdate = false;

                    // Forcer VueParDefaut à "Row" seulement si vide ou valeur invalide
                    if (string.IsNullOrEmpty(_parametresActuels.VueParDefaut) ||
                        (_parametresActuels.VueParDefaut != "Row" && _parametresActuels.VueParDefaut != "Cartes"))
                    {
                        _parametresActuels.VueParDefaut = "Row";
                        needsUpdate = true;
                    }

                    // Forcer TrierParDefaut à "Plus récent au plus ancien" SEULEMENT si vide
                    if (string.IsNullOrEmpty(_parametresActuels.TrierParDefaut))
                    {
                        _parametresActuels.TrierParDefaut = "Plus récent au plus ancien";
                        needsUpdate = true;
                    }

                    // Si on a modifié quelque chose, sauvegarder
                    if (needsUpdate)
                    {
                        _parametresActuels.MettreAJourParametres(_connectionString);
                    }
                }

                RemplirInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des paramètres : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemplirInterface()
        {
            if (_parametresActuels == null) return;

            // Affichage clavier
            switch (_parametresActuels.AfficherClavier)
            {
                case "Oui":
                    CbAfficherClavier.SelectedIndex = 0;
                    break;
                case "Non":
                    CbAfficherClavier.SelectedIndex = 1;
                    break;
                case "Manuel":
                default:
                    CbAfficherClavier.SelectedIndex = 2;
                    break;
            }

            switch (_parametresActuels.TailleIcones)
            {
                case "Grandes":
                    CbTailleIcones.SelectedIndex = 0;
                    break;
                case "Moyennes":
                    CbTailleIcones.SelectedIndex = 1;
                    break;
                case "Petites":
                    CbTailleIcones.SelectedIndex = 2;
                    break;
                default:
                    CbTailleIcones.SelectedIndex = 1; // Moyennes par défaut
                    break;
            }

            // Checkboxes
            ChkMasquerEtiquettesVides.IsChecked = _parametresActuels.MasquerEtiquettesVides;
            ChkSupprimerArticlesQuantiteZero.IsChecked = _parametresActuels.SupprimerArticlesQuantiteZero;
            ChkImprimerFactureParDefaut.IsChecked = _parametresActuels.ImprimerFactureParDefaut;
            ChkImprimerTicketParDefaut.IsChecked = _parametresActuels.ImprimerTicketParDefaut;

            // Vue par défaut - SET THIS FIRST, then update visibility
            string vueParDefaut = string.IsNullOrEmpty(_parametresActuels.VueParDefaut) ? "Row" : _parametresActuels.VueParDefaut;
            CbVueParDefaut.SelectedIndex = vueParDefaut == "Cartes" ? 0 : 1;

            // NOW update visibility based on the actual value
            TailleIconesBorder.Visibility = (vueParDefaut == "Cartes") ? Visibility.Visible : Visibility.Collapsed;

            // Tri par défaut
            switch (_parametresActuels.TrierParDefaut)
            {
                case "Nom (A-Z)":
                    CbTrierParDefaut.SelectedIndex = 0;
                    break;
                case "Nom (Z-A)":
                    CbTrierParDefaut.SelectedIndex = 1;
                    break;
                case "Prix croissant":
                    CbTrierParDefaut.SelectedIndex = 2;
                    break;
                case "Prix décroissant":
                    CbTrierParDefaut.SelectedIndex = 3;
                    break;
                case "Quantité croissante":
                    CbTrierParDefaut.SelectedIndex = 4;
                    break;
                case "Quantité décroissante":
                    CbTrierParDefaut.SelectedIndex = 5;
                    break;
                case "Plus récent au plus ancien":
                    CbTrierParDefaut.SelectedIndex = 6;
                    break;
                case "Plus ancien au plus récent":
                    CbTrierParDefaut.SelectedIndex = 7;
                    break;
                default:
                    CbTrierParDefaut.SelectedIndex = 0;
                    break;
            }

            // Méthode de paiement - Sélectionner par nom
            if (!string.IsNullOrEmpty(_parametresActuels.MethodePaiementParDefaut))
            {
                for (int i = 0; i < CbMethodePaiementParDefaut.Items.Count; i++)
                {
                    ComboBoxItem item = CbMethodePaiementParDefaut.Items[i] as ComboBoxItem;
                    if (item != null && item.Content.ToString() == _parametresActuels.MethodePaiementParDefaut)
                    {
                        CbMethodePaiementParDefaut.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        // Gestionnaire pour le changement de vue
        private void CbVueParDefaut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TailleIconesBorder == null) return;

            // Si "Cartes" est sélectionné (index 0), afficher
            // Si "Row" est sélectionné (index 1), masquer
            TailleIconesBorder.Visibility = (CbVueParDefaut.SelectedIndex == 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer les valeurs de l'interface
                string afficherClavier = CbAfficherClavier.SelectedIndex == 0 ? "Oui" :
                                        CbAfficherClavier.SelectedIndex == 1 ? "Non" : "Manuel";

                // Vue par défaut
                string vueParDefaut = CbVueParDefaut.SelectedIndex == 0 ? "Cartes" : "Row";

                // Tri par défaut
                string trierParDefaut = "";
                switch (CbTrierParDefaut.SelectedIndex)
                {
                    case 0: trierParDefaut = "Nom (A-Z)"; break;
                    case 1: trierParDefaut = "Nom (Z-A)"; break;
                    case 2: trierParDefaut = "Prix croissant"; break;
                    case 3: trierParDefaut = "Prix décroissant"; break;
                    case 4: trierParDefaut = "Quantité croissante"; break;
                    case 5: trierParDefaut = "Quantité décroissante"; break;
                    case 6: trierParDefaut = "Plus récent au plus ancien"; break;
                    case 7: trierParDefaut = "Plus ancien au plus récent"; break;
                    default: trierParDefaut = "Nom (A-Z)"; break;
                }

                // Récupérer le nom de la méthode de paiement depuis le ComboBox
                string methodePaiement = "";
                if (CbMethodePaiementParDefaut.SelectedItem is ComboBoxItem selectedItem)
                {
                    methodePaiement = selectedItem.Content.ToString();
                }

                string tailleIcones = "";
                switch (CbTailleIcones.SelectedIndex)
                {
                    case 0: tailleIcones = "Grandes"; break;
                    case 1: tailleIcones = "Moyennes"; break;
                    case 2: tailleIcones = "Petites"; break;
                    default: tailleIcones = "Moyennes"; break;
                }

                // Mettre à jour l'objet
                _parametresActuels.AfficherClavier = afficherClavier;
                _parametresActuels.MasquerEtiquettesVides = ChkMasquerEtiquettesVides.IsChecked ?? false;
                _parametresActuels.SupprimerArticlesQuantiteZero = ChkSupprimerArticlesQuantiteZero.IsChecked ?? false;
                _parametresActuels.ImprimerFactureParDefaut = ChkImprimerFactureParDefaut.IsChecked ?? false;
                _parametresActuels.ImprimerTicketParDefaut = ChkImprimerTicketParDefaut.IsChecked ?? false;
                _parametresActuels.MethodePaiementParDefaut = methodePaiement;
                _parametresActuels.VueParDefaut = vueParDefaut;
                _parametresActuels.TrierParDefaut = trierParDefaut;
                _parametresActuels.TailleIcones = tailleIcones;

                // Sauvegarder dans la BD
                bool success = _parametresActuels.MettreAJourParametres(_connectionString);

                if (success)
                {
                    MessageBox.Show("Paramètres enregistrés avec succès !",
                        "Succès", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Recharger pour mettre à jour
                    ChargerParametres();
                }
                else
                {
                    MessageBox.Show("Erreur lors de l'enregistrement des paramètres.",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'enregistrement des paramètres : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Recharger les paramètres pour annuler les modifications
            ChargerParametres();
        }

        // Méthode publique pour obtenir les paramètres actuels
        public ParametresGeneraux ObtenirParametres()
        {
            return _parametresActuels;
        }

        // Méthode pour obtenir l'ID de la méthode de paiement sélectionnée (optionnel)
        public int? ObtenirPaymentMethodIdSelectionne()
        {
            if (CbMethodePaiementParDefaut.SelectedItem is ComboBoxItem selectedItem)
            {
                return (int)selectedItem.Tag;
            }
            return null;
        }
    }
}