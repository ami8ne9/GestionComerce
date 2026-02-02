using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Threading;

namespace GestionComerce
{
    public partial class WKeyboard : Window
    {
        private bool isShiftPressed = false;
        private bool isCapsLockOn = false;
        private bool isDragging = false;
        private Point dragStartPoint;
        private int _currentUserId;
        private static bool _autoShowEnabled = false;

        private static WKeyboard _instance;
        private DispatcherTimer enableTimer;
        private void UpdateAutoShowBehavior()
        {
            try
            {
                var parametres = Superete.ParametresGeneraux.ObtenirParametresParUserId(_currentUserId, "Server=localhost\\SQLEXPRESS;Database=GESTIONCOMERCEP;Trusted_Connection=True;");

                if (parametres != null && parametres.AfficherClavier == "Oui")
                {
                    _autoShowEnabled = true;
                    StartListeningForFocus();
                }
                else
                {
                    _autoShowEnabled = false;
                }
            }
            catch
            {
                _autoShowEnabled = false;
            }
        }

        private void StartListeningForFocus()
        {
            // This will be triggered when text inputs get focus
            EventManager.RegisterClassHandler(typeof(System.Windows.Controls.TextBox),
                System.Windows.Controls.TextBox.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_autoShowEnabled && _instance != null)
            {
                _instance.Show();
                _instance.Activate();
            }
        }
        public static void ShowKeyboard(int userId = 0)
        {
            if (_instance == null)
            {
                _instance = new WKeyboard();
                _instance.Closed += (s, e) => _instance = null;
                _instance.Show();
            }
            else
            {
                // Don't steal focus - just make it visible if hidden
                if (!_instance.IsVisible)
                {
                    _instance.Show();
                }
                // Don't call Activate() - that steals focus
            }
        }
        public WKeyboard()
        {
            InitializeComponent();
            this.SourceInitialized += Window_SourceInitialized;
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Timer to constantly re-enable the window in case dialogs disable it
            enableTimer = new DispatcherTimer();
            enableTimer.Interval = TimeSpan.FromMilliseconds(100);
            enableTimer.Tick += (s, args) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                EnableWindow(hwnd, true);
            };
            enableTimer.Start();
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        #region Window Setup (Non-Activating)

        public GlobalButtonWindow gwd;
        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;

            IntPtr exStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
            exStyle = new IntPtr(exStyle.ToInt32() | WS_EX_NOACTIVATE | WS_EX_TOPMOST);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_MOUSEACTIVATE = 0x0021;
            const int MA_NOACTIVATE = 0x0003;
            const int WM_ENABLE = 0x000A;

            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }

            // If Windows tries to disable us (because of a modal dialog), re-enable immediately
            if (msg == WM_ENABLE)
            {
                if (wParam == IntPtr.Zero) // Being disabled
                {
                    EnableWindow(hwnd, true);
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }
        #endregion

        #region Key Click
        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                string tag = button.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) return;

                if (tag == "Shift") { ToggleShift(); return; }
                if (tag == "CapsLock") { ToggleCapsLock(); return; }

                SendCharacterToFocusedControl(tag);
            }
        }
        #endregion

        #region Shift / CapsLock
        private void ToggleShift()
        {
            isShiftPressed = !isShiftPressed;
            UpdateShiftButtonAppearance();
        }

        private void ToggleCapsLock()
        {
            isCapsLockOn = !isCapsLockOn;
            UpdateCapsLockButtonAppearance();
        }

        private void UpdateShiftButtonAppearance()
        {
            var color = isShiftPressed ?
                new SolidColorBrush(Color.FromRgb(100, 150, 255)) :
                new SolidColorBrush(Color.FromRgb(240, 240, 240));
            ShiftButton1.Background = color;
            ShiftButton2.Background = color;
        }

        private void UpdateCapsLockButtonAppearance()
        {
            CapsButton.Background = isCapsLockOn ?
                new SolidColorBrush(Color.FromRgb(100, 150, 255)) :
                new SolidColorBrush(Color.FromRgb(240, 240, 240));
        }
        #endregion

        #region Dragging
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is System.Windows.Controls.Border ||
                e.OriginalSource is System.Windows.Controls.Grid ||
                e.OriginalSource is System.Windows.Controls.TextBlock)
            {
                isDragging = true;
                dragStartPoint = e.GetPosition(this);
                this.CaptureMouse();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = PointToScreen(e.GetPosition(this));
                this.Left = currentPosition.X - dragStartPoint.X;
                this.Top = currentPosition.Y - dragStartPoint.Y;
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                this.ReleaseMouseCapture();
            }
        }
        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            enableTimer?.Stop();
            this.Close();
        }

        #region Send Key to Focused Control
        private void SendCharacterToFocusedControl(string character)
        {
            IntPtr focusedHandle = GetFocusedControlHandle();
            if (focusedHandle == IntPtr.Zero) return;

            if (character == "Backspace") { SendKeyToHandle(focusedHandle, 0x08); return; }
            if (character == "Enter") { SendKeyToHandle(focusedHandle, 0x0D); return; }
            if (character == "Tab") { SendKeyToHandle(focusedHandle, 0x09); return; }
            if (character == "Space") { SendUnicodeCharToHandle(focusedHandle, ' '); return; }

            char ch = character[0];
            bool shouldBeUppercase = isShiftPressed ^ isCapsLockOn;

            if (char.IsLetter(ch))
                ch = shouldBeUppercase ? char.ToUpper(ch) : char.ToLower(ch);
            else if (isShiftPressed)
                ch = GetShiftedCharacter(ch);

            SendUnicodeCharToHandle(focusedHandle, ch);

            if (isShiftPressed)
            {
                isShiftPressed = false;
                UpdateShiftButtonAppearance();
            }
        }

        private char GetShiftedCharacter(char ch)
        {
            switch (ch)
            {
                case '`': return '~';
                case '1': return '!';
                case '2': return '@';
                case '3': return '#';
                case '4': return '$';
                case '5': return '%';
                case '6': return '^';
                case '7': return '&';
                case '8': return '*';
                case '9': return '(';
                case '0': return ')';
                case '-': return '_';
                case '=': return '+';
                case '[': return '{';
                case ']': return '}';
                case '\\': return '|';
                case ';': return ':';
                case '\'': return '"';
                case ',': return '<';
                case '.': return '>';
                case '/': return '?';
                default: return ch;
            }
        }
        #endregion

        #region P/Invoke for Focused Control
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern IntPtr GetFocus();
        [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("user32.dll")] private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_CHAR = 0x0102;

        private IntPtr GetFocusedControlHandle()
        {
            IntPtr fgWindow = GetForegroundWindow();
            if (fgWindow == IntPtr.Zero) return IntPtr.Zero;

            GetWindowThreadProcessId(fgWindow, out uint fgThreadId);
            uint currentThreadId = GetCurrentThreadId();

            IntPtr focusedHandle = IntPtr.Zero;

            if (fgThreadId != currentThreadId)
            {
                AttachThreadInput(currentThreadId, fgThreadId, true);
                focusedHandle = GetFocus();
                AttachThreadInput(currentThreadId, fgThreadId, false);
            }
            else
            {
                focusedHandle = GetFocus();
            }

            return focusedHandle;
        }

        private void SendUnicodeCharToHandle(IntPtr hWnd, char ch)
        {
            SendMessage(hWnd, WM_CHAR, (IntPtr)ch, IntPtr.Zero);
        }

        private void SendKeyToHandle(IntPtr hWnd, ushort vkCode)
        {
            SendMessage(hWnd, 0x0100, (IntPtr)vkCode, IntPtr.Zero); // WM_KEYDOWN
            SendMessage(hWnd, 0x0101, (IntPtr)vkCode, IntPtr.Zero); // WM_KEYUP
        }
        #endregion

        #region Non-Activating Window P/Invoke
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOPMOST = 0x00000008;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLong64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLong64(hWnd, nIndex);
            else
                return new IntPtr(GetWindowLong32(hWnd, nIndex));
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        #endregion
    }
}