﻿using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SciServer.Logging;
using System.Web.Http;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sciserver_webService.Common
{

    public class LoggedInfo
    {
        /// <summary>
        /// IP address of the client.
        /// </summary>
        public string ClientIP = null;
        /// <summary>
        /// Name of the task that is being requested. The name identifies whether the request comes from Skyserver or SkyserverWS
        /// </summary>
        public string TaskName = null;
        /// <summary>
        /// Entry point of Skyserver Webservice that is being called by the request.
        /// </summary>
        public string EntryPoint = null;
        /// <summary>
        /// Must be set in case of Custom, Debug, Info and Warn messages.
        /// </summary>
        public string Message = null;
        /// <summary>
        /// Must be set in case of Error or Fatal messages
        /// </summary>
        public Exception Exception = null;
        /// <summary>
        /// Request header. Must be always set.
        /// </summary>
        public System.Net.Http.Headers.HttpRequestHeaders Headers = null;
        /// <summary>
        /// URI of the request.
        /// </summary>
        public Uri URI = null;
        /// <summary>
        /// SQL commands that were executed.
        /// </summary>
        public string query = "";

        public string Referrer = "";

        public bool DoShowInUserHistory = false;

        public string ShortTaskName = "";

    }

    public class SciserverLogging
    {
        public Message message = null;
        public string userid = null; // before knowing whether it is an authenticated user.
        public string user_name = null;
        private bool HasAthenticatedWithActivityInfo = false;
        public string Token = "";
        public bool IsValidUser = false;

        public SciserverLogging(LoggedInfo ActivityInfo)
        {
            AthenticateUser(ActivityInfo);
        }

        public SciserverLogging()
        {

        }

        public void LogActivity(LoggedInfo ActivityInfo, string TypeOfLogging)
        {
            if(!HasAthenticatedWithActivityInfo)
                AthenticateUser(ActivityInfo);


            Logger log = (HttpContext.Current.ApplicationInstance as MvcApplication).Log;

            if (TypeOfLogging == "CustomMessage")
            {
                message = log.CreateCustomMessage(KeyWords.loggingMessageType, ActivityInfo.Message);
            }
            else if (TypeOfLogging == "SkyserverMessage")
            {
                message = log.CreateSkyserverMessage(ActivityInfo.Message, ActivityInfo.DoShowInUserHistory);
            }
            else if (TypeOfLogging == "DebugMessage")
            {
                message = log.CreateDebugMessage(ActivityInfo.Message);
            }
            else if (TypeOfLogging == "ErrorMessage")
            {
                message = log.CreateErrorMessage(ActivityInfo.Exception, ActivityInfo.Message);
            }
            else if (TypeOfLogging == "FatalMessage")
            {
                message = log.CreateFatalMessage(ActivityInfo.Exception, ActivityInfo.Message);
            }
            else if (TypeOfLogging == "InfoMessage")
            {
                message = log.CreateInfoMessage(ActivityInfo.Message);
            }
            else if (TypeOfLogging == "WarnMessage")
            {
                message = log.CreateWarnMessage(ActivityInfo.Message);
                //message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());break;
            }
            else
            {
                message = log.CreateInfoMessage("Empty Message");
                //message = log.CreateCustomMessage(KeyWords.loggingMessageType, api.ControllerContext.Request.ToString());break;
            }
            message.UserId = userid;
            message.ClientIP = ActivityInfo.ClientIP ?? "";
            message.TaskName = ActivityInfo.TaskName ?? "";
            message.UserName = user_name;
            message.UserToken = String.IsNullOrEmpty(this.Token) ? null : Token;
            log.SendMessage(message);
        }



        public void AthenticateUser(LoggedInfo ActivityInfo)
        {
            IEnumerable<string> values;
            string token = "";

            //try getting token from the Header
            if (ActivityInfo.Headers.TryGetValues(KeyWords.XAuthToken, out values))
            {
                try
                {
                    token = values.First();
                }
                catch (Exception e) { }
            }
            //try getting token from cookie in Header
            else
            {
                try
                {
                    IEnumerable<string> cookies = ActivityInfo.Headers.GetValues("Cookie");
                    string info = KeyWords.CookieToken + "=";
                    foreach (string cookie in cookies)
                    {
                        if (cookie.Contains(info))
                        {
                            string tok = cookie.Split(';').Where((i => i.Contains(info))).First().Trim();
                            token = tok.Remove(tok.IndexOf(info), info.Length);
                        }
                    }
                }
                catch { }
            }
            //try getting token from the URI:
            if (ActivityInfo.URI != null && String.IsNullOrEmpty(token))
            {
                try
                {
                    NameValueCollection col = ActivityInfo.URI.ParseQueryString();
                    token = col.Get("token");
                }
                catch { }
            }


            if (!String.IsNullOrEmpty(token))
            {
                try
                {
                    Token = token;
                    var userAccess = Keystone.Authenticate(token);
                    userid = userAccess.User.Id;
                    user_name = userAccess.User.Name;
                    IsValidUser = true;
                }
                catch { IsValidUser = false; }
            }
            HasAthenticatedWithActivityInfo = true;
        }

        public void AthenticateUser()
        {
            string token = ""; 

            //try getting token from the Header
            IEnumerable<string> values = null;
            try{values = HttpContext.Current.Request.Headers.GetValues(KeyWords.XAuthToken);}catch{}
            if (values!=null)
            {
                try
                {
                    token = values.First();
                }
                catch (Exception e) { }
            }
            //try getting token from cookie in Header
            else
            {
                try
                {
                    IEnumerable<string> cookies = HttpContext.Current.Request.Headers.GetValues("Cookie");
                    string info = KeyWords.CookieToken + "=";
                    foreach (string cookie in cookies)
                    {
                        if (cookie.Contains(info))
                        {
                            string tok = cookie.Split(';').Where((i => i.Contains(info))).First().Trim();
                            token = tok.Remove(tok.IndexOf(info), info.Length);
                        }
                    }
                }
                catch { }
            }
            //try getting token from the URI:
            if (HttpContext.Current.Request.Url != null && String.IsNullOrEmpty(token))
            {
                try
                {
                    NameValueCollection col = HttpContext.Current.Request.Url.ParseQueryString();
                    token = col.Get("token");
                }
                catch { }
            }


            if (!String.IsNullOrEmpty(token))
            {
                try
                {
                    Token = token;
                    var userAccess = Keystone.Authenticate(token);
                    userid = userAccess.User.Id;
                    user_name = userAccess.User.Name;
                    IsValidUser = true;
                }
                catch { IsValidUser = false; }
            }
            HasAthenticatedWithActivityInfo = false;
        }




    }
}