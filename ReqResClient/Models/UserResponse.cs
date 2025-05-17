using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReqResClient.Models
{
    public class UserResponse
    {
        [JsonPropertyName("data")]
        public User? Data { get; set; }
    }
}
