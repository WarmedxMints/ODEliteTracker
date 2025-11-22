using ODEliteTracker.ViewModels;
using ODEliteTracker.ViewModels.ModelViews.Massacre;
using System.Windows.Controls;

namespace ODEliteTracker.Views
{
    /// <summary>
    /// Interaction logic for MassacreMissionView.xaml
    /// </summary>
    public partial class MassacreMissionView : UserControl
    {
        public MassacreMissionView()
        {
            InitializeComponent();
        }

        private void MissionsDataGridRow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is MassacreMissionsViewModel vm && e.Source is DataGridRow row && row.DataContext is MassacreMissionVM mission)
            {
                var stacks = vm.Stacks.Where(x => string.Equals(x.IssuingFaction, mission.IssuingFaction));

                if (stacks.Any() == false)
                {
                    e.Handled = true;
                    return;
                }

                foreach (var stack in stacks)
                {
                    stack.Highlight = true;
                }
            }
            e.Handled = true;
        }

        private void MissionsDataGridRow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is MassacreMissionsViewModel vm && e.Source is DataGridRow row && row.DataContext is MassacreMissionVM mission)
            {
                var stacks = vm.Stacks.Where(x => string.Equals(x.IssuingFaction, mission.IssuingFaction));

                if (stacks.Any() == false)
                {
                    e.Handled = true;
                    return;
                }

                foreach (var stack in stacks)
                {
                    stack.Highlight = false;
                }
            }
            e.Handled = true;
        }

        private void StacksDataGridRow_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is MassacreMissionsViewModel vm && e.Source is DataGridRow row && row.DataContext is MassacreStackVM stack)
            {
                var missions = vm.ActiveMissions.Where(x => string.Equals(x.IssuingFaction, stack.IssuingFaction));

                if (missions.Any() == false)
                {
                    e.Handled = true;
                    return;
                }

                foreach (var mission in missions)
                {
                    mission.Highlight = true;
                }
            }
            e.Handled = true;
        }

        private void StacksDataGridRow_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is MassacreMissionsViewModel vm && e.Source is DataGridRow row && row.DataContext is MassacreStackVM stack)
            {
                var missions = vm.ActiveMissions.Where(x => string.Equals(x.IssuingFaction, stack.IssuingFaction));

                if (missions.Any() == false)
                {
                    e.Handled = true;
                    return;
                }

                foreach (var mission in missions)
                {
                    mission.Highlight = false;
                }
            }
            e.Handled = true;
        }
    }
}
