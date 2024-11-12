namespace ApplicationCore.Common.Types;

public class Filter(OrderBy orderBy, Order order, List<string> categories)
{
    public OrderBy orderBy = orderBy;
    public Order order = order;
    public List<string> categories = categories;
}