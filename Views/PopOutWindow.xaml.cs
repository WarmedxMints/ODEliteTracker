using ODEliteTracker.ViewModels.PopOuts;
using ODMVVM.Helpers;
using ODMVVM.Views;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for PopOutWindow.xaml
    /// </summary>
    public partial class PopOutWindow : ODWindowBase
    {
        public PopOutWindow(PopOutViewModel model)
        {
            DataContext = model;
            viewModel = model;
            checkForMouseTimer = new(250);
            checkForMouseTimer.Elapsed += OnMouseTimer;
            viewModel.CloseWindowEvent += ViewModel_CloseWindowEvent;
            Loaded += PopOutWindow_Loaded;
            Closing += PopOutWindow_Closing;
            SetAsToolWindow();
            InitializeComponent();
        }

        private bool _isTransparent;
        private bool _forceClosed;

        private void ViewModel_CloseWindowEvent(object? sender, EventArgs e)
        {
            checkForMouseTimer.Stop();
            checkForMouseTimer.Dispose();
            _forceClosed = true;
            Close();
        }

        private void PopOutWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_forceClosed == false)
            {
                checkForMouseTimer.Stop();
                checkForMouseTimer.Dispose();
                viewModel.OnWindowClosing();
            }
        }

        private void PopOutWindow_Loaded(object sender, RoutedEventArgs e)
        {
            checkForMouseTimer.Start();
            SetRect();
        }

        private Rect windowRect = new();
        private readonly System.Timers.Timer checkForMouseTimer;
        private readonly PopOutViewModel viewModel;

        private void OnMouseTimer(object? sender, ElapsedEventArgs e)
        {
            if (_forceClosed)
                return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(viewModel.PauseWindowListener)
                {
                    viewModel.OnMouseEnter_Leave(true);
                    return;
                }
                //Get the mouse position
                var mousePos = System.Windows.Forms.Control.MousePosition;
                //Check if the mouse is within the window
                var containsMouse = windowRect.Contains(mousePos.X, mousePos.Y);


                if (viewModel.ClickThrough)
                {
                    DealWithClickThrough(containsMouse || this.IsMouseOver);
                    return;
                }

                if (this.IsMouseOver)
                {
                    viewModel.OnMouseEnter_Leave(false);
                    return;
                }
                viewModel.OnMouseEnter_Leave(!containsMouse);
            });
        }

        private void DealWithClickThrough(bool isMouseOver)
        {
            if (isMouseOver == false)
            {
                if (_isTransparent == false)
                {
                    this.SetWindowExTransparent();
                    _isTransparent = true;
                    viewModel.OnMouseEnter_Leave(true);
                    return;
                }
                return;
            }

            if (_isTransparent == false || !Keyboard.IsKeyDown(Key.LeftShift))
                return;

            this.SetWindowNormal();
            _isTransparent = false;
            viewModel.OnMouseEnter_Leave(false);
        }

        private void SetRect()
        {
            Point screenCoordinates = this.PointToScreen(new Point(0, 0));
            windowRect = new(screenCoordinates.X - 10, screenCoordinates.Y - 10, this.Width + 20, this.Height + 20);
        }

        private void PopOut_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded == false)
                return;

            SetRect();
            AdjustPopout();
        }

        private void PopOut_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoaded == false)
                return;
            SetRect();
            AdjustPopout();
        }

        private void AdjustPopout()
        {
            double placementTargetWidth = PopOut.ActualWidth;
            double toolTipWidth = ToolBarPopUp.Width;
            ToolBarPopUp.HorizontalOffset = ToolBarPopUp.HorizontalOffset + 1;
            ToolBarPopUp.HorizontalOffset = (placementTargetWidth / 2.0) - (toolTipWidth / 2.0);
        }
    }
}
