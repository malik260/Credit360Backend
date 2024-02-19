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
        private const string Origin = "Origin";
        private const string AccessControlRequestMethod = "Access-Control-Request-Method";
        private const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        private const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        private const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        private const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";
        //protected override Task<HttpResponseMessage> SendAsync2(HttpRequestMessage request,
        //                                                       CancellationToken cancellationToken)
        //{
        //    bool isCorsRequest = request.Headers.Contains(Origin);
        //    bool isPreflightRequest = request.Method == HttpMethod.Options;
        //    if (isCorsRequest)
        //    {
        //        if (isPreflightRequest)
        //        {
        //            var response = new HttpResponseMessage(HttpStatusCode.OK);
        //            response.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

        //            string accessControlRequestMethod =
        //                request.Headers.GetValues(AccessControlRequestMethod).FirstOrDefault();
        //            if (accessControlRequestMethod != null)
        //            {
        //                response.Headers.Add(AccessControlAllowMethods, accessControlRequestMethod);
        //            }

        //            string requestedHeaders = string.Join(", ", request.Headers.GetValues(AccessControlRequestHeaders));
        //            if (!string.IsNullOrEmpty(requestedHeaders))
        //            {
        //                response.Headers.Add(AccessControlAllowHeaders, requestedHeaders);
        //            }

        //            var tcs = new TaskCompletionSource<HttpResponseMessage>();
        //            tcs.SetResult(response);
        //            return tcs.Task;
        //        }
        //        return base.SendAsync(request, cancellationToken).ContinueWith(t =>
        //        {
        //            HttpResponseMessage resp = t.Result;
        //            resp.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
        //            return resp;
        //        });
        //    }
        //    return base.SendAsync(request, cancellationToken);
        //}
        protected override async Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage request, CancellationToken cancellationToken)
        {

                string url = HttpContext.Current.Request.CurrentExecutionFilePath;
            string method = HttpContext.Current.Request.HttpMethod;
            var httpClient = new HttpClient();
            var token = HttpContext.Current.Request.Headers["Authorization"];
            string countryCode = HttpContext.Current.Request.Headers["X-COUNTRYCODE"];
            var excemptedUrls = new List<string>();
            //int idValue = 0;
           //excemptedUrls.Add("/api/v1/credit/additional-comment-edit");
           excemptedUrls.Add("/api/v1/credit/update-subsidiary-basic-transaction/");
            if (countryCode != null && countryCode != "NG" && !excemptedUrls.Contains(HttpContext.Current.Request.CurrentExecutionFilePath)
                && !HttpContext.Current.Request.CurrentExecutionFilePath.Contains("/api/v1/credit/update-subsidiary-basic-transaction/"))
           excemptedUrls.Add("/api/v1/credit/get-subsidiary-basic-approvalLevelId/");
            if (countryCode != null && countryCode != "NG" && !excemptedUrls.Contains(HttpContext.Current.Request.CurrentExecutionFilePath)
                && !HttpContext.Current.Request.CurrentExecutionFilePath.Contains("/api/v1/credit/get-subsidiary-basic-approvalLevelId/"))
            {


                if (method == "GET")
                {
                    if (countryCode != null && countryCode != "NG" 
                    && HttpContext.Current.Request.RawUrl.Contains("/api/v1/credit/documentation/operation/"))
                    {
                        var _absoluteURL = getUrl(countryCode);
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("Authorization", token);
                        httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                        string _remoteURL = $"{_absoluteURL}{HttpContext.Current.Request.RawUrl}";
                        var _responseString = await httpClient.GetAsync(_remoteURL);
                        var _result = await _responseString.Content.ReadAsAsync<object>();
                        var _tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage _message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        _message.Content = new StringContent(JsonConvert.SerializeObject(_result), Encoding.UTF8, "application/json");
                        _message.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                        _tsc.SetResult(_message);
                        return _tsc.Task.Result;
                    }


                    var absoluteURL = getUrl(countryCode);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", token);
                    httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.RawUrl}";
                    var responseString = await httpClient.GetAsync(remoteURL);
                    var result = await responseString.Content.ReadAsAsync<object>();
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();
                    HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                    message.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
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
                        httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                        var json = JsonConvert.SerializeObject(values);
                        var content = new StringContent(values, Encoding.UTF8, "application/json");
                        string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.RawUrl}";
                        var responseString = await httpClient.PostAsync(remoteURL, content);
                        var result = await responseString.Content.ReadAsAsync<object>();
                        var tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                        message.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
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
                        httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                        var json = JsonConvert.SerializeObject(values);
                        var content = new StringContent(values, Encoding.UTF8, "application/json");
                        string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.RawUrl}";
                        var responseString = await httpClient.PutAsync(remoteURL, content);
                        var result = await responseString.Content.ReadAsAsync<object>();
                        var tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                        message.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());
                        tsc.SetResult(message);
                        return tsc.Task.Result;
                    }

                }
                else if (method == "DELETE")
                {

                    using (var reader = new StreamReader(HttpContext.Current.Request.InputStream))
                    {
                        // This will equal to "charset = UTF-8 & param1 = val1 & param2 = val2 & param3 = val3 & param4 = val4"
                        string values = reader.ReadToEnd();
                        var absoluteURL = getUrl(countryCode);
                        httpClient.DefaultRequestHeaders.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        httpClient.DefaultRequestHeaders.Add("Authorization", token);
                        httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                        var json = JsonConvert.SerializeObject(values);
                        var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                        string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.RawUrl}";
                        var responseString = await httpClient.DeleteAsync(remoteURL);
                        var result = await responseString.Content.ReadAsAsync<object>();
                        var tsc = new TaskCompletionSource<HttpResponseMessage>();
                        HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                        message.Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json");
                        message.Headers.Add(AccessControlAllowOrigin, request.Headers.GetValues(Origin).First());

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
