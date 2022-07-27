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
    public class SubsediaryParentController : ISubsediaryParentController
    {
        private HttpClient httpClient;
        public SubsediaryParentController()
        {
            this.httpClient = new HttpClient();
            //  this.httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["SubsediaryAPIUrl"] + "api/v1/credit/limitvalidations");
        }

        private string getUrl(string countryCode)
        {
            var url = ConfigurationManager.AppSettings[countryCode];
            return url;
            //return "https://fintrakcredit360api2.azurewebsites.net";
          
        }

        public async Task<object> post(string countryCode, string url, object body)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);
                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.PostAsync(remoteURL, content);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
            public async Task<object> post(string countryCode, object body)
            {
                try
                {
                    var token = HttpContext.Current.Request.Headers["Authorization"];
                    var absoluteURL = getUrl(countryCode);
                    this.httpClient.DefaultRequestHeaders.Clear();
                    this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                    this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                    var json = JsonConvert.SerializeObject(body);
                    var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                    string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                    var responseString = await this.httpClient.PostAsync(remoteURL, content);
                    var result = await responseString.Content.ReadAsAsync<object>();

                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

       
        public async Task<object> put(string countryCode, string url, object body)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);
                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.PutAsync(remoteURL, content);
                var result = await responseString.Content.ReadAsAsync<object>();

            return result;
            }catch (Exception ex)
            {
                    throw new Exception(ex.Message);
            }
         }

        public async Task<object> put(string countryCode, object body)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);
                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.PutAsync(remoteURL, content);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<object> get(string countryCode, string url)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);
                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.GetAsync(remoteURL);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task<object> get(string countryCode)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);
                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.GetAsync(remoteURL);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task<object> delete(string countryCode, string url)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);



                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.DeleteAsync(remoteURL);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch(Exception ex){
                throw new Exception(ex.Message);
            }

        }
        public async Task<object> delete(string countryCode)
        {
            try
            {
                var token = HttpContext.Current.Request.Headers["Authorization"];
                var absoluteURL = getUrl(countryCode);



                this.httpClient.DefaultRequestHeaders.Clear();
                this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                this.httpClient.DefaultRequestHeaders.Add("Authorization", token);
                this.httpClient.DefaultRequestHeaders.Add("isRemote", "1");
                string remoteURL = $"{absoluteURL}{HttpContext.Current.Request.CurrentExecutionFilePath}";
                var responseString = await this.httpClient.DeleteAsync(remoteURL);
                var result = await responseString.Content.ReadAsAsync<object>();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public string  getCountryCode()
        {
           string countryCode = HttpContext.Current.Request.Headers["X-COUNTRYCODE"];
            return "SA";
            
        }
    }
}
