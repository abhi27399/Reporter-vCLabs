using Autodesk.Navisworks.Api;
using System.Drawing;
using System.Linq;

namespace Reporter_vCLabs.Model
{
    /// <summary>
    /// A class which has a savedviewpoint instance and its details as properties; and some methods to perform manipulation on it
    /// </summary>
    public class SuperSavedViewPoint
    {
        public SuperSavedViewPoint(SavedViewpoint savedViewpoint)
        {
            _savedViewpoint = savedViewpoint;
            
            _toBePlotted = true;

            if (savedViewpoint.DisplayName[1] == '.' || savedViewpoint.DisplayName[2] == '.' || savedViewpoint.DisplayName[3] == '.')
            {
                _hasSerialNumber = true;
                _serialNumber = savedViewpoint.DisplayName.Split('.')[0];
            }

            var comments = savedViewpoint.Comments.ToList();

            if (comments.Count == 2)
            {
                var trade = comments[0].Body;
                if (!trade.Equals("Trade : NA"))
                {
                    _trade = trade.Split(':').Last().TrimStart();
                }

                else
                {
                    _trade = "Other Trade";
                }

                var severity = comments[1].Body;
                if (!severity.Equals("Severity : NA"))
                {
                    _severity = severity.Split(':').Last().TrimStart();
                }

                else
                {
                    _severity = "Other Severity";
                }
            }

            else
            {
                _trade = "Other Trade";

                _severity = "Other Severity";
            }
            
        }

        private readonly SavedViewpoint _savedViewpoint;       

        private string _imagePath;

        private string _trade;

        private string _severity;

        private bool _isPlanView;

        private bool _toBePlotted;

        private bool _hasSerialNumber;

        private string _serialNumber;

        public SavedViewpoint SavedViewpoint { get => _savedViewpoint;}        
        public string ImagePath { get => _imagePath; set => _imagePath = value; }
        public string Trade { get => _trade; set => _trade = value; }
        public string Severity { get => _severity; set => _severity = value; }
        public bool IsPlanView { get => _isPlanView; set => _isPlanView = value; }
        public bool ToBePlotted { get => _toBePlotted; set => _toBePlotted = value; }
        public bool HasSerialNumber { get => _hasSerialNumber; set => _hasSerialNumber = value; }
        public string SerialNumber { get => _serialNumber; set => _serialNumber = value; }


        public Bitmap GenerateImage(double sizeEnhancingFactor)
        {
            return _savedViewpoint.GenerateImage(sizeEnhancingFactor);
        }

        public Bitmap GenerateImage()
        {
            return _savedViewpoint.GenerateImage();
        }
    }

    
}
