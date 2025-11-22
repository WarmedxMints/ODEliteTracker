using ODMVVM.Helpers;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ColonisationBuildWishListItem : ODObservableObject
    {
        public ColonisationBuild Build { get; set; }
        public string BuildName => Build.GetEnumDescription();

        private int count;
        public int Count
        {
            get => count;
            set 
            { 
                count = value;
                OnPropertyChanged(nameof(CountString));
            }
        }

        public string CountString => count.ToString("N0");
    }
}