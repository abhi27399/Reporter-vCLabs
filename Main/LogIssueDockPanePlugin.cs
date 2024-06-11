using Autodesk.Navisworks.Api.Plugins;
using Autodesk.Navisworks.Gui.Roamer.AIRLook;
using Autodesk.Windows;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Reporter_vCLabs
{
    [Plugin("LogIssue", "vConstruct", DisplayName = "Log Issue")]
    [DockPanePlugin(250, 300, AutoScroll = true, FixedSize =false)]
    public class LogIssueDockPanePlugin : DockPanePlugin
    {
        public static bool IsVisible {  get; set; }

        public override System.Windows.Forms.Control CreateControlPane()
        {
            try
            {
                AppDomain appDomain = AppDomain.CurrentDomain;
                appDomain.AssemblyLoad += AppDomain_AssemblyLoad;

                Autodesk.Navisworks.Api.Application.ActiveDocument.SavedViewpoints.CurrentSavedViewpointChanged += SavedViewpoints_CurrentSavedViewpointChanged;

                ElementHost control = new ElementHost
                {
                    AutoSize = true,
                    Child = new LogIssue(),
                    Dock = DockStyle.None,
                };

                control.CreateControl();
                IsVisible = true;

                return control;
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error LogIssueDockPanePlugin_1: {ex.Message}");
                return null;
            }
        }

        private void AppDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            var assem = appDomain.GetAssemblies();
            var ass = Assembly.GetEntryAssembly();
        }

        public override void DestroyControlPane(System.Windows.Forms.Control pane)
        {
            try
            {
                Autodesk.Navisworks.Api.Application.ActiveDocument.SavedViewpoints.CurrentSavedViewpointChanged -= SavedViewpoints_CurrentSavedViewpointChanged;

                var control = pane as ElementHost;
                control.Dispose();
                IsVisible = false;
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error LogIssueDockPanePlugin_2: {ex.Message}");
            }
            
        }

        public override void OnVisibleChanged()
        {
            //ExecuteNWRibbonExportButton("ID_TabOutput", "ID_ExportDataPanel", "Viewpoints\nReport");

            IsVisible = base.Visible;
        }

        private void SavedViewpoints_CurrentSavedViewpointChanged(object sender, EventArgs e)
        {
            LogIssue.SavedViewpoints_CurrentSavedViewpointChanged();
        }

        private void ExecuteNWRibbonExportButton(string tabId, string panelSourceId, string buttonText)
        {
            try
            {
                foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
                {
                    if (tab.Id.Equals(tabId))
                    {
                        foreach (RibbonPanel panel in tab.Panels)
                        {
                            if (panel.Source.Id.Equals(panelSourceId))
                            {
                                foreach(RibbonItem item in panel.Source.Items)
                                {
                                    if(item.Text == buttonText && item is NWRibbonExportButton nWRibbonExportButton)
                                    {
                                        if (nWRibbonExportButton.CanExecute(""))
                                        {
                                            //var windows1 = OpenWindowGetter.GetOpenWindows();

                                            nWRibbonExportButton.Execute("");

                                            //var windows2 = OpenWindowGetter.GetOpenWindows();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch { }
        }

        

    }

    
}
