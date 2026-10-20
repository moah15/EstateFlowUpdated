using EstateFlow.Api.Services.Letters;
using EstateFlow.Api.Services.Notifications;
using EstateFlow.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/estates/{estateId:guid}/letters")]
public class LettersController : ControllerBase
{
    private readonly IEstateRepository _repository;
    private readonly LetterGeneratorFactory _factory;
    private readonly INotificationService _notificationService;

    public LettersController(
        IEstateRepository repository,
        LetterGeneratorFactory factory,
        INotificationService notificationService)
    {
        _repository = repository;
        _factory = factory;
        _notificationService = notificationService;
    }

    [HttpPost("vehicle/{assetId:guid}")]
    public async Task<IActionResult> GenerateVehicleLetter(
        Guid estateId,
        Guid assetId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId);

        if (estate is null)
            return NotFound(new
            {
                message = "Estate not found."
            });

        var asset =
            estate.MovableAssets
                .FirstOrDefault(
                    a => a.AssetId == assetId);

        if (asset is null || !asset.IsVehicle)
        {
            return BadRequest(new
            {
                message =
                    "Movable asset not found or is not a vehicle."
            });
        }

        var generator =
            _factory.Create("Vehicle");

        var letter =
            generator.Generate(
                estate,
                asset);

        return File(
            System.Text.Encoding.UTF8.GetBytes(
                letter.Content),
            "text/plain",
            letter.FileName);
    }

    [HttpPost("institution/{claimId:guid}")]
    public async Task<IActionResult> GenerateInstitutionLetter(
        Guid estateId,
        Guid claimId)
    {
        var estate =
            await _repository.GetByIdAsync(estateId);

        if (estate is null)
            return NotFound(new
            {
                message = "Estate not found."
            });

        var claim =
            estate.FinancialClaims
                .FirstOrDefault(
                    c => c.ClaimId == claimId);

        if (claim is null)
        {
            return NotFound(new
            {
                message =
                    "Financial claim not found."
            });
        }

        var generator =
            _factory.Create("FinancialClaim");

        var letter =
            generator.Generate(
                estate,
                claim);

        return File(
            System.Text.Encoding.UTF8.GetBytes(
                letter.Content),
            "text/plain",
            letter.FileName);
    }
}
