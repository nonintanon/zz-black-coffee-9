using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace zupzip.demo.Controllers
{
    public class TwitterController : Controller
    {
        public class TwitAuthenticateResponse
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
        }

        private string AccessToken {
            get {
                var claims = (ClaimsIdentity)HttpContext.GetOwinContext().Authentication.User.Identity;
                if (claims != null) {
                    var fb_access_token = claims.FindFirstValue("urn:twitter:access_token");
                    if (!string.IsNullOrWhiteSpace(fb_access_token)) {
                        return fb_access_token;
                    }
                }

                return string.Empty;
            }
        }
        private string ApiCall(string action, NameValueCollection queries = null) {

            string result = string.Empty;

            try {

                // if null, create new one, even it's empty
                if (queries == null)
                    queries = new NameValueCollection();

                // always attach access_token
                queries.Add("access_token", this.AccessToken);

                var graphApiUrl = new UriBuilder();
                graphApiUrl.Scheme = "https";
                graphApiUrl.Host = "api.twitter.com";
                graphApiUrl.Path = action;
                graphApiUrl.Query = string.Join("&", queries.AllKeys.Select(c => string.Format("{0}={1}", HttpUtility.UrlEncode(c), HttpUtility.UrlEncode(queries[c]))));

                var oAuthConsumerKey = "AElY2mfoFZewCToE7NWYg";
                var oAuthConsumerSecret = "MNRU6K9X3lgAc6xUgJcrHW5BcKBlvuBl9iHYklhUmw";
                var oAuthUrl = "https://api.twitter.com/oauth2/token";
                var screenname = "nonintanon";

                // Do the Authenticate
                var authHeaderFormat = "Basic {0}";

                var authHeader = string.Format(authHeaderFormat,
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(Uri.EscapeDataString(oAuthConsumerKey) + ":" +
                    Uri.EscapeDataString((oAuthConsumerSecret)))
                ));

                var postBody = "grant_type=client_credentials";

                HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(oAuthUrl);
                authRequest.Headers.Add("Authorization", authHeader);
                authRequest.Method = "POST";
                authRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                authRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (Stream stream = authRequest.GetRequestStream()) {
                    byte[] content = ASCIIEncoding.ASCII.GetBytes(postBody);
                    stream.Write(content, 0, content.Length);
                }

                authRequest.Headers.Add("Accept-Encoding", "gzip");

                WebResponse authResponse = authRequest.GetResponse();
                // deserialize into an object
                TwitAuthenticateResponse twitAuthResponse;
                using (authResponse) {
                    using (var reader = new StreamReader(authResponse.GetResponseStream())) {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objectText = reader.ReadToEnd();
                        twitAuthResponse = JsonConvert.DeserializeObject<TwitAuthenticateResponse>(objectText);
                    }
                }

                // Do the timeline
                var timelineFormat = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name={0}&include_rts=1&exclude_replies=1&count=5";
                var timelineUrl = string.Format(timelineFormat, screenname);
                HttpWebRequest timeLineRequest = (HttpWebRequest)WebRequest.Create(timelineUrl);
                var timelineHeaderFormat = "{0} {1}";
                timeLineRequest.Headers.Add("Authorization", string.Format(timelineHeaderFormat, twitAuthResponse.token_type, twitAuthResponse.access_token));
                timeLineRequest.Method = "Get";
                WebResponse timeLineResponse = timeLineRequest.GetResponse();
                var timeLineJson = string.Empty;
                using (timeLineResponse) {
                    using (var reader = new StreamReader(timeLineResponse.GetResponseStream())) {
                        timeLineJson = reader.ReadToEnd();
                        return timeLineJson;
                    }
                }
            } catch (Exception ex) {
                if (ex.Message.Contains("400")) {
                    // invalid response
                    result = ex.Message;
                }
            }

            return result;
        }

        //
        // GET: /Twitter/
        public ActionResult Index() {
            if (string.IsNullOrWhiteSpace(this.AccessToken)) {
                throw new Exception("Cannot find AccessToken");
            }
            var response = this.ApiCall("/1.1/statuses/user_timeline.json");
            dynamic model = JsonConvert.DeserializeObject(response);
            return View(model);
        }
    }
}