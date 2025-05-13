namespace ApplicationCore.Common.Types;

public class Filter(OrderBy orderBy = OrderBy.TITLE, Order order = Order.ASCENDING, List<string>? categories = null, List<string>? availableIngredients = null, int count = 10, int offset = 0)
{
    public OrderBy orderBy = orderBy;
    public Order order = order;
    public List<string> categories = categories ?? [];
    public List<string> availableIngredients = availableIngredients ?? [];
    public int count = count;
    public int offset = offset;
}