namespace ApplicationCore.Common.Types;

public class Filter(OrderBy orderBy = OrderBy.TITLE, Order order = Order.ASCENDING, List<string>? categories = null, List<string>? ingredients = null, int count = 10, int offset = 0)
{
    public OrderBy OrderBy = orderBy;
    public Order Order = order;
    public List<string> Categories = categories ?? [];
    public List<string> Ingredients = ingredients ?? [];
    public int Count = count;
    public int Offset = offset;
}