using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Interop;
using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Navisworks.Gui.Roamer.AIRLook;
using Autodesk.Navisworks.Gui.ToolkitWrappers;
using Autodesk.Windows;
using Newtonsoft.Json;
using Reporter_vCLabs.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.util.collections;
using System.Windows.Forms;
using static SSO_Integrator.SSOIntegrator;

namespace Reporter_vCLabs
{
    [Plugin("Reporter", "vConstruct")]
    //[RibbonLayout("vCLabsRibbon.xaml")]
    //[RibbonTab("vCLabs", DisplayName = "vCLabs")]
    [Command("LogIssue", ToolTip = "Log Issues as SavedViewpoints")]
    [Command("PreviewPlan", ToolTip = "Preview Plans with Issues marked on it")]
    [Command("Settings")]
    [Command("SelectTemplate", DisplayName ="Select Template", ToolTip = "Loads a PDF Template for a Report ")]
    [Command("GenerateReport", DisplayName = "Generate Report", ToolTip = "Creates a PDF Report of SavedViewpoints")]
    [Command("SerialNumber", ToolTip = "Inserts or Removes Serial Number in Display Name of SavedViewpoints")]
    [Command("Statistics")]
    
    public class ReporterCommandHandlerPlugin : CommandHandlerPlugin
    {
        public static LoginResponseModel LoginResponseModelResult { get; set; }

        public static string UserValidity {  get; set; }

        public static string VersionFromStore {  get; set; }

        public static List<SuperSavedViewPoint> SuperSavedViewPoints { get; set; } = new List<SuperSavedViewPoint>();

        protected override void OnLoaded()
        {
            try
            {
                var result = SSO();

                Autodesk.Navisworks.Api.Application.ActiveDocument.SavedViewpoints.Changed += SavedViewpoints_Changed;

            }

            catch(Exception ex)
            {
                MessageBox.Show($"Error with SSO : {ex.Message}");
            }
        }

        private void SavedViewpoints_Changed(object sender, SavedItemChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override int ExecuteCommand(string name, params string[] parameters)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);

            // Getting the active _document
            Document document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document.IsClear)
            {
                System.Windows.MessageBox.Show("No Active Document");
                return -1;
            }

            // Making list of saved viewpoints
            SuperSavedViewPoints = document.GetSuperSavedViewPoints();

            if (UserValidity == null)
            {
                return -1;
            }

            if(UserValidity.Contains("Valid User"))
            {
                switch (name)
                {
                    case "LogIssue":


                        SwitchVisibility();

                        break;

                    case "PreviewPlan":

                        try
                        {
                            MessageBox.Show("Feature Under Development");
                        }

                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }

                        

                        //PreviewPlan previewPlan = new PreviewPlan();
                        //previewPlan.Show();

                        break;

                    case "SelectTemplate":

                        SelectTemplateView templateWPF = new SelectTemplateView();
                        templateWPF.ShowDialog();

                        break;

                    case "GenerateReport":

                        try
                        {
                            GenerateReportWPF createReportWPF = new GenerateReportWPF();
                            createReportWPF.ShowDialog();
                            GenerateReportWPF.IsRunning = false;
                        }

                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show($"Error : {ex.Message}");
                        }

                        break;

                    case "SerialNumber":

                        UpdateSerialNumber(document);

                        break;

                    case "Settings":

                        Settings settings = new Settings();
                        settings.ShowDialog();

                        break;

                    case "Statistics":

                        ShowStatistics();

                        break;

                }
            }

            return 0;
        }

        private async Task<LoginResponseModel> SSO()
        {
            SSO sSO = new SSO();
            var result = await sSO.GetSSoResult();

            try
            {
                LoginResponseModelResult = result;
                StoreValidations(sSO, result);
            }

            catch (Exception ex)
            {
                
            }

            return result;
        }

        private async void StoreValidations(SSO sSO,LoginResponseModel result)
        {
            var userValidity = await sSO.Validate_StoreUser(result.Email, "78c79707-cbc1-40c5-9ddc-4b3a0b599f9c", result.AccessToken);

            var versionFromStore = await sSO.VersionFetch_Store(result.Email, "78c79707-cbc1-40c5-9ddc-4b3a0b599f9c", result.AccessToken);

            try
            {
                UserValidity = userValidity;
                VersionFromStore = versionFromStore;

                string versionPattern = @"\d+\.\d+";
                Match match = Regex.Match(versionFromStore, versionPattern);

                if (match.Success)
                {
                    versionFromStore = match.Value;
                }

                if (userValidity.Contains("Valid User"))
                {
                    VersionFetch versionFetch = new VersionFetch();
                    var version1 = new Version(versionFromStore);
                    var version2 = new Version(versionFetch.VersionNumber);
                    var resultVersion = version1.CompareTo(version2);

                    if (resultVersion > 0)
                    {
                        MessageBox.Show("Latest Version of Reporter is available on vCLabs Store");
                    }
                }

                else
                {
                    MessageBox.Show("Please Subscribe to vCLabs Store for Reporter");
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        

        private void SwitchVisibility()
        {
            try
            {
                NWDockWindow commentsDockWindow = null;
                NWDockWindow savedViewpointsWindow = null;

                // Getting View tab
                foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
                {
                    if (tab.Id.Equals("ID_TabView"))
                    {
                        // Getting the Workspace panel 
                        foreach (RibbonPanel panel in tab.Panels)
                        {
                            if (panel.Source.Id == "ID_WorkspacePanel")
                            {
                                // Getting Windows Checklist Button
                                RibbonChecklistButton windowsChecklistButton = panel.Source.Items[0] as RibbonChecklistButton;

                                // Getting commandItems for dock windows
                                foreach (RibbonItem item in windowsChecklistButton.Items)
                                {
                                    if(item.Text != null)
                                    {
                                        if (item.Text.Equals("Comments"))
                                        {
                                            // Getting the dock windows
                                            commentsDockWindow = (item as NWRibbonDockWindowCommandItem).DockWindow as NWDockWindow;
                                        }

                                        else if (item.Text.Equals("Saved Viewpoints"))
                                        {
                                            savedViewpointsWindow = (item as NWRibbonDockWindowCommandItem).DockWindow as NWDockWindow;
                                        }
                                    }
                                }

                                break ;
                            }
                        }

                        break;
                    }
                }

                PluginRecord logIssuePluginRecord = Autodesk.Navisworks.Api.Application.Plugins.FindPlugin("LogIssue.vConstruct");

                if (logIssuePluginRecord != null && logIssuePluginRecord is DockPanePluginRecord && logIssuePluginRecord.IsEnabled)
                {
                    logIssuePluginRecord.TryLoadPlugin();
                }

                DockPanePlugin logIssueDockPanePlugin = logIssuePluginRecord.LoadedPlugin as DockPanePlugin;


                // Switching the visibilty
                if (!commentsDockWindow.Closed && !savedViewpointsWindow.Closed && logIssueDockPanePlugin.Visible)
                {
                    commentsDockWindow.Closed = savedViewpointsWindow.Closed = true;
                    logIssueDockPanePlugin.Visible = false;
                }

                else
                {
                    commentsDockWindow.Closed = savedViewpointsWindow.Closed = false;
                    logIssueDockPanePlugin.Visible = true;
                    
                    LogIssue.AddComboBoxItems();
                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error : {ex.Message}");
            }
            
        }

        private RibbonButton FindRibbonButton(string buttonID, string tabID, string panelSourceID)
        {
            RibbonButton button = new RibbonButton();

            // Getting vCLabs tab
            foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
            {
                if (tab.Id == tabID)
                {
                    // Getting the Reporter panel 
                    foreach (RibbonPanel panel in tab.Panels)
                    {
                        if (panel.Source.Id == panelSourceID)
                        {
                            // Getting the button
                            button = panel.FindItem(buttonID, true) as RibbonButton;

                            break;
                        }
                    }

                    break;
                }
            }

            return button;
        }

        /// <summary>
        /// Updates the serial numbers of saved viewpoints from top to bottom in Saved Viewpoints Window of Navisworks
        /// </summary>
        /// <param name="document"></param>
        private void UpdateSerialNumber(Document document)
        {
            try
            {
                // Getting the serialNumberButton
                RibbonButton serialNumberButton = FindRibbonButton("Reporter.vConstruct.SerialNumber", "vCLabsRibbon.vConstruct.vcLabsAddIns", "Reporter");

                int SrN0 = 1;

                switch (serialNumberButton.Text)
                {
                    case "Insert Serial Number":

                        if(SuperSavedViewPoints.Exists(ssvp => !ssvp.HasSerialNumber))
                        {
                            foreach(var superSavedViewPoint in SuperSavedViewPoints.Where(ssvp => ssvp.HasSerialNumber))
                            {
                                var SrNoLength = superSavedViewPoint.SerialNumber.Length;
                                document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Remove(0, SrNoLength + 1));
                                superSavedViewPoint.SerialNumber = string.Empty;
                                superSavedViewPoint.HasSerialNumber = false;
                            }

                            foreach(var superSavedViewPoint in SuperSavedViewPoints)
                            {
                                document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Insert(0, SrN0.ToString() + "."));
                                superSavedViewPoint.SerialNumber = SrN0.ToString();
                                superSavedViewPoint.HasSerialNumber = true;
                                SrN0++;
                            }

                            serialNumberButton.Text = "Remove Serial Number";
                        }

                        else
                        {
                            serialNumberButton.Text = "Remove Serial Number";
                        }

                        break;

                    case "Remove Serial Number":

                        foreach (var superSavedViewPoint in SuperSavedViewPoints.Where(ssvp => ssvp.HasSerialNumber))
                        {
                            var SrNoLength = superSavedViewPoint.SerialNumber.Length;
                            document.SavedViewpoints.EditDisplayName(superSavedViewPoint.SavedViewpoint, superSavedViewPoint.SavedViewpoint.DisplayName.Remove(0, SrNoLength + 1));
                            superSavedViewPoint.SerialNumber = string.Empty;
                            superSavedViewPoint.HasSerialNumber = false;
                        }

                        serialNumberButton.Text = "Insert Serial Number";

                        break;

                }

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error : {ex.Message}");
            }
        }

        /// <summary>
        /// Shows the statistics of issues based on trade and severity
        /// </summary>
        private void ShowStatistics()
        {
            try
            {
                Dictionary<string, Dictionary<string, int>> tradeAndSeverityStatistics = new Dictionary<string, Dictionary<string, int>>();

                foreach(var ssvp in SuperSavedViewPoints)
                {
                    if (!tradeAndSeverityStatistics.ContainsKey(ssvp.Trade))
                    {
                        tradeAndSeverityStatistics[ssvp.Trade] = new Dictionary<string, int>();
                    }

                    if (!tradeAndSeverityStatistics[ssvp.Trade].ContainsKey(ssvp.Severity))
                    {
                        tradeAndSeverityStatistics[ssvp.Trade][ssvp.Severity] = 0;
                    }

                    tradeAndSeverityStatistics[ssvp.Trade][ssvp.Severity]++;
                }

                string header = "Region Name; Project Name; Pour Name; Date";

                string datatable = string.Empty;

                if(Settings.ProjectDetailsDictionary.Count !=  0)
                {
                    datatable = $"{Settings.ProjectDetailsDictionary["Region Name"]}; {Settings.ProjectDetailsDictionary["Region Name"]}; {Settings.ProjectDetailsDictionary["Pour Name"]}; {Settings.ProjectDetailsDictionary["Date"]}";
                }

                else if(File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\projectDetailsDictionary.txt"))
                {
                    // Specifying the file rootOptionsPath for getting previously project details dictionary
                    string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\projectDetailsDictionary.txt";

                    // Reading the JSON string from the file
                    string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                    // Deserializing the JSON string back to a dictionary of objects
                    Dictionary<string, string> deserializedProjectDetailsDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStringFromFile);

                    datatable = $"{deserializedProjectDetailsDictionary["Region Name"]}; {deserializedProjectDetailsDictionary["Region Name"]}; {deserializedProjectDetailsDictionary["Pour Name"]}; {deserializedProjectDetailsDictionary["Date"]}";
                }

                else
                {
                    datatable = " ; ; ; ;";
                }

                foreach(var keyValuePairOftradeAndSeverityStatistics in tradeAndSeverityStatistics)
                {
                    foreach(var keyValuePair in keyValuePairOftradeAndSeverityStatistics.Value)
                    {
                        string issue = $"; {keyValuePairOftradeAndSeverityStatistics.Key} : '{keyValuePair.Key}' Issues";
                        header += issue;

                        string issueCount = $"; {keyValuePair.Value}";
                        datatable += issueCount;
                    }
                }

                header += "; Total Issues";
                datatable += $"; {SuperSavedViewPoints.Count}";

                var result = System.Windows.MessageBox.Show(header + "\n" + "\n" + datatable, "Export Statistics", System.Windows.MessageBoxButton.OKCancel);

                if (result == System.Windows.MessageBoxResult.OK)
                {
                    // Creating a SaveFileDialog instance
                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "TXT files (*.txt*)|*.txt*",
                        Title = "Save TXT File",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        DefaultExt= ".txt",
                        OverwritePrompt = false,
                    };


                    // Showing the SaveFileDialog and checking if the user clicked OK
                    if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        // Getting the selected file name 
                        string txtFilePath = saveFileDialog.FileName;

                        if(File.Exists(txtFilePath))
                        {
                            File.AppendAllText(txtFilePath, "\n" + datatable);
                        }

                        else
                        {
                            File.WriteAllText(txtFilePath, header + "\n" + datatable);
                        }

                        Process.Start(txtFilePath);
                    }
                }


            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Geting Statistics : {ex.Message}");
            }
        }

        /// <summary>
        /// Increases the font size of markups (redline texts) on the viewport as per applied Image Quality in Settings
        /// </summary>
        public static void IncreaseMarkupFontSize()
        {
            try
            {
                if (Settings.ImageQualityValue == 1)
                {
                    return;
                }

                else
                {
                    using (LcUOptionLock olock = new LcUOptionLock())
                    {
                        LcUOptionSet rootOptions = LcUOption.GetRoot(olock);

                        var interfaceOptions = rootOptions.GetSubOptions(1);

                        var markupOptions = interfaceOptions.GetSubOptions(8);

                        if (markupOptions != null)
                        {
                            int count = markupOptions.GetNumOptions();

                            VariantData variantData = new VariantData();

                            markupOptions.GetValue(3, variantData);

                            int fontSize = variantData.ToInt32();

                            fontSize = Convert.ToInt32(fontSize * Settings.ImageQualityValue);

                            VariantData newVariantData = VariantData.FromInt32(fontSize);

                            markupOptions.SetValue(3, newVariantData);
                        }
                    }
                }

                
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Increasing Markup FontSize : {ex.Message}");
            }
        }

        /// <summary>
        /// Decreases the font size of markups (redline texts) on the viewport as per applied Image Quality in Settings 
        /// </summary>
        public static void DecreaseMarkupFontSize()
        {
            try
            {
                if (Settings.ImageQualityValue == 1)
                {
                    return;
                }

                else
                {
                    using (LcUOptionLock olock = new LcUOptionLock())
                    {
                        LcUOptionSet rootOptions = LcUOption.GetRoot(olock);

                        var interfaceOptions = rootOptions.GetSubOptions(1);

                        var markupOptions = interfaceOptions.GetSubOptions(8);

                        if (markupOptions != null)
                        {
                            int count = markupOptions.GetNumOptions();

                            VariantData variantData = new VariantData();

                            markupOptions.GetValue(3, variantData);

                            int fontSize = variantData.ToInt32();

                            fontSize = Convert.ToInt32(fontSize / Settings.ImageQualityValue);

                            VariantData newVariantData = VariantData.FromInt32(fontSize);

                            markupOptions.SetValue(3, newVariantData);
                        }
                    }
                }


            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Decreasing Markup FontSize : {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the Size Enhancing Factor from the applied Image Quality in Settings
        /// </summary>
        /// <returns>Size Enhancing Factor</returns>
        public static double GetSizeEnhancingFactor()
        {
            double sizeEnhancingFactor = Settings.ImageQualityValue;
            return sizeEnhancingFactor;
        }

        private Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("SSO"))
            {
                string assemblyFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\SSO-Integrator.dll";
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("BouncyCastle.Crypto"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Reporter vCLabs\BouncyCastle.Crypto.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("BouncyCastle.Cryptography"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Reporter vCLabs\BouncyCastle.Cryptography.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Common.Logging.Core"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Common.Logging.Core.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Common.Logging"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Common.Logging.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("EPPlus"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\EPPlus.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.barcodes"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.barcodes.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.bouncy-castle-adapter"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.bouncy-castle-adapter.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.bouncy-castle-connector"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.bouncy-castle-connector.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.commons"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.commons.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.forms"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.forms.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.io"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.io.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.kernel"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.kernel.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.layout"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.layout.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.licensekey"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.licensekey.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.pdfa"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.pdfa.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.pdfua"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.pdfua.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.sign"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.sign.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.styledxmlparser"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.styledxmlparser.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itext.svg"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itext.svg.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("itextsharp"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\itextsharp.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Bcl.AsyncInterfaces"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Bcl.AsyncInterfaces.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.DependencyInjection.Abstractions"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.DependencyInjection.Abstractions.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.DependencyInjection"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.DependencyInjection.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.Logging.Abstractions"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.Logging.Abstractions.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.Logging"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.Logging.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.Options"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.Options.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.Extensions.Primitives"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.Extensions.Primitives.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Microsoft.IO.RecyclableMemoryStream"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Microsoft.IO.RecyclableMemoryStream.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Buffers"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Buffers.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.ComponentModel.Annotations"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.ComponentModel.Annotations.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Diagnostics.DiagnosticSource"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Diagnostics.DiagnosticSource.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Memory"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Memory.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Numerics.Vectors"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Numerics.Vectors.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Runtime.CompilerServices.Unsafe"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Runtime.CompilerServices.Unsafe.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.Threading.Tasks.Extensions"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.Threading.Tasks.Extensions.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("System.ValueTuple"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\System.ValueTuple.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else if (args.Name.Contains("Xceed.Wpf.Toolkit"))
            {
                string assemblyFileName = Path.GetDirectoryName(@"C:\Program Files\Autodesk\Navisworks Manage 2023\Dependencies\Xceed.Wpf.Toolkit.dll");
                return Assembly.LoadFrom(assemblyFileName);
            }

            else
                return null;
        }
    }

    
}
