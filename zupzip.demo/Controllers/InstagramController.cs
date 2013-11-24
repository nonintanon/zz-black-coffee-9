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
    public class InstagramController : Controller
    {
        private string AccessToken {
            get {
                var claims = (ClaimsIdentity)HttpContext.GetOwinContext().Authentication.User.Identity;
                if (claims != null) {
                    var instagram_access_token = claims.FindFirstValue("urn:instagram:access_token");
                    if (!string.IsNullOrWhiteSpace(instagram_access_token)) {
                        return instagram_access_token;
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

                var requestUrl = new UriBuilder();
                requestUrl.Scheme = "https";
                requestUrl.Host = "api.instagram.com";
                requestUrl.Path = action;
                requestUrl.Query = string.Join("&", queries.AllKeys.Select(c => string.Format("{0}={1}", HttpUtility.UrlEncode(c), HttpUtility.UrlEncode(queries[c]))));

                var req = (HttpWebRequest)WebRequest.Create(requestUrl.ToString());
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

        //
        // GET: /Instagram/
        public ActionResult Index() {
            if (string.IsNullOrWhiteSpace(this.AccessToken)) {
                throw new Exception("Cannot find AccessToken");
            }

            var response = ApiCall("/v1/users/self/feed");
            dynamic model = JObject.Parse(response);
            return View(model);
        }
    }
}