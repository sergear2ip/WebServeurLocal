using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace SR.ServeurLib
{

    public class WebServeurLib
    {
        private EventLog libEventLog;

        private readonly string racine;
        private HttpListener listener;
        private bool running;
        private readonly string prefixe;
        private static Dictionary<string, string> typesDict;

        public WebServeurLib(string prefix, string rootDir)
        {
            InitializeComponent();
            libEventLog = new EventLog();
            if (!EventLog.SourceExists("WebServeur"))
            {
                EventLog.CreateEventSource(
                    "WebServeur", "WebSLog");
            }
            libEventLog.Source = "WebServeur";
            libEventLog.Log = "WebSLog";
            racine = rootDir;
            prefixe = prefix;
            LoadMimeType();
        }
        public void Stop()
        {
            running = false;
        }

        public void Start()
        {
            if (running) return;

            if (!HttpListener.IsSupported)
            {
                libEventLog.WriteEntry("ERREUR! classe HttpListener non supportée par cette version de Windows");
                return;
            }

            if (prefixe == null)
            {
                libEventLog.WriteEntry("ERREUR! manque prefixe");
                return;
            }

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(prefixe);
                listener.Start();

                running = true;
            }
            catch (HttpListenerException ex)
            {
                libEventLog.WriteEntry("ERREUR! HttpListener (" + prefixe + ")");
                libEventLog.WriteEntry(ex.ToString());
                running = false;
            }

            if (running)
            {
                libEventLog.WriteEntry("Web serveur Start (" + prefixe + ")");

                Task.Factory.StartNew(() =>
                {
                    while (running)
                    {
                        TraiteRequete(listener.GetContext());
                    }

                    listener.Close();
                    libEventLog.WriteEntry("Web serveur Stop");
                });
            }

        }

        private void TraiteRequete(HttpListenerContext context)
        {

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            var fileName = request.Url.AbsolutePath;

            // requete GET
            NameValueCollection queryStringCollection = request.QueryString;
            string action = queryStringCollection["action"];

            fileName = fileName.Substring(1);

            if (string.IsNullOrEmpty(fileName))
                fileName = "index.html";
            else
            {
                var parts = fileName.Split('/');
                if (parts.Length > 0 && string.IsNullOrEmpty(parts[parts.Length - 1]))
                    fileName = Path.Combine(fileName, "index.html");
            }

            fileName = Path.Combine(racine, fileName);

            response.ContentType = GetMimeType(fileName);

            FileStream fileStream = null;

            // Si requete GET action=save écriture du fichier
            if (action == "save")
                using (var reader = new StreamReader(request.InputStream,
                                      request.ContentEncoding))
                {
                    using (var writer = new StreamWriter(fileName, append: false))
                    {
                        writer.Write(reader.ReadToEnd());
                    }
                }

            try
            {
                string ext = Path.GetExtension(fileName);
                if (ext == ".php")
                {
                    TraitePHP(context, fileName);
                }
                fileStream = new FileStream(fileName, FileMode.Open);

                var buffer = new byte[1024 * 16];
                int nbytes;

                while ((nbytes = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    response.OutputStream.Write(buffer, 0, nbytes);
            }
            catch (Exception e)
            {
                libEventLog.WriteEntry("ERREUR! fichier ");
                libEventLog.WriteEntry(e.ToString());
                response.StatusCode = 404;
            }
            finally
            {
                fileStream?.Close();
                response.OutputStream.Close();
            }
        }

        // Interprète php avec php-cgi.exe
        private void TraitePHP(HttpListenerContext context, string documentRoot)
        {
            HttpListenerResponse response = context.Response;
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var scriptFilePath = Path.GetFullPath(documentRoot);
            var scriptFileName = Path.GetFileName(documentRoot);
            var tempPath = Path.GetTempPath();
            using (var process = new Process())
            {
                var index = context.Request.RawUrl.IndexOf("?");
                var queryString = index == -1 ? "" : context.Request.RawUrl.Substring(index + 1);
                byte[] requestBody;
                using (var ms = new MemoryStream())
                {
                    context.Request.InputStream.CopyTo(ms);
                    requestBody = ms.ToArray();
                }

                process.StartInfo.FileName = Path.Combine(exePath, @"php-8.3.1\php-cgi.exe");
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = true;

                process.StartInfo.EnvironmentVariables.Clear();

                process.StartInfo.EnvironmentVariables.Add("DOCUMENT_ROOT", exePath);
                process.StartInfo.EnvironmentVariables.Add("GATEWAY_INTERFACE", "CGI/1.1");
                process.StartInfo.EnvironmentVariables.Add("SERVER_PROTOCOL", "HTTP/1.1");
                process.StartInfo.EnvironmentVariables.Add("SCRIPT_NAME", scriptFileName);
                process.StartInfo.EnvironmentVariables.Add("SCRIPT_FILENAME", scriptFilePath);
                process.StartInfo.EnvironmentVariables.Add("QUERY_STRING", queryString);
                process.StartInfo.EnvironmentVariables.Add("REDIRECT_STATUS", "200");
                process.StartInfo.EnvironmentVariables.Add("CONTENT_TYPE", context.Request.ContentType);
                process.StartInfo.EnvironmentVariables.Add("REQUEST_METHOD", context.Request.HttpMethod);
                process.StartInfo.EnvironmentVariables.Add("REMOTE_ADDR", context.Request.RemoteEndPoint.Address.ToString());
                process.StartInfo.EnvironmentVariables.Add("REMOTE_PORT", context.Request.RemoteEndPoint.Port.ToString());
                process.StartInfo.EnvironmentVariables.Add("REFERER", context.Request.UrlReferrer?.ToString() ?? "");
                process.StartInfo.EnvironmentVariables.Add("TMPDIR", tempPath);
                process.StartInfo.EnvironmentVariables.Add("TEMP", tempPath);
                process.StartInfo.EnvironmentVariables.Add("USER_AGENT", context.Request.UserAgent);
                process.StartInfo.EnvironmentVariables.Add("HTTP_COOKIE", context.Request.Headers["Cookie"]);
                process.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT", context.Request.Headers["Accept"]);
                process.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_CHARSET", context.Request.Headers["Accept-Charset"]);
                process.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_ENCODING", context.Request.Headers["Accept-Encoding"]);
                process.StartInfo.EnvironmentVariables.Add("HTTP_ACCEPT_LANGUAGE", context.Request.Headers["Accept-Language"]);

                process.Start();

                using (var sw = process.StandardInput)
                    sw.BaseStream.Write(requestBody, 0, requestBody.Length);

                var headersEnd = false;
                using (var sr = process.StandardOutput)

                using (StreamWriter output = new StreamWriter(response.OutputStream))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!headersEnd)
                        {
                            if (line == "")
                            {
                                headersEnd = true;
                                continue;
                            }

                            // Entêtes
                            index = line.IndexOf(':');
                            var name = line.Substring(0, index);
                            var value = line.Substring(index + 2);
                            context.Response.Headers[name] = value;

                        }
                        else
                        {
                            output.Write(line);
                        }
                    }
                    response.OutputStream.Write(requestBody, 0, requestBody.Length);
                    process.WaitForExit(1000);
                    process.Close();
                }
            }

        }
        public static string GetMimeType(string fileName)
        {
            if (typesDict.TryGetValue(Path.GetExtension(fileName).Remove(0, 1), out string type))
                return type;
            else return "unknown/unknown";
        }
        private void LoadMimeType()
        {
            libEventLog.WriteEntry("In LoadMimeType");
            typesDict = new Dictionary<string, string>();
            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            using (StreamReader sr = new StreamReader(Path.Combine(exePath, "mimesType.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] keyval = line.Split('=');
                    if (keyval.Length == 2)
                    {
                        typesDict.Add(keyval[0], keyval[1]);
                    }
                }
            }
        }
        private void InitializeComponent()
        {
            libEventLog = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(libEventLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(libEventLog)).EndInit();

        }

    }
}

