using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace zupzip.demo.Controllers
{
    public class FacebookController : Controller
    {
        private string AccessToken {
            get {
                var claims = (ClaimsIdentity)HttpContext.GetOwinContext().Authentication.User.Identity;
                if (claims != null) {
                    var fb_access_token = claims.FindFirstValue("urn:facebook:access_token");
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
                graphApiUrl.Host = "graph.facebook.com";
                graphApiUrl.Path = action;
                graphApiUrl.Query = string.Join("&", queries.AllKeys.Select(c => string.Format("{0}={1}", HttpUtility.UrlEncode(c), HttpUtility.UrlEncode(queries[c]))));

                var req = (HttpWebRequest)WebRequest.Create(graphApiUrl.ToString());
                var res = (HttpWebResponse)req.GetResponse();

                using (StreamReader stream = new StreamReader(res.GetResponseStream())) {
                    result = stream.ReadToEnd();
                    stream.Close();
                }
            } catch (Exception ex) {
                if (ex.Message.Contains("400")) {
                    // invalid response
                    result = ex.Message;
                }
            }

            return result;
        }

        public ActionResult Index() {

            if (string.IsNullOrWhiteSpace(this.AccessToken)) {
                throw new Exception("Cannot find AccessToken");
            }

            var response = ApiCall("/178407509027964/attending"); // ZupZip event
            dynamic model = JObject.Parse(response);
            return View(model);
        }
    }
}