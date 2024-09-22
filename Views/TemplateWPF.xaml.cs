using Reporter_vCLabs.ViewModels;
using System.Windows;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for SelectTemplateWPF.xaml
    /// </summary>
    public partial class SelectTemplateView : Window
    {
        private SelectTemplateViewModel selectTemplateViewModel;

        public SelectTemplateView()
        {
            InitializeComponent();

            selectTemplateViewModel = new SelectTemplateViewModel(this);
            this.DataContext = selectTemplateViewModel;
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DefineSpaceForTradeLegends_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DefineSpaceForTradeLegends_Button.IsEnabled = true;
            SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked = true;
        }

        private void DefineSpaceForTradeLegends_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DefineSpaceForTradeLegends_Button.IsEnabled = false ;
            SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked = false;
        }

        private void DefineSpaceForSeverityLegends_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DefineSpaceForSeverityLegends_Button.IsEnabled = true;
            SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked = true;
        }

        private void DefineSpaceForSeverityLegends_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DefineSpaceForSeverityLegends_Button.IsEnabled = false;
            SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked = false;
        }
    }
}
