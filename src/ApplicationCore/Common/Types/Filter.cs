namespace ApplicationCore.Common.Types;

public class Filter(OrderBy? orderBy, Order? order, List<string>? categories, List<string>? availableIngredients, int? count, int? offset)
{
    public OrderBy orderBy = orderBy ?? OrderBy.TITLE;
    public Order order = order ?? Order.ASCENDING;
    public List<string> categories = categories ?? [];
    public List<string> availableIngredients = availableIngredients ?? [];
    public int count = count ?? 10;
    public int offset = offset ?? 0;
}