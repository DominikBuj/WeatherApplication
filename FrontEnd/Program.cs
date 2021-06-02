using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FrontEnd
{
    class Program
    {
        private static readonly string hostUrl = "http://localhost:8080/";
        private static readonly string apiKey = "IQEdBbRsgJ";
        private static readonly string[] cities = new string[]
        {
            "Warszawa",
            "Łódź",
            "Wrocław",
            "Szczecin",
            "Rzeszów",
            "Kraków",
            "Gdańsk",
            "Suwałki"
        };
        public enum HttpMethod
        {
            GET,
            POST,
            PUT,
            PATCH,
            DELETE
        }

        private static string ReadFile(string fileName)
        {
            try
            {
                return File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not read the file: {fileName}");
                Console.WriteLine(e.Message);
            }
            return string.Empty;
        }

        public class RestClient
        {
            public string EndPoint { get; set; }

            public RestClient()
            {
                EndPoint = "http://localhost:5000/api/weather/";
            }

            public string MakeRequest(string request = "", string parameters = "")
            {
                string responseString = String.Empty;

                try
                {
                    //  Initializing the request
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(EndPoint + request + parameters);
                    httpWebRequest.Method = HttpMethod.POST.ToString();
                    httpWebRequest.ContentLength = 0;
                    httpWebRequest.ContentType = "text/json";
                    httpWebRequest.Headers["ApiKey"] = apiKey;

                    HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

                    //  Checking the HTTP status code
                    HttpStatusCode status = response.StatusCode;
                    if (status != HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Request {HttpMethod.POST.ToString()} failed! Received HTTP code {status}");
                        throw new ApplicationException("Could not service a request!");
                    }

                    //  Reading the response
                    using Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        using StreamReader reader = new StreamReader(responseStream);
                        responseString = reader.ReadToEnd();
                    }
                }
                catch (ApplicationException e)
                {
                    Console.WriteLine($"Application exception: {e.Message}");
                    return "ERROR";
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unhandled exception: {e.Message}");
                    return "ERROR";
                }

                RestResponse restResponse = JsonConvert.DeserializeObject<RestResponse>(responseString);
                responseString = restResponse.AverageValue;
                return responseString;
            }
        }

        class HttpServer
        {
            private static HttpListener listener = null;
            private static RestClient restClient = null;
            private static string pageHtml = ReadFile("pageHtml.txt");

            private static string FormatProperty(string property)
            {
                return float.Parse(property, CultureInfo.InvariantCulture.NumberFormat).ToString("0.00");
            }
            private static string GetPropertyRow(string property)
            {
                string propertyRow = String.Empty;
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Warszawa&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Łódź&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Szczecin&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Wrocław&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Gdańsk&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Kraków&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Suwałki&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("average", $"?c=Rzeszów&p={property}&d=7"))}</td>";
                propertyRow += $"<td>{FormatProperty(restClient.MakeRequest("poland", $"?p={property}&d=7"))}</td>";
                return propertyRow;
            }

            private static string GetTableHtml()
            {
                string tableHtml = String.Empty;
                string tableStyle = @"{border: 1px solid black; text-align: center;}";
                tableHtml = String.Format(pageHtml, GetPropertyRow("Temperature"), GetPropertyRow("Pressure"),
                    GetPropertyRow("Humidity"), GetPropertyRow("Precipitation"), GetPropertyRow("WindSpeed"),
                    GetPropertyRow("WindDirection"), tableStyle);
                return tableHtml;
            }

            private static byte[] PrepareResponse(HttpListenerRequest request, ref HttpListenerResponse response)
            {
                string requestUrl = request.Url.ToString();
                Console.WriteLine("Received request");
                Console.WriteLine($"Method: {request.HttpMethod} Url: {requestUrl}");
                string urlParameters = requestUrl.Split(hostUrl)[1];
                if (urlParameters.Equals("favicon.ico")) return new byte[] { };
                string bodyHtml = GetTableHtml();
                byte[] responseData = Encoding.UTF8.GetBytes(bodyHtml);
                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = responseData.LongLength;
                return responseData;
            }

            private static async Task HandleRequests()
            {
                try
                {
                    while (true)
                    {
                        HttpListenerContext context = await listener.GetContextAsync();
                        HttpListenerRequest request = context.Request;
                        HttpListenerResponse response = context.Response;
                        byte[] responseData = PrepareResponse(request, ref response);
                        await response.OutputStream.WriteAsync(responseData, 0, responseData.Length);
                        response.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not handle a request!");
                    Console.WriteLine(e.Message);
                }
            }

            public HttpServer()
            {
                try
                {
                    restClient = new RestClient();
                    listener = new HttpListener();
                    listener.Prefixes.Add(hostUrl);
                    listener.Start();
                    Console.WriteLine("Listening for incoming requests...");
                    Console.WriteLine($"Go to address: {hostUrl}");

                    Task handleRequestsTask = HandleRequests();
                    handleRequestsTask.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not start the listener!");
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Console.WriteLine("Closing the listener...");
                    if (listener != null) listener.Close();
                    Console.WriteLine("Done!");
                }
            }
        }

        static void Main(string[] args)
        {
            HttpServer server = new HttpServer();
        }
    }
}
