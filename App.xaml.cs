using Superete;
using System;
using System.Windows;
using System.Windows.Input;

namespace GestionComerce
{
    public partial class App : Application
    {
        private GlobalButtonWindow _globalButton;
        private int _currentUserId;
        private string _keyboardSetting = "Manuel";
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // CHECK 1: Handle registration command from installer
            if (e.Args.Length > 0 && e.Args[0] == "/register")
            {
                try
                {
                    MachineLock.RegisterInstallation();
                    // Silent success - installer will continue
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Registration failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Current.Shutdown();
                return;
            }

            // CHECK 2: Handle database setup command from installer
            if (e.Args.Length > 0 && e.Args[0] == "/setupdb")
            {
                try
                {
                    DatabaseSetup.EnsureDatabaseExists();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database setup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Current.Shutdown();
                return;
            }

            // CHECK 3: Ensure database exists before starting app
            if (!DatabaseSetup.EnsureDatabaseExists())
            {
                MessageBox.Show("Cannot start application without database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            // CHECK 4: Normal startup - create main window
            MainWindow main = new MainWindow();

            // If MainWindow constructor failed (machine lock or expiry), don't continue
            if (main == null || Current.MainWindow == null)
            {
                return;
            }

            main.Show();

            // Create global button window
            // Create global button window and store reference
            _globalButton = new GlobalButtonWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Topmost = true,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = System.Windows.Media.Brushes.Transparent
            };

            PositionBottomRight(main, _globalButton);
            _globalButton.Show();

            main.LocationChanged += (s, ev) => PositionBottomRight(main, _globalButton);
            main.SizeChanged += (s, ev) => PositionBottomRight(main, _globalButton);

            // Handle minimize / restore safely
            main.StateChanged += (s, ev) =>
            {
                if (!_globalButton.IsLoaded) return; // prevent access after close
                if (main.WindowState == WindowState.Minimized)
                    _globalButton.Hide();
                else
                    _globalButton.Show();
            };

            // Close the floating window when the main window closes
            main.Closed += (s, ev) =>
            {
                if (_globalButton.IsLoaded)
                {
                    _globalButton.Close();
                }
            };
        }
        public void SetUserForKeyboard(int userId)
        {
            _currentUserId = userId;

            // Load keyboard setting
            try
            {
                var parametres = Superete.ParametresGeneraux.ObtenirParametresParUserId(userId, "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;");
                if (parametres != null)
                {
                    _keyboardSetting = parametres.AfficherClavier;
                }
            }
            catch { }

            // Update global button visibility
            if (_globalButton != null)
            {
                _globalButton.SetUser(userId);
            }

            // Register global focus event if "Oui"
            if (_keyboardSetting == "Oui")
            {
                EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                    System.Windows.Controls.TextBox.GotFocusEvent,
                    new RoutedEventHandler(OnTextBoxGotFocus));

                EventManager.RegisterClassHandler(typeof(System.Windows.Controls.PasswordBox),
                    System.Windows.Controls.PasswordBox.GotFocusEvent,
                    new RoutedEventHandler(OnTextBoxGotFocus));

                // ADD THESE: Register MouseDown to detect clicks even when already focused
                EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                    System.Windows.Controls.TextBox.PreviewMouseDownEvent,
                    new MouseButtonEventHandler(OnTextBoxMouseDown));

                EventManager.RegisterClassHandler(typeof(System.Windows.Controls.PasswordBox),
                    System.Windows.Controls.PasswordBox.PreviewMouseDownEvent,
                    new MouseButtonEventHandler(OnTextBoxMouseDown));
            }
        }

        private void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (_keyboardSetting == "Oui")
            {
                WKeyboard.ShowKeyboard(_currentUserId);
            }
        }

        // ADD THIS METHOD:
        private void OnTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_keyboardSetting == "Oui")
            {
                WKeyboard.ShowKeyboard(_currentUserId);
            }
        }
        private void PositionBottomRight(Window main, Window _globalButton)
        {
            // Get usable screen area (excludes taskbar)
            var workingArea = SystemParameters.WorkArea;
            if (main.WindowState == WindowState.Maximized)
            {
                // When maximized, use the working area coordinates instead of main.Left/Top
                _globalButton.Left = workingArea.Right - _globalButton.Width - 10;
                _globalButton.Top = workingArea.Bottom - _globalButton.Height - 10;
            }
            else
            {
                // Normal state – follow the main window's corner
                _globalButton.Left = main.Left + main.Width - _globalButton.Width - 10;
                _globalButton.Top = main.Top + main.Height - _globalButton.Height - 10;
            }
        }
    }
}