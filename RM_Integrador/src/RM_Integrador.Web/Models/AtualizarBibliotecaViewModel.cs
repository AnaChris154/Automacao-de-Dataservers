using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RM_Integrador.Web.Models
{
    public class AtualizarBibliotecaViewModel
    {
        [Required(ErrorMessage = "O nome do DataServer é obrigatório")]
        [Display(Name = "Nome do DataServer")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "A rotina é obrigatória")]
        [Display(Name = "Rotina")]
        public string Routine { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Palavras-chave (separadas por vírgula)")]
        public string Keywords { get; set; } = string.Empty;

        [Display(Name = "Chaves Primárias (separadas por vírgula)")]
        public string PrimaryKeys { get; set; } = string.Empty;

        [Display(Name = "Exemplo de GET")]
        public string GetExample { get; set; } = string.Empty;

        [Display(Name = "Exemplo de POST")]
        public string PostExample { get; set; } = string.Empty;
        
        // Campos de Documentação/Dicas
        [Display(Name = "Erros Comuns")]
        public string? CommonErrors { get; set; }
        
        [Display(Name = "Dicas de Consumo")]
        public string? ConsumptionTips { get; set; }
        
        [Display(Name = "Dicas de Filtros")]
        public string? FilterTips { get; set; }
        
        [Display(Name = "Exemplos de Uso")]
        public string? UsageExamples { get; set; }
        
        [Display(Name = "Observações Gerais")]
        public string? Notes { get; set; }
        
        // Campo para controlar se o usuário quer adicionar dicas
        public bool WantToAddDocumentation { get; set; } = false;
    }
}