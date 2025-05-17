using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResClient.Configuration
{
    public class ReqResApiOptions
    {
        public const string SectionName = "ReqResApi";
        public string BaseUrl { get; set; } = "https://reqres.in/api";
        public string ApiKey { get; set; } = "reqres-free-v1";
        public int TimeOutSeconds { get; set; } = 30;
        public bool EnableCaching { get; set; } = true;
        public int CacheTimeoutMinutes { get; set; } = 5;
    }
}
