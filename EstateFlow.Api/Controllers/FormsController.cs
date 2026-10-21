using EstateFlow.Api.Services.Forms;
using EstateFlow.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstateFlow.Api.Controllers;

// Estate Documents: the Master's Office forms and their filled-in versions.
[Authorize]
[ApiController]
public class FormsController : ControllerBase
{
    private readonly FormService _forms;

    public FormsController(FormService forms)
    {
        _forms = forms;
    }

    // GET /api/forms  → every form, grouped under its heading
    [HttpGet("api/forms")]
    public ActionResult<List<FormGroupDto>> GetCatalog() =>
        Ok(_forms.GetCatalog());

    // GET /api/forms/{id}/blank  → the empty form
    [HttpGet("api/forms/{id}/blank")]
    public IActionResult DownloadBlank(string id)
    {
        var blank = _forms.GetBlank(id);

        return blank is null
            ? NotFound(new { message = $"Form '{id}' was not found." })
            : File(blank.Value.Content, "application/pdf", blank.Value.FileName);
    }

    // GET /api/estates/{estateId}/forms/{id}  → the form with this estate's details filled in
    [HttpGet("api/estates/{estateId:guid}/forms/{id}")]
    public async Task<ActionResult<EstateFormDto>> GetEstateForm(Guid estateId, string id)
    {
        try
        {
            return Ok(await _forms.GetEstateFormAsync(estateId, id));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST /api/estates/{estateId}/forms/{id}/pdf  → the finished PDF
    [HttpPost("api/estates/{estateId:guid}/forms/{id}/pdf")]
    public async Task<IActionResult> Generate(
        Guid estateId,
        string id,
        [FromBody] GenerateFormRequest request)
    {
        try
        {
            var (content, fileName) = await _forms.GenerateAsync(estateId, id, request.Values);
            return File(content, "application/pdf", fileName);
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
}
