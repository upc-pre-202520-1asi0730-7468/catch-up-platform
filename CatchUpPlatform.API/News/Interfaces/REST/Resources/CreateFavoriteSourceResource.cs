using System.ComponentModel.DataAnnotations;

namespace CatchUpPlatform.API.News.Interfaces.REST.Resources;

public record CreateFavoriteSourceResource(
    [Required]
    string NewsApiKey, 
    [Required]
    string SourceId);