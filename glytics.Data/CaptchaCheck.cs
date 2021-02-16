using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace glytics.Data
{
    public class CaptchaCheck
    {
        private string Response { get; }
        public CaptchaCheck(string response)
        {
            Response = response;
        }

        public bool Verify()
        {
            var result = false;
            var secretKey = Environment.GetEnvironmentVariable("RECAPTCHA_GLYTICS");  
            var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";  
            var requestUri = string.Format(apiUrl, secretKey, Response);  
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
  
            using(WebResponse response = request.GetResponse())  
            {  
                using (StreamReader stream = new StreamReader(response.GetResponseStream()!))  
                {  
                    JObject jResponse = JObject.Parse(stream.ReadToEnd());

                    var isSuccess = jResponse.Value<bool>("success");  
                    result = (isSuccess) ? true : false;  
                }  
            }
            
            return result;  
        }
    }
}