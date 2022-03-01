using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FintrakBanking.AccessSubsediary
{
    public class SubsediaryHttpHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken)
        {

            string url = HttpContext.Current.Request.CurrentExecutionFilePath;
            string method = HttpContext.Current.Request.HttpMethod;
            var httpClient = new HttpClient();
            var token = HttpContext.Current.Request.Headers["Authorization"];
            string countryCode = HttpContext.Current.Request.Headers["X-COUNTRYCODE"];
            if (countryCode != null && countryCode != "NG")
            {


                if (method == "GET")
                {


                    var absoluteURL = getUrl(countryCode);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", token);
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                    var responseString = await httpClient.GetAsync(remoteURL);
                    var result = await responseString.Content.ReadAsAsync<object>();
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();

                    HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                    tsc.SetResult(message);
                    return tsc.Task.Result;


                }
                else if (method == "POST")
                {
                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        // This will equal to "charset = UTF-8 & param1 = val1 & param2 = val2 & param3 = val3 & param4 = val4"
                        string values = reader.ReadToEnd();
                        var absoluteURL = getUrl(countryCode);
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("Authorization", token);
                        var json = JsonConvert.SerializeObject(values);
                        var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                        string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                        var responseString = await httpClient.PostAsync(remoteURL, content);
                        var result = await responseString.Content.ReadAsAsync<object>();
                        var tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");

                        tsc.SetResult(message);
                        return tsc.Task.Result;
                    }



                }
                else if (method == "PUT")
                {

                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        // This will equal to "charset = UTF-8 & param1 = val1 & param2 = val2 & param3 = val3 & param4 = val4"
                        string values = reader.ReadToEnd();
                        var absoluteURL = getUrl(countryCode);
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("Authorization", token);
                        var json = JsonConvert.SerializeObject(values);
                        var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                        string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                        var responseString = await httpClient.PutAsync(remoteURL, content);
                        var result = await responseString.Content.ReadAsAsync<object>();
                        var tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                        tsc.SetResult(message);
                        return tsc.Task.Result;
                    }
                }
            }

            return base.SendAsync(request, cancellationToken).Result;
        }
        private string getUrl(string countryCode)
        {
            var url = ConfigurationManager.AppSettings[countryCode];
            return url;
            //return "https://fintrakcredit360api2.azurewebsites.net";

        }
    }
}
