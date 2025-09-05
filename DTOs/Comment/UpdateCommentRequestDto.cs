using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.DTOs.Comment
{
    public class UpdateCommentRequestDto
    {
        [Required]
        [MinLength(8, ErrorMessage = "Title must be at least 8 characters long.")]
        [MaxLength(64, ErrorMessage = "Title cannot exceed 64 characters.")]
         public string Title { get; set; } = string.Empty;
           [Required]
          [MinLength(8, ErrorMessage = "Body must be at least 8 characters long.")]
        [MaxLength(256, ErrorMessage = "Body cannot exceed 256 characters.")]
        public string Body { get; set; } = string.Empty;
    }
}