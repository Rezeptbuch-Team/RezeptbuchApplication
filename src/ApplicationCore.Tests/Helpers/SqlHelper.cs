namespace ApplicationCore.Tests.Helpers;

public static class SqlHelper
{
    /// <summary>
    /// Remve extra whitespaces and new-lines from the SQL string
    /// </summary>
    /// <param name="sql"></param>
    /// <returns></returns>
    public static string NormalizeSql(string sql)
    {
        return string.Join(" ", sql.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries));
    }
}