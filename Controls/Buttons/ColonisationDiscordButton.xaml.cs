using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ODEliteTracker.Controls.Buttons
{
    /// <summary>
    /// Interaction logic for ColonisationDiscordButton.xaml
    /// </summary>
    public partial class ColonisationDiscordButton : UserControl
    {
        public ColonisationDiscordButton()
        {
            InitializeComponent();
        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(ColonisationDiscordButton), new PropertyMetadata(string.Empty));



        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ColonisationDiscordButton));

        private void Popup_Closed(object sender, EventArgs e)
        {
            if (!object.Equals(this.button, Mouse.DirectlyOver))
            {
                this.button.IsChecked = false;
            }
        }

        private void Popup_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.button.IsChecked = false;
        }
    }
}
