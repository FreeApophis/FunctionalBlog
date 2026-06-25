namespace FunctionalBlog.Admin;

// Aggregate content counts shown on the /admin overview. IncompleteIngredients drives the
// "needs attention" notice (ingredients still missing nutrition/other data).
public sealed record DashboardStats(
    int Articles,
    int Recipes,
    int Ingredients,
    int Pages,
    int Images,
    int Users,
    int IncompleteIngredients);
