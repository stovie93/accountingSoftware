namespace PropertyViewerAccounting.Api.Services;

public class UserContext
{
    public Guid UserId { get; set; }
    public string UserRole { get; set; } = string.Empty;
}
