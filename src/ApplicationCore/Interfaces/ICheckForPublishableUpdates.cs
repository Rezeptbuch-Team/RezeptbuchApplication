using ApplicationCore.Common.Types;

namespace ApplicationCore.Interfaces;

public interface ICheckForPublishableUpdates
{
    public Task<List<PublishableUpdate>> GetPublishableUpdates();
}