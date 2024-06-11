using Newtonsoft.Json;
using static SSO_Integrator.SSOIntegrator;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Windows;

namespace Reporter_vCLabs
{
    internal class SSO
    {
        private string AppID {  get; set; }

        internal SSO() 
        {
            AppID = "78c79707-cbc1-40c5-9ddc-4b3a0b599f9c";
        }

        public async Task<LoginResponseModel> GetSSoResult()
        {
            try
            {
                var sso = new SSO_Integrator.SSOIntegrator(AppID);
                string res = await sso.Authenticate(SSOtype.SSO_ANY);
                var result = JsonConvert.DeserializeObject<LoginResponseModel>(res);
                return result;
            }

            catch (Exception ex)
            {
                MessageBox.Show("Message: " + ex.Message + "\n" +
                    "Inner Exception: " + ex.InnerException + "\n" +
                    "Source: " + ex.Source);
            }

            return null;
        }
        public async Task<string> Validate_StoreUser(string email, string appID, string accessToken)
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://vclabsapi-staging.azurewebsites.net/api/UserApps/Status/{email}/{appID}");
                //request.Headers.Add("Cookie", "ARRAffinity=898bd46e2ceb0275ff3d804f2919e82488c4a4748acaff73abf8e83b08a5bf9c; ARRAffinitySameSite=898bd46e2ceb0275ff3d804f2919e82488c4a4748acaff73abf8e83b08a5bf9c");
                request.Headers.Add("ApiKey", "431ba60ff7c3469184f8847dd7b09ba0");
                //request.Headers.Add("Authorization", "Bearer ", accessToken);
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                //request.Headers.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",accessToken);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var result1 = JsonConvert.DeserializeObject<StatusResponseModel>(result.ToString());
                if (result1.Status == 2 && result1.ValidUntil > DateTime.Now)
                {
                    return $"Valid User till {result1.ValidUntil}";
                }
                else
                {
                    return null;
                }
            }

            catch(Exception ex)
            {
                
            }

            return null;
            
        }
        public async Task<string> VersionFetch_Store(string email, string appID, string accessToken)
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var request = new HttpRequestMessage(HttpMethod.Get, $"https://vclabsapi-staging.azurewebsites.net/api/AppVersions/CheckVer/{email}/{appID}");
                //request.Headers.Add("Cookie", "ARRAffinity=898bd46e2ceb0275ff3d804f2919e82488c4a4748acaff73abf8e83b08a5bf9c; ARRAffinitySameSite=898bd46e2ceb0275ff3d804f2919e82488c4a4748acaff73abf8e83b08a5bf9c");
                request.Headers.Add("ApiKey", "431ba60ff7c3469184f8847dd7b09ba0");
                //request.Headers.Add("Authorization", "Bearer ", accessToken);
                request.Headers.Add("Authorization", "Bearer " + accessToken);
                //request.Headers.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",accessToken);
                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                //var result1 = JsonConvert.DeserializeObject<VersionResponseModel>(result.ToString());
                //var result1 = JsonConvert.DeserializeObject<VersionFetchStore>(result);
                if (response.StatusCode.ToString() == "OK")
                {
                    return result;
                    //return result1.Version;
                }
                else
                {
                    return null;
                }
            }

            catch(Exception ex)
            {

            }

            return null;
        }

    }
}
