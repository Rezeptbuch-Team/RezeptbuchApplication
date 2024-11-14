namespace ApplicationCore.Common.Types;

public class LocalFilter(OrderBy orderBy, Order order, List<string> categories, List<string> availableIngredients, int count, int offset) : Filter(orderBy, order, categories, count, offset) {
    public readonly List<string> availableIngredients = availableIngredients;
}