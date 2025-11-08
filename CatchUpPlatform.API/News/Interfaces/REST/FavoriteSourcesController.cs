using System.Net.Mime;
using CatchUpPlatform.API.News.Domain.Model.Queries;
using CatchUpPlatform.API.News.Domain.Services;
using CatchUpPlatform.API.News.Interfaces.REST.Resources;
using CatchUpPlatform.API.News.Interfaces.REST.Transform;
using CatchUpPlatform.API.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;

namespace CatchUpPlatform.API.News.Interfaces.REST;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Favorite Sources")]
public class FavoriteSourcesController(
    IFavoriteSourceCommandService favoriteSourceCommandService,
    IFavoriteSourceQueryService favoriteSourceQueryService,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{

    [HttpGet("{id}:int")]
    [SwaggerOperation(
        Summary = "Gets a favorite source by id",
        Description = "Gets a favorite source for a given id",
        OperationId = "GetFavoriteSourceById")]
    [SwaggerResponse(200, "Favorite source found", typeof(FavoriteSourceResource))]
    [SwaggerResponse(404, "Favorite source not found")]
    public async Task<IActionResult> GetFavoriteSourceById(int id)
    {
        var getFavoriteSourceByIdQuery = new GetFavoriteSourceByIdQuery(id);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByIdQuery);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a favorite source",
        Description = "Creates a favorite source with the provided information",
        OperationId = "CreateFavoriteSource")]
    [SwaggerResponse(201, "Favorite source created", typeof(FavoriteSourceResource))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(409, "Favorite source with this SourceId and NewsApiKey already exists")]
    public async Task<IActionResult> CreateFavoriteSource([FromBody] CreateFavoriteSourceResource resource)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var createFavoriteSourceCommand = CreateFavoriteSourceCommandFromResourceAssembler.ToCommandFromResource(resource);
        try
        {
            var result = await favoriteSourceCommandService.Handle(createFavoriteSourceCommand);
            if (result is null) return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
            return CreatedAtAction(nameof(GetFavoriteSourceById), new { id = result.Id },
                FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result));
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(localizer["NewsFavoriteSourceDuplicated"].Value);
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }

    private async Task<IActionResult> GetFavoriteSourceByNewsApiKeyAndSourceId(string newsApiKey, string sourceId)
    {
        var getFavoriteSourceByNewsApiKeyAndSourceIdQuery =
            new GetFavoriteSourceByNewsApiKeyAndSourceIdQuery(newsApiKey, sourceId);
        var result = await favoriteSourceQueryService.Handle(getFavoriteSourceByNewsApiKeyAndSourceIdQuery);
        if (result is null) return NotFound();
        var resource = FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity(result);
        return Ok(resource);
    }

    private async Task<IActionResult> GetAllFavoriteSourcesByNewsApiKey(string newsApiKey)
    {
        var getAllFavoriteSourcesByNewsApiKeyQuery = new GetAllFavoriteSourcesByNewsApiKeyQuery(newsApiKey);
        var result = await favoriteSourceQueryService.Handle(getAllFavoriteSourcesByNewsApiKeyQuery);
        var resources = result.Select(FavoriteSourceResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Gets favorite sources by NewsApiKey and optional SourceId",
        Description = "Gets favorite sources for a given NewsApiKey and optional SourceId",
        OperationId = "GetFavoriteSourcesFromQuery")]
    [SwaggerResponse(200, "Favorite source(s) found", typeof(IEnumerable<FavoriteSourceResource>))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<IActionResult> GetFavoriteSourcesFromQuery([FromQuery] string newsApiKey,
        [FromQuery] string sourceId = "")
    {
        if (string.IsNullOrEmpty(newsApiKey)) return BadRequest();
        return string.IsNullOrEmpty(sourceId)
            ? await GetAllFavoriteSourcesByNewsApiKey(newsApiKey)
            : await GetFavoriteSourceByNewsApiKeyAndSourceId(newsApiKey, sourceId);
    }
    
}