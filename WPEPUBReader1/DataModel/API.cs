using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

//https://dev-bookeditor.bookscriptor.ru/node/api-gm/user/auth
//login 4myself@4myself.ru
//password 4myself
namespace WPEPUBReader1.DataModel
{
    class API
    {
        private HttpClient _httpClient;
        private HttpResponseMessage _response;
        private string _username = "4myself@4myself.ru";
        private string _password = "4myself";
        //private string _url = "https://dev-bookeditor.bookscriptor.ru/node/api-gm/user/auth";

        public async Task TestingAPI(string _url)
        {
            _httpClient = new HttpClient();

            // Add a user-agent header
            var headers = _httpClient.DefaultRequestHeaders;

            // HttpProductInfoHeaderValueCollection is a collection of 
            // HttpProductInfoHeaderValue items used for the user-agent header

            // The safe way to check a header value from the user is the TryParseAdd method
            // Since we know this header is okay, we use ParseAdd with will throw an exception
            // with a bad value 
            headers.UserAgent.ParseAdd("ie");
            headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            headers.Authorization = CreateBasicHeader(_username, _password);

            _response = new HttpResponseMessage();
            Uri resourceUri;
            if (!Uri.TryCreate(_url.Trim(), UriKind.Absolute, out resourceUri))
            {
                //Invalid URI
                return;
            }
            
            string responseBodyAsText;
            try
            {
                _response = await _httpClient.GetAsync(resourceUri);
                _response.EnsureSuccessStatusCode();
                responseBodyAsText = await _response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                // Need to convert int HResult to hex string
                var _resultStr = ex.HResult.ToString("X");
                Debug.WriteLine($"----\nError = {_resultStr}\nMessage: {ex.Message}\n----");
                responseBodyAsText = "";
            }
            Debug.WriteLine($"\n-----\nResponse code: {_response.StatusCode} : {_response.ReasonPhrase}\n----\n");

            // Format the HTTP response to display better
            responseBodyAsText = responseBodyAsText.Replace("<br>", Environment.NewLine);
            Debug.WriteLine($"{responseBodyAsText}");
        }

        public HttpCredentialsHeaderValue CreateBasicHeader(string username, string password)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(username + ":" + password);
            return new HttpCredentialsHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }
    }
   

}
