using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FintrakBanking.AccessSubsediary
{
    class SubsediaryInterceptor : IHttpModule
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {

            EventHandlerTaskAsyncHelper asyncHelperObject = new EventHandlerTaskAsyncHelper(OnBeginRequest);
            context.AddOnPostAuthorizeRequestAsync(asyncHelperObject.BeginEventHandler, asyncHelperObject.EndEventHandler);
        }

        private async Task<object> OnBeginRequest(object sender, EventArgs e)
        {

            
            HttpContext context = ((HttpApplication)sender).Context;
            string url = context.Request.CurrentExecutionFilePath;
            string method = context.Request.HttpMethod;
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
                    httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                    var responseString = await httpClient.GetAsync(remoteURL);
                    var result = await responseString.Content.ReadAsAsync<object>();

                    return result;



                }
                else if (method == "POST")
                {
                  
                    var absoluteURL = getUrl(countryCode);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", token);
                    httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                    var body = HttpUtility.UrlDecode(context.Request.Form.ToString());
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                    var responseString = await httpClient.PostAsync(remoteURL, content);
                    var result = await responseString.Content.ReadAsAsync<object>();
                }
                else if (method == "PUT")
                {

                    var absoluteURL = getUrl(countryCode);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", token);
                    httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                    var body = HttpUtility.UrlDecode(context.Request.Form.ToString());
                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                    var responseString = await httpClient.PutAsync(remoteURL, content);
                    var result = await responseString.Content.ReadAsAsync<object>();
                    context.Response.Clear();
                    context.Response.ClearHeaders();

                    context.Response.StatusCode = 200;
                    context.Response.StatusDescription = "OK";
                    context.Response.Write(result);
                    context.Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    return null;
                }
            }

            return -1;
        }

        private string getUrl(string countryCode)
        {
            var url = ConfigurationManager.AppSettings[countryCode];
            return url;
            //return "https://fintrakcredit360api2.azurewebsites.net";
 
        }
    }
}

