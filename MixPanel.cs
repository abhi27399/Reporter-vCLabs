using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reporter_vCLabs
{
    internal class MixPanel
    {
        public async Task<HttpResponseMessage> Track(string EventName, string EventData, string AccessToken)
        {
            HttpClient client = new HttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Post, "https://vclabsapi-staging.azurewebsites.net/api/MixPanel?event="
                        + EventName +
                        "&interfaceOptionsVariantdata=" + EventData);

            request.Headers.Add("ApiKey", "431ba60ff7c3469184f8847dd7b09ba0");
            request.Headers.Add("Authorization", "Bearer " + AccessToken);

            HttpResponseMessage responseMessage = null;

            try
            {
                responseMessage = await client.SendAsync(request);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return responseMessage;
        }
    }

    
}
