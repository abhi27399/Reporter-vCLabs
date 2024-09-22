namespace Reporter_vCLabs.Model
{
    public class Redlines
    {
        public string Type { get; set; }
        public int Version { get; set; }
        public object[] Values { get; set; } = { };

        public void EnhanceThickness(double enhancingFactor)
        {
            if (enhancingFactor == 0 || enhancingFactor == 1) 
            {  
                return;             
            }

            for (int i = 0; i < Values.Length; i++)
            {
                dynamic redline = Values[i];
                redline.Thickness = redline.Thickness * enhancingFactor;
            }
        }
    }



}
