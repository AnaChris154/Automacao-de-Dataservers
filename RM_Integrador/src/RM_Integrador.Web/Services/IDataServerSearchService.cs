using System.Collections.Generic;
using System.Threading.Tasks;
using RM_Integrador.Shared.Models; // Alterado de ConsumoDS.Models

namespace RM_Integrador.Web.Services
{
    public interface IDataServerSearchService
    {
        Task<IEnumerable<DataServerInfo>> SearchByKeywordsAsync(string term);
        Task<IEnumerable<DataServerInfo>> GetAllDataServersAsync();
        Task<IEnumerable<DataServerInfo>> SearchDataServersAsync(string searchTerm);
        Task<IEnumerable<DataServerInfo>> SearchExamplesAsync(string dataServerName);
    }
}