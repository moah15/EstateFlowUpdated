using EstateFlow.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace EstateFlow.Api.Filters;

// Global guard: any route containing {estateId} is only reachable by the
// estate controller the estate is assigned to. Everyone else gets 404.
public class EstateOwnershipFilter : IAsyncActionFilter
{
    private readonly EstateFlowDbContext _db;

    public EstateOwnershipFilter(EstateFlowDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("estateId", out var raw) &&
            Guid.TryParse(raw?.ToString(), out var estateId))
        {
            var claim = context.HttpContext.User.FindFirst("controllerId")?.Value;

            var owner =
                await _db.Estates
                    .AsNoTracking()
                    .Where(e => e.EstateId == estateId)
                    .Select(e => (Guid?)e.ControllerId)
                    .FirstOrDefaultAsync();

            if (owner is null ||
                claim is null ||
                !Guid.TryParse(claim, out var controllerId) ||
                owner.Value != controllerId)
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        await next();
    }
}
