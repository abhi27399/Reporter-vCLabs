using Reporter_vCLabs.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void Browse_Button_Click(object sender, RoutedEventArgs e)
        {
            selectTemplateViewModel.BrowseTemplate();
        }

        private void DefineSpaceForImage_Button_Click(object sender, RoutedEventArgs e)
        {
            selectTemplateViewModel.ShowTemplate();
        }

        private void DefineSpaceForTradeLegends_Button_Click(object sender, RoutedEventArgs e)
        {
            selectTemplateViewModel.ShowTemplate();
        }

        private void DefineSpaceForSeverityLegends_Button_Click(object sender, RoutedEventArgs e)
        {
            selectTemplateViewModel.ShowTemplate();
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
