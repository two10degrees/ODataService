using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using Kayak;
using Kayak.Http;
using Two10.ODataService.Controllers;
using System.IO;
using System.Web;

namespace Two10.ODataService
{
    // demonstrates how to use kayak.
    //
    // important bits: kayak uses a single worker thread, represented
    // by KayakScheduler. You can post work to the scheduler from any
    // thread by using its Post() method.
    //
    // if an exception bubbles up to the scheduler, it's passed to the 
    // scheduler delegate.
    //
    // HTTP requests are handled by an IHttpRequestDelegate. the OnRequest
    // method receives the request headers and body as well as an
    // IHttpResponseDelegate which can be used to generate a response.
    //
    // Request and response body streams are represented by IDataProducer.
    // the semantics of IDataProducer are nearly identical to those of
    // IObservable, the difference being the OnData method of IDataConsumer 
    // (analogous to the OnNext method of IObserver) takes an additional 
    // continuation argument and returns a bool. this is a mechanism to
    // enable a consumer to 'throttle back' a producer.
    //
    // a consumer should return true if it will invoke the continuation, and
    // false otherwise. if the consumer returns true, the producer should not 
    // call OnNext again until the continuation it provided to the consumer is
    // invoked. a producer may provide a null continuation to prohibit the
    // consumer from 'throttling back' the producer.
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //Debug.AutoFlush = true;
#endif

            var scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
            scheduler.Post(() =>
            {
                KayakServer.Factory
                    .CreateHttp(new RequestDelegate(), scheduler)
                    .Listen(new IPEndPoint(IPAddress.Any, 8080));
            });

            // runs scheduler on calling thread. this method will block until
            // someone calls Stop() on the scheduler.
            scheduler.Start();
        }

        class SchedulerDelegate : ISchedulerDelegate
        {
            public void OnException(IScheduler scheduler, Exception e)
            {
                Debug.WriteLine("Error on scheduler.");
                e.DebugStackTrace();
            }

            public void OnStop(IScheduler scheduler)
            {

            }
        }

        class RequestDelegate : IHttpRequestDelegate
        {
            public void OnRequest(HttpRequestHead request, IDataProducer requestBody, IHttpResponseDelegate response)
            {

                try
                {
                    Console.WriteLine(request.Uri);
                    ControllerRequest controllerRequest = CreateControllerRequest(request);
                    IController controller = CreateController(controllerRequest);

                    if (null == controller)
                    {
                        Return404(request, response);
                        return;
                    }

                    ControllerResponse controllerResponse = controller.Execute(controllerRequest);

                    if (controllerResponse == null || controllerResponse.View == null)
                    {
                        Return404(request, response);
                        return;
                    }
                    string view = GetView(controllerResponse.View);
                    
                    string output = RazorEngine.Razor.Parse<object>(view, controllerResponse.Model);
                    Return200(response, output, controllerResponse.ContentType);
                }
                catch (Exception ex)
                {
                    Return200(response, ex.ToString(), "text/plain");
                }
                    
            }

            private static IController CreateController(ControllerRequest controllerRequest)
            {
                IController controller = null;
                if (controllerRequest.Path.Length == 0 || controllerRequest.Path[0] == "Design")
                {
                    controller = new DesignController();
                }
                else if (controllerRequest.Path[0] == "Data")
                {
                    controller = new DataController();
                }
                return controller;
            }

            private static ControllerRequest CreateControllerRequest(HttpRequestHead request)
            {
                ControllerRequest controllerRequest = new ControllerRequest();
                string[] uriComponents = request.Uri.Split('?');

                if (uriComponents.Length == 0)
                {
                    controllerRequest.Path = new string[0];
                }
                else
                {
                    controllerRequest.Path = (from s in uriComponents[0].Split('/') where !string.IsNullOrWhiteSpace(s) select HttpUtility.UrlDecode(s).Trim()).ToArray();
                }

                controllerRequest.Query = new Dictionary<string, string>();
                if (uriComponents.Length >= 2)
                {
                    string[] queryParts = uriComponents[1].Split('&');
                    foreach (string part in queryParts)
                    {
                        string[] keyValue = part.Split('=');
                        if (keyValue.Length > 0)
                        {
                            controllerRequest.Query.Add(HttpUtility.UrlDecode(keyValue[0]), keyValue.Length > 1 ? HttpUtility.UrlDecode(keyValue[1]) : null);
                        }
                    }
                }
                return controllerRequest;
            }



            private static void Return200(IHttpResponseDelegate response, string content, string contentType)
            {
                var headers = new HttpResponseHead()
                {
                    Status = "200 OK",
                    Headers = new Dictionary<string, string>() 
                    {
                        { "Content-Type", contentType },
                        { "Content-Length", content.Length.ToString() },
                    }
                };

                var body = new BufferedProducer(content);
                response.OnResponse(headers, body);
            }

            private static void Return404(HttpRequestHead request, IHttpResponseDelegate response)
            {
                var responseBody = "The resource you requested ('" + request.Uri + "') could not be found.";
                var headers = new HttpResponseHead()
                {
                    Status = "404 Not Found",
                    Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                };
                var body = new BufferedProducer(responseBody);

                response.OnResponse(headers, body);
            }

            private static string GetView(string viewName)
            {
                using (StreamReader streamReader = new StreamReader("Views\\" + viewName + ".cshtml"))
                {
                    return streamReader.ReadToEnd();
                }
            }

        }

        class BufferedProducer : IDataProducer
        {
            ArraySegment<byte> data;

            public BufferedProducer(string data) : this(data, Encoding.UTF8) { }
            public BufferedProducer(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
            public BufferedProducer(byte[] data) : this(new ArraySegment<byte>(data)) { }
            public BufferedProducer(ArraySegment<byte> data)
            {
                this.data = data;
            }

            public IDisposable Connect(IDataConsumer channel)
            {
                // null continuation, consumer must swallow the data immediately.
                channel.OnData(data, null);
                channel.OnEnd();
                return null;
            }
        }

       
    }
}
