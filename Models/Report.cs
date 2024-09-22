using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using Reporter_vCLabs.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using Image = iText.Layout.Element.Image;
using Path = System.IO.Path;
using System.Windows;
using iText.Layout;

namespace Reporter_vCLabs.Model
{
    public class Report
    {
        private readonly string filepath;

        private readonly string templatePath;

        private readonly int numberOfImagesPerPage;

        private readonly List<SuperSavedViewPoint> superSavedViewPoints;

        private int pageRotation;

        private float pageWidth;

        private float pageHeight;

        private PdfAnnotation[] spacePdfAnnotations;

        private Rect imageSpaceRect;

        private Rect tradeLegendSpaceRect;

        private Rect severityLegendSpaceRect;

        public Report(string filepath, string templatePath, int numberOfImagesPerPage, List<SuperSavedViewPoint> superSavedViewPoints)
        {
            this.filepath = filepath;
            this.templatePath = templatePath;
            this.numberOfImagesPerPage = numberOfImagesPerPage;
            this.superSavedViewPoints = superSavedViewPoints;

            

        }

        public void Generate()
        {
            try
            {
                FileStream outputFileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write);
                PdfWriter reportPdfWriter = new PdfWriter(outputFileStream);
                PdfDocument reportPdfDocument = new PdfDocument(reportPdfWriter);
                Document reportDocument = new iText.Layout.Document(reportPdfDocument);

                FileStream inputfileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                PdfReader templatePdfReader = new PdfReader(inputfileStream);
                PdfDocument templatePdfDocument = new PdfDocument(templatePdfReader);


                ReadTemplate(templatePdfDocument);

                CopyTemplateToReport(templatePdfDocument, reportPdfDocument);

                GenerateImages();

                

                

                ImageData imageData = ImageDataFactory.Create(@"C:\Users\abhinavc\OneDrive - DPR Construction\Desktop\001.00 ST vs PL_T0_L01_IBOS_01 Electrical Room_E-19_ [Typical Issues] - Overview.jpeg");
                Image image = new Image(imageData);


                image.ScaleAbsolute(200f, 150f);
                image.SetFixedPosition(3, imageSpaceRect.Llx, imageSpaceRect.Lly);

                reportDocument.Add(image);

                reportDocument.Close();
            }

            catch(Exception e)
            {
                e.Log("Report", "Generate");
                MessageBox.Show("Error Generating Report");
            }
        }


        private void ReadTemplate(PdfDocument templatePdfDocument)
        {
            PdfPage templatePage = templatePdfDocument.GetFirstPage();
            pageRotation = templatePage.GetRotation();

            iText.Kernel.Geom.Rectangle templatePageRect = templatePage.GetPageSizeWithRotation();
            pageWidth = templatePageRect.GetWidth();
            pageHeight = templatePageRect.GetHeight();

            PdfAnnotation[] pdfAnnotations = GetPdfAnnotations(templatePage, subType: PdfName.Square);

            if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
            {
                spacePdfAnnotations = new PdfAnnotation[3] { pdfAnnotations[2], pdfAnnotations[1], pdfAnnotations[0] };
                Rect[] rects = GetRects(spacePdfAnnotations);
                imageSpaceRect = rects[0];
                tradeLegendSpaceRect = rects[1];
                severityLegendSpaceRect = rects[2];
            }

            else if (SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && !SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
            {
                spacePdfAnnotations = new PdfAnnotation[2] { pdfAnnotations[1], pdfAnnotations[0] };
                Rect[] rects = GetRects(spacePdfAnnotations);
                imageSpaceRect = rects[0];
                tradeLegendSpaceRect = rects[1];
            }

            else if (!SelectTemplateViewModel.IsDefineSpaceForTradeLegends_CheckBox_Checked && SelectTemplateViewModel.IsDefineSpaceForSeverityLegends_CheckBox_Checked)
            {
                spacePdfAnnotations = new PdfAnnotation[2] { pdfAnnotations[1], pdfAnnotations[0] };
                Rect[] rects = GetRects(spacePdfAnnotations);
                imageSpaceRect = rects[0];
                severityLegendSpaceRect = rects[1];
            }

            else
            {
                spacePdfAnnotations = new PdfAnnotation[1] { pdfAnnotations[1] };
                Rect[] rects = GetRects(pdfAnnotations[1]);
                imageSpaceRect = rects[0];
            }

            
        }

        private void CopyTemplateToReport(PdfDocument templatePdf, PdfDocument reportPdf)
        {
            PdfPage templatePage = templatePdf.GetFirstPage();

            foreach(PdfAnnotation spacePdfAnnotation in spacePdfAnnotations)
            {
                templatePage.RemoveAnnotation(spacePdfAnnotation);
            }

            for (int i = 0; i < GetNumberOfPages(); i++)
            {
                templatePdf.CopyPagesTo(1, 1, reportPdf).First().SetIgnorePageRotationForContent(true);
            }
        }

        private PdfAnnotation[] GetPdfAnnotations(PdfPage pdfPage, PdfName subType)
        {
            IList<PdfAnnotation> pdfAnnotations = pdfPage.GetAnnotations();

            SortedDictionary<DateTime, PdfAnnotation> dateTimeAndPdfAnnotationsDictionary = new SortedDictionary<DateTime, PdfAnnotation>();

            foreach (PdfAnnotation pdfAnnotation in pdfAnnotations)
            {
                if (pdfAnnotation.GetSubtype().Equals(subType))
                {
                    string creationDate = pdfAnnotation.GetPdfObject().GetAsString(PdfName.CreationDate).ToString();
                    
                    var cdtSplit = creationDate.Split(':', '+');

                    var cdtYear = Convert.ToInt32(cdtSplit[1].Substring(0, 4));
                    var cdtMonth = Convert.ToInt32(cdtSplit[1].Substring(4, 2));
                    var cdtDay = Convert.ToInt32(cdtSplit[1].Substring(6, 2));
                    var cdtHH = Convert.ToInt32(cdtSplit[1].Substring(8, 2));
                    var cdtMM = Convert.ToInt32(cdtSplit[1].Substring(10, 2));
                    var cdtSS = Convert.ToInt32(cdtSplit[1].Substring(12, 2));

                    DateTime creationDateTime = new DateTime(cdtYear, cdtMonth, cdtDay, cdtHH, cdtMM, cdtSS);

                    dateTimeAndPdfAnnotationsDictionary.Add(creationDateTime, pdfAnnotation);


                }

            }

            PdfAnnotation[] pdfAnnotationsArray = new PdfAnnotation[dateTimeAndPdfAnnotationsDictionary.Count];

            for (int i = 0; i < pdfAnnotationsArray.Length; i++)
            {
                pdfAnnotationsArray[i] = dateTimeAndPdfAnnotationsDictionary.ElementAt(dateTimeAndPdfAnnotationsDictionary.Count - (i + 1)).Value;
            }

            return pdfAnnotationsArray;
        }

        private Rect[] GetRects(params PdfAnnotation[] pdfAnnotations)
        {
            Rect[] rects = new Rect[pdfAnnotations.Length];

            for(int i = 0; i < pdfAnnotations.Length; i++)
            {
                if (pdfAnnotations[i] == null)
                {
                    rects[i] = null;
                    continue;
                }

                float[] rectArray = pdfAnnotations[i].GetRectangle().ToFloatArray();

                Rect rect = new Rect();

                switch(pageRotation)
                {
                    case 0:

                        rect.Llx = rectArray[0];
                        rect.Lly = rectArray[1];
                        rect.Urx = rectArray[2];
                        rect.Ury = rectArray[3];

                        break;

                    case 90:

                        rect.Llx = rectArray[1];
                        rect.Lly = pageHeight - rectArray[2];
                        rect.Urx = rectArray[3];
                        rect.Ury = pageHeight - rectArray[0];

                        break;

                    case 180:

                        rect.Llx = pageWidth - rectArray[2];
                        rect.Lly = pageHeight - rectArray[3];
                        rect.Urx = pageWidth - rectArray[0];
                        rect.Ury = pageHeight - rectArray[1];

                        break;

                    case 270:

                        rect.Llx = pageWidth - rectArray[3];
                        rect.Lly = rectArray[0];
                        rect.Urx = pageWidth - rectArray[1];
                        rect.Ury = rectArray[2];

                        break;

                }

                rects[i] = rect;
            }

            return rects;
        }

        private void GenerateImages()
        {
            try
            {
                int p = 1;

                int i = 1;

                ReporterCommandHandlerPlugin.IncreaseMarkupFontSize();

                double sizeEnhancingFactor = ReporterCommandHandlerPlugin.GetSizeEnhancingFactor();

                // Creating folder and _path for the image

                string imagesFolderPath = Path.Combine(Path.GetDirectoryName(filepath), $"{Path.GetFileNameWithoutExtension(filepath)}_images");
                Directory.CreateDirectory(imagesFolderPath);

                foreach (var superSavedViewPoint in superSavedViewPoints)
                {
                    // Creating Bitmap of the superSavedViewPoint
                    Bitmap bitmap = superSavedViewPoint.GenerateImage(sizeEnhancingFactor);

                    if (superSavedViewPoint.IsPlanView)
                    {
                        superSavedViewPoint.ImagePath = System.IO.Path.Combine(imagesFolderPath, $"Planview {p}" + ".jpeg");
                        p++;
                    }

                    else
                    {
                        superSavedViewPoint.ImagePath = System.IO.Path.Combine(imagesFolderPath, $"Issueview {i}" + ".jpeg");
                        i++;
                    }

                    // Saving the image
                    bitmap.Save(superSavedViewPoint.ImagePath, ImageFormat.Jpeg);

                }

                ReporterCommandHandlerPlugin.DecreaseMarkupFontSize();
            }

            catch(Exception e)
            {
                e.Log("Report", "GenerateImages");
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
                        pagecount = superSavedViewPoints.Count;
                    }
                    break;

                case 4:
                    {
                        pagecount = superSavedViewPoints.Count(item => item.IsPlanView) + (superSavedViewPoints.Count(item => !item.IsPlanView) + 3) / 4;
                    }
                    break;

                case 6:
                    {
                        pagecount = superSavedViewPoints.Count(item => item.IsPlanView) + (superSavedViewPoints.Count(item => !item.IsPlanView) + 5) / 6;
                    }
                    break;

            }

            return pagecount;
        }

        private void WriteReport(PdfDocument reportPdfDocument)
        {
            Document reportDocument = new Document(reportPdfDocument);
        }

        private void AddPlanViewAndDescription(Document reportDocument)
        {
            int pageNo = 0;

            foreach (SuperSavedViewPoint superSavedViewPoint in superSavedViewPoints.Where(item => item.IsPlanView))
            {

            }
        }

        private void AddIssueViewAndDescription(Document reportDocument)
        {

        }


        public void Show()
        {
            Process.Start(filepath);
        }
        


    }

    

}


