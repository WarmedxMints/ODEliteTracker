using Newtonsoft.Json.Linq;
using ODEliteTracker.Models.Settings;
using ODMVVM.Commands;
using ODMVVM.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public abstract class PopOutViewModel : ODObservableObject
    {
		public PopOutViewModel() 
		{
			HideToolButtonBar = new ODRelayCommand(OnHideToolBar);
			ResetUIScaleCommand = new ODRelayCommand(OnResetUIScale, (_) => UiScale != 1);
			ResetWindowPositionCommand = new ODRelayCommand(OnResetPosition);
			CloseWindowCommand = new ODRelayCommand(OnCloseWindow);

        }

        private bool _isClosing;
		public event EventHandler<PopOutViewModel>? WindowClosed;
		public event EventHandler? CloseWindowEvent;

        public ODWindowPosition Position { get; set; } = new();
		public abstract string Name { get; }
		public string Title => Count > 1 ? $"{Name} ({Count:N0})" : Name;
        public abstract bool IsLive { get; }
        public virtual bool IsBusy => !IsLive;
		public bool PauseWindowListener { get; set; }	
		public abstract Uri TitleBarIcon { get; }
        public ICommand HideToolButtonBar { get; }
        public ICommand ResetUIScaleCommand { get; }
        public ICommand ResetWindowPositionCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public int Count { get; set; }

        private readonly double opacityWhenMouseOver = 1;

		private double opacity = 1d; 
		public double Opacity
		{
			get
			{
				if(_isClosing == true || IsMouseOver == false || OpacityBtn)
					return opacity;
				return opacityWhenMouseOver;
			}
			set
			{
				opacity = value;
				OnPropertyChanged(nameof(Opacity));
			}
		}

        private double uiScale = 1;
        public double UiScale
        {
            get
            {
				return uiScale;
            }
            set
            {
                uiScale = value;
                OnPropertyChanged(nameof(UiScale));
            }
        }

		private bool showTitle;
		public bool ShowTitle
		{
			get => showTitle;
			set
			{
				showTitle = value;
				OnPropertyChanged(nameof(ShowTitle));
			}
		}

		private bool clickThrough;
		public bool ClickThrough
		{
			get => clickThrough;
			set
			{
				clickThrough = value;
				OnPropertyChanged(nameof(ClickThrough));
			}
		}

		private bool alwaysOnTop = true;
		public bool AlwaysOnTop
		{
			get => alwaysOnTop;
			set
			{
				alwaysOnTop = value;
				OnPropertyChanged(nameof(AlwaysOnTop));
			}
		}

		private bool showInTaskBar = true;
		public bool ShowInTaskBar
		{
			get => showInTaskBar;
			set
			{
				showInTaskBar = value;
				OnPropertyChanged(nameof(ShowInTaskBar));
			}
		}

		private Visibility titleBarVisibility = Visibility.Visible;
		public Visibility TitleBarVisibility
        {
			get
			{
				if (IsLive == false)
					return Visibility.Visible;
				return titleBarVisibility;
			}
			set
			{
                titleBarVisibility = value;
				OnPropertyChanged(nameof(TitleBarVisibility));
			}
		}

		private bool isMouseOver;
		public bool IsMouseOver
		{
			get => isMouseOver;
			set
			{
				isMouseOver = value;
				OnPropertyChanged(nameof(IsMouseOver));
			}
		}

		private bool opacityBtn;
		public bool OpacityBtn
		{
			get => opacityBtn;
			set
			{
				opacityBtn = value;
                OnPropertyChanged(nameof(Opacity));
                OnPropertyChanged(nameof(OpacityBtn));
			}
		}

		private readonly Thickness zeroThickness = new(0d);
		private readonly Thickness threeThickness = new(4d);

		private Thickness borderThickness = new(4d);
		public Thickness BorderThickness
        {
			get
			{
				if (IsLive == false)
					return threeThickness;
				return borderThickness;
			}
			set
			{
				borderThickness = value;
				OnPropertyChanged(nameof(BorderThickness));
			}
		}
		public virtual JObject? AdditionalSettings { get; set; }

		public virtual void OnMouseEnter_Leave(bool mouseLeave)
		{
            TitleBarVisibility = mouseLeave ? Visibility.Collapsed : Visibility.Visible;
            IsMouseOver = !mouseLeave;
            BorderThickness = mouseLeave ? zeroThickness : threeThickness;
			OnPropertyChanged(nameof(Opacity));
        }

        public void OnModelLive()
        {
            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(IsLive));
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(TitleBarVisibility));
        }

        private void OnHideToolBar(object? obj)
        {
            TitleBarVisibility = TitleBarVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            IsMouseOver = !IsMouseOver;
        }

        private void OnResetUIScale(object? obj)
        {
            UiScale = 1;
        }

        internal void OnWindowClosing()
        {
            Dispose();
            _isClosing = true;
            WindowClosed?.Invoke(this, this);
        }

        internal void CloseWindow()
        {
			Dispose();
            CloseWindowEvent?.Invoke(this, EventArgs.Empty);
        }

        internal virtual void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 800, 450);
        }

        private void OnCloseWindow(object? obj)
        {
            CloseWindow();
            WindowClosed?.Invoke(this, this);
        }

        internal void ApplyParams(PopOutParams popOutParams)
        {
            Count = popOutParams.Count;
            Opacity = popOutParams.Opacity;
            AlwaysOnTop = popOutParams.AlwaysOnTop;
            ShowInTaskBar = popOutParams.ShowInTaskBar;
            ShowTitle = popOutParams.ShowTitle;
            Opacity = popOutParams.Opacity;
            UiScale = popOutParams.UiScale;
            ClickThrough = popOutParams.ClickThrough;
            Position = popOutParams.Position.Clone();
			AdditionalSettings = popOutParams.AdditionalSettings;
			ParamsUpdated();
        }
        internal virtual JObject? GetAdditionalSettings()
        {
            return AdditionalSettings;
        }

        protected virtual void ParamsUpdated() { }
		protected virtual void Dispose() { }
    }
}
