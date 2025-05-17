using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResClient.Interfaces
{
    public interface IReqResApiClient
    {
        Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);
    }
}
