using Autodesk.Navisworks.Api;
using iTextSharp.text;
using Newtonsoft.Json.Linq;
using Reporter_vCLabs.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for PreviewPlan.xaml
    /// </summary>
    public partial class PreviewPlan : Window
    {
        private int currentindex = 0;

        private List<SuperSavedViewPoint> superSavedViewPoints = new List<SuperSavedViewPoint>();

        private readonly List<SuperSavedViewPoint> planViews = new List<SuperSavedViewPoint>();
        public PreviewPlan()
        {
            InitializeComponent();

            Previous_Button.IsEnabled = false;

            Next_Button.IsEnabled = false;

            superSavedViewPoints = ReporterCommandHandlerPlugin.SuperSavedViewPoints;

            AddItemsInSelect_Planview_ComboBox();
        }

        private void AddItemsInSelect_Planview_ComboBox()
        {
            try
            {
                // Adding saved viewpoints in ComboBox on UI
                AddComboBoxItemsWithCheckBox();
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Getting Savedviewpoints : {ex.Message}");
            }

        }

        private void AddComboBoxItemsWithCheckBox()
        {
            foreach (var svp in superSavedViewPoints)
            {
                // Creating a CheckBox inside a ComboBoxItem
                System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox { Content = svp.SavedViewpoint.DisplayName };
                ComboBoxItem comboBoxItem = new ComboBoxItem { Content = checkBox };

                // Add ComboBoxItems to the ComboBox
                Select_Planview_ComboBox.Items.Add(comboBoxItem);

                // Setting the selection event for the CheckBox
                checkBox.Checked += CheckBox_Checked;

                // setting the deselection event for the CheckBox
                checkBox.Unchecked += CheckBox_Unchecked;
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;

            foreach (var svp in superSavedViewPoints)
            {
                if (svp.SavedViewpoint.DisplayName == checkBox.Content as string)
                {
                    svp.IsPlanView = true;
                    
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox checkBox = sender as System.Windows.Controls.CheckBox;

            foreach (var svp in superSavedViewPoints)
            {
                if (svp.SavedViewpoint.DisplayName == checkBox.Content as string)
                {
                    svp.IsPlanView = false;
                }

            }
        }

        private void Next_Button_Click(object sender, RoutedEventArgs e)
        {
            // Clearing all issue position rectangles for previous Plan View
            canvas.Children.Clear();

            if (currentindex < superSavedViewPoints.Count(item => item.IsPlanView) - 1)
            {
                currentindex++;
                LoadImagePreview(currentindex);

            }
            
        }

        private void Previous_Button_Click(object sender, RoutedEventArgs e)
        {
            // Clearing all issue position rectangles for previous Plan View
            canvas.Children.Clear();

            if (!(currentindex < 1))
            {
                currentindex--;
                LoadImagePreview(currentindex);
            }
            
        }

        private void LoadImagePreview(int indexOfPlanView)
        {
            try
            {
                // Creating Bitmap of the plan view
                Bitmap bitmap = planViews[indexOfPlanView].GenerateImage();

                // Setting the Bitmap on the Image control by converting it into BitmapImage
                imagePreview.Source = BitmapExtensions.ToBitmapImage(bitmap);
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading image: {ex.Message}");
            }

            
        }

        private void Preveiw_Button_Click(object sender, RoutedEventArgs e)
        {
            LoadImagePreview(currentindex);

            //PlotIssues();
        }

        private void PlotIssues()
        {
            try
            {
                var planView = planViews[currentindex];

                // Getting the rotation of plan view along z axis
                var rotation = planView.SavedViewpoint.Viewpoint.Rotation.ToString().Trim('(', ')').Replace(" ", "").Split(',');
                double rot_angle = - Convert.ToDouble(rotation[2]) / (180d / Math.PI);

                // Getting viewpoint data of the planv view
                var planView_viewPointCameraJson = planView.SavedViewpoint.Viewpoint.GetCamera();
                var planViewCameraJSONdata = JObject.Parse(planView_viewPointCameraJson);

                // Getting X and Y coordinates of the planview's position
                var planViewX = float.Parse(planViewCameraJSONdata["Position"][0].ToString());
                var planViewY = float.Parse(planViewCameraJSONdata["Position"][1].ToString());

                var planViewHorizontalExtent = float.Parse((planViewCameraJSONdata["HorizontalExtent"]).ToString());
                var planViewVerticalExtent = float.Parse((planViewCameraJSONdata["VerticalExtent"]).ToString());

                var reductionFactor_X = 1028f / planViewHorizontalExtent;
                var reductionFactor_Y = 684f / planViewVerticalExtent;


                foreach (var issueView in superSavedViewPoints.Where(item => !item.IsPlanView))
                {
                    var svpInfo = issueView.SavedViewpoint.DisplayName.Split('`', '.', '-');

                    // Getting X and Y coordinates of the issue's position
                    var issueViewX = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.X.ToString());
                    var issueViewY = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.Y.ToString());

                    float issue_X = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Cos(rot_angle) - (issueViewY - planViewY) * reductionFactor_Y * Math.Sin(rot_angle));
                    float issue_Y = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Sin(rot_angle) + (issueViewY - planViewY) * reductionFactor_Y * Math.Cos(rot_angle));

                    var ulx = 10f + 514f + issue_X;
                    var uly = 10f + 342f - issue_Y;

                    if (ulx < 10f || ulx > 1038f || uly < 10f || uly > 694f)
                    {
                        continue;
                    }

                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle
                    {
                        Width = 20,
                        Height = 20,
                        Fill = System.Windows.Media.Brushes.Transparent,
                        StrokeThickness = 2,

                    };


                    TextBlock textBlock = new TextBlock
                    {
                        Text = svpInfo[1],
                        FontSize = 15d,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,

                    };

                    if (svpInfo[0] == "r" || svpInfo[0] == "R")
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.Red;
                        textBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }

                    else if (svpInfo[0] == "b" || svpInfo[0] == "B")
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.Blue;
                        textBlock.Foreground = System.Windows.Media.Brushes.Blue;
                    }

                    else if (svpInfo[0] == "y" || svpInfo[0] == "Y")
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.Yellow;
                        rectangle.Fill = System.Windows.Media.Brushes.Yellow;
                        textBlock.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    else if (svpInfo[0] == "o" || svpInfo[0] == "O")
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.Orange;
                        textBlock.Foreground = System.Windows.Media.Brushes.Orange;
                    }

                    else
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.White;
                    }

                    // Setting the position of the issue position rectangles on the canvas
                    Canvas.SetLeft(rectangle, ulx - 10);
                    Canvas.SetTop(rectangle, uly + 10);

                    Canvas.SetLeft(textBlock, ulx - 7.5);
                    Canvas.SetTop(textBlock, uly + 7.5);

                    // Adding the issue position rectangles to the Canvas
                    canvas.Children.Add(rectangle);
                    canvas.Children.Add(textBlock);
                }

            }
            catch(Exception ex) 
            {
                System.Windows.MessageBox.Show($"Error Ploting Issues: {ex.Message}");
            }
        }

        private void Select_Planview_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
