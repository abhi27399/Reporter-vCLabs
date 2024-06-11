using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Reporter_vCLabs.Model
{
    public class Report
    {
        private readonly string filepath;

        private readonly string templatePath;

        private readonly int numberOfImagesPerPage;

        private readonly List<SuperSavedViewPoint> superSavedViewPoints;

        public Report(string filepath, string templatePath, int numberOfImagesPerPage, List<SuperSavedViewPoint> superSavedViewPoints)
        {
            this.filepath = filepath;
            this.templatePath = templatePath;
            this.numberOfImagesPerPage = numberOfImagesPerPage;
            this.superSavedViewPoints = superSavedViewPoints;
        }

        public void Generate()
        {
            FileStream outputFileStream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.Write);
            PdfWriter reportPdfWriter = new PdfWriter(outputFileStream);
            PdfDocument reportPdfDocument = new PdfDocument(reportPdfWriter);
            Document reportDocument = new Document(reportPdfDocument);

            FileStream inputfileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            PdfReader templatePdfReader = new PdfReader(inputfileStream);
            PdfDocument templatePdfDocument = new PdfDocument(templatePdfReader);


            PdfPage templatePage = templatePdfDocument.GetPage(1);
            iText.Kernel.Geom.Rectangle templatePageRect = templatePage.GetPageSizeWithRotation();
            int templatePageRotation = templatePage.GetRotation();

            IList<PdfAnnotation> pdfAnnotations = templatePage.GetAnnotations();

            SortedDictionary<DateTime, PdfAnnotation> dateTimeDictionary = new SortedDictionary<DateTime, PdfAnnotation>();

            foreach (PdfAnnotation pdfAnnotation in pdfAnnotations)
            {
                if (pdfAnnotation.GetSubtype().Equals(PdfName.Square))
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

                    dateTimeDictionary.Add(creationDateTime, pdfAnnotation);


                }

            }

            float[] imageSpaceRect = new float[] { };
            float[] tradeLegendSpaceRect = new float[] { };
            float[] severityLegendsRect = new float[] { };

            if (1f.Equals(1f))
            {
                PdfAnnotation imageSpaceAnnotation = dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 3).Value;
                PdfAnnotation tradeLegendSpaceAnnotation = dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 2).Value;
                PdfAnnotation severityLegendsAnnotation = dateTimeDictionary.ElementAt(dateTimeDictionary.Count - 1).Value;

                imageSpaceRect = imageSpaceAnnotation.GetRectangle().ToFloatArray();
                tradeLegendSpaceRect = tradeLegendSpaceAnnotation.GetRectangle().ToFloatArray();
                severityLegendsRect = severityLegendsAnnotation.GetRectangle().ToFloatArray();

                templatePage.RemoveAnnotation(imageSpaceAnnotation);
                templatePage.RemoveAnnotation(tradeLegendSpaceAnnotation);
                templatePage.RemoveAnnotation(severityLegendsAnnotation);
            }



            for (int i = 0; i < GetNumberOfPages(); i++)
            {
                templatePdfDocument.CopyPagesTo(1, 1, reportPdfDocument).First().SetIgnorePageRotationForContent(true);
            }



            float imageSpaceRect_left = 0;
            float imageSpaceRect_bottom = 0;
            float imageSpaceRect_right = 0;
            float imageSpaceRect_top = 0;

            if (templatePageRotation.Equals(0))
            {
                imageSpaceRect_left = imageSpaceRect[0];
                imageSpaceRect_bottom = imageSpaceRect[1];
                imageSpaceRect_right = imageSpaceRect[2];
                imageSpaceRect_top = imageSpaceRect[3];
            }

            else if (templatePageRotation.Equals(90))
            {

            }

            else if (templatePageRotation.Equals(180))
            {

            }

            else if (templatePageRotation.Equals(270))
            {

            }










            ImageData imageData = ImageDataFactory.Create(@"C:\Users\abhinavc\OneDrive - DPR Construction\Desktop\001.00 ST vs PL_T0_L01_IBOS_01 Electrical Room_E-19_ [Typical Issues] - Overview.jpeg");
            Image image = new Image(imageData);


            image.ScaleAbsolute(200f, 150f);
            image.SetFixedPosition(3, imageSpaceRect_left, imageSpaceRect_bottom);

            reportDocument.Add(image);



            reportDocument.Close();





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






        public void Show()
        {
            Process.Start(filepath);

            
        }
        


    }

    

}


