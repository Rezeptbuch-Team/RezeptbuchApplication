using ApplicationCore.Common.Types;
using ApplicationCore.Interfaces;

namespace ApplicationCore.Model;

public class CheckForPublishableUpdates : ICheckForPublishableUpdates
{
    public async Task<List<PublishableUpdate>> GetPublishableUpdates()
    {
        throw new NotImplementedException();
    }
}