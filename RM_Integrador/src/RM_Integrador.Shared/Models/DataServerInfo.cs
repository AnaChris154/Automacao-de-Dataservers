using System.Collections.Generic;

namespace RM_Integrador.Shared.Models
{
    public class DataServerInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Routine { get; set; } = string.Empty;
        public List<string> PrimaryKeys { get; set; } = new List<string>();
        public string Description { get; set; } = string.Empty;
        public string GetExample { get; set; } = "{}";
        public string PostExample { get; set; } = "{}";
        public bool RequiresFilter { get; set; }
        public string? FilterFormat { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        
        // Campos de Documentação/Dicas
        public string? CommonErrors { get; set; }
        public string? ConsumptionTips { get; set; }
        public string? FilterTips { get; set; }
        public string? UsageExamples { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastDocumentationUpdate { get; set; }
        public string? DocumentationAuthor { get; set; }
    }
}