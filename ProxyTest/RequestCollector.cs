using System;
using System.Collections.Generic;
using System.Linq;
using FryProxy;
using FryProxy.Headers;
using log4net;

namespace ProxyTest
{
    /// <summary>
    ///     Collect information about incoming proxy HTTP request
    /// </summary>
    public class RequestCollector
    {
        /// <summary>
        ///     Filter which dismiss everything
        /// </summary>
        public static readonly Predicate<HttpRequestHeader> DismissAllFilter = _ => false;

        protected readonly ILog Logger;
        private readonly List<ProxiedRequest> _proxiedRequests;
        private readonly List<string> _blackListOfURLs;
        private readonly bool _collectBody;
        /// <summary>
        ///     Filter applied to every request saying whether it should be collected or not
        /// </summary>
        public Predicate<HttpRequestHeader> RequestFilter;

        public RequestCollector(Predicate<HttpRequestHeader> requestFilter, bool collectBody = false) : this(collectBody)
        {
            RequestFilter = requestFilter;
        }

        public RequestCollector(bool collectBody = false)
        {
            Logger = LogManager.GetLogger(GetType());
            _proxiedRequests = new List<ProxiedRequest>();
            _blackListOfURLs = new List<string>();
            _collectBody = collectBody;
        }

        /// <summary>
        ///     Readonly collection of collected requests headers
        /// </summary>
        public ICollection<HttpRequestHeader> Requests
        {
            get { return _proxiedRequests.Select(r => r.RequestHeader).ToList(); }
        }

        /// <summary>
        ///     Collection of collected URIs
        /// </summary>
        public IEnumerable<String> RequestUriList
        {
            get
            {
                if (Requests == null || !Requests.Any()) return new List<string>();
                return Requests.Where(_ => _ != null).Select(_ => _.RequestURI);
            }
        }

        /// <summary>
        ///     Collection of collected URLs
        /// </summary>
        public IEnumerable<String> RequestUrlList
        {
            get { return Requests.Where(_ => _ != null).Select(_ => _.Host + _.RequestURI); }
        }

        /// <summary>
        ///     Collection of collected request bodies
        /// </summary>
        public IEnumerable<String> RequestBodyList
        {
            get { return _proxiedRequests.Where(_ => _?.RequestBody != null).Select(r => r.RequestBody).ToList(); }
        }

        public string GetRequestBodyByUri(string requestUri)
        {
            return _proxiedRequests.Where(r => r.RequestHeader.RequestURI.Contains(requestUri))
                .Select(r => r.RequestBody)
                .FirstOrDefault();
        }

        public string GetRequestBodyByRequestMethod(string requestMethod)
        {
            return _proxiedRequests.Where(x => x.RequestBody != null && x.RequestBody.Contains(requestMethod))
                .Select(x => x.RequestBody).FirstOrDefault();
        }

        /// <summary>
        ///     Collection of collected response bodies
        /// </summary>
        public IEnumerable<String> ResponseBodyList
        {
            get { return _proxiedRequests.Select(r => r.ResponseBody).ToList(); }
        }

        /// <summary>
        /// Gets the response headers 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpHeaders ResponseHeadersByRequest(string url)
        {
            if (_proxiedRequests
                .Any(r => r.RequestHeader.RequestURI.Contains(url) & r.ResponseHeader.StatusCode == 200))
            {
                return _proxiedRequests
                    .Where(r => r.RequestHeader.RequestURI.Contains(url) & r.ResponseHeader.StatusCode == 200)
                    .Select(r => r.ResponseHeader).ToList().FirstOrDefault().Headers;
            }
            return null;
        }

        public string GetResponseBodyByRequestMethod(string requestMethod, bool getLast = false)
        {
            var requests = _proxiedRequests.Where(x => x.RequestBody != null && x.RequestBody.Contains(requestMethod))
                .Select(x => x.ResponseBody);
            return getLast ? requests.LastOrDefault() : requests.FirstOrDefault();
        }

        /// <summary>
        ///     Start collecting requests for given browser
        /// </summary>
        /// <param name="browser">procied browser</param>
        public void Attach(ProxiedBrowser browser)
        {
            browser.Proxy.OnRequestReceived += HandleRequestEvent;
            browser.Proxy.OnResponseReceived += HandleResponseEvent;
            browser.Proxy.OnProcessingComplete += HandleProcessingComplete;
        }

        private void HandleRequestEvent(ProcessingContext context)
        {
            if (context == null)
            {
                Logger.Warn("ProcessingContext is null");
                return;
            }

            if (Filter(context))
            {
                Collect(context);
            }
        }

        protected virtual void Collect(ProcessingContext context)
        {
            if (ShouldCollectBody(context))
            {
                context.RequestHeader.AcceptEncoding = "identity";
                context.ClientStream = new BufferedReadStream(context.ClientStream);
            }
        }


        private void HandleResponseEvent(ProcessingContext context)
        {
            if (context == null)
            {
                Logger.Warn("ProcessingContext is null");
                return;
            }

            if (Filter(context) && ShouldCollectBody(context))
            {
                context.ServerStream = new BufferedReadStream(context.ServerStream);
            }

            if (_blackListOfURLs.Count != 0 &&
                _blackListOfURLs.Any(b => context.RequestHeader.RequestURI.Contains(b)))
            {
                context.ResponseHeader.StatusCode = 404;
                context.ServerStream = null;
            }
        }

        private void HandleProcessingComplete(ProcessingContext context)
        {
            if (context == null || !Filter(context))
            {
                return;
            }

            var bufferedRequestStream = context.ClientStream as BufferedReadStream;
            var bufferedResponseStream = context.ServerStream as BufferedReadStream;

            _proxiedRequests.Add(

                new ProxiedRequest(context.RequestHeader,
                    context.ResponseHeader,
                    bufferedRequestStream?.Content,
                    bufferedResponseStream?.Content)
            );
        }

        public bool AreRequestsProcessed()
        {
            return (_proxiedRequests.Count > 0);
        }

        /// <summary>
        /// Check if requestHeader URI contains specific request filter
        /// </summary>
        /// <param name="requestFilter"></param>
        /// <returns></returns>
        public bool IsSpecificRequestProcessed(string requestFilter)
        {
            return _proxiedRequests.Count > 0 && _proxiedRequests
                .Any(r => r.RequestHeader.RequestURI != null && r.RequestHeader.RequestURI.Contains(requestFilter) || r.RequestBody != null && r.RequestBody.Contains(requestFilter));
        }

        /// <summary>
        /// Check if requestHeader URI contains specific category and action
        /// </summary>
        /// <param name="eventCategory"></param>
        /// <param name="eventAction"></param>
        /// <returns></returns>
        public bool IsSpecificRequestProcessedByCategoryAction(string eventCategory, string eventAction)
        {
            string filter = $"ec={eventCategory}&ea={eventAction}";
            return _proxiedRequests.Count > 0 && _proxiedRequests
                .Any(r => System.Uri.UnescapeDataString(r.RequestHeader.RequestURI).Contains(filter));
        }

        /// <summary>
        ///     Remove all collected requests
        /// </summary>
        public virtual void Clear()
        {
            if (_proxiedRequests.Count > 0)
            {
                _proxiedRequests.Clear();
            }
        }

        /// <summary>
        /// Add URL to black list
        /// For this URL 404 error will be simulated
        /// </summary>
        /// <param name="url"></param>
        public void AddUrlToBlackList(string url)
        {
            _blackListOfURLs.Add(url);
        }

        /// <summary>
        /// Add list of URLs to black list
        /// </summary>
        /// <param name="urlList"></param>
        public void AddListOfURLsToBlackList(List<string> urlList)
        {
            _blackListOfURLs.AddRange(urlList);
        }

        /// <summary>
        /// Clear all URLs from black list
        /// </summary>
        public void ClearBlackListOfURLs()
        {
            _blackListOfURLs.Clear();
        }

        protected virtual Boolean Filter(ProcessingContext context)
        {
            if (context.RequestHeader == null)
            {
                Logger.Warn("ProcessingContext.RequestHeader is not available");
                return false;
            }
            try
            {
                return RequestFilter == null || RequestFilter(context.RequestHeader);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in custom filer", ex);
            }

            return false;
        }

        private bool ShouldCollectBody(ProcessingContext context)
        {
            return _collectBody
                   && context?.ClientStream != null
                   && (context.RequestHeader?.Method?.Equals("post", StringComparison.InvariantCultureIgnoreCase) ?? false);
        }
    }
}