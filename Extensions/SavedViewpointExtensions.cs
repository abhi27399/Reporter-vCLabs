using Autodesk.Navisworks.Api;
using Newtonsoft.Json;
using Reporter_vCLabs.Model;
using System.Drawing;

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

            redlines.EnhanceThickness(sizeEnhancingFactor);

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
