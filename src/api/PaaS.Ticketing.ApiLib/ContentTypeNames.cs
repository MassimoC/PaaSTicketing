using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaaS.Ticketing.ApiLib
{
    public static class ContentTypeNames
    {
        public static class Application
        {
            public const string Json = "application/json";
            public const string JsonPatch = "application/json-patch+json";
            public const string JsonMerge = "application/merge-patch+json";
            public const string JsonProblem = "application/problem+json";
            public const string XmlProblem = "application/problem+xml";
        }
        public static class Text
        {
            public const string Xml = "text/xml";
            public const string Json = "text/json";
            public const string Plain = "text/plain";
        }
    }
}
