using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ODEliteTracker.Controls.Text
{
    /// <summary>
    /// Interaction logic for TextWithHeader.xaml
    /// </summary>
    public partial class TextWithHeader : UserControl
    {
        public TextWithHeader()
        {
            InitializeComponent();
        }

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(TextWithHeader), new PropertyMetadata("Header Text"));

        public double HeaderFontSize
        {
            get { return (double)GetValue(HeaderFontSizeProperty); }
            set { SetValue(HeaderFontSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderFontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderFontSizeProperty =
            DependencyProperty.Register(nameof(HeaderFontSize), typeof(double), typeof(TextWithHeader), new PropertyMetadata(20d));

        public VerticalAlignment HeaderVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(HeaderVerticalAlignmentProperty); }
            set { SetValue(HeaderVerticalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderVerticalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderVerticalAlignmentProperty =
            DependencyProperty.Register(nameof(HeaderVerticalAlignment), typeof(VerticalAlignment), typeof(TextWithHeader), new PropertyMetadata(VerticalAlignment.Center));

        public HorizontalAlignment HeaderHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(HeaderHorizontalAlignmentProperty); }
            set { SetValue(HeaderHorizontalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderHorizontalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderHorizontalAlignmentProperty =
            DependencyProperty.Register(nameof(HeaderHorizontalAlignment), typeof(HorizontalAlignment), typeof(TextWithHeader), new PropertyMetadata(HorizontalAlignment.Center));

        public Brush HeaderFontColour
        {
            get { return (Brush)GetValue(HeaderFontColourProperty); }
            set { SetValue(HeaderFontColourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderFontColour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderFontColourProperty =
            DependencyProperty.Register(nameof(HeaderFontColour), typeof(Brush), typeof(TextWithHeader), new PropertyMetadata(Brushes.Black));

        public Thickness HeaderMargin
        {
            get { return (Thickness)GetValue(HeaderMarginProperty); }
            set { SetValue(HeaderMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeaderMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(nameof(HeaderMargin), typeof(Thickness), typeof(TextWithHeader), new PropertyMetadata());


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextWithHeader), new PropertyMetadata("Main Text"));

        public double TextSize
        {
            get { return (double)GetValue(TextSizeProperty); }
            set { SetValue(TextSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextSizeProperty =
            DependencyProperty.Register(nameof(TextSize), typeof(double), typeof(TextWithHeader), new PropertyMetadata(16d));

        public VerticalAlignment TextVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(TextVerticalAlignmentProperty); }
            set { SetValue(TextVerticalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextVerticalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextVerticalAlignmentProperty =
            DependencyProperty.Register(nameof(TextVerticalAlignment), typeof(VerticalAlignment), typeof(TextWithHeader), new PropertyMetadata(VerticalAlignment.Center));

        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TextHorizontalAlignmentProperty); }
            set { SetValue(TextHorizontalAlignmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextHorizontalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextHorizontalAlignmentProperty =
            DependencyProperty.Register(nameof(TextHorizontalAlignment), typeof(HorizontalAlignment), typeof(TextWithHeader), new PropertyMetadata(HorizontalAlignment.Center));

        public Brush TextColour
        {
            get { return (Brush)GetValue(TextColourProperty); }
            set { SetValue(TextColourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextColour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextColourProperty =
            DependencyProperty.Register(nameof(TextColour), typeof(Brush), typeof(TextWithHeader), new PropertyMetadata(Brushes.Black));

        public Thickness TextMargin
        {
            get { return (Thickness)GetValue(TextMarginProperty); }
            set { SetValue(TextMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextMarginProperty =
            DependencyProperty.Register(nameof(TextMargin), typeof(Thickness), typeof(TextWithHeader), new PropertyMetadata());


    }
}
