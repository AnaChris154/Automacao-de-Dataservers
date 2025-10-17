using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RM_Integrador.Web.Data;
using RM_Integrador.Shared.Models;

namespace RM_Integrador.Web.Services
{
    public class DataServerSearchService : IDataServerSearchService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataServerSearchService> _logger;

        public DataServerSearchService(ApplicationDbContext context, ILogger<DataServerSearchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método usado pelo JsonViewer
        public async Task<IEnumerable<DataServerInfo>> SearchExamplesAsync(string dataServerName)
        {
            try
            {
                if (string.IsNullOrEmpty(dataServerName))
                {
                    return await _context.DataServers
                        .Select(ds => new DataServerInfo
                        {
                            Id = ds.Id,
                            Name = ds.Name,
                            Routine = ds.Routine,
                            Description = ds.Description,
                            Keywords = ds.Keywords,
                            PrimaryKeys = ds.PrimaryKeys,
                            GetExample = ds.GetExample,
                            PostExample = ds.PostExample,
                            CommonErrors = ds.CommonErrors,
                            ConsumptionTips = ds.ConsumptionTips,
                            FilterTips = ds.FilterTips,
                            UsageExamples = ds.UsageExamples,
                            Notes = ds.Notes
                        })
                        .OrderBy(e => e.Name)
                        .Take(10)
                        .ToListAsync();
                }

                return await _context.DataServers
                    .Where(e => e.Name.Contains(dataServerName))
                    .Select(ds => new DataServerInfo
                    {
                        Id = ds.Id,
                        Name = ds.Name,
                        Routine = ds.Routine,
                        Description = ds.Description,
                        Keywords = ds.Keywords,
                        PrimaryKeys = ds.PrimaryKeys,
                        GetExample = ds.GetExample,
                        PostExample = ds.PostExample,
                        CommonErrors = ds.CommonErrors,
                        ConsumptionTips = ds.ConsumptionTips,
                        FilterTips = ds.FilterTips,
                        UsageExamples = ds.UsageExamples,
                        Notes = ds.Notes
                    })
                    .OrderBy(e => e.Name)
                    .Take(10)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching examples: {ex.Message}");
                throw;
            }
        }

        // Método usado pelo SearchDS
        public async Task<IEnumerable<DataServerInfo>> SearchByKeywordsAsync(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                    return new List<DataServerInfo>();

                var searchTerms = term.ToLower()
                    .Replace("como", "")
                    .Replace("no rm", "")
                    .Replace("um", "")
                    .Replace("uma", "")
                    .Trim()
                    .Split(' ')
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();

                var results = await _context.DataServers
                    .Where(ds => ds.Keywords != null)
                    .ToListAsync();

                return results
                    .Where(ds => ds.Keywords != null &&
                        searchTerms.Any(term =>
                            string.Join(",", ds.Keywords).ToLower().Contains(term)))
                    .Select(ds => new DataServerInfo
                    {
                        Id = ds.Id,
                        Name = ds.Name,
                        Routine = ds.Routine,
                        Description = ds.Description,
                        Keywords = ds.Keywords,
                        PrimaryKeys = ds.PrimaryKeys,
                        GetExample = ds.GetExample,
                        PostExample = ds.PostExample,
                        CommonErrors = ds.CommonErrors,
                        ConsumptionTips = ds.ConsumptionTips,
                        FilterTips = ds.FilterTips,
                        UsageExamples = ds.UsageExamples,
                        Notes = ds.Notes
                    })
                    .OrderByDescending(ds =>
                        searchTerms.Count(term =>
                            string.Join(",", ds.Keywords).ToLower().Contains(term)))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro na busca por keywords: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<DataServerInfo>> SearchDataServersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<DataServerInfo>();

            return await _context.DataServers
                .Where(ds => ds.Name.Contains(searchTerm) || 
                           ds.Routine.Contains(searchTerm) || 
                           ds.Description.Contains(searchTerm))
                .Select(ds => new DataServerInfo
                {
                    Id = ds.Id,
                    Name = ds.Name,
                    Routine = ds.Routine,
                    Description = ds.Description,
                    GetExample = ds.GetExample,
                    PostExample = ds.PostExample,
                    Keywords = ds.Keywords,
                    PrimaryKeys = ds.PrimaryKeys,
                    CommonErrors = ds.CommonErrors,
                    ConsumptionTips = ds.ConsumptionTips,
                    FilterTips = ds.FilterTips,
                    UsageExamples = ds.UsageExamples,
                    Notes = ds.Notes
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<DataServerInfo>> GetAllDataServersAsync()
        {
            try
            {
                return await _context.DataServers
                    .Select(ds => new DataServerInfo
                    {
                        Id = ds.Id,
                        Name = ds.Name,
                        Routine = ds.Routine,
                        Description = ds.Description,
                        Keywords = ds.Keywords,
                        PrimaryKeys = ds.PrimaryKeys,
                        GetExample = ds.GetExample,
                        PostExample = ds.PostExample,
                        CommonErrors = ds.CommonErrors,
                        ConsumptionTips = ds.ConsumptionTips,
                        FilterTips = ds.FilterTips,
                        UsageExamples = ds.UsageExamples,
                        Notes = ds.Notes
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar todos os DataServers: {ex.Message}");
                throw;
            }
        }
    }
}