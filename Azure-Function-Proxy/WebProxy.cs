using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Azure_Function_Proxy
{
    public static class WebProxy
    {

        public struct HttpResponse
        {
            public HttpResponseMessage ResponseMessage;
            public string Content;
        }

        [FunctionName("WebProxy")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var url = req.Query["url"].ToString();

            if (url == "")
                return new BadRequestObjectResult("Please pass an url on the query string");

            var t = Task.Run(() => RequestHandler(url));
            t.Wait();

            return CreateResponse(t.Result);
        }

        public static ContentResult CreateResponse(HttpResponse httpResponse)
        {
            return new ContentResult()
            {
                Content = httpResponse.Content,
                StatusCode = (int)httpResponse.ResponseMessage.StatusCode,
                ContentType = httpResponse.ResponseMessage.Content.Headers.ContentType.ToString(),
            };
        }

        public static async Task<HttpResponse> RequestHandler(string url)
        {
            using (var client = new HttpClient())
            {
                using (var response = await client.GetAsync(url))
                {
                    using (var content = response.Content)
                    {
                        var responseContent = await content.ReadAsStringAsync();
                        return new HttpResponse()
                        {
                            ResponseMessage = response,
                            Content = responseContent
                        };
                    }
                }
            }
        }

    }
}
