using System.Windows;
using System.Windows.Data;


namespace WebPad
{
    /// <summary>
    /// Interaction logic for References.xaml
    /// </summary>
    public partial class ReferencesWindow : Window
    {
        private readonly Rendering.References _references;

        public ReferencesWindow(Rendering.References references)
        {
            InitializeComponent();
            _references = references;
            referencesGrid.ItemsSource = _references;
        }

        public Rendering.References GetReferences()
        {
            return _references;
        }
    }
}
