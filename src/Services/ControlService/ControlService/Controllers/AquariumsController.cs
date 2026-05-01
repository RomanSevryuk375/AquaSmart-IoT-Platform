using Control.Application.DTOs.Ecosystem;
using Control.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Control.API.Controllers;

[ApiController]
[Route("api/control/v1/aquariums")]
public class AquariumsController(IEcosystemService aquariumService) : ControllerBase
{
    private const string NameGetById = "GetAquariumById";

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EcosystemResponseDto>>> GetAllAquariumsAsync(
        [FromQuery] EcosystemFilterDto filter,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await aquariumService.GetAllEcosystemsAsync(filter, skip, take, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}", Name = NameGetById)]
    public async Task<ActionResult<EcosystemResponseDto>> GetAquariumByIdAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await aquariumService.GetAquariumByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAquariumAsync(
        [FromBody] EcosystemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var id = await aquariumService.CreateEcosystemAsync(request, cancellationToken);
        var createdData = await aquariumService.GetAquariumByIdAsync(id, cancellationToken);

        return CreatedAtRoute(
            NameGetById,
            new { id },
            createdData);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateAquariumAsync([FromRoute] Guid id,
        [FromBody] EcosystemUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await aquariumService.UpdateEcosystemAsync(id, request, cancellationToken);
        return NoContent(); 
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAquariumAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await aquariumService.DeleteEcosystemAsync(id, cancellationToken);
        return NoContent(); 
    }
}
