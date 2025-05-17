using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ReqResClient.Configuration;
using ReqResClient.Exceptions;
using ReqResClient.Interfaces;
using ReqResClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReqResClient.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IReqResApiClient _apiClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExternalUserService> _logger;
        private readonly ReqResApiOptions _options;
        public ExternalUserService(IReqResApiClient apiClient, IMemoryCache cache,
            IOptions<ReqResApiOptions> options, ILogger<ExternalUserService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            string cacheKey = $"user_{userId}";

            if(_options.EnableCaching && _cache.TryGetValue(cacheKey, out User? cachedUser) &&  cachedUser != null)
            {
                _logger.LogInformation("Retrieved user {UserId} from cache", userId);
                return cachedUser;
            }
            try
            {
                var response = await _apiClient.GetAsync<UserResponse>($"users/{userId}", cancellationToken);

                if(response.Data == null)
                {
                    throw new NotFoundException($"User wid ID {userId} not found");
                }

                if (_options.EnableCaching)
                {
                    _cache.Set(cacheKey, response.Data, TimeSpan.FromMinutes(_options.CacheTimeoutMinutes));
                }
                return response.Data;
            }
            catch (NotFoundException)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                throw;
            }
            catch(Exception ex) when (ex is not NotFoundException) 
            {
                _logger.LogError(ex, "Failed to get user with ID {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            string cacheKey = "all_users";

            if(_options.EnableCaching && _cache.TryGetValue(cacheKey, out IEnumerable<User>? cachedUsers) && cachedUsers != null)
            {
                _logger.LogInformation("Retrieved all users from cache");
                return cachedUsers;
            }
            try
            {
                var allUsers = new List<User>();
                int currentPage = 1;
                int totalPages;

                do
                {
                    var response = await _apiClient.GetAsync<PaginatedResponse<User>>(
                        $"users?page={currentPage}", cancellationToken);

                    allUsers.AddRange(response.Data);
                    totalPages = response.TotalPages;
                    currentPage++;
                }
                while (currentPage <= totalPages);

                if (_options.EnableCaching)
                {
                    _cache.Set(cacheKey, allUsers, TimeSpan.FromMinutes(_options.CacheTimeoutMinutes));
                }
                return allUsers;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to get all users");
                throw;
            }
        }        
    }
}
