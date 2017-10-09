using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
namespace WebServer
{

    class WebServer
    {
        private HttpListener httpListener;
        private ResourceManager resourceManager;
        internal ResourceManager ResourceManager
        {
            get {
                return resourceManager;
            }

            set {
                resourceManager = value;
            }
        }

        /// <summary>
        /// Start the HttpListener for the server
        /// </summary>
        /// <param name="prefixes">String array of prefixes to listen on</param>
        /// <returns>true if HttpListener was started correctly </returns>
        public bool Start(string[] prefixes)
        {
            if (!HttpListener.IsSupported) {
                Logging.logMessage(Logging.MessageType.Debug, "HttpListener unsupported on current system");
                return false;
            }

            if (prefixes == null || prefixes.Length == 0) {
                throw new ArgumentException("Prefixes are incorrect");
            }

            httpListener = new HttpListener();

            foreach (string s in prefixes) {
                httpListener.Prefixes.Add(s);
            }

            httpListener.Start();
            return true;
        }

        /// <summary>
        /// Starts the main server threadpool(s)
        /// </summary>
        public void startListening()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Logging.logMessage(Logging.MessageType.Debug, "Server listening");
                while (httpListener.IsListening) {

                    ThreadPool.QueueUserWorkItem((context) =>
                    {
                        var listenerContext = context as HttpListenerContext;

                        try {
                            getResponse(ref listenerContext);
                        }
                        catch (Exception e) {
                            Logging.logMessage(Logging.MessageType.Error, e.Message);
                        }
                        finally {
                            listenerContext.Response.OutputStream.Close();
                        }

                    }, httpListener.GetContext());
                }
            });
        }

        public void Stop()
        {
            httpListener.Stop();
            httpListener.Close();
        }

        /// <summary>
        /// Modifies the HttpListenerContext's Response given the Request
        /// </summary>
        /// <param name="listenerContext">The listener context containing the Request</param>
        /// <returns>true if request was valid </returns>
        private bool getResponse(ref HttpListenerContext listenerContext)
        {

            Logging.logMessage(Logging.MessageType.Access, listenerContext.Request.UserHostAddress + " Requested " + listenerContext.Request.Url.LocalPath);

            HttpListenerResponse httpResponse = listenerContext.Response;
            HttpListenerRequest httpRequest = listenerContext.Request;

            if (httpRequest.AcceptTypes == null || httpRequest.AcceptTypes.Length == 0) { //Request doesn't have a type so respond 400
                httpResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                return false;
            }

            string acceptType = httpRequest.AcceptTypes[0].Substring(0, httpRequest.AcceptTypes[0].IndexOf('/')); //Get fist part of accept type eg *plain*/html
            Byte[] fileBuffer = resourceManager.getFile(listenerContext.Request.Url.LocalPath); //Try to load the file requested

            if (fileBuffer == null) { //If the buffer is null the file doesnt exist so respond 404
                httpResponse.StatusCode = (int)HttpStatusCode.NotFound;
                return false;
            }


            switch (acceptType) {
                case "text":
                    httpResponse.ContentType = "text/html";
                    break;
                case "image":
                    httpResponse.ContentType = "text/image";
                    break;
                case "audio":
                    httpResponse.ContentType = "text/audio";
                    break;
                case "video":
                    httpResponse.ContentType = "text/video";
                    break;
                case "application":
                    httpResponse.ContentType = "text/application";
                    break;
                default:
                    return false;
            }

            httpResponse.ContentLength64 = fileBuffer.Length;
            httpResponse.OutputStream.Write(fileBuffer, 0, fileBuffer.Length);

            return true;
        }
    }
}
