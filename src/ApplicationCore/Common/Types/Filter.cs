namespace ApplicationCore.Common.Types;

public class Filter(OrderBy orderBy, Order order, List<string> categories, int count, int offset)
{
    public OrderBy orderBy = orderBy;
    public Order order = order;
    public List<string> categories = categories;
    public int count = count;
    public int offset = offset;
}