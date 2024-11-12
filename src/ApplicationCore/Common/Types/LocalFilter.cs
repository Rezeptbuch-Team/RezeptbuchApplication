namespace ApplicationCore.Common.Types;

public class LocalFilter(OrderBy orderBy, Order order, List<string> categories, List<string> availableIngredients) : Filter(orderBy, order, categories) {
    public readonly List<string> availableIngredients = availableIngredients;
}