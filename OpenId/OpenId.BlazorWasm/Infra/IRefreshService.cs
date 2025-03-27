namespace OpenId.BlazorWasm.Infra;

public interface IRefreshService
{
    Task<AuthRefreshResult> RefreshAsync();
}