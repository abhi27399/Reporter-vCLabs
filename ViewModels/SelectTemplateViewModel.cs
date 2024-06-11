using Reporter_vCLabs.;
using Reporter_vCLabs.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Reporter_vCLabs.ViewModels
{
    public class SelectTemplateViewModel : INotifyPropertyChanged
    {
        private SelectTemplateView selectTemplateView;

        public static string TemplatePath { get; set; }

        public static bool IsDefineSpaceForTradeLegends_CheckBox_Checked { get; set; }
        public static bool IsDefineSpaceForSeverityLegends_CheckBox_Checked { get; set; }

        public SelectTemplateViewModel(SelectTemplateView selectTemplateView)
        {
            this.selectTemplateView = selectTemplateView;
        }

        public void BrowseTemplate()
        {
            OpenFileDialog fdlg = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "PDF files (*.pdf*)|*.pdf*",
                Title = "Browse Template",
            };

            if (fdlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TemplatePath = fdlg.FileName;
                selectTemplateView.DefineSpaceForImage_Button.IsEnabled = true;
            }
        }

        public void ShowTemplate()
        {
            Process.Start(TemplatePath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
