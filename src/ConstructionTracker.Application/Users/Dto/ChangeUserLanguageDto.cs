using System.ComponentModel.DataAnnotations;

namespace ConstructionTracker.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}