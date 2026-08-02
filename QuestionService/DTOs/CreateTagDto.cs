using System.ComponentModel.DataAnnotations;

namespace QuestionService.DTOs;

public record CreateTagDto([Required]string Name, [Required] string Slug, [Required]string Description);