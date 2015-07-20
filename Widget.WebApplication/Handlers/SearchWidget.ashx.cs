using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Widget.WebApplication.Models;

namespace Widget.WebApplication.Handlers {
    /// <summary>
    /// Summary description for SearchWidget
    /// </summary>
    public class SearchWidget : IHttpHandler {

        public void ProcessRequest(HttpContext context) {
            HttpRequest request = context.Request;

            string type = request["t"];

            if (string.IsNullOrEmpty(type))
                return;

            var requestURL = request.Url.Scheme + "://" + request.Url.Authority;

            context.Response.ContentType = "text/javascript";
            context.Response.Write(getPartialViewResponse(requestURL));
        }

        public bool IsReusable {
            get {
                return false;
            }
        }

        #region Helpers

        internal static string RenderViewToString(string controllerName, string viewName,
            ViewDataDictionary viewData, bool partial = false) {

            var context = HttpContext.Current;
            var contextBase = new HttpContextWrapper(context);
            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);

            var controllerContext = new ControllerContext(contextBase, routeData, new EmptyController());
            var razorViewEngine = new RazorViewEngine();

            ViewEngineResult razorViewResult = null;

            if (partial) {
                razorViewResult = razorViewEngine.FindPartialView(controllerContext, viewName, false);
            } else {
                razorViewResult = razorViewEngine.FindView(controllerContext, viewName, "", false);
            }

            var writer = new StringWriter();

            var viewContext = new ViewContext(controllerContext, razorViewResult.View,
                viewData, new TempDataDictionary(), writer);
            razorViewResult.View.Render(viewContext, writer);

            return writer.ToString();
        }

        internal static string getPartialViewResponse(string url) {
            var request = getViewDataDictionary(url);

            var str = RenderViewToString("widget", "searchhotelpartial", request, true);

            //str = str.Substring(str.IndexOf("<form "));
            //str = str.Substring(0, str.IndexOf("</form>"));

            StringBuilder sbScript = new StringBuilder(1024 * 2);
            sbScript.Append(@"(function() { 'use strict'; ");

            var placeholderId = "i" + Guid.NewGuid().ToString().Replace("-", "");
            sbScript.Append(" document.write(\"<span id='" + placeholderId + "' style='display:none;'></span>\");");

            sbScript.Append(@"
                var html='" + str.Replace("'", "\\'").Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\\\r") + @"';

                var scripts=['" + url + @"/Handlers/WebResources.ashx?s=SearchWidgetScripts&t=text/javascript&v=1'];

                var styles=['" + url + @"/Handlers/WebResources.ashx?s=SearchWidgetCss&t=text/css&v=1'];

                var loadScript = function(url, callback){
                    var scriptTag = document.createElement('script');
                    scriptTag.setAttribute('type', 'text/javascript');
                    scriptTag.setAttribute('src', url);
                    if (typeof callback !== 'undefined') {
                        if (scriptTag.readyState) {
                            /* For old versions of IE */
                            scriptTag.onreadystatechange = function () { 
                                if (this.readyState === 'complete' || this.readyState === 'loaded') {
                                    callback();
                                }
                            };
                        } else {
                            scriptTag.onload = callback;
                        }
                    }
                    (document.getElementsByTagName('head')[0] || document.documentElement).appendChild(scriptTag); 
                };

                var loadStyles = function(url, callback){
                    var styleTag = document.createElement('link');
                    styleTag.setAttribute('type', 'text/css');
                    styleTag.setAttribute('rel', 'stylesheet');
                    styleTag.setAttribute('href', url);
                    if (typeof callback !== 'undefined') {
                        if (styleTag.readyState) {
                            /* For old versions of IE */
                            styleTag.onreadystatechange = function () { 
                                if (this.readyState === 'complete' || this.readyState === 'loaded') {
                                    callback();
                                }
                            };
                        } else {
                            styleTag.onload = callback;
                        }
                    }
                    (document.getElementsByTagName('head')[0] || document.documentElement).appendChild(styleTag); 
                };

                var setup = function () {
                    var e=document.getElementById('" + placeholderId + @"');

                    if(!e){ return; }

                    e.innerHTML = html; 

                    setTimeout(function(){
                      e.style.display = 'inline';
                    }, 1000); 
                };

            ");

            sbScript.Append(@"
                if(scripts.length == 0 || styles.length == 0){
                    setTimeout(setup, 50); //Give the browser some time to render the placeholder before trying to look it up and populate/update it's content
                } else {
                    var currentScript=0;
                    var currentStyle=0;
                    var loadNextScript = function(){
                        if(currentScript<scripts.length){
                            loadScript(scripts[currentScript++], loadNextScript);
                        } else {
                            var loadNextStyle = function(){
                                if (currentStyle<styles.length) {
                                    loadStyles(styles[currentStyle++], loadNextStyle);
                                } else {
                                    setup();
                                }
                            }
                            loadNextStyle();
                        }
                    };
                    loadNextScript();
                }

            })();");

            return sbScript.ToString();
        }

        internal static ViewDataDictionary getViewDataDictionary(string url) {
            var response = new ViewDataDictionary();

            var model = new PropertySearchParameterUIModel();

            model.NoOfRooms = 1;
            model.Rooms = createEmptyRooms(10).ToList();

            response.Model = model;

            response.Add("MaxRooms", 10);

            var list = Enumerable.Range(1, 10).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            response.Add("Rooms", list);

            list = Enumerable.Range(1, 10).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            response.Add("Adults", list);

            list = Enumerable.Range(0, 10).
                Select(n => new SelectListItem {
                    Text = n.ToString(),
                    Value = n.ToString()
                }).ToList();
            response.Add("Children", list);

            return response;
        }

        internal static IEnumerable<RoomDetailsUIModel> createEmptyRooms(int maxNoOfRooms) {
            foreach (var room in Enumerable.Range(1, maxNoOfRooms))
                yield return new RoomDetailsUIModel {
                    RoomNo = room,
                    NoOfAdults = 1,
                    NoOfChildren = 0
                };
        }

        #endregion
    }

    class EmptyController : ControllerBase {
        protected override void ExecuteCore() { }
    }
}