using Autodesk.Navisworks.Api;
using Reporter_vCLabs.Model;
using System.Collections.Generic;

namespace Reporter_vCLabs
{
    public static class DocumentExtensions
    {
        public static List<SavedViewpoint> GetSavedViewpoints(this Document document)
        {
            SavedItemCollection savedItems = document.SavedViewpoints.Value;

            return GetSavedViewpointsFrom(savedItems);
        }

        private static List<SavedViewpoint> GetSavedViewpointsFrom(SavedItemCollection savedItems)
        {
            List<SavedViewpoint> savedViewpoints = new List<SavedViewpoint>();

            foreach (var savedItem in savedItems)
            {
                if (savedItem.IsGroup == false)
                {
                    savedViewpoints.Add(savedItem as SavedViewpoint);
                }

                else
                {
                    var childrenCollection = (savedItem as GroupItem).Children;
                    GetSavedViewpointsFrom(childrenCollection);
                }
            }

            return savedViewpoints;
        }

        public static List<SuperSavedViewPoint> GetSuperSavedViewPoints(this Document document)
        {
            SavedItemCollection savedItems = document.SavedViewpoints;

            return GetSuperSavedViewPoints(savedItems);
        }

        private static List<SuperSavedViewPoint> GetSuperSavedViewPoints(SavedItemCollection savedItems)
        {
            List<SuperSavedViewPoint> superSavedViewPoints = new List<SuperSavedViewPoint>();

            foreach (var item in savedItems)
            {
                if (item.IsGroup == false)
                {
                    SuperSavedViewPoint superSavedViewPoint = new SuperSavedViewPoint(item as SavedViewpoint);
                    superSavedViewPoints.Add(superSavedViewPoint);
                }

                else
                {
                    var childrenItemCollection = ((GroupItem)item).Children;
                    GetSuperSavedViewPoints(childrenItemCollection);
                }

            }

            return superSavedViewPoints;
        }
    }
}
