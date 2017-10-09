using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    class bradserver
    {
        static void Main(string[] args)
        {

            Logging.setup();

            Logging.startLoggingThread();

            ResourceManager resourceManager = new ResourceManager();
            resourceManager.loadResources();

            WebServer webServer = new WebServer();
            webServer.ResourceManager = resourceManager;

            String[] URIs =
            {
                "http://localhost:8080/"
            };

            webServer.Start(URIs);
            webServer.startListening();

            Console.WriteLine("Server started, press any key to stop");
            Console.ReadKey();
            webServer.Stop();

        }
    }
}
