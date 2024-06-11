using Autodesk.Navisworks.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for More.xaml
    /// </summary>
    public partial class More : Window
    {
        private Document _document;

        private static bool hasSerialNumber = false;

        private static bool hasSeverityFlag = true;

        public More(Document document)
        {
            InitializeComponent();

            if (hasSerialNumber)
            {
                SerialNumber_Button.Content = "Remove";
            }

            if (!hasSeverityFlag)
            {
                SeverityFlag_Button.Content = "Retrieve";
            }

            this._document = document;

            SuperSavedViewPoints.MakeSavedViewpointsListOf(document.SavedViewpoints);
        }

        private void SerialNumber_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int SrN0 = 1;

                foreach (var superSavedViewPoint in SuperSavedViewPoints.List)
                {
                    if (SerialNumber_Button.Content.Equals("Insert"))
                    {
                        if (SeverityFlag_Button.Content.Equals("Remove"))
                        {
                            _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Insert(2, SrN0.ToString() + "."));
                        }

                        else if (SeverityFlag_Button.Content.Equals("Retrieve"))
                        {
                            _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Insert(0, SrN0.ToString() + "."));
                        }
                    }

                    else if (SerialNumber_Button.Content.Equals("Remove"))
                    {
                        if (SeverityFlag_Button.Content.Equals("Remove"))
                        {
                            _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Replace("`"+ SrN0.ToString() + ".", "`"));
                        }

                        else if (SeverityFlag_Button.Content.Equals("Retrieve"))
                        {
                            _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Remove(0, SrN0.ToString().Length + 1));
                        }

                    }

                    SrN0++;
                }

                if (SerialNumber_Button.Content.Equals("Insert"))
                {
                    SerialNumber_Button.Content = "Remove";
                    hasSerialNumber = true;
                }

                else if (SerialNumber_Button.Content.Equals("Remove"))
                {
                    SerialNumber_Button.Content = "Insert";
                    hasSerialNumber = false;
                }

            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error : {ex.Message}");
            }
        }

        private void SeverityFlag_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach(var superSavedViewPoint in SuperSavedViewPoints.List)
                {
                    if (SeverityFlag_Button.Content.Equals("Remove"))
                    {
                        if (!SuperSavedViewPoints.SvpGUID_and_SeverityFlagDict.Keys.Contains(superSavedViewPoint.SavedViewpoint.Guid))
                        {
                            SuperSavedViewPoints.SvpGUID_and_SeverityFlagDict.Add(superSavedViewPoint.SavedViewpoint.Guid, superSavedViewPoint.SavedViewpoint.DisplayName.Substring(0, 2));
                        }

                        _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Remove(0, 2));
                    }

                    else if (SeverityFlag_Button.Content.Equals("Retrieve"))
                    {
                        if (SuperSavedViewPoints.SvpGUID_and_SeverityFlagDict.Keys.Contains(superSavedViewPoint.SavedViewpoint.Guid))
                        {
                            var severityFlag = SuperSavedViewPoints.SvpGUID_and_SeverityFlagDict[superSavedViewPoint.SavedViewpoint.Guid];

                            _document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Insert(0, severityFlag.ToString()));
                        }
                    }
                }

                if (SeverityFlag_Button.Content.Equals("Remove"))
                {
                    SeverityFlag_Button.Content = "Retrieve";
                    hasSeverityFlag = false;
                }

                else if (SeverityFlag_Button.Content.Equals("Retrieve"))
                {
                    SeverityFlag_Button.Content = "Remove";
                    hasSeverityFlag = true;
                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error : {ex.Message}");
            }
        }
    }

}
