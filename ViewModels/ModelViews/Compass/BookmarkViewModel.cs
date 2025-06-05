using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Compass
{
    public sealed class BookmarkViewModel : ODObservableObject
    {
		public BookmarkViewModel() { }


		private double latitude;
		public double Latitude
		{
			get => latitude;
			set
			{
				latitude = value;
				OnPropertyChanged(nameof(Latitude));
			}
		}

		private double longitude;
		public double Longitude
		{
			get => longitude;
			set
			{
				longitude = value;
				OnPropertyChanged(nameof(Longitude));
			}
		}

		public long SystemAddress { get; set; }
		public long BodyID { get; set; }

		private string systemName = string.Empty;
		public string SystemName
		{
			get => systemName;
			set
			{
				systemName = value;
				OnPropertyChanged(nameof(SystemName));
			}
		}

		private string bodyName = string.Empty;
		public string BodyName
		{
			get => bodyName;
			set
			{
				bodyName = value;
				OnPropertyChanged(nameof(BodyName));
			}
		}

		private string name = string.Empty;
		public string Name
		{
			get => name;
			set
			{
				name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

        internal void Reset()
        {
            Name = string.Empty;
			BodyName = string.Empty;
			BodyID = 0;
			SystemAddress = 0;
			Longitude = 0;
			Latitude = 0;
        }
    }
}
