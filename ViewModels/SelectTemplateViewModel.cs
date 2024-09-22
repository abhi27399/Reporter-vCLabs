using Reporter_vCLabs.Command;
using System;
using System.ComponentModel;
using System.Diagnostics;
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

        public ICommand BrowseCommand { get; set; }

        public ICommand ShowCommand { get; set; }

        public SelectTemplateViewModel(SelectTemplateView selectTemplateView)
        {
            this.selectTemplateView = selectTemplateView;

            BrowseCommand = new RelayCommand(BrowseTemplate);
            ShowCommand = new RelayCommand(ShowTemplate);
        }

        private void BrowseTemplate()
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

        private void ShowTemplate()
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
