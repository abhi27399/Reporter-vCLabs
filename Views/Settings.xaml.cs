using Newtonsoft.Json;
using Reporter_vCLabs.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        private static Dictionary<string, string> _projectDetailsDictionary = new Dictionary<string, string>();

        private static List<Trade> _tradeList = new List<Trade>();

        private static List<Severity> _severityList = new List<Severity>();

        public static Dictionary<string, string> ProjectDetailsDictionary { get => _projectDetailsDictionary; set => _projectDetailsDictionary = value; }
        public static List<Trade> TradeList { get => _tradeList; set => _tradeList = value; }
        public static List<Severity> SeverityList { get => _severityList; set => _severityList = value; }
        public static double ImageQualityValue { get; set; } = 1d;

        public SettingsView()
        {
            InitializeComponent();

            // Adding initial input control sets on UI
            AddMultipleSetsOfInputControl(1,2, tradeFieldsGrid);
            AddMultipleSetsOfInputControl(1,2, severityFieldsGrid);

            // Populating input fields with locally saved tradelist
            PopulateInputFields();

        }

        private void PopulateInputFields()
        {
            if (File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt"))
            {
                PopulateTradeInputFields();
            }

            if (File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt"))
            {
                PopulateSeverityInputFields();
            }

            if (File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\projectDetailsDictionary.txt"))
            {
                PopulateProjectDetailFields();
            }
        }

        private void AddMultipleSetsOfInputControl(int fromRowIndex, int count, Grid grid)
        {
            for (int i = fromRowIndex; i < fromRowIndex + count; i++)
            {
                AddInputControlSet(i, grid);
            }
        }

        private void Ok_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Cosing Legneds wpf
            this.Close();
        }

        private void Apply_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Retrieving inputs
            RetrieveInputVaues();

            // Populating trade and severity comboboxes in LogIssue view
            LogIssue.AddComboBoxItems();

            Apply_Btn.IsEnabled = false;
        }

        private void PopulateProjectDetailFields()
        {
            try
            {
                _projectDetailsDictionary.Clear();

                // Specifying the file path for getting previously project details dictionary
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\projectDetailsDictionary.txt";

                // Reading the JSON string from the file
                string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                // Deserializing the JSON string back to a dictionary of objects
                Dictionary<string,string> deserializedProjectDetailsDictionary = JsonConvert.DeserializeObject<Dictionary<string,string>>(jsonStringFromFile);

                // Setting project details in input fields
                Region_TextBox.Text = deserializedProjectDetailsDictionary["Region Name"];
                Project_Name_TextBox.Text = deserializedProjectDetailsDictionary["Project Name"];
                Pour_Name_TextBox.Text = deserializedProjectDetailsDictionary["Pour Name"];
                Date_TextBox.Text = deserializedProjectDetailsDictionary["Date"];
            }

            catch (Exception ex)
            {
                ex.Log("LogIssue", "PopulateProjectDetailFields");
            }
        }

        private void PopulateTradeInputFields()
        {
            try
            {
                _tradeList.Clear();

                // Specifying the file path for getting previously saved trade list
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt";

                // Reading the JSON string from the file
                string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                // Deserializing the JSON string back to a list of objects
                List<Trade> deserializedTradeList = JsonConvert.DeserializeObject<List<Trade>>(jsonStringFromFile, new JsonSerializerSettings
                {
                    Converters = { new ColorConverter() }
                });

                int t = 0;

                int tradeCount = deserializedTradeList.Count;

                if (tradeCount > 3)
                {
                    AddMultipleSetsOfInputControl(3, tradeCount - 3, tradeFieldsGrid);
                }

                for (int i = 0; i < tradeFieldsGrid.Children.Count; i += 3)
                {
                    if (t < deserializedTradeList.Count)
                    {
                        (tradeFieldsGrid.Children[i] as TextBox).Text = deserializedTradeList[t].Name;
                        (tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor = System.Windows.Media.Color.FromArgb(deserializedTradeList[t].Border_Color.A, deserializedTradeList[t].Border_Color.R, deserializedTradeList[t].Border_Color.G, deserializedTradeList[t].Border_Color.B);
                        (tradeFieldsGrid.Children[i + 2] as TextBox).Text = deserializedTradeList[t].Border_Thickness.ToString();

                        t++;
                    }

                }
            }

            catch(Exception ex)
            {
                ex.Log("LogIssue", "PopulateTradeInputFields");
            }

            
        }

        private void PopulateSeverityInputFields()
        {
            try
            {
                _severityList.Clear();

                // Specifying the file path for getting previously saved severity list
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt";

                // Reading the JSON string from the file
                string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                // Deserializing the JSON string back to a list of objects
                List<Severity> deserializedSeverityList = JsonConvert.DeserializeObject<List<Severity>>(jsonStringFromFile, new JsonSerializerSettings
                {
                    Converters = { new ColorConverter() }
                });

                int s = 0;

                int severityCount = deserializedSeverityList.Count;

                if (severityCount > 3)
                {
                    AddMultipleSetsOfInputControl(3, severityCount - 3, severityFieldsGrid);
                }

                for (int i = 0; i < severityFieldsGrid.Children.Count; i += 3)
                {
                    if (s < deserializedSeverityList.Count)
                    {
                        (severityFieldsGrid.Children[i] as TextBox).Text = deserializedSeverityList[s].Type;
                        (severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor = System.Windows.Media.Color.FromArgb(deserializedSeverityList[s].Text_Color.A, deserializedSeverityList[s].Text_Color.R, deserializedSeverityList[s].Text_Color.G, deserializedSeverityList[s].Text_Color.B);
                        (severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor = System.Windows.Media.Color.FromArgb(deserializedSeverityList[s].Background_Color.A, deserializedSeverityList[s].Background_Color.R, deserializedSeverityList[s].Background_Color.G, deserializedSeverityList[s].Background_Color.B);

                        s++;
                    }

                }
            }

            catch (Exception ex)
            {
                ex.Log("LogIssue", "PopulateSeverityInputFields");
            }
        }

        private void Add_Trade_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddInputControlSet(tradeFieldsGrid.RowDefinitions.Count, tradeFieldsGrid);
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Adding Input Control Set :{ex.Message}");
            }
            
        }

        private void Add_Severity_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddInputControlSet(severityFieldsGrid.RowDefinitions.Count, severityFieldsGrid);
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Adding Input Control Set :{ex.Message}");
            }
        }

        private void AddSpaceForInputControlSetTo(Grid grid)
        {
            grid.Height += 40d;

            RowDefinition row = new RowDefinition { Height = new GridLength(1d, GridUnitType.Star) };
            grid.RowDefinitions.Add(row);
            
        }

        private void AddInputControlSet(int rowIndex, Grid grid)
        {
            // Adding space
            AddSpaceForInputControlSetTo(grid);

            // Creating a new textBox for Trade Name input or Severity Type input
            TextBox newTextBox1 = new TextBox
            {
                Width = 200,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center

            };

            // Setting row and column
            Grid.SetRow(newTextBox1, rowIndex);
            Grid.SetColumn(newTextBox1, 0);

            // Adding the textBox in the grid
            grid.Children.Add(newTextBox1);

            // Creating a new colorPicker for Description Box Border Color input or Description Box Text Color input
            ColorPicker newColorPicker1 = new ColorPicker
            {
                Width = 125,
                Height = 25,
                DisplayColorAndName = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            // Setting row and column
            Grid.SetRow(newColorPicker1, rowIndex);
            Grid.SetColumn(newColorPicker1, 1);

            // Adding the colorPicker in the grid
            grid.Children.Add(newColorPicker1);


            if(grid == tradeFieldsGrid)
            {
                // Creating a new textBox for Description Box Border Thickness input
                TextBox newTextBox2 = new TextBox
                {
                    Width = 100,
                    Height = 25,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                // Setting row and column
                Grid.SetRow(newTextBox2, rowIndex);
                Grid.SetColumn(newTextBox2, 2);

                // Adding the textBox in the grid
                grid.Children.Add(newTextBox2);
            }

            else if(grid == severityFieldsGrid)
            {
                // Creating a new borderColor colorPicker for Description Box Background_Color input
                ColorPicker newColorPicker2 = new ColorPicker
                {
                    Width = 100,
                    Height = 25,
                    DisplayColorAndName = true,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                // Setting row and column
                Grid.SetRow(newColorPicker2, rowIndex);
                Grid.SetColumn(newColorPicker2, 2);

                // Adding the colorPicker in the grid
                grid.Children.Add(newColorPicker2);
            }

            
        }

        private void RetrieveInputVaues()
        {
            RetrieveTradeInputFields();

            RetrieveSeverityInputFields();

            RetrieveProjectDetails();

            RetrieveImageQualityValue();
        }

        private void RetrieveProjectDetails()
        {
            try
            {
                _projectDetailsDictionary.Clear();

                // Adding project details into a dictionary
                _projectDetailsDictionary.Add("Region Name", Region_TextBox.Text);
                _projectDetailsDictionary.Add("Project Name", Project_Name_TextBox.Text);
                _projectDetailsDictionary.Add("Pour Name", Pour_Name_TextBox.Text);
                _projectDetailsDictionary.Add("Date", Date_TextBox.Text);

                // Specifying the file path for saving the dictionary for next use
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\projectDetailsDictionary.txt";

                // Converting the dictionary to a JSON string
                string jsonString = JsonConvert.SerializeObject(_projectDetailsDictionary, Formatting.Indented);

                // Writing the JSON string to the file
                System.IO.File.WriteAllText(filePath, jsonString);

            }

            catch (Exception ex)
            {
                ex.Log("LogIssue", "RetrieveProjectDetails");
            }
        }

        private void RetrieveTradeInputFields()
        {
            try
            {
                _tradeList.Clear();

                for (int i = 0; i < tradeFieldsGrid.Children.Count; i += 3)
                {
                    if ((tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor != null)
                    {
                        Trade trade = new Trade
                        {
                            Name = (tradeFieldsGrid.Children[i] as TextBox).Text,
                            Border_Color = System.Drawing.Color.FromArgb((tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.A, (tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.R, (tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.G, (tradeFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.B),
                            Border_Thickness = Convert.ToSingle((tradeFieldsGrid.Children[i + 2] as TextBox).Text)
                        };

                        TradeList.Add(trade);
                    }

                }

                // Specifying the file _path for saving tradeList for next use
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt";

                // Converting the list to a JSON string
                string jsonString = JsonConvert.SerializeObject(_tradeList, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new ColorConverter() }
                });

                // Writing the JSON string to the file
                System.IO.File.WriteAllText(filePath, jsonString);
            }

            catch(Exception ex)
            {
                ex.Log("LogIssue", "RetrieveTradeInputFields");
            }
        }

        private void RetrieveSeverityInputFields()
        {
            try
            {
                _severityList.Clear();

                for (int i = 0; i < severityFieldsGrid.Children.Count; i += 3)
                {
                    if ((severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor != null && (severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor != null)
                    {
                        Severity severity = new Severity
                        {
                            Type = (severityFieldsGrid.Children[i] as TextBox).Text,
                            Text_Color = System.Drawing.Color.FromArgb((severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.A, (severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.R, (severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.G, (severityFieldsGrid.Children[i + 1] as ColorPicker).SelectedColor.Value.B),
                            Background_Color = System.Drawing.Color.FromArgb((severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor.Value.A, (severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor.Value.R, (severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor.Value.G, (severityFieldsGrid.Children[i + 2] as ColorPicker).SelectedColor.Value.B)
                        };

                        _severityList.Add(severity);
                    }
                }

                // Specifying the file path for saving tradeList for next use
                string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt";

                // Converting the list to a JSON string
                string jsonString = JsonConvert.SerializeObject(_severityList, Formatting.Indented, new JsonSerializerSettings
                {
                    Converters = { new ColorConverter() }
                });

                // Writing the JSON string to the file
                System.IO.File.WriteAllText(filePath, jsonString);
            }

            catch (Exception ex)
            {
                ex.Log("LogIssue", "RetrieveSeverityInputFields");
            }
        }

        private void RetrieveImageQualityValue()
        {
            try
            {
                ImageQualityValue = ImageQualitySLider.Value;
            }

            catch (Exception ex)
            {
                ex.Log("LogIssue", "RetrieveImageQualityValue");
            }
        }

        
    }
        
}
