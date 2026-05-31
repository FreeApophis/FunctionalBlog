namespace FunctionalBlog.Domain.Recipes;

public sealed record Ingredient(
    IngredientId Id,
    IngredientName Name,
    string Image,
    string Description,
    decimal Density,
    decimal PieceCount,
    decimal CalorificValue,
    decimal Protein,
    decimal Fat,
    decimal Carbohydrates,
    decimal Sugar,
    decimal Fiber)
{
    public static Ingredient Create(
        IngredientId id,
        IngredientName name,
        string image,
        string description,
        decimal density,
        decimal pieceCount,
        decimal calorificValue,
        decimal protein,
        decimal fat,
        decimal carbohydrates,
        decimal sugar,
        decimal fiber) =>
        new(id, name, image, description, density, pieceCount, calorificValue, protein, fat, carbohydrates, sugar, fiber);
}
