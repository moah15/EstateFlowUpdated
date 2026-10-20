using EstateFlow.Api.Services;
using EstateFlow.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EstatesController : ControllerBase
{
    private readonly IEstateService _estateService;

    public EstatesController(
        IEstateService estateService)
    {
        _estateService = estateService;
    }

    // ─────────────────────────────────────────────
    // Estates
    // ─────────────────────────────────────────────

    [HttpGet]
    public async Task<ActionResult<List<EstateSummaryDto>>>
        GetAll()
    {
        var controllerIdClaim =
            User.FindFirst("controllerId")?.Value;

        if (controllerIdClaim is null ||
            !Guid.TryParse(controllerIdClaim, out var controllerId))
        {
            return Forbid();
        }

        var estates =
            await _estateService
                .GetEstatesForControllerAsync(controllerId);

        return Ok(estates);
    }

    [HttpGet("{estateId:guid}")]
    public async Task<ActionResult<EstateDto>>
        GetById(Guid estateId)
    {
        var estate =
            await _estateService.GetEstateAsync(
                estateId);

        if (estate is null)
        {
            return NotFound(new
            {
                message =
                    $"Estate {estateId} was not found."
            });
        }

        var controllerIdClaim =
            User.FindFirst("controllerId")?.Value;

        if (controllerIdClaim is null ||
            !Guid.TryParse(controllerIdClaim, out var controllerId) ||
            estate.ControllerId != controllerId)
        {
            return NotFound(new
            {
                message =
                    $"Estate {estateId} was not found."
            });
        }

        return Ok(estate);
    }

    [HttpGet("controller/{controllerId:guid}")]
    public async Task<ActionResult<List<EstateSummaryDto>>>
        GetByController(Guid controllerId)
    {
        var controllerIdClaim =
            User.FindFirst("controllerId")?.Value;

        if (controllerIdClaim is null ||
            !Guid.TryParse(controllerIdClaim, out var currentControllerId) ||
            currentControllerId != controllerId)
        {
            return NotFound();
        }

        var estates =
            await _estateService
                .GetEstatesForControllerAsync(
                    controllerId);

        return Ok(estates);
    }

    [HttpPost]
    public async Task<ActionResult> Create(
        [FromBody] CreateEstateRequest request)
    {
        var controllerIdClaim =
            User.FindFirst("controllerId")?.Value;

        if (controllerIdClaim is null ||
            !Guid.TryParse(
                controllerIdClaim,
                out var controllerId))
        {
            return Forbid(
                "This account is not linked to a staff record and cannot create estates.");
        }

        try
        {
            var newEstateId =
                await _estateService.CreateEstateAsync(
                    request,
                    controllerId);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    estateId = newEstateId
                },
                new
                {
                    estateId = newEstateId
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{estateId:guid}")]
    public async Task<ActionResult> Update(
        Guid estateId,
        [FromBody] UpdateEstateRequest request)
    {
        try
        {
            await _estateService.UpdateEstateAsync(
                estateId,
                request);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Delete Estate
    // ─────────────────────────────────────────────

    [HttpDelete("{estateId:guid}")]
    public async Task<ActionResult> DeleteEstate(
        Guid estateId)
    {
        try
        {
            await _estateService.DeleteEstateAsync(
                estateId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Progress
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/progress-stages")]
    public async Task<ActionResult> AddProgressStage(
        Guid estateId,
        [FromBody] CreateProgressStageRequest request)
    {
        try
        {
            var stageId =
                await _estateService
                    .AddProgressStageAsync(
                        estateId,
                        request);

            return Ok(new
            {
                stageId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPatch("{estateId:guid}/progress-stage")]
    public async Task<ActionResult> UpdateProgressStage(
        Guid estateId,
        [FromBody] UpdateProgressStageRequest request)
    {
        try
        {
            var updatedStageId =
                await _estateService
                    .UpdateProgressStageAsync(
                        estateId,
                        request);

            return Ok(new
            {
                stageId = updatedStageId,
                status = request.Status
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/progress-stages/{stageId:guid}")]
    public async Task<ActionResult> DeleteProgressStage(
        Guid estateId,
        Guid stageId)
    {
        try
        {
            await _estateService
                .DeleteProgressStageAsync(
                    estateId,
                    stageId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Executors
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/executors")]
    public async Task<ActionResult> AddExecutor(
        Guid estateId,
        [FromBody] CreateExecutorRequest request)
    {
        try
        {
            var newExecutorId =
                await _estateService.AddExecutorAsync(
                    estateId,
                    request);

            return Ok(new
            {
                executorId = newExecutorId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/executors/{executorId:guid}")]
    public async Task<ActionResult> DeleteExecutor(
        Guid estateId,
        Guid executorId)
    {
        try
        {
            await _estateService.DeleteExecutorAsync(
                estateId,
                executorId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
    [HttpPut("{estateId:guid}/executors/{executorId:guid}")]
    public async Task<ActionResult> UpdateExecutor(
    Guid estateId,
    Guid executorId,
    [FromBody] UpdateExecutorRequest request)
    {
        try
        {
            await _estateService.UpdateExecutorAsync(
                estateId,
                executorId,
                request);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{estateId:guid}/executors/{executorId:guid}/letters")]
    public async Task<ActionResult> UploadExecutorLetters(
        Guid estateId,
        Guid executorId,
        IFormFile file)
    {
        try
        {
            var url = await _estateService.UploadExecutorLettersAsync(
                estateId,
                executorId,
                file);

            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{estateId:guid}/advertisements")]
    public async Task<ActionResult> UpsertAdvertisement(
        Guid estateId,
        [FromBody] UpsertAdvertisementRequest request)
    {
        try
        {
            await _estateService.UpsertAdvertisementAsync(
                estateId,
                request);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    // ─────────────────────────────────────────────
    // Beneficiaries
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/beneficiaries")]
    public async Task<ActionResult> AddBeneficiary(
        Guid estateId,
        [FromBody] CreateBeneficiaryRequest request)
    {
        try
        {
            var beneficiaryId =
                await _estateService
                    .AddBeneficiaryAsync(
                        estateId,
                        request);

            return Ok(new
            {
                beneficiaryId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/beneficiaries/{beneficiaryId:guid}")]
    public async Task<ActionResult> DeleteBeneficiary(
        Guid estateId,
        Guid beneficiaryId)
    {
        try
        {
            await _estateService
                .DeleteBeneficiaryAsync(
                    estateId,
                    beneficiaryId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Movable Assets
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/movable-assets")]
    public async Task<ActionResult> AddMovableAsset(
        Guid estateId,
        [FromBody] CreateMovableAssetRequest request)
    {
        try
        {
            var assetId =
                await _estateService
                    .AddMovableAssetAsync(
                        estateId,
                        request);

            return Ok(new
            {
                assetId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/movable-assets/{assetId:guid}")]
    public async Task<ActionResult> DeleteMovableAsset(
        Guid estateId,
        Guid assetId)
    {
        try
        {
            await _estateService
                .DeleteMovableAssetAsync(
                    estateId,
                    assetId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Immovable Properties
    // ─────────────────────────────────────────────

    [HttpPost(
        "{estateId:guid}/immovable-properties")]
    public async Task<ActionResult> AddImmovableProperty(
        Guid estateId,
        [FromBody] CreateImmovablePropertyRequest request)
    {
        try
        {
            var assetId =
                await _estateService
                    .AddImmovablePropertyAsync(
                        estateId,
                        request);

            return Ok(new
            {
                assetId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/immovable-properties/{assetId:guid}")]
    public async Task<ActionResult>
        DeleteImmovableProperty(
            Guid estateId,
            Guid assetId)
    {
        try
        {
            await _estateService
                .DeleteImmovablePropertyAsync(
                    estateId,
                    assetId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ─────────────────────────────────────────────
    // Financial Claims
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/financial-claims")]
    public async Task<ActionResult> AddFinancialClaim(
        Guid estateId,
        [FromBody] CreateFinancialClaimRequest request)
    {
        try
        {
            var claimId =
                await _estateService
                    .AddFinancialClaimAsync(
                        estateId,
                        request);

            return Ok(new
            {
                claimId
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete(
        "{estateId:guid}/financial-claims/{claimId:guid}")]
    public async Task<ActionResult> DeleteFinancialClaim(
        Guid estateId,
        Guid claimId)
    {
        try
        {
            await _estateService
                .DeleteFinancialClaimAsync(
                    estateId,
                    claimId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
    // ─────────────────────────────────────────────
    // Liabilities
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/liabilities")]
    public async Task<ActionResult> AddLiability(
        Guid estateId,
        [FromBody] CreateLiabilityRequest request)
    {
        try
        {
            var liabilityId =
                await _estateService.AddLiabilityAsync(
                    estateId,
                    request);

            return Ok(new { liabilityId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete(
        "{estateId:guid}/liabilities/{liabilityId:guid}")]
    public async Task<ActionResult> DeleteLiability(
        Guid estateId,
        Guid liabilityId)
    {
        try
        {
            await _estateService.DeleteLiabilityAsync(
                estateId,
                liabilityId);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ─────────────────────────────────────────────
    // Liquidation & Distribution Draft
    // ─────────────────────────────────────────────

    [HttpGet("{estateId:guid}/liquidation-distribution")]
    public async Task<ActionResult<LiquidationDistributionDraftDto>>
        GetLiquidationDistributionDraft(Guid estateId)
    {
        try
        {
            var estate = await _estateService.GetEstateAsync(estateId);
            if (estate is null)
                return NotFound();

            var controllerIdClaim =
                User.FindFirst("controllerId")?.Value;

            if (controllerIdClaim is null ||
                !Guid.TryParse(controllerIdClaim, out var controllerId) ||
                estate.ControllerId != controllerId)
            {
                return NotFound();
            }

            var draft =
                await _estateService
                    .GetLiquidationDistributionDraftAsync(
                        estateId);

            return Ok(draft);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ─────────────────────────────────────────────
    // Cashbook (estate bank account)
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/cashbook")]
    public async Task<ActionResult> AddCashbookEntry(
        Guid estateId,
        [FromBody] CreateCashbookEntryRequest request)
    {
        try
        {
            var entryId = await _estateService.AddCashbookEntryAsync(estateId, request);
            return Ok(new { entryId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{estateId:guid}/cashbook/{entryId:guid}")]
    public async Task<ActionResult> DeleteCashbookEntry(Guid estateId, Guid entryId)
    {
        try
        {
            await _estateService.DeleteCashbookEntryAsync(estateId, entryId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{estateId:guid}/cashbook/{entryId:guid}/reconciled")]
    public async Task<ActionResult> SetCashbookReconciled(
        Guid estateId,
        Guid entryId,
        [FromBody] SetReconciledRequest request)
    {
        try
        {
            await _estateService.SetCashbookReconciledAsync(estateId, entryId, request.IsReconciled);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{estateId:guid}/cashbook/pdf")]
    public async Task<IActionResult> DownloadCashbookPdf(Guid estateId)
    {
        try
        {
            var (content, fileName) = await _estateService.GetCashbookPdfAsync(estateId);
            return File(content, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ─────────────────────────────────────────────
    // Checklist
    // ─────────────────────────────────────────────

    [HttpPost("{estateId:guid}/checklist")]
    public async Task<ActionResult> AddChecklistItem(
        Guid estateId,
        [FromBody] CreateChecklistItemRequest request)
    {
        try
        {
            var itemId = await _estateService.AddChecklistItemAsync(estateId, request);
            return Ok(new { itemId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("{estateId:guid}/checklist/seed")]
    public async Task<ActionResult> SeedChecklist(Guid estateId)
    {
        try
        {
            var added = await _estateService.SeedChecklistAsync(estateId);
            return Ok(new { added });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{estateId:guid}/checklist/{itemId:guid}/status")]
    public async Task<ActionResult> UpdateChecklistStatus(
        Guid estateId,
        Guid itemId,
        [FromBody] UpdateChecklistStatusRequest request)
    {
        try
        {
            await _estateService.UpdateChecklistStatusAsync(estateId, itemId, request.Status);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{estateId:guid}/checklist/{itemId:guid}")]
    public async Task<ActionResult> DeleteChecklistItem(Guid estateId, Guid itemId)
    {
        try
        {
            await _estateService.DeleteChecklistItemAsync(estateId, itemId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ─────────────────────────────────────────────
    // Audit trail
    // ─────────────────────────────────────────────

    [HttpGet("{estateId:guid}/audit")]
    public async Task<ActionResult<List<AuditEntryDto>>> GetAuditTrail(
        Guid estateId,
        [FromQuery] int take = 200)
    {
        var rows = await _estateService.GetAuditTrailAsync(estateId, take);
        return Ok(rows);
    }

    // ─────────────────────────────────────────────
    // Liquidation & Distribution Account (PDF download)
    // ─────────────────────────────────────────────

    [HttpGet("{estateId:guid}/liquidation-distribution/pdf")]
    public async Task<IActionResult> DownloadLiquidationAccountPdf(Guid estateId)
    {
        try
        {
            var estate = await _estateService.GetEstateAsync(estateId);
            if (estate is null)
                return NotFound();

            var controllerIdClaim =
                User.FindFirst("controllerId")?.Value;

            if (controllerIdClaim is null ||
                !Guid.TryParse(controllerIdClaim, out var controllerId) ||
                estate.ControllerId != controllerId)
            {
                return NotFound();
            }

            var (content, fileName) =
                await _estateService.GetLiquidationAccountPdfAsync(estateId);

            return File(content, "application/pdf", fileName);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}