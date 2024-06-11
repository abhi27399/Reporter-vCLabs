using Autodesk.Navisworks.Api;
using Newtonsoft.Json;
using Reporter_vCLabs.Model;
using System.Drawing;
using System.Windows.Documents;

namespace Reporter_vCLabs
{
    public static class SavedViewpointExtensions
    {
        public static Bitmap GenerateImage(this SavedViewpoint savedViewpoint, double sizeEnhancingFactor)
        {
            Document document = Application.ActiveDocument;
            document.SavedViewpoints.CurrentSavedViewpoint = savedViewpoint;
            View view = document.ActiveView;

            string redlineCollectionJSON = view.GetRedlines();

            Redlines redlines = JsonConvert.DeserializeObject<Redlines>(redlineCollectionJSON);

            for (int i = 0; i < redlines.Values.Length; i++)
            {
                dynamic redline = redlines.Values[i];
                redline.Thickness = redline.Thickness * sizeEnhancingFactor;
            }

            string modiefiedRedlineCollectionJSON = JsonConvert.SerializeObject(redlines);
            view.SetRedlines(modiefiedRedlineCollectionJSON);

            Bitmap bitmap = view.GenerateImage(ImageGenerationStyle.ScenePlusOverlay, (int)(view.Width * sizeEnhancingFactor), (int)(view.Height * sizeEnhancingFactor));

            view.SetRedlines(redlineCollectionJSON);

            return bitmap;
        }

        public static Bitmap GenerateImage(this SavedViewpoint savedViewpoint)
        {
            return GenerateImage(savedViewpoint, 1d);
        }

        
    }
}
