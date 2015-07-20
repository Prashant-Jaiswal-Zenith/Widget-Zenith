using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace Widget.WebApplication.Handlers {
    /// <summary>
    /// Summary description for WebResources
    /// </summary>
    public class WebResources : IHttpHandler {

        private const bool DO_GZIP = true;
        private readonly static TimeSpan CACHE_DURATION = TimeSpan.FromDays(0.02);
        private readonly static string JS_TYPE = "text/javascript";
        private readonly static string CSS_TYPE = "text/css";

        public void ProcessRequest(HttpContext context) {
            HttpRequest request = context.Request;

            // Read setName, contentType and version. All are required. They are
            // used as cache key
            string setName = request["s"] ?? string.Empty;
            string contentType = request["t"] ?? string.Empty;
            string version = request["v"] ?? string.Empty;

            // Decide if browser supports compressed response
            bool isCompressed = DO_GZIP && this.CanGZip(context.Request);

            // Response is written as UTF8 encoding. If you are using languages like
            // Arabic, you should change this to proper encoding
            UTF8Encoding encoding = new UTF8Encoding(false);

            // If the set has already been cached, write the response directly from
            // cache. Otherwise generate the response and cache it
            if (!this.WriteFromCache(context, setName, version, isCompressed, contentType)) {

                using (MemoryStream memoryStream = new MemoryStream(5000)) {

                    // Decide regular stream or GZipStream based on whether the response
                    // can be cached or not
                    using (Stream writer = isCompressed ?
                        (Stream)(new GZipStream(memoryStream, CompressionMode.Compress)) : memoryStream) {

                        // Load the files defined in and process each file
                        string setDefinition = GetFilelist(setName) ?? "";

                        string[] fileNames = setDefinition.Split(new char[] { ',' },
                            StringSplitOptions.RemoveEmptyEntries);

                        string minifiedString = string.Empty;
                        var minifier = new Minifier();
                        StringBuilder allScripts = new StringBuilder();

                        if (contentType == JS_TYPE) {
                            foreach (string fileName in fileNames) {
                                byte[] fileBytes = this.GetFileBytes(context, fileName.Trim(), encoding);
                                allScripts.Append(Encoding.UTF8.GetString(fileBytes));
                                //writer.Write(fileBytes, 0, fileBytes.Length);
                            }

                            var codeSettings = new CodeSettings();
                            codeSettings.MinifyCode = true;
                            codeSettings.OutputMode = OutputMode.SingleLine;
                            //codeSettings.OutputMode = OutputMode.MultipleLines;
                            codeSettings.InlineSafeStrings = true;
                            codeSettings.MacSafariQuirks = true;
                            codeSettings.RemoveUnneededCode = true;
                            codeSettings.LocalRenaming = LocalRenaming.CrunchAll;
                            codeSettings.EvalTreatment = EvalTreatment.MakeAllSafe;
                            //codeSettings.CombineDuplicateLiterals = true;
                            codeSettings.PreserveFunctionNames = false;
                            minifiedString = minifier.MinifyJavaScript(allScripts.ToString(), codeSettings);
                        }

                        if (contentType == CSS_TYPE) {
                            CssParser parser = new CssParser();
                            foreach (string fileName in fileNames) {
                                byte[] fileBytes = this.GetFileBytes(context, fileName.Trim(), encoding);
                                string crunchedStyles = parser.Parse(Encoding.UTF8.GetString(fileBytes));
                                allScripts.Append(crunchedStyles);
                            }

                            var cssSettings = new CssSettings();
                            cssSettings.CommentMode = CssComment.None;
                            cssSettings.ColorNames = CssColor.Strict;
                            //cssSettings.ExpandOutput = true;
                            cssSettings.TermSemicolons = true;
                            minifiedString = minifier.MinifyStyleSheet(allScripts.ToString(), cssSettings);
                        }

                        byte[] rikiki = Encoding.UTF8.GetBytes(minifiedString);
                        writer.Write(rikiki, 0, rikiki.Length);
                        writer.Close();
                    }

                    // Cache the combined response so that it can be directly written
                    // in subsequent calls
                    byte[] responseBytes = memoryStream.ToArray();
                    context.Cache.Insert(
                        GetCacheKey(setName, version, isCompressed),
                        responseBytes,
                        null,
                        Cache.NoAbsoluteExpiration,
                        CACHE_DURATION);

                    // Generate the response
                    this.WriteBytes(responseBytes, context, isCompressed, contentType);
                }
            }
        }

        public bool IsReusable {
            get {
                return false;
            }
        }


        private byte[] GetFileBytes(HttpContext context, string virtualPath, Encoding encoding) {

            if (virtualPath.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)) {
                using (WebClient client = new WebClient()) {
                    return client.DownloadData(virtualPath);
                }
            } else {
                string physicalPath = context.Server.MapPath(virtualPath);
                byte[] bytes = File.ReadAllBytes(physicalPath);
                // TODO: Convert unicode files to specified encoding. For now, assuming

                // files are either ASCII or UTF8
                return bytes;
            }
        }

        private bool WriteFromCache(HttpContext context, string setName, string version, bool isCompressed, string contentType) {
            byte[] responseBytes = context.Cache[GetCacheKey(setName, version, isCompressed)] as byte[];
            if (null == responseBytes || 0 == responseBytes.Length)
                return false;
            this.WriteBytes(responseBytes, context, isCompressed, contentType);
            return true;
        }

        private void WriteBytes(byte[] bytes, HttpContext context, bool isCompressed, string contentType) {
            HttpResponse response = context.Response;
            response.AppendHeader("Content-Length", bytes.Length.ToString());
            response.ContentType = contentType;
            if (isCompressed)
                response.AppendHeader("Content-Encoding", "gzip");
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.Add(CACHE_DURATION));
            context.Response.Cache.SetMaxAge(CACHE_DURATION);
            context.Response.Cache.AppendCacheExtension("must-revalidate, proxy-revalidate");
            context.Response.Cache.SetETag((Guid.NewGuid().ToString()));
            context.Response.Expires = 3600;

            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.Flush();
        }

        private bool CanGZip(HttpRequest request) {
            string acceptEncoding = request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(acceptEncoding) && (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate")))
                return true;
            return false;
        }

        private string GetCacheKey(string setName, string version, bool isCompressed) {
            return "HttpCombiner." + setName + "." + version + "." + isCompressed;
        }

        private string GetFilelist(string setName) {
            if (setName == "SearchWidgetScripts") { //Search Widget JS
                return @"~/Scripts/app/Widget/_Widget-jquery-2.1.4.js,
                        ~/Scripts/app/Widget/_Widget-modernizr-2.8.3.js,
                        ~/Scripts/app/Widget/_Widget-jquery-ui-1.11.4.js,
                        ~/Scripts/app/Widget/_Widget-jquery.validate.js,
                        ~/Scripts/app/Widget/_Widget-jquery.validate.unobtrusive.js,
                        ~/Scripts/app/Search/SearchWidget.js";
            } else if (setName == "SearchWidgetCss") { //Search Widget CSS
                return @"~/Content/themes/Widget/core.css,
                        ~/Content/themes/Widget/theme.css,
                        ~/Content/themes/Widget/autocomplete.css,
                        ~/Content/themes/Widget/datepicker.css,
                        ~/Content/themes/Widget/menu.css,
                        ~/Content/themes/SearchWidget.css";
            } else
                return string.Empty;
        }

    }
}