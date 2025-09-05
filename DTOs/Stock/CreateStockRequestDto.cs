using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DTOs
{
    public class CreateStockRequestDto
    {
        [Required]
        [MaxLength(10, ErrorMessage = "Symbol cannot exceed 10 characters.")]
        public string Symbol { get; set; } = string.Empty;
        [Required]
        [MaxLength(16, ErrorMessage = "Company Name cannot exceed 16 characters.")]
        public string CompanyName { get; set; } = string.Empty;
        [Required]
        [Range(1, 1000000, ErrorMessage = "Purchase price must be between 1 and 1,000,000.")]
        public decimal Purchase { get; set; }
        [Required]
       [Range(0.001, 1000, ErrorMessage = "Shares must be between 0.001 and 1,000.")]
        public decimal LastDiv { get; set; }
        [Required]
        [MaxLength(16, ErrorMessage = "Industry Name cannot exceed 16 characters.")]
        public string Industry { get; set; } = string.Empty;
        [Required]
        [Range(1, 99999999999999, ErrorMessage = "Market Cap must be between 1 and 99,999,999,999.")]
        public long MarketCap { get; set; } 
    }
}