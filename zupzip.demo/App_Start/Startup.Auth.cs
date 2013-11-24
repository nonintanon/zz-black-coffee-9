using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Twitter;
using Owin;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace zupzip.demo
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app) {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });

            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Basic
            //app.UseTwitterAuthentication(consumerKey: WebConfigurationManager.AppSettings["TwitterConsumerKey"].ToString(), consumerSecret: WebConfigurationManager.AppSettings["TwitterConsumerSecret"].ToString());
            //app.UseFacebookAuthentication(appId: WebConfigurationManager.AppSettings["FacebookAppId"].ToString(), appSecret: WebConfigurationManager.AppSettings["FacebookAppSecret"].ToString());

            #region GIVE ME access_token!
            const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
            app.UseTwitterAuthentication(new TwitterAuthenticationOptions() {
                ConsumerKey = WebConfigurationManager.AppSettings["TwitterConsumerKey"].ToString(),
                ConsumerSecret = WebConfigurationManager.AppSettings["TwitterConsumerSecret"].ToString(),
                Provider = new TwitterAuthenticationProvider() {
                    OnAuthenticated = context => {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:access_token", context.AccessToken, XmlSchemaString, "Twitter"));
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:twitter:access_token_secret", context.AccessToken, XmlSchemaString, "Twitter"));
                        return Task.FromResult(0);
                    }
                }
            });
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions() {
                AppId = WebConfigurationManager.AppSettings["FacebookAppId"].ToString(),
                AppSecret = WebConfigurationManager.AppSettings["FacebookAppSecret"].ToString(),
                Provider = new FacebookAuthenticationProvider() {
                    OnAuthenticated = context => {
                        context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken, XmlSchemaString, "Facebook"));
                        return Task.FromResult(0);
                    }
                }
            });
            #endregion

            #region Instagram Time!
            app.UseInstagramAuthentication(WebConfigurationManager.AppSettings["InstagramAppId"].ToString(), WebConfigurationManager.AppSettings["InstagramAppSecret"].ToString());
            #endregion
        }
    }
}