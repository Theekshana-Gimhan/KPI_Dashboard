using KPI_Dashboard.Data;
using System.Security.Claims;
using System.Text.Json;

public class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContext)
    {
        _context = context;
        _httpContext = httpContext;
    }

    public async Task<KPIAuditTrail> LogActionAsync(
        string actionType,
        string action,
        object? oldState = null,
        object? newState = null,
        int? targetId = null)
    {
        var userId = _httpContext.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        var auditLog = new KPIAuditTrail
        {
            UserId = userId ?? "system",
            ActionType = actionType,
            Action = action,
            OldValues = oldState != null ? JsonSerializer.Serialize(oldState) : null,
            NewValues = newState != null ? JsonSerializer.Serialize(newState) : null,
            TargetId = targetId,
            Timestamp = DateTime.UtcNow
        };

        _context.KPIAuditTrails.Add(auditLog);
        await _context.SaveChangesAsync();

        return auditLog;
    }

    public async Task UpdateAuditLogAsync(int auditLogId, bool isSuccess, string? errorMessage = null)
    {
        var log = await _context.KPIAuditTrails.FindAsync(auditLogId);
        if (log != null)
        {
            log.IsSuccess = isSuccess;
            log.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync();
        }
    }
}