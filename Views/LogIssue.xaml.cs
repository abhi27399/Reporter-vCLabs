using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Gui.Roamer;
using Newtonsoft.Json;
using Reporter_vCLabs.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for LogIssue.xaml
    /// </summary>
    public partial class LogIssue : UserControl
    {
        public static TextBox Description_TextBox { get; set; } = new TextBox()
        {
            Height = 20,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 10,
        };

        public static ComboBox Trade_ComboBox { get; set; } = new ComboBox() 
        {
            Height = 20,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 10,
            IsEditable = true,
            IsReadOnly = false,
            IsTextSearchEnabled = true,
            IsTextSearchCaseSensitive = false,
        };

        public static ComboBox Severity_ComboBox { get; set; } = new ComboBox() 
        {
            Height = 20,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 10,
            IsEditable = true,
            IsReadOnly = false,
            IsTextSearchEnabled = true,
            IsTextSearchCaseSensitive = false,
        };

        public LogIssue()
        {
            InitializeComponent();

            Grid.SetRow(Description_TextBox, 0);
            Grid.SetColumn(Description_TextBox, 1);
            grid.Children.Add(Description_TextBox);

            Grid.SetRow(Trade_ComboBox, 1);
            Grid.SetColumn(Trade_ComboBox, 1);
            grid.Children.Add(Trade_ComboBox);

            Grid.SetRow(Severity_ComboBox, 2);
            Grid.SetColumn(Severity_ComboBox, 1);
            grid.Children.Add(Severity_ComboBox);
        }
                
        public static void SavedViewpoints_CurrentSavedViewpointChanged()
        {
            try
            {
                if (GenerateReportWPF.IsRunning)
                {
                    return;
                }

                if (!LogIssueDockPanePlugin.IsVisible)
                {
                    return;
                }

                var document = Autodesk.Navisworks.Api.Application.ActiveDocument;

                var currentSavedViewpoint = document.SavedViewpoints.CurrentSavedViewpoint;

                if(Trade_ComboBox.Items.Count == 0 & Severity_ComboBox.Items.Count == 0)
                {
                    AddComboBoxItems();
                }

                if (currentSavedViewpoint != null)
                {
                    List<SuperSavedViewPoint> superSavedViewPoints = document.GetSuperSavedViewPoints();

                    var superSavedViewPoint = superSavedViewPoints.Find(ssvp => ssvp.SavedViewpoint == currentSavedViewpoint);

                    Description_TextBox.Text = superSavedViewPoint.SavedViewpoint.DisplayName;

                    foreach (ComboBoxItem comboBoxItem in Trade_ComboBox.Items)
                    {
                        if (comboBoxItem.Content.ToString().Equals(superSavedViewPoint.Trade))
                        {
                            Trade_ComboBox.SelectedItem = comboBoxItem;
                        }
                    }

                    foreach (ComboBoxItem comboBoxItem in Severity_ComboBox.Items)
                    {
                        if (comboBoxItem.Content.ToString().Equals(superSavedViewPoint.Severity))
                        {
                            Severity_ComboBox.SelectedItem = comboBoxItem;
                        }
                    }

                }


                else
                {
                    Description_TextBox.Text = string.Empty;

                    Trade_ComboBox.SelectedIndex = -1;

                    Severity_ComboBox.SelectedIndex = -1;
                }
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Occured: {ex.Message}");
            }
        }

        private static void AddTradeComboBoxItems()
        {
            try
            {
                if (Trade_ComboBox.Items.Count > 0)
                {
                    Trade_ComboBox.Items.Clear();
                }

                // Specifying the file path for getting previously saved trade list
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt";

                // Reading the JSON string from the file
                string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                // Deserializing the JSON string back to a list of objects
                List<Trade> deserializedTradeList = JsonConvert.DeserializeObject<List<Trade>>(jsonStringFromFile, new JsonSerializerSettings
                {
                    Converters = { new Reporter_vCLabs.ColorConverter() }
                });

                foreach (var trade in deserializedTradeList)
                {
                    ComboBoxItem Trade_ComboBoxItem = new ComboBoxItem
                    {
                        Content = trade.Name,
                        BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(trade.Border_Color.A, trade.Border_Color.R, trade.Border_Color.G, trade.Border_Color.B)),
                        BorderThickness = new Thickness(trade.Border_Thickness),
                    };

                    Trade_ComboBox.Items.Add(Trade_ComboBoxItem);
                }
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Occured: {ex.Message}");
            }
        }

        private static void AddSeverityComboBoxItems()
        {
            try
            {
                if (Severity_ComboBox.Items.Count > 0)
                {
                    Severity_ComboBox.Items.Clear();
                }

                // Specifying the file path for getting previously saved severity list
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt";

                // Reading the JSON string from the file
                string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                // Deserializing the JSON string back to a list of objects
                List<Severity> deserializedSeverityList = JsonConvert.DeserializeObject<List<Severity>>(jsonStringFromFile, new JsonSerializerSettings
                {
                    Converters = { new ColorConverter() }
                });

                foreach (var severity in deserializedSeverityList)
                {
                    ComboBoxItem Severity_ComboBoxItem = new ComboBoxItem
                    {
                        Content = severity.Type,
                        Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(severity.Text_Color.A, severity.Text_Color.R, severity.Text_Color.G, severity.Text_Color.B)),
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(severity.Background_Color.A, severity.Background_Color.R, severity.Background_Color.G, severity.Background_Color.B))
                    };

                    Severity_ComboBox.Items.Add(Severity_ComboBoxItem);
                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Occured: {ex.Message}");
            }
        }

        public static void AddComboBoxItems()
        {
            if (File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt"))
            {
                AddTradeComboBoxItems();
            }

            if (File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt"))
            {
                AddSeverityComboBoxItems();
            }
        }

        private void UpdateSavedviewpoint_Button_Click(object sender, RoutedEventArgs e)
        {
            var document = Autodesk.Navisworks.Api.Application.ActiveDocument;

            if (document.IsClear)
            {
                System.Windows.MessageBox.Show("No Active Document");
                return;
            }

            UpdateSavedViewpoint(document);
        }

        private void UpdateSavedViewpoint(Document document)
        {
            try
            {
                // Getting the current saved viewpoint
                var currentSavedViewpoint = document.SavedViewpoints.CurrentSavedViewpoint;

                if (currentSavedViewpoint == null)
                {
                    return;
                }

                string issueDescription = Description_TextBox.Text.ToString();

                // Updating its name
                document.SavedViewpoints.EditDisplayName(currentSavedViewpoint, issueDescription);

                
                string trade = string.Empty;
                string severity = string.Empty;

                if (Trade_ComboBox.SelectedItem != null && Severity_ComboBox.SelectedItem != null)
                {
                    trade = $"Trade : {(Trade_ComboBox.SelectedItem as ComboBoxItem).Content}" ;
                    severity = $"Severity : {(Severity_ComboBox.SelectedItem as ComboBoxItem).Content}" ;
                }

                else if (Trade_ComboBox.SelectedItem == null && Severity_ComboBox.SelectedItem != null)
                {
                    trade = "Trade : NA";
                    severity = $"Severity : {(Severity_ComboBox.SelectedItem as ComboBoxItem).Content}";
                }

                else if (Trade_ComboBox.SelectedItem != null && Severity_ComboBox.SelectedItem == null)
                {
                    trade = $"Trade : {(Trade_ComboBox.SelectedItem as ComboBoxItem).Content}";
                    severity = "Severity : NA";
                }

                else
                {
                    trade = "Trade : NA";
                    severity = "Severity : NA";
                }

                CommentCollection comments = new CommentCollection
                        {
                            document.CreateCommentWithUniqueId(trade, CommentStatus.New),
                            document.CreateCommentWithUniqueId(severity, CommentStatus.New)
                        };

                // Updating its comments
                document.SavedViewpoints.EditComments(currentSavedViewpoint, comments);
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Occured: {ex.Message}");
            }

        }

    }

    

}
