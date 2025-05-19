namespace ApplicationCore.Interfaces;

public interface IOnlineIdentificationService {
    public Task<string?> GetUUID();
    public Task<string> CreateUUID();
}