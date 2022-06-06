﻿using System;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;

namespace FryProxy.Headers {

    public class HttpResponseHeader : HttpMessageHeader {

        public const String AgeHeader = "Age";

        public const String EtagHeader = "Etag";

        public const String VaryHeader = "Vary";

        public const String ServerHeader = "Server";

        public const String LocationHeader = "Location";

        public const String RetryAfterHeader = "Retry-After";

        public const String AcceptRangesHeader = "Accept-Ranges";

        public const String WWWAuthenticateHeader = "WWW-Authenticate";
        public const String ProxyAuthenticateHeader = "Proxy-Authenticate";

        private static readonly Regex ResponseLineRegex = new Regex(
            @"HTTP/(?<version>\d\.\d)\s(?<status>\d{3})\s(?<reason>.*)", RegexOptions.Compiled
            );

        public HttpResponseHeader(HttpMessageHeader header) : base(header.StartLine, header.Headers)
        {
            StartLine = base.StartLine;
        }

        public HttpResponseHeader(Int32 statusCode, String statusMessage, String version)
        {
            StatusCode = statusCode;
            Reason = statusMessage;
            Version = version;
        }

        public HttpResponseHeader(String startLine) : base(startLine) {
            StartLine = base.StartLine;
        }

        /// <summary>
        ///     HTTP response status code
        /// </summary>
        public Int32 StatusCode { get; set; }

        /// <summary>
        ///     HTTP protocol version
        /// </summary>
        public String Version { get; set; }

        /// <summary>
        ///     HTTP respnse status message
        /// </summary>
        public String Reason { get; set; }

        /// <summary>
        ///     First line of HTTP response message
        /// </summary>
        /// <exception cref="ArgumentException">
        ///     If Status-Line is invalid
        /// </exception>
        public override sealed String StartLine {
            get { return String.Format("HTTP/{0} {1} {2}", Version, StatusCode, Reason); }

            set {
                var match = ResponseLineRegex.Match(value);

                if (!match.Success) {
                    throw new ArgumentException("Ivalid Response-Line", "value");
                }

                Reason = match.Groups["reason"].Value;
                Version = match.Groups["version"].Value;
                StatusCode = Int32.Parse(match.Groups["status"].Value);

                base.StartLine = value;
            }
        }

        public String Age {
            get { return Headers[AgeHeader]; }
            set { Headers[AgeHeader] = value; }
        }

        public String Etag {
            get { return Headers[EtagHeader]; }
            set { Headers[EtagHeader] = value; }
        }

        public String Vary {
            get { return Headers[VaryHeader]; }
            set { Headers[VaryHeader] = value; }
        }

        public String Server {
            get { return Headers[ServerHeader]; }
            set { Headers[ServerHeader] = value; }
        }

        public String Location {
            get { return Headers[LocationHeader]; }
            set { Headers[LocationHeader] = value; }
        }

        public String RetryAfter {
            get { return Headers[RetryAfterHeader]; }
            set { Headers[RetryAfterHeader] = value; }
        }

        public String AcceptRanges {
            get { return Headers[AcceptRangesHeader]; }
            set { Headers[AcceptRangesHeader] = value; }
        }

        public String WWWAuthenticate {
            get { return Headers[WWWAuthenticateHeader]; }
            set { Headers[WWWAuthenticateHeader] = value; }
        }

        public String ProxyAuthenticate {
            get { return Headers[ProxyAuthenticateHeader]; }
            set { Headers[ProxyAuthenticateHeader] = value; }
        }
    }

}