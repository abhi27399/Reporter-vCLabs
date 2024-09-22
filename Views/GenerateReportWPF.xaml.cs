using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reporter_vCLabs.Model;
using Reporter_vCLabs.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using static Autodesk.Navisworks.Gui.Roamer.CommandLineConfig;


namespace Reporter_vCLabs
{
    /// <summary>
    /// Interaction logic for GenerateReportView.xaml
    /// </summary>
    public partial class GenerateReportView : Window
    {
        private List<SuperSavedViewPoint> SuperSavedViewPoints { get; set; }

        private int numberOfImagesPerPage;

        private string selectedFolderPath;

        private string reportFileName;

        private float pageHeight = 0;
        private float pageWidth = 0;

        private float imageHeight = 0;
        private float imageWidth = 0;

        private float annot_lry = 0;
        private float annot_lrx = 0;
        private float annot_uly = 0;
        private float annot_ulx = 0;

        private float tlAnnot_lry = 0;
        private float tlAnnot_uly = 0;
        private float tlAnnot_lrx = 0;
        private float tlAnnot_ulx = 0;

        private float slAnnot_lry = 0;
        private float slAnnot_uly = 0;
        private float slAnnot_lrx = 0;
        private float slAnnot_ulx = 0;

        private float table_llx = 0;
        private float table_lly = 0;

        private float cell_width = 0;
        private float cell_height = 0;

        private float cell_margin;

        private bool errorocurred = false;

        private static bool isRunning = false;

        public int NumberOfImagesPerPage { get => numberOfImagesPerPage; set => numberOfImagesPerPage = value; }
        public static bool IsRunning { get => isRunning; set => isRunning = value; }

        private SuperSavedViewPoint _planView;

        private List<SuperSavedViewPoint> _issueViewsToBePlotted;

        public GenerateReportView()
        {
            InitializeComponent();

            SuperSavedViewPoints = ReporterCommandHandlerPlugin.SuperSavedViewPoints;

            //Select_Planview_ComboBox.ItemsSource = SuperSavedViewPoints;

            GetPlanViewFromPreviewPlanView();

            GetIssueViewsToBePlottedFromPlanView();

            isRunning = true;
        }

        private void GetPlanViewFromPreviewPlanView()
        {
            var planView = PreviewPlan.PlanView;
            bool exists = false;
            if (planView != null)
            {
                exists = SuperSavedViewPoints.Exists(p => p.SavedViewpoint.Guid.Equals(planView.SavedViewpoint.Guid));
            }

            if (exists)
            {
                _planView = SuperSavedViewPoints.Find(p => p.SavedViewpoint.Guid.Equals(planView.SavedViewpoint.Guid));
                _planView.IsPlanView = true;
                _planView.ImagePath = $@"C:\ProgramData\Autodesk\Navisworks Manage 2023\Reporter_CanvasCache\{_planView.SavedViewpoint.Guid}_canvasImage.jpeg";
            }
        }

        private void GetIssueViewsToBePlottedFromPlanView()
        {
            var issueViewsToBePlotted = PreviewPlan.IssueViewToBePlotted;

            if(issueViewsToBePlotted.Count == 0) 
            {
                _issueViewsToBePlotted = new List<SuperSavedViewPoint>(SuperSavedViewPoints);
                return; 
            }

            var exists = SuperSavedViewPoints.Exists(ssvp => ssvp.SavedViewpoint.Guid.Equals(issueViewsToBePlotted.First().SavedViewpoint.Guid));

            if(exists)
            {
                _issueViewsToBePlotted = new List<SuperSavedViewPoint>(issueViewsToBePlotted);
            }

            else
            {
                _issueViewsToBePlotted = new List<SuperSavedViewPoint>(SuperSavedViewPoints);
            }
        }

        private void AddPlanView_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Select_Planview_ComboBox.IsEnabled = true;
        }

        private void AddPlanView_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Select_Planview_ComboBox.IsEnabled = false;
        }

        private void No_Of_Images_per_page_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NumberOfImagesPerPage = Convert.ToInt32((No_Of_Images_per_page_ComboBox.SelectedItem as ComboBoxItem)?.DataContext);
        }

        private void Generate_Report_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Creating a SaveFileDialog instance
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf*)|*.pdf*",
                    Title = "Save PDF Report File",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                // Showing the SaveFileDialog and checking if the user clicked OK
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Getting the selected file name 
                    var reportFilePath = saveFileDialog.FileName;
                    string[] directoryLevels = reportFilePath.Split(System.IO.Path.DirectorySeparatorChar);
                    reportFileName = directoryLevels[directoryLevels.Length - 1];

                    // Getting the selected folder _path
                    selectedFolderPath = Path.GetDirectoryName(reportFilePath);
                }
                
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Browsing Directory : {ex.Message}");
                isRunning = false;
                return;
            }

            // Generating the report
            GenerateReport();

            isRunning = false;

            try
            {
                if (!errorocurred)
                {
                    System.Windows.MessageBox.Show("The report is created and saved viepoints are exported.");

                    Process.Start(selectedFolderPath + "\\" + reportFileName + ".pdf");

                    try
                    {
                        int totalNoOfSavedViewpoints = _issueViewsToBePlotted.Count;

                        Dictionary<string, int> savedViewpointsBySeverity = new Dictionary<string, int>();
                        Dictionary<string, int> savedViewpointsByTrade = new Dictionary<string, int>();


                        foreach (var ssvp in _issueViewsToBePlotted)
                        {
                            if (savedViewpointsBySeverity.ContainsKey(ssvp.Severity))
                            {
                                savedViewpointsBySeverity[ssvp.Severity]++;
                            }

                            else
                            {
                                savedViewpointsBySeverity[ssvp.Severity] = 1;
                            }

                            if (savedViewpointsByTrade.ContainsKey(ssvp.Trade))
                            {
                                savedViewpointsByTrade[ssvp.Trade]++;
                            }

                            else
                            {
                                savedViewpointsByTrade[ssvp.Trade] = 1;
                            }
                        }

                        object reportGenerationEventDataObject = new
                        {
                            ApplicationName = "Reporter - Navisworks Plugin",
                            UserEmail = ReporterCommandHandlerPlugin.LoginResponseModelResult.Email,
                            FileName = $"{Autodesk.Navisworks.Api.Application.ActiveDocument.FileName.Split(System.IO.Path.DirectorySeparatorChar).Last()}",
                            TotalNoOfSavedViewpoints = totalNoOfSavedViewpoints,
                            SavedViewpointsBySeverity = savedViewpointsBySeverity,
                            SavedViewpointsByTrade = savedViewpointsByTrade,
                        };

                        MixPanel mixPanel = new MixPanel();

                        string reportGenerationEventDataJSON = JsonConvert.SerializeObject(reportGenerationEventDataObject);
                        var reportGenerationTracking = mixPanel.Track("Report Generation", reportGenerationEventDataJSON, ReporterCommandHandlerPlugin.LoginResponseModelResult.AccessToken);
                    }

                    catch(Exception ex)
                    {
                        ex.Log("GenearteReportWPF", $"Error Mixpanel Tracking : {ex.Message}");
                    }

                    this.Close();
                }

                else
                {
                    System.Windows.MessageBox.Show("The report is not created due to an error but saved viepoints are exported.");                    
                }
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Opening The PDF Report : {ex.Message}");
            }
            
        }

        private void GenerateReport()
        {
            try
            {
                // Creating Images of saved viewpoints
                CreateImagesOfSavedViewpoints();

                // Getting page count for the report
                int pageCount = GetNumberOfPages();

                // SelectTemplateWPF for the report
                string templatePath = SelectTemplateViewModel.TemplatePath;

                // Reading SelectTemplateWPF
                ReadTemplate(templatePath);

                // Adding Pages to the Report
                AddPages(pageCount);

                // Writing the Report
                WriteReport();

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Creating Report : {ex.Message}");
                errorocurred = true;
            }

        }

        private void AddPlanViewsAndDescription(PdfStamper pdfStamper, BaseFont baseFont)
        {
            if(_planView is null)
            {
                return;
            }

            int pageNo = 0;

            //foreach (var planView in SuperSavedViewPoints.Where(item => item.IsPlanView))
            //{
                //Creating a pdfContentByte for storing the content of the first pageAsPdfDictionary
                var pdfContentByte = pdfStamper.GetOverContent(++pageNo);

                //Creating an image instance from the image _path
                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(_planView.ImagePath);
                imageHeight = image.Height;
                imageWidth = image.Width;

                iTextSharp.text.Paragraph paragraph = new iTextSharp.text.Paragraph("Plan View")
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    Font = new iTextSharp.text.Font(baseFont, 10f)
                };

                PdfTemplate nameTemplate = pdfContentByte.CreateTemplate(250, 25);
                ColumnText ct = new ColumnText(nameTemplate);


                // Setting the dimensions of the issueDescriptionTemplate
                ct.SetSimpleColumn(0, 0, 250, 25);
                ct.AddElement(paragraph);
                ct.Go();

                // Adding the PdfTemplate to the page content at the specified position
                pdfContentByte.AddTemplate(nameTemplate, (pageWidth - annot_ulx) + (annot_ulx - annot_lrx) / 2 - 125, annot_lry);

                //Setting the iamge's scale to fit the space
                image.ScaleAbsolute(annot_ulx - annot_lrx, annot_uly - annot_lry - 25);

                //Setting the image's Absolute Postion in the page
                image.SetAbsolutePosition(pageWidth - annot_ulx, annot_lry + 25);

                pdfContentByte.AddImage(image);

            //}
        }

        private void AddIssueViewsAndDescription(PdfStamper pdfStamper, BaseFont baseFont)
        {
            //int pageNo = _issueViewsToBePlotted.Count(item => item.IsPlanView) + 1;
            int pageNo = 2;

            //Adding issueViewPoint from next page
            switch (NumberOfImagesPerPage)
            {
                case 1:
                    {
                        foreach (var issueViewPoint in _issueViewsToBePlotted.Where(item => !item.IsPlanView))
                        {
                            table_llx = pageWidth - annot_ulx;
                            table_lly = annot_lry;                            

                            //Creating a pdfContentByte for storing the content 
                            var pdfContentByte_after_planview = pdfStamper.GetOverContent(pageNo++);

                            var image = iTextSharp.text.Image.GetInstance(issueViewPoint.ImagePath);

                            var trade = issueViewPoint.Trade;
                            var severity = issueViewPoint.Severity;

                            Paragraph paragraph = new Paragraph(issueViewPoint.SavedViewpoint.DisplayName)
                            {
                                Alignment = Element.ALIGN_CENTER,
                                Font = new iTextSharp.text.Font(baseFont, 10f)
                            };


                            // Setting the dimensions of issue description's template 
                            float idt_Width = 240f;
                            float idt_Height = 40f;

                            PdfTemplate issueDescriptionTemplate = pdfContentByte_after_planview.CreateTemplate(idt_Width, idt_Height);
                            ColumnText ct = new ColumnText(issueDescriptionTemplate);

                            // Setting the dimensions of the coulmn text
                            ct.SetSimpleColumn(0, 0, idt_Width, idt_Height);
                            ct.AddElement(paragraph);
                            ct.Go();

                            iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx , annot_lry, table_llx + (annot_ulx - annot_lrx) / 2 + 125, annot_lry + 25);

                            Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                            pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                            // Adding the PdfTemplate to the pageAsPdfDictionary content at the specified position
                            pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, (pageWidth - annot_ulx) + (annot_ulx - annot_lrx) / 2 - 125, annot_lry);

                            // Setting the iamge's scale to fit the space
                            image.ScaleAbsolute((annot_ulx - annot_lrx), (imageHeight * (annot_ulx - annot_lrx) / imageWidth));

                            // Setting the image's Absolute Postion in the pageAsPdfDictionary
                            image.SetAbsolutePosition(pageWidth - annot_ulx, annot_lry + 25);

                            pdfContentByte_after_planview.AddImage(image);

                            if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked)
                            {
                                AddTradeLegends(pdfContentByte_after_planview);
                            }

                            if (SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                            {
                                AddSeverityLegends(pdfContentByte_after_planview, baseFont);
                            }

                        }

                    }
                    break;

                case 4:
                    {
                        // Setting Table's lower X and Y coordinates and dimensions
                        table_llx = pageWidth - annot_ulx;
                        table_lly = annot_lry;

                        cell_width = (annot_ulx - annot_lrx) / 2f;
                        cell_height = (annot_uly - annot_lry) / 2f;

                        cell_margin = 2.5f;

                        int i = 0;

                        foreach (var issueViewPoint in _issueViewsToBePlotted.Where(item => !item.IsPlanView))
                        {
                            // Creating a pdfContentByte for storing the content 
                            var pdfContentByte_after_planview = pdfStamper.GetOverContent(pageNo);

                            // Creating an image instance from the image _path
                            var image = iTextSharp.text.Image.GetInstance(issueViewPoint.ImagePath);

                            var trade = issueViewPoint.Trade;
                            var severity = issueViewPoint.Severity;

                            Paragraph paragraph = new Paragraph(issueViewPoint.SavedViewpoint.DisplayName)
                            {
                                Alignment = Element.ALIGN_CENTER,
                                Font = new iTextSharp.text.Font(baseFont, 10f)
                            };

                            // Setting the dimensions of issue description's template 
                            float idt_Width = cell_width - 2 * cell_margin;
                            float idt_Height = 40f;

                            PdfTemplate issueDescriptionTemplate = pdfContentByte_after_planview.CreateTemplate(idt_Width, idt_Height);
                            ColumnText ct = new ColumnText(issueDescriptionTemplate);

                            // Setting the dimensions of the coulmn text
                            ct.SetSimpleColumn(0, 0, idt_Width, idt_Height);
                            ct.AddElement(paragraph);
                            ct.Go();

                            // Scaling the image
                            image.ScaleAbsolute(cell_width - 2 * cell_margin, cell_height - 3 * cell_margin - idt_Height);
                            image.SetDpi(192, 192);

                            if (i == 0)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx, table_lly + cell_height, table_llx + cell_width, table_lly + 2 * cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                // Setting the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin, table_lly + cell_height + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin, table_lly + cell_height + cell_margin, table_llx + cell_margin + idt_Width, table_lly + cell_height + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                // Adding the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin, table_lly + cell_height + cell_margin);

                                if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked)
                                {
                                    AddTradeLegends(pdfContentByte_after_planview);
                                }

                                if (SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                                {
                                    AddSeverityLegends(pdfContentByte_after_planview, baseFont);
                                }

                            }

                            else if (i == 1)
                            {

                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + cell_width, table_lly + cell_height, table_llx + 2 * cell_width, table_lly + 2 * cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + cell_width, table_lly + cell_height + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + cell_width, table_lly + cell_height + cell_margin, table_llx + cell_margin + cell_width + idt_Width, table_lly + cell_height + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + cell_width, table_lly + cell_height + cell_margin);

                            }

                            else if (i == 2)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx, table_lly, table_llx + cell_width, table_lly + cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin, table_lly + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin, table_lly + cell_margin, table_llx + cell_margin + idt_Width, table_lly + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect,  trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                // Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin, table_lly + cell_margin);
                            }

                            else
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + cell_width, table_lly, table_llx + 2 * cell_width, table_lly + cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + cell_width, table_lly + idt_Height + 2 * cell_margin);


                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + cell_width, table_lly + cell_margin, table_llx + cell_margin + cell_width + idt_Width, table_lly + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + cell_width, table_lly + cell_margin);

                                pageNo++;

                            }

                            pdfContentByte_after_planview.AddImage(image);

                            

                            i++;
                            i %= 4;

                        }


                    }
                    break;

                case 6:
                    {
                        int i = 0;

                        // Setting Table's lower X and Y coordinates and dimensions
                        table_llx = pageWidth - annot_ulx;
                        table_lly = annot_lry;

                        cell_width = (annot_ulx - annot_lrx) / 3f;
                        cell_height = (annot_uly - annot_lry) / 2f;

                        cell_margin = 2.5f;

                        foreach (var issueViewPoint in _issueViewsToBePlotted.Where(item => !item.IsPlanView))
                        {
                            // Creating a pdfContentByte for storing the content 
                            var pdfContentByte_after_planview = pdfStamper.GetOverContent(pageNo);

                            // Creating an image instance from the image _path
                            var image = iTextSharp.text.Image.GetInstance(issueViewPoint.ImagePath);

                            var trade = issueViewPoint.Trade;
                            var severity = issueViewPoint.Severity;

                            Paragraph paragraph = new iTextSharp.text.Paragraph(issueViewPoint.SavedViewpoint.DisplayName)
                            {
                                Alignment = Element.ALIGN_CENTER,
                                Font = new iTextSharp.text.Font(baseFont, 10f)
                            };

                            // Setting the dimensions of issue description's template 
                            float idt_Width = cell_width - 2 * cell_margin;
                            float idt_Height = 40f;

                            PdfTemplate issueDescriptionTemplate = pdfContentByte_after_planview.CreateTemplate(idt_Width, idt_Height);
                            ColumnText ct = new ColumnText(issueDescriptionTemplate);

                            // Setting the dimensions of the coulmn text
                            ct.SetSimpleColumn(0, 0, idt_Width, idt_Height);
                            ct.AddElement(paragraph);
                            ct.Go();

                            // Scaling the image
                            image.ScaleAbsolute(cell_width - 2 * cell_margin, cell_height - 3 * cell_margin - idt_Height);

                            if (i == 0)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx, table_lly + cell_height, table_llx + cell_width, table_lly + 2 * cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                // Setting the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin, table_lly + cell_height + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin, table_lly + cell_height + cell_margin, table_llx + cell_margin + idt_Width, table_lly + cell_height + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                // Adding the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin, table_lly + cell_height + cell_margin);

                                if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked)
                                {
                                    AddTradeLegends(pdfContentByte_after_planview);
                                }

                                if (SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                                {
                                    AddSeverityLegends(pdfContentByte_after_planview, baseFont);
                                }

                            }

                            else if (i == 1)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + cell_width, table_lly + cell_height, table_llx + 2 * cell_width, table_lly + 2 * cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + cell_width, table_lly + cell_height + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + cell_width, table_lly + cell_height + cell_margin, table_llx + cell_margin + cell_width + idt_Width, table_lly + cell_height + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + cell_width, table_lly + cell_height + cell_margin);


                            }

                            else if (i == 2)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + 2 * cell_width, table_lly + cell_height, table_llx + 3 * cell_width, table_lly + 2 * cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + 2 * cell_width, table_lly + cell_height + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + 2 * cell_width, table_lly + cell_height + cell_margin, table_llx + cell_margin + 2 * cell_width + idt_Width, table_lly + cell_height + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + 2 * cell_width, table_lly + cell_height + cell_margin);


                            }

                            else if (i == 3)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx, table_lly, table_llx + cell_width, table_lly + cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin, table_lly + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin, table_lly + cell_margin, table_llx + cell_margin + idt_Width, table_lly + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                // Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin, table_lly + cell_margin);



                            }

                            else if (i == 4)
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + cell_width, table_lly, table_llx + 2 * cell_width, table_lly + cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + cell_width, table_lly + idt_Height + 2 * cell_margin);


                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + cell_width, table_lly + cell_margin, table_llx + cell_margin + cell_width + idt_Width, table_lly + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + cell_width, table_lly + cell_margin);



                            }

                            else
                            {
                                iTextSharp.text.Rectangle partitionRect = new iTextSharp.text.Rectangle(table_llx + 2 * cell_width, table_lly, table_llx + 3 * cell_width, table_lly + cell_height)
                                {
                                    Border = 15,
                                    BorderWidth = 1f,
                                    BorderColor = BaseColor.BLACK
                                };

                                pdfContentByte_after_planview.Rectangle(partitionRect);

                                //Set the image's Absolute Postion in the pageAsPdfDictionary
                                image.SetAbsolutePosition(table_llx + cell_margin + 2 * cell_width, table_lly + idt_Height + 2 * cell_margin);

                                iTextSharp.text.Rectangle issueDescriptionRect = new iTextSharp.text.Rectangle(table_llx + cell_margin + 2 * cell_width, table_lly + cell_margin, table_llx + cell_margin + 2 * cell_width + idt_Width, table_lly + cell_margin + idt_Height);

                                Classify_SeverityAndTrade(pdfContentByte_after_planview, issueDescriptionRect, trade, severity);

                                pdfContentByte_after_planview.Rectangle(issueDescriptionRect);

                                //Add the PdfTemplate to the pageAsPdfDictionary content at the specified position
                                pdfContentByte_after_planview.AddTemplate(issueDescriptionTemplate, table_llx + cell_margin + 2 * cell_width, table_lly + cell_margin);

                                pageNo++;

                            }


                            pdfContentByte_after_planview.AddImage(image);

                            

                            i++;
                            i %= 6;

                        }

                    }
                    break;

            }
        }

        private void WriteReport()
        {
            try
            {
                // Creating a pdfreader
                PdfReader reader = new PdfReader(new FileStream(selectedFolderPath + "\\" + "templatePDF.pdf", FileMode.Open, FileAccess.Read, FileShare.Read));

                // Creating a pdfstamper
                var pdfStamper = new PdfStamper(reader, new FileStream(selectedFolderPath + "\\" + reportFileName + ".pdf", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));

                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, true);

                AddPlanViewsAndDescription(pdfStamper, baseFont);

                AddIssueViewsAndDescription(pdfStamper, baseFont);

                // Closing the PdfStamper
                pdfStamper.Close();

                // Closing the PdfReader
                reader.Close();

                System.IO.File.Delete(selectedFolderPath + "\\" + "templatePDF.pdf");

                // Ploting issues on plan view
                //PlotIssuesOnPlanView();
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Writing Report : {ex.Message}");
                errorocurred = true;
            }
            
        }

        private void ReadTemplate(string templatePath)
        {
            try
            {
                using (PdfReader reader = new PdfReader(new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    using (PdfStamper stamper = new PdfStamper(reader, new FileStream(selectedFolderPath + "\\" + "template.pdf", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
                    {
                        // Getting the pageAsPdfDictionary as pdfDictionary
                        PdfDictionary pageAsPdfDictionary = reader.GetPageN(1);

                        ExtractPageDimensions(pageAsPdfDictionary);

                        ExtractMarkupDimensions(pageAsPdfDictionary);
                    }

                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Reading SelectTemplateWPF : {ex.Message}");
                errorocurred = true;
            }

        }

        private void ExtractPageDimensions(PdfDictionary pageAsPdfDictionary)
        {
            try
            {
                // Retrieving the MediaBox array (contains pageAsPdfDictionary dimensions)
                PdfArray mediaBox = pageAsPdfDictionary.GetAsArray(iTextSharp.text.pdf.PdfName.MEDIABOX);

                pageHeight = mediaBox.GetAsNumber(2).FloatValue;
                pageWidth = mediaBox.GetAsNumber(3).FloatValue;
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Extracting Page Dimensions from ReportTemplate : {ex.Message}");
                errorocurred = true;
            }

        }

        private void ExtractMarkupDimensions(PdfDictionary pageAsPdfDictionary)
        {
            try
            {
                // Getting the annotsAsPdfArray of the pageAsPdfDictionary as pdfAnnots
                PdfArray annotsAsPdfArray = pageAsPdfDictionary.GetAsArray(PdfName.ANNOTS);

                var dateTimeDictionary = new SortedDictionary<DateTime, int>();

                if (annotsAsPdfArray != null)
                {
                    foreach (PdfObject annot in annotsAsPdfArray.ArrayList)
                    {
                        PdfDictionary annotation = (PdfDictionary)PdfReader.GetPdfObject(annot);

                        if (annotation.Get(PdfName.SUBTYPE).Equals(PdfName.SQUARE))
                        {
                            var cdt = annotation.GetAsString(PdfName.CREATIONDATE).ToString();
                            var cdtSplit = cdt.Split(':', '+');

                            var cdtYear = Convert.ToInt32(cdtSplit[1].Substring(0, 4));
                            var cdtMonth = Convert.ToInt32(cdtSplit[1].Substring(4, 2));
                            var cdtDay = Convert.ToInt32(cdtSplit[1].Substring(6, 2));
                            var cdtHH = Convert.ToInt32(cdtSplit[1].Substring(8, 2));
                            var cdtMM = Convert.ToInt32(cdtSplit[1].Substring(10, 2));
                            var cdtSS = Convert.ToInt32(cdtSplit[1].Substring(12, 2));

                            DateTime creationDateTime = new DateTime(cdtYear, cdtMonth, cdtDay, cdtHH, cdtMM, cdtSS);

                            dateTimeDictionary.Add(creationDateTime, annotsAsPdfArray.ArrayList.IndexOf(annot));

                        }

                    }

                }

                if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                {
                    // Extracting the image space rectangle information
                    PdfArray rect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 3).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    annot_lry = ((PdfNumber)rect[0]).FloatValue; //annot_lry
                    annot_lrx = ((PdfNumber)rect[1]).FloatValue; //annot_lrx
                    annot_uly = ((PdfNumber)rect[2]).FloatValue; //annot_uly
                    annot_ulx = ((PdfNumber)rect[3]).FloatValue; //annot_ulx

                    // Extracting the trade legend rectangle information
                    PdfArray tlRect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    tlAnnot_lry = ((PdfNumber)tlRect[0]).FloatValue; //tlAnnot_lry
                    tlAnnot_lrx = ((PdfNumber)tlRect[1]).FloatValue; //tlAnnot_lrx
                    tlAnnot_uly = ((PdfNumber)tlRect[2]).FloatValue; //tlAnnot_uly
                    tlAnnot_ulx = ((PdfNumber)tlRect[3]).FloatValue; //tlAnnot_ulx

                    // Extracting the severity legend rectangle information
                    PdfArray slRect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    slAnnot_lry = ((PdfNumber)slRect[0]).FloatValue; //slAnnot_lry
                    slAnnot_lrx = ((PdfNumber)slRect[1]).FloatValue; //slAnnot_lrx
                    slAnnot_uly = ((PdfNumber)slRect[2]).FloatValue; //slAnnot_uly
                    slAnnot_ulx = ((PdfNumber)slRect[3]).FloatValue; //slAnnot_ulx

                    // Removing rectangles after getting their annotations
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value);
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value);
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 3).Value);

                }

                else if(SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && !SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                {
                    // Extracting the image space rectangle information
                    PdfArray rect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    annot_lry = ((PdfNumber)rect[0]).FloatValue; //annot_lry
                    annot_lrx = ((PdfNumber)rect[1]).FloatValue; //annot_lrx
                    annot_uly = ((PdfNumber)rect[2]).FloatValue; //annot_uly
                    annot_ulx = ((PdfNumber)rect[3]).FloatValue; //annot_ulx

                    // Extracting the trade legend rectangle information
                    PdfArray tlRect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    tlAnnot_lry = ((PdfNumber)tlRect[0]).FloatValue; //tlAnnot_lry
                    tlAnnot_lrx = ((PdfNumber)tlRect[1]).FloatValue; //tlAnnot_lrx
                    tlAnnot_uly = ((PdfNumber)tlRect[2]).FloatValue; //tlAnnot_uly
                    tlAnnot_ulx = ((PdfNumber)tlRect[3]).FloatValue; //tlAnnot_ulx

                    // Removing rectangles after getting their annotations
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value);
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value);
                }

                else if(!SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
                {
                    // Extracting the image space rectangle information
                    PdfArray rect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    annot_lry = ((PdfNumber)rect[0]).FloatValue; //annot_lry
                    annot_lrx = ((PdfNumber)rect[1]).FloatValue; //annot_lrx
                    annot_uly = ((PdfNumber)rect[2]).FloatValue; //annot_uly
                    annot_ulx = ((PdfNumber)rect[3]).FloatValue; //annot_ulx

                    // Extracting the severity legend rectangle information
                    PdfArray slRect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value]) as PdfDictionary).GetAsArray(PdfName.RECT);

                    slAnnot_lry = ((PdfNumber)slRect[0]).FloatValue; //slAnnot_lry
                    slAnnot_lrx = ((PdfNumber)slRect[1]).FloatValue; //slAnnot_lrx
                    slAnnot_uly = ((PdfNumber)slRect[2]).FloatValue; //slAnnot_uly
                    slAnnot_ulx = ((PdfNumber)slRect[3]).FloatValue; //slAnnot_ulx

                    // Removing rectangles after getting their annotations
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value);
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value);
                }

                else
                {
                    // Extracting the image space rectangle information
                    PdfArray rect = (PdfReader.GetPdfObject(annotsAsPdfArray.ArrayList[dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value]) as PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.RECT);

                    annot_lry = ((PdfNumber)rect[0]).FloatValue; //annot_lry
                    annot_lrx = ((PdfNumber)rect[1]).FloatValue; //annot_lrx
                    annot_uly = ((PdfNumber)rect[2]).FloatValue; //annot_uly
                    annot_ulx = ((PdfNumber)rect[3]).FloatValue; //annot_ulx

                    // Removing rectangles after getting their annotations
                    annotsAsPdfArray.Remove(dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value);
                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Extracting Markup Dimensions : {ex.Message}");
                errorocurred = true;
            }
        }

        private void AddPages(int pageCount)
        {
            try
            {
                using (iText.Kernel.Pdf.PdfWriter pdfWriter = new iText.Kernel.Pdf.PdfWriter(selectedFolderPath + "\\" + "templatePDF.pdf"))
                {
                    using (iText.Kernel.Pdf.PdfReader pdfReader = new iText.Kernel.Pdf.PdfReader(selectedFolderPath + "\\" + "template.pdf"))
                    {
                        using (iText.Kernel.Pdf.PdfDocument pdfDoc = new iText.Kernel.Pdf.PdfDocument(pdfReader))
                        {
                            using (iText.Kernel.Pdf.PdfDocument pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfWriter))
                            {
                                for (int p = 0; p < pageCount; p++)
                                {
                                    pdfDoc.CopyPagesTo(1, 1, pdfDocument);
                                }
                            }
                        }
                    }
                }

                System.IO.File.Delete(selectedFolderPath + "\\" + "template.pdf");
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Adding Pages : {ex.Message}");
            }
        }

        public void CreateImagesOfSavedViewpoints()
        {
            try
            {
                int p = 1;

                int i = 1;

                ReporterCommandHandlerPlugin.IncreaseMarkupFontSize();

                double sizeEnhancingFactor = ReporterCommandHandlerPlugin.GetSizeEnhancingFactor();

                foreach (var superSavedViewPoint in _issueViewsToBePlotted.Where(ssvp=>!ssvp.IsPlanView))
                {
                    // Creating Bitmap of the superSavedViewPoint
                    Bitmap bitmap = superSavedViewPoint.GenerateImage(sizeEnhancingFactor);

                    // Creating folder and _path for the image
                    Directory.CreateDirectory(selectedFolderPath + "\\" + reportFileName + "_images");
                    
                    if (superSavedViewPoint.IsPlanView)
                    {
                        superSavedViewPoint.ImagePath = System.IO.Path.Combine(selectedFolderPath + "\\" + reportFileName + "_images", $"Planview {p}" + ".jpeg");
                        p++;
                    }

                    else
                    {
                        superSavedViewPoint.ImagePath = System.IO.Path.Combine(selectedFolderPath + "\\" + reportFileName + "_images", $"Issueview {i}" + ".jpeg");
                        i++;
                    }

                    // Saving the image
                    bitmap.Save(superSavedViewPoint.ImagePath, ImageFormat.Jpeg);

                }

                ReporterCommandHandlerPlugin.DecreaseMarkupFontSize();
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Creating Image : {ex.Message}");
                errorocurred = true;
            }

        }

        private void AddSeverityLegends(PdfContentByte pdfContentByte,  BaseFont baseFont)
        {
            try
            {
                var severity_spaceRect_ulx = pageWidth - slAnnot_ulx;
                var severity_spaceRect_uly = slAnnot_uly;

                var rect_h = 12f;
                var rect_w = 200f;

                pdfContentByte.SetColorFill(BaseColor.BLACK);

                pdfContentByte.SaveState();
                pdfContentByte.BeginText();

                pdfContentByte.SetFontAndSize(baseFont, 10f);
                pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, "SEVERITY LEGEND:", severity_spaceRect_ulx + 5f, severity_spaceRect_uly - 15f, 0);


                pdfContentByte.EndText();
                pdfContentByte.RestoreState();

                if(SettingsView.SeverityList.Count != 0)
                {
                    for(int i = 0; i < SettingsView.SeverityList.Count; i++)
                    {
                        iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(severity_spaceRect_ulx + 5f, severity_spaceRect_uly - 15f - (i + 1) * rect_h, severity_spaceRect_ulx + 5f + rect_w, severity_spaceRect_uly - 15f - i * rect_h)
                        {
                            Border = 15,
                            BackgroundColor = new BaseColor(SettingsView.SeverityList[i].Background_Color),
                        };

                        pdfContentByte.Rectangle(rect);

                        pdfContentByte.SetColorFill(new BaseColor(SettingsView.SeverityList[i].Text_Color));

                        pdfContentByte.SaveState();
                        pdfContentByte.BeginText();

                        pdfContentByte.SetFontAndSize(baseFont, 8f);
                        pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, SettingsView.SeverityList[i].Type, rect.Left, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                        pdfContentByte.EndText();
                        pdfContentByte.RestoreState();
                    }
                }

                else if(File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt"))
                {
                    // Specifying the file path for getting previously saved severity list
                    string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt";

                    // Reading the JSON string from the file
                    string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                    // Deserializing the JSON string back to a list of objects
                    List<Severity> deserializedSeverityList = JsonConvert.DeserializeObject<List<Severity>>(jsonStringFromFile, new JsonSerializerSettings
                    {
                        Converters = { new ColorConverter() }
                    });

                    for (int i = 0; i < deserializedSeverityList.Count; i++)
                    {
                        iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(severity_spaceRect_ulx + 5f, severity_spaceRect_uly - 15f - (i + 1) * rect_h, severity_spaceRect_ulx + 5f + rect_w, severity_spaceRect_uly - 15f - i * rect_h)
                        {
                            Border = 15,
                            BackgroundColor = new BaseColor(deserializedSeverityList[i].Background_Color),
                        };

                        pdfContentByte.Rectangle(rect);

                        pdfContentByte.SetColorFill(new BaseColor(deserializedSeverityList[i].Text_Color));

                        pdfContentByte.SaveState();
                        pdfContentByte.BeginText();

                        pdfContentByte.SetFontAndSize(baseFont, 8f);
                        pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, deserializedSeverityList[i].Type, rect.Left, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                        pdfContentByte.EndText();
                        pdfContentByte.RestoreState();
                    }
                }
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Adding Severity Legends : {ex.Message}");
            }
        }

        private void AddTradeLegends(PdfContentByte pdfContentByte) 
        {
            try
            {
                var trade_spaceRect_ulx = pageWidth - tlAnnot_ulx;
                var trade_spaceRect_uly = tlAnnot_uly;

                var rect_margin = 4f;
                var rect_h = 12f;

                pdfContentByte.SetColorFill(BaseColor.BLACK);

                pdfContentByte.SaveState();
                pdfContentByte.BeginText();

                pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 12f);
                pdfContentByte.ShowTextAligned(Element.ALIGN_LEFT, "TRADE LEGEND:", trade_spaceRect_ulx + 10f, trade_spaceRect_uly - 15f, 0);


                pdfContentByte.EndText();
                pdfContentByte.RestoreState();

                if (SettingsView.TradeList.Count != 0)
                {
                    if (SettingsView.TradeList.Count <= 5)
                    {
                        for (int i = 0; i < SettingsView.TradeList.Count; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - (i + 1) * rect_h, trade_spaceRect_ulx + rect_margin + 60f, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - i * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(SettingsView.TradeList[i].Border_Color),
                                BorderWidth = 1,
                            };

                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, SettingsView.TradeList[i].Name, (rect.Left + rect.Right) / 2f, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }
                    }

                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - (i + 1) * rect_h, trade_spaceRect_ulx + rect_margin + 45f, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - i * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(SettingsView.TradeList[i].Border_Color),
                                BorderWidth = 1,
                            };

                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, SettingsView.TradeList[i].Name, (rect.Left + rect.Right) / 2f, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }

                        for (int i = 5; i < SettingsView.TradeList.Count; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin + (tlAnnot_ulx - tlAnnot_lrx) / 2, trade_spaceRect_uly - 15f - (i + 1 - 5) * rect_margin - (i + 1 - 5) * rect_h, trade_spaceRect_ulx + rect_margin + 45f + (tlAnnot_ulx - tlAnnot_lrx) / 2, trade_spaceRect_uly - 15f - (i + 1 - 5) * rect_margin - (i - 5) * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(SettingsView.TradeList[i].Border_Color),
                                BorderWidth = 1,
                            };
                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, SettingsView.TradeList[i].Name, (rect.Left + rect.Right) / 2f, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }
                    }
                }

                else if(File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt"))
                {
                    // Specifying the file path for getting previously saved trade list
                    string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt";

                    // Reading the JSON string from the file
                    string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                    // Deserializing the JSON string back to a list of objects
                    List<Trade> deserializedTradeList = JsonConvert.DeserializeObject<List<Trade>>(jsonStringFromFile, new JsonSerializerSettings
                    {
                        Converters = { new Reporter_vCLabs.ColorConverter() }
                    });

                    if (deserializedTradeList.Count <= 5)
                    {
                        for (int i = 0; i < deserializedTradeList.Count; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - (i + 1) * rect_h, trade_spaceRect_ulx + rect_margin + 60f, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - i * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(deserializedTradeList[i].Border_Color),
                                BorderWidth = 1,
                            };

                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize( BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, deserializedTradeList[i].Name, (rect.Left + rect.Right)/2f , (rect.Bottom + rect.Top)/2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }
                    }

                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - (i + 1) * rect_h, trade_spaceRect_ulx + rect_margin + 45f, trade_spaceRect_uly - 15f - (i + 1) * rect_margin - i * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(deserializedTradeList[i].Border_Color),
                                BorderWidth = 1,
                            };

                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, deserializedTradeList[i].Name, (rect.Left + rect.Right) / 2f, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }

                        for (int i = 5; i < deserializedTradeList.Count; i++)
                        {
                            iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(trade_spaceRect_ulx + rect_margin + (tlAnnot_ulx - tlAnnot_lrx) / 2, trade_spaceRect_uly - 15f - (i + 1 - 5) * rect_margin - (i + 1 - 5) * rect_h, trade_spaceRect_ulx + rect_margin + 45f + (tlAnnot_ulx - tlAnnot_lrx) / 2, trade_spaceRect_uly - 15f - (i + 1 - 5) * rect_margin - (i - 5) * rect_h)
                            {
                                Border = 15,
                                BackgroundColor = new BaseColor(System.Drawing.Color.Transparent),
                                BorderColor = new BaseColor(deserializedTradeList[i].Border_Color),
                                BorderWidth = 1,
                            };

                            pdfContentByte.Rectangle(rect);

                            pdfContentByte.SetColorFill(BaseColor.BLACK);

                            pdfContentByte.SaveState();
                            pdfContentByte.BeginText();

                            pdfContentByte.SetFontAndSize(BaseFont.CreateFont(), 8f);
                            pdfContentByte.ShowTextAligned(Element.ALIGN_CENTER, deserializedTradeList[i].Name, (rect.Left + rect.Right) / 2f, (rect.Bottom + rect.Top) / 2f - 4f, 0);

                            pdfContentByte.EndText();
                            pdfContentByte.RestoreState();
                        }
                    }
                }                                
            } 

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Adding Trade Legends : {ex.Message}");
            }
            
        }

        public void PlotIssuesOnPlanView()
        {
            try
            {
                // GenerateReportView a PdfReader to open the existing PDF
                PdfReader reader = new PdfReader(selectedFolderPath + "\\" + "reportPDF_noPlot.pdf");

                // GenerateReportView a PdfStamper to modify the PDF
                PdfStamper stamper = new PdfStamper(reader, new FileStream(selectedFolderPath + "\\" + reportFileName + ".pdf", FileMode.OpenOrCreate));

                int planViewPageNo = 1;

                foreach (var planView in SuperSavedViewPoints.Where(item => item.IsPlanView))
                {
                    // Getting the rotation of plan view along z axis
                    var rotation = planView.SavedViewpoint.Viewpoint.Rotation.ToString().Trim('(', ')').Replace(" ", "").Split(',');
                    double rot_angle = -Convert.ToDouble(rotation[2]) / (180d / Math.PI);

                    // Getting viewpoint data of the planv view
                    var planView_viewPointCameraJson = planView.SavedViewpoint.Viewpoint.GetCamera();
                    var planViewCameraJSONdata = JObject.Parse(planView_viewPointCameraJson);

                    // Getting X and Y coordinates of the planview's position
                    float planViewX = float.Parse((planViewCameraJSONdata["Position"][0]).ToString());
                    float planViewY = float.Parse((planViewCameraJSONdata["Position"][1]).ToString());

                    // Setting X and Y coordinate of the centre of plan view's image in pdf
                    float plan_centre_X = pageWidth - annot_ulx + (annot_ulx - annot_lrx) / 2;
                    float plan_centre_Y = annot_lry + 25 + (imageHeight * (annot_ulx - annot_lrx) / imageWidth) / 2;

                    // Getting horizontal and vertical extent of plan view
                    float planViewHorizontalExtent = float.Parse((planViewCameraJSONdata["HorizontalExtent"]).ToString());
                    float planViewVerticalExtent = float.Parse((planViewCameraJSONdata["VerticalExtent"]).ToString());

                    // Setting Reduction Factor
                    float reductionFactor_X = (annot_ulx - annot_lrx) / planViewHorizontalExtent;
                    float reductionFactor_Y = (imageHeight * (annot_ulx - annot_lrx) / imageWidth) / planViewVerticalExtent;

                    foreach (var issueView in SuperSavedViewPoints.Where(item => !item.IsPlanView))
                    {
                        var svpInfo = issueView.SavedViewpoint.DisplayName.Split('.')[0];

                        // Getting X and Y coordinates of the issue's position
                        float issueViewX = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.X.ToString());
                        float issueViewY = float.Parse(issueView.SavedViewpoint.Viewpoint.Position.Y.ToString());

                        // Setting X and Y coordinate of the issue's position in pdf
                        float issue_X = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Cos(rot_angle) - (issueViewY - planViewY) * reductionFactor_Y * Math.Sin(rot_angle));
                        float issue_Y = (float)((issueViewX - planViewX) * reductionFactor_X * Math.Sin(rot_angle) + (issueViewY - planViewY) * reductionFactor_Y * Math.Cos(rot_angle));

                        // Setting Coordinates for the hyperlink rectangle 
                        float llx = plan_centre_X + issue_X - 5;
                        float lly = plan_centre_Y + issue_Y - 5;
                        float urx = plan_centre_X + issue_X + 5;
                        float ury = plan_centre_Y + issue_Y + 5;

                        if ((llx + urx) / 2f < (pageWidth - annot_ulx) || (llx + urx) / 2f > (pageWidth - annot_lrx) || (lly + ury) / 2f < annot_lry || (lly + ury) / 2f > annot_uly)
                        {
                            continue;
                        }

                        // Setting go to 'pageAsPdfDictionary No' for the hyperlink acttion
                        int pageNo = planViewPageNo + 1 + (Convert.ToInt32(svpInfo) - 1) / NumberOfImagesPerPage;

                        // Creating a PdfAction for hyperlink
                        PdfAction pdfAction = iTextSharp.text.pdf.PdfAction.GotoLocalPage(pageNo, new PdfDestination(iTextSharp.text.pdf.PdfDestination.FIT), stamper.Writer);

                        // Creating a PdfAnnotation (link annotation) for the hyperlink
                        PdfAnnotation linkAnnotation = iTextSharp.text.pdf.PdfAnnotation.CreateLink(stamper.Writer, new iTextSharp.text.Rectangle(llx, lly, urx, ury), iTextSharp.text.pdf.PdfAnnotation.HIGHLIGHT_INVERT, pdfAction);

                        // Border_Color
                        foreach(var Severity in SettingsView.SeverityList)
                        {
                            if(issueView.Severity == Severity.Type) 
                            {
                                linkAnnotation.MKBorderColor = new BaseColor(Severity.Text_Color);
                                linkAnnotation.MKBackgroundColor = new BaseColor(Severity.Background_Color);
                            }

                            else
                            {
                                linkAnnotation.MKBorderColor = BaseColor.BLACK;
                                linkAnnotation.MKBackgroundColor = BaseColor.WHITE;
                            }
                        }

                        // Border style
                        linkAnnotation.Border = new PdfBorderArray(0, 0, 1);

                        // Adding the link annotation to the first pageAsPdfDictionary of the PDF 
                        stamper.AddAnnotation(linkAnnotation, planViewPageNo);
                    }

                    planViewPageNo++;
                }

                // Closing the PdfStamper
                stamper.Close();

                // Closing the PdfReader
                reader.Close();

                System.IO.File.Delete(selectedFolderPath + "\\" + "reportPDF_noPlot.pdf");
            }

            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Plotting Issues : {ex.Message}");
                errorocurred = true;
            }


        }

        public void Classify_SeverityAndTrade(PdfContentByte pdfContentByte, iTextSharp.text.Rectangle issueDescriptionRect, string trade, string severity)
        {

            try
            {
                //For Severity
                {
                    if(SettingsView.SeverityList.Count != 0)
                    {
                        if(SettingsView.SeverityList.Exists(s => s.Type.Equals(severity)))
                        {
                            var Severity = SettingsView.SeverityList.Find(s => s.Type.Equals(severity));

                            pdfContentByte.SetColorFill(new BaseColor(Severity.Text_Color));
                            issueDescriptionRect.BackgroundColor = new BaseColor(Severity.Background_Color);
                        }

                        else
                        {
                            pdfContentByte.SetColorFill(BaseColor.BLACK);
                            issueDescriptionRect.BackgroundColor = BaseColor.WHITE;
                        }
                    }

                    else
                    {
                        if(File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt"))
                        {
                            // Specifying the file path for getting previously saved severity list
                            string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\severityList.txt";

                            // Reading the JSON string from the file
                            string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                            // Deserializing the JSON string back to a list of objects
                            List<Severity> deserializedSeverityList = JsonConvert.DeserializeObject<List<Severity>>(jsonStringFromFile, new JsonSerializerSettings
                            {
                                Converters = { new ColorConverter() }
                            });

                            if (deserializedSeverityList.Exists(s => s.Type.Equals(severity)))
                            {
                                var Severity = deserializedSeverityList.Find(s => s.Type.Equals(severity));

                                pdfContentByte.SetColorFill(new BaseColor(Severity.Text_Color));
                                issueDescriptionRect.BackgroundColor = new BaseColor(Severity.Background_Color);
                            }

                            else
                            {
                                pdfContentByte.SetColorFill(BaseColor.BLACK);
                                issueDescriptionRect.BackgroundColor = BaseColor.WHITE;
                            }
                        }

                        else
                        {
                            pdfContentByte.SetColorFill(BaseColor.BLACK);
                            issueDescriptionRect.BackgroundColor = BaseColor.WHITE;
                        }

                        
                    }
                }

                //For Trade
                {
                    issueDescriptionRect.Border = 15;

                    if(SettingsView.TradeList.Count != 0)
                    {

                        if(SettingsView.TradeList.Exists(t => t.Name.Equals(trade)))
                        {
                            var Trade = SettingsView.TradeList.Find(t => t.Name.Equals(trade));

                            issueDescriptionRect.BorderWidth = Trade.Border_Thickness;
                            issueDescriptionRect.BorderColor = new BaseColor(Trade.Border_Color);
                        }

                        else
                        {
                            issueDescriptionRect.BorderWidth = 1;
                            issueDescriptionRect.BorderColor = BaseColor.BLACK;
                        }
                    }

                    else
                    {
                        if(File.Exists(@"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt"))
                        {
                            // Specifying the file path for getting previously saved trade list
                            string filePath = @"C:\ProgramData\Autodesk\Navisworks Manage 2023\tradeList.txt";

                            // Reading the JSON string from the file
                            string jsonStringFromFile = System.IO.File.ReadAllText(filePath);

                            // Deserializing the JSON string back to a list of objects
                            List<Trade> deserializedTradeList = JsonConvert.DeserializeObject<List<Trade>>(jsonStringFromFile, new JsonSerializerSettings
                            {
                                Converters = { new Reporter_vCLabs.ColorConverter() }
                            });

                            if (deserializedTradeList.Exists(t => t.Name.Equals(trade)))
                            {
                                var Trade = deserializedTradeList.Find(t => t.Name.Equals(trade));

                                issueDescriptionRect.BorderWidth = Trade.Border_Thickness;
                                issueDescriptionRect.BorderColor = new BaseColor(Trade.Border_Color);
                            }

                            else
                            {
                                issueDescriptionRect.BorderWidth = 1;
                                issueDescriptionRect.BorderColor = BaseColor.BLACK;
                            }
                        }

                        else
                        {
                            issueDescriptionRect.BorderWidth = 1;
                            issueDescriptionRect.BorderColor = BaseColor.BLACK;
                        }

                    }

                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error Classifying Severity and Trade : {ex.Message}");
                errorocurred = true;
            }

            

        }

        private int GetNumberOfPages()
        {
            int pagecount = 0;

            // Getting the pagecount
            switch (numberOfImagesPerPage)
            {
                case 1:
                    {
                        pagecount = _issueViewsToBePlotted.Count + 1;
                    }
                    break;

                case 4:
                    {
                        //pagecount = _issueViewsToBePlotted.Count(item => item.IsPlanView) + (_issueViewsToBePlotted.Count(item => !item.IsPlanView) + 3) / 4;
                        pagecount = (_issueViewsToBePlotted.Count + 3) / 4 + 1;
                    }
                    break;

                case 6:
                    {
                        //pagecount = _issueViewsToBePlotted.Count(item => item.IsPlanView) + (_issueViewsToBePlotted.Count(item => !item.IsPlanView) + 5) / 6;
                        pagecount = (_issueViewsToBePlotted.Count + 5) / 6 + 1;
                    }
                    break;

            }

            return pagecount;
        }

        private void PlotIssues()
        {

        }
        

        
    }
}
