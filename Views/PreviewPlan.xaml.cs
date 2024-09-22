using Newtonsoft.Json.Linq;
using Reporter_vCLabs.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for PreviewPlan.xaml
    /// </summary>
    public partial class PreviewPlan : Window
    {
        private ObservableCollection<SuperSavedViewPoint> SuperSavedViewPoints {  get; set; } = new ObservableCollection<SuperSavedViewPoint>();
        
        private List<Severity> SeverityList { get; set; } = new List<Severity>();

        public static SuperSavedViewPoint PlanView { get; set; }

        public static List<SuperSavedViewPoint> IssueViewToBePlotted { get; set;} = new List<SuperSavedViewPoint>();

        public PreviewPlan()
        {
            InitializeComponent();
            
            Apply_Button.IsEnabled = false;

            ReporterCommandHandlerPlugin.SuperSavedViewPoints.ForEach(ssvp => { SuperSavedViewPoints.Add(ssvp); });
            SeverityList = SettingsView.SeverityList; 

            SelectPlan_ComboBox.ItemsSource = SuperSavedViewPoints;
            SelectIssue_CheckComboBox.ItemsSource = SuperSavedViewPoints;
        }

        private void Preveiw_Button_Click(object sender, RoutedEventArgs e)
        {

            if (SelectPlan_ComboBox.SelectedItem is null)
            {
                MessageBox.Show("Select Plan View");
                return;
            }

            if (SelectIssue_CheckComboBox.SelectedItems.Count.Equals(0))
            {
                MessageBox.Show("Select Issue Views");
                return;
            }

            SuperSavedViewPoint selectedPlan = SelectPlan_ComboBox.SelectedItem as SuperSavedViewPoint;
            System.Collections.IList selectedIssues = SelectIssue_CheckComboBox.SelectedItems;

            LoadImagePreview(selectedPlan);

            PlotIssues(selectedPlan, selectedIssues);

            Apply_Button.IsEnabled = true;
        }


        private void LoadImagePreview( SuperSavedViewPoint superSavedViewPoint)
        {
            try
            {
                // Creating Bitmap of the plan view
                Bitmap bitmap = superSavedViewPoint.GenerateImage();

                // Setting the Bitmap on the Image control by converting it into BitmapImage
                imagePreview.Source = bitmap.ToBitmapImage();
            }

            catch (Exception ex)
            {
                ex.Log("Preview Plan", "LoadImagePreview");
            }

            
        }

        private void PlotIssues(SuperSavedViewPoint planView, System.Collections.IList issueViews)
        {
            try
            {
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

                var reductionFactor_X = 1200f / planViewHorizontalExtent;
                var reductionFactor_Y = 780f / planViewVerticalExtent;


                foreach (SuperSavedViewPoint issueView in issueViews)
                {
                    var serialNo = issueView.SerialNumber;

                    // Getting X and Y coordinates of the issue's position
                    var issueViewX = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.X.ToString());
                    var issueViewY = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.Y.ToString());

                    float issue_X = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Cos(rot_angle) - (issueViewY - planViewY) * reductionFactor_Y * Math.Sin(rot_angle));
                    float issue_Y = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Sin(rot_angle) + (issueViewY - planViewY) * reductionFactor_Y * Math.Cos(rot_angle));

                    var ulx = 600f + issue_X;
                    var uly = 400f - issue_Y;

                    if (ulx < 20f || ulx > 1160f || uly < 40f || uly > 780f)
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
                        Text = serialNo,
                        FontSize = 15d,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                        VerticalAlignment = System.Windows.VerticalAlignment.Center,

                    };


                    Severity severity = SeverityList.Find(s => s.Type.Equals(issueView.Severity));

                    if (severity is null)
                    {
                        rectangle.Stroke = System.Windows.Media.Brushes.White;
                        textBlock.Foreground = System.Windows.Media.Brushes.White;
                    }

                    else
                    {                        
                        rectangle.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(severity.Text_Color.A, severity.Text_Color.R, severity.Text_Color.G, severity.Text_Color.B));
                        rectangle.Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(severity.Background_Color.A, severity.Background_Color.R, severity.Background_Color.G, severity.Background_Color.B));
                        textBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(severity.Text_Color.A, severity.Text_Color.R, severity.Text_Color.G, severity.Text_Color.B));
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
                MessageBox.Show($"Error Ploting Issues: {ex.Message}");
            }
        }

        private void SaveCanvasAsImage(Canvas canvas, string filename)
        {
            // Setting up a RenderTargetBitmap to render the Canvas
            var renderTarget = new RenderTargetBitmap(
                (int)canvas.ActualWidth,
                (int)canvas.ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);

            // Rendering the Canvas to the RenderTargetBitmap
            renderTarget.Render(canvas);

            // Creating a PNG BitmapEncoder to save the RenderTargetBitmap
            var jpegEncoder = new JpegBitmapEncoder();
            jpegEncoder.Frames.Add(BitmapFrame.Create(renderTarget));            

            // Saving the image to a file
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                jpegEncoder.Save(fs);
            }

            //MessageBox.Show($"Canvas saved as {filename}");
            MessageBox.Show("Plan View Applied");
        }


        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Apply_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlanView = (SelectPlan_ComboBox.SelectedItem as SuperSavedViewPoint);
                var guid = PlanView.SavedViewpoint.Guid;
                PlanView.IsPlanView = true;

                var selectedIssues = SelectIssue_CheckComboBox.SelectedItems;
                IssueViewToBePlotted.Clear();
                foreach (var selectedIssue in selectedIssues)
                {
                    IssueViewToBePlotted.Add(selectedIssue as SuperSavedViewPoint);
                }

                if (!Directory.Exists($@"C:\ProgramData\Autodesk\Navisworks Manage 2023\Reporter_CanvasCache"))
                {
                    Directory.CreateDirectory($@"C:\ProgramData\Autodesk\Navisworks Manage 2023\Reporter_CanvasCache");
                }

                SaveCanvasAsImage(canvas, $@"C:\ProgramData\Autodesk\Navisworks Manage 2023\Reporter_CanvasCache\{guid}_canvasImage.jpeg");
                Close();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error Applying Plan View");
                ex.Log("PreviewPlan", "Apply_Button_Click");
            }
            
        }
    }
}
