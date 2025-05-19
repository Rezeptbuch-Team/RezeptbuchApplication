namespace ApplicationCore.Common.Types;

public enum PublishOption
{
    /// <summary>
    /// Not published or user has not activated online services yet
    /// </summary>
    FORBIDDEN,
    /// <summary>
    /// Has not been published yet. Give user the possibility to upload it
    /// </summary>
    NOT_PUBLISHED,
    /// <summary>
    /// Has been published. No further action needed (gray out button)
    /// </summary>
    PUBLISHED,
    /// <summary>
    /// Old version has been published. Give user the possibility to upload the new version
    /// </summary>
    OUTDATED
}