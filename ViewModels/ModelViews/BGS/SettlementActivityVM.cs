using ODEliteTracker.Models.BGS;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Windows;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class SettlementActivityVM(SettlementActivity activity) : ODObservableObject
    {
        private readonly SettlementActivity activity = activity;

        public string Activity => activity.Activity.GetEnumDescription();
        public string KillCount => $"{activity.KillCount:N0}";
        public string TotalBonds => $"{activity.TotalBonds:N0}";
        public string SupportingFaction => activity.SupportingFaction;
        public string VictimFaction => activity.VictimFaction;
        public string SettlementName => activity.SettlementName;
        public Visibility DisplayBonds
        {
            get
            {
                return activity.Activity > OnFootActivity.Raid ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        internal void Update()
        {
            OnPropertyChanged(nameof(Activity));
            OnPropertyChanged(nameof(KillCount));
            OnPropertyChanged(nameof(TotalBonds));
            OnPropertyChanged(nameof(SupportingFaction));
            OnPropertyChanged(nameof(VictimFaction));
            OnPropertyChanged(nameof(SettlementName));
            OnPropertyChanged(nameof(DisplayBonds));           
        }
    }
}
