using System;
using FryProxy;

namespace ProxyTest
{
    public interface IBrowserProxy
    {

        /// <summary>
        ///     Host proxy is listening on
        /// </summary>
        String HostName { get; }

        /// <summary>
        ///     Port HTTP proxy is listening on
        /// </summary>
        Int32 HttpPort { get; }

        /// <summary>
        ///     Port SSL proxy is listening on
        /// </summary>
        Int32 SslPort { get; }

        /// <summary>
        ///     Fired when browser sends a request
        /// </summary>
        Action<ProcessingContext> OnRequestReceived { get; set; }

        /// <summary>
        ///     Fired when browser receives response
        /// </summary>
        Action<ProcessingContext> OnResponseReceived { get; set; }

        Action<ProcessingContext> OnProcessingComplete { get; set; }

        Action<ProcessingContext> OnServerConnected { get; set; }

        Action<ProcessingContext> OnResponseSent { get; set; }
        /// <summary>
        ///     Whether current proxy is active
        /// </summary>
        Boolean IsListening { get; }

        /// <summary>
        ///     Stop accepting requests
        /// </summary>
        void Stop();

        /// <summary>
        ///     Start accepting requests
        /// </summary>
        void Start();

    }
}