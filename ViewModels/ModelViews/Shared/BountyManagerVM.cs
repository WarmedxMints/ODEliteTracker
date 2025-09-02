using ODEliteTracker.Stores;
using ODMVVM.Commands;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.ComponentModel;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.ModelViews.Shared
{
    public enum BountySorting
    {
        Name,
        Value,
        [Description("System Factions")]
        SystemFactions
    }

    public sealed class BountyManagerVM : ODObservableObject, IDisposable
    {
        public BountyManagerVM(SharedDataStore sharedData, BGSViewModel bGSViewModel)
        {
            this.sharedData = sharedData;
            this.bGSViewModel = bGSViewModel;
            this.sharedData.BountiesUpdated += OnBountiesUpdated;
            this.sharedData.StoreLive += OnStoreLive;

            if (this.sharedData.IsLive)
            {
                OnStoreLive(null, true);
            }

            SetTopMostFaction = new ODRelayCommand<string>(OnSetTopMostFaction);
            ClearTopMostFaction = new ODRelayCommand((_) => OnSetTopMostFaction(null));
            AddIgnoredBounties = new ODRelayCommand<string>(OnAddIgnoredBounty);
            RemoveIgnoredBounties = new ODRelayCommand<string>(OnRemoveIgnoredBounty);
        }

        private readonly SharedDataStore sharedData;
        private readonly BGSViewModel bGSViewModel;
        public EventHandler<string>? TopFactionSet;

        public ICommand SetTopMostFaction { get; }
        public ICommand ClearTopMostFaction { get; }
        public ICommand AddIgnoredBounties { get; }
        public ICommand RemoveIgnoredBounties { get; }

        private IEnumerable<BountyVM> _bounties = [];

        public BountySorting Sorting
        {
            get => bGSViewModel.Settings.BGSViewSettings.Sorting;
            set
            {
                bGSViewModel.Settings.BGSViewSettings.Sorting = value;
                OnPropertyChanged(nameof(Sorting));
                OnPropertyChanged(nameof(Bounties));
            }
        }

        private string topFaction = string.Empty;
        public string TopFaction
        {
            get => topFaction;
            set
            {
                if (string.Equals(value, topFaction, StringComparison.OrdinalIgnoreCase))
                    return;

                topFaction = value;
                TopFactionSet?.Invoke(this, TopFaction);
                OnPropertyChanged(nameof(TopFaction));
                OnPropertyChanged(nameof(Bounties));
            }
        }

        public IEnumerable<BountyVM> Bounties
        {
            get
            {
                switch (Sorting)
                {
                    case BountySorting.Value:
                        return _bounties.OrderByDescending(x => string.Equals(x.Name, TopFaction)).ThenBy(x => x.Value);
                    case BountySorting.SystemFactions:
                        if (bGSViewModel.SelectedSystem == null || bGSViewModel.SelectedSystem.Factions == null)
                            return _bounties.OrderByDescending(x => string.Equals(x.Name, TopFaction)).ThenBy(x => x.Value);

                        var factions = bGSViewModel.SelectedSystem.Factions.Select(x => x.Name).ToList();

                        return _bounties.OrderByDescending(x => string.Equals(x.Name, TopFaction))
                            .ThenByDescending(x => factions.Contains(x.Name))
                            .ThenBy(x => factions.IndexOf(x.Name));
                    default:
                    case BountySorting.Name:
                        return _bounties.OrderByDescending(x => string.Equals(x.Name, TopFaction)).ThenBy(x => x.Name);
                }
            }
        }

        public string TotalBVCount => _bounties.Any() ? $"{_bounties.Sum(x => x.CountInt):N0}" : "0";
        public string TotalBVValue => _bounties.Any() ? $"{_bounties.Sum(x => x.ValueLong):N0}" : "0";
        public void UpdateBounties()
        {
            _bounties = sharedData.GetBounties().Select(x => new BountyVM(x));
            OnPropertyChanged(nameof(Bounties));
            OnPropertyChanged(nameof(TotalBVCount));
            OnPropertyChanged(nameof(TotalBVValue));
        }

        public void OnSelectedSystemChanged()
        {
            if (Sorting == BountySorting.SystemFactions)
                OnPropertyChanged(nameof(Bounties));
        }

        private void OnBountiesUpdated(object? sender, EventArgs e)
        {
            UpdateBounties();
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e == false)
                return;

            UpdateBounties();
        }

        private void OnSetTopMostFaction(string? obj)
        {
            TopFaction = string.IsNullOrEmpty(obj) ? string.Empty : obj;
        }

        private void OnAddIgnoredBounty(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return;

            if (string.Equals(obj, "All"))
            {
                var mb = ODDialogService.ShowWithOwner(null, "Add To Ignored Bounties?", $"Ignore all current bounties?", System.Windows.MessageBoxButton.YesNo);
                if (mb == System.Windows.MessageBoxResult.Yes)
                {
                    sharedData.IgnoreAllBounties();
                }
                return;
            }
            var messageBox = ODDialogService.ShowWithOwner(null, "Add To Ignored Bounties?", $"Ignore bounties from {obj} before now?", System.Windows.MessageBoxButton.YesNo);
            if (messageBox == System.Windows.MessageBoxResult.Yes)
            {
                sharedData.AddIgnoredBountyFaction(obj);
            }

        }

        private void OnRemoveIgnoredBounty(string obj)
        {
            sharedData.RemoveIgnoreBountyFaction(obj);
        }

        public void Dispose()
        {
            this.sharedData.BountiesUpdated -= OnBountiesUpdated;
            this.sharedData.StoreLive -= OnStoreLive;
        }
    }
}
