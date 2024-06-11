using System;
using System.Deployment.Application;
using System.Reflection;

namespace Reporter_vCLabs
{
    internal class VersionFetch
    {
        private string _versionNumber = GetRunningVersion().ToString();

        public string VersionNumber
        {
            get { return _versionNumber; }
            set { _versionNumber = value; }
        }

        public static Version GetRunningVersion()
        {
            try
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch (Exception)
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }
    }

    
}
