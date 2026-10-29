namespace ODEliteTracker.Models.Settings
{
    public sealed class SpanshRouteSettings
    {
        public bool AutoCopySystemToClipboard { get; set; } = true;
        public Dictionary<int,List<SpanshAPISave>> CmdrApiData { get; set; } = [];
        public Dictionary<int, SpanshRouteTypeComboBox> SelectedApi { get; set; } = [];
        public Dictionary<int, int> SelectedShipID { get; set; } = [];
        public int RouteControllerSelectedTab { get; set; }

        internal SpanshRouteTypeComboBox GetApiSelection(int commanderId)
        {
            if (SelectedApi.TryGetValue(commanderId, out var apiSelection))
            {
                return SelectedApi[commanderId];
            }
            SelectedApi.Add(commanderId, SpanshRouteTypeComboBox.GalaxyPlotter);

            return SpanshRouteTypeComboBox.GalaxyPlotter;
        }

        internal void SetApiSelection(int commanderId, SpanshRouteTypeComboBox apiSelection)
        {
            if (SelectedApi.ContainsKey(commanderId))
            {
                SelectedApi[commanderId] = apiSelection;
                return;
            }

            SelectedApi.TryAdd(commanderId, apiSelection);
        }        

        internal List<SpanshAPISave> GetApiSave(int commanderId)
        {
            if (CmdrApiData.TryGetValue(commanderId, out var save))
                return save;

            var ret = new List<SpanshAPISave>();
            CmdrApiData.TryAdd(commanderId, ret);
            return ret;
               
        }

        internal void SaveAPiData(int selectedCommanderID, List<SpanshAPISave> saveData)
        {
            if (CmdrApiData.ContainsKey(selectedCommanderID))
                {
                CmdrApiData[selectedCommanderID] = saveData;
                return;
            }

            CmdrApiData.TryAdd(selectedCommanderID, saveData);
        }

        internal int GetSelectedShipID(int selectedCommanderID)
        {
            if (SelectedShipID.TryGetValue(selectedCommanderID, out var id))
                return id;

            SelectedShipID.TryAdd(selectedCommanderID, 0);
            return 0;
        }

        internal void SetSelectedShipID(int selectedCommanderID, int value)
        {
            if (SelectedShipID.ContainsKey(selectedCommanderID))
            {
                SelectedShipID[selectedCommanderID] = value;
                return;
            }

            SelectedShipID.TryAdd(selectedCommanderID, value);
        }
    }
}
