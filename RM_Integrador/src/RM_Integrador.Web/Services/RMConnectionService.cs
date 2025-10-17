using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace RM_Integrador.Web.Services
{
    public interface IRMConnectionService
    {
        Task<bool> ValidateConnection(string baseUrl, string username, string password);
    }

    public class RMConnectionService : IRMConnectionService
    {
        private readonly IHttpClientFactory _clientFactory;

        public RMConnectionService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<bool> ValidateConnection(string baseUrl, string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    return false;
                }

                using var client = _clientFactory.CreateClient("RMClient");
                var response = await client.GetAsync(baseUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}