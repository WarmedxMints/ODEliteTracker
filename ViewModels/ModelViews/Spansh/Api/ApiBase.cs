using ODEliteTracker.Models;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public abstract class ApiBase : ODObservableObject 
    {
        public abstract SpanshApiParameters ApiParameters { get; }
        public abstract ApiRouteSystemVM SourceSystem { get; set; }
        public virtual ApiRouteSystemVM? TargetSystem { get; set; }
        protected ObservableCollection<ApiRouteSystemVM> additionalSystems = [];
        public virtual ObservableCollection<ApiRouteSystemVM> AdditionalSystems { get => additionalSystems; set {  additionalSystems = value;  OnPropertyChanged(nameof(AdditionalSystems)); } }
        public ApiBase()
        {
            PropertyChanged += ApiBasePropertyChanged;
        }

        protected virtual void ApiBasePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            //If any property is changed then we clear the job id as it would be a different query
            if (ApiParameters != null)
                ApiParameters.JobId = null;
        }
    
        internal SpanshRouteType RouteType => ApiParameters.GetRouteType();
        internal abstract bool CanGenerate();
        internal abstract List<ApiRouteSystemVM> GetSystemNamesToCheck();
        internal abstract void LoadDefaults();
        //internal abstract void LoadParameters(SpanshApiParameters apiParameters);
        internal abstract SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm);
        internal virtual void ImportStats(ShipInfoVM vm) { }
        internal virtual void ImportStats(Models.FleetCarrier.FleetCarrier? vm) { }
        internal virtual bool AddAdditonalSystem(string systemName) 
        {
            var known = additionalSystems.FirstOrDefault(x => string.Equals(x.SystemName, systemName, StringComparison.InvariantCultureIgnoreCase));

            if (known != null)
                return false;

            additionalSystems.AddItem(new ApiRouteSystemVM(systemName));
            return true;
        }
        internal virtual bool CanAddAdditional(List<string> systemNames) => false;

        internal abstract void LoadFromSave(SpanshAPISave saveData);
        internal abstract SpanshAPISave GetSave();

        internal virtual bool CanReverseRoute => SourceSystem.HasValue && (TargetSystem?.HasValue ?? false);
        public virtual bool CanImportDestinations => false;
        public virtual bool CanImportStats => false;
        internal virtual bool WarnAboutScoop => false;
        internal virtual bool WarnAboutAFMU => false;
        internal virtual bool WarnAboutDSS => false;
        public virtual string ButtonText => "Apply Ship Stats";
        internal virtual SpanshRouteTypeComboBox RouteTypeBox()
        {
            return RouteType switch
            {
                SpanshRouteType.FleetCarrier => SpanshRouteTypeComboBox.FleetCarrier,
                SpanshRouteType.NeutronRoute => SpanshRouteTypeComboBox.NeutronRoute,
                SpanshRouteType.GalaxyPlotterOld or SpanshRouteType.GalaxyPlotter => SpanshRouteTypeComboBox.GalaxyPlotter,
                SpanshRouteType.WorldTypeRoute => SpanshRouteTypeComboBox.WorldTypeRoute,
                SpanshRouteType.TouristRoute => SpanshRouteTypeComboBox.TouristRoute,
                SpanshRouteType.ExobiologyOld or SpanshRouteType.Exobiology => SpanshRouteTypeComboBox.Exobiology,
                SpanshRouteType.Colonisation => SpanshRouteTypeComboBox.Colonisation,
                SpanshRouteType.TradePlanner => SpanshRouteTypeComboBox.TradePlanner,
                _ => SpanshRouteTypeComboBox.RoadToRiches,
            };
        }
    }
}
