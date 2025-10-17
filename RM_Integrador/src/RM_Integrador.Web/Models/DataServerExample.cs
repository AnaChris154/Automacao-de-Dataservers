using System;
using System.ComponentModel.DataAnnotations;

namespace RM_Integrador.Web.Models
{
    public class DataServerExample
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string DataServerName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string JsonExample { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Method { get; set; } = "GET";

        public DateTime CreatedAt { get; set; }
    }
}