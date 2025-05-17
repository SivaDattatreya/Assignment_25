using ReqResClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResClient.Interfaces
{
    public interface IExternalUserService
    {
        Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    }
}
