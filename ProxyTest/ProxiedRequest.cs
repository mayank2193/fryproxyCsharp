using System;
using System.Text;
using FryProxy.Headers;

namespace ProxyTest
{
    class ProxiedRequest
    {
        public ProxiedRequest(
            HttpRequestHeader requestHeader,
            HttpResponseHeader responseHeader,
            byte[] requestBody,
            byte[] responseBody)
        {
            RequestHeader = requestHeader;
            ResponseHeader = responseHeader;

            string requestEncoding = requestHeader.EntityHeaders.ContentEncoding ?? Encoding.UTF8.WebName;
            string responseEncoding = responseHeader.EntityHeaders.ContentEncoding ?? Encoding.UTF8.WebName;

            if (requestBody != null)
            {
                RequestBody = Encoding.GetEncoding(requestEncoding).GetString(requestBody);
            }
            try
            {
                if (responseBody != null)
                {
                    if (responseEncoding == "gzip")
                    {
                        Console.WriteLine("WARNING! Attempt to decode gzip content. URI = " + requestHeader.RequestURI);
                        ResponseBody = string.Empty;
                    }
                    else
                    {
                        ResponseBody = Encoding.GetEncoding(responseEncoding).GetString(responseBody);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR. Exception on processing URI=" + requestHeader.RequestURI);
                Console.WriteLine(e);
            }
        }

        public HttpRequestHeader RequestHeader { get; }
        public HttpResponseHeader ResponseHeader { get; }
        public string RequestBody { get; }
        public string ResponseBody { get; }
    }
}