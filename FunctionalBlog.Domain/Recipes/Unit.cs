namespace FunctionalBlog.Domain.Recipes;

[DiscriminatedUnion]
public abstract partial record Unit(string Name, string Abbreviation, decimal Factor)
{
    public sealed partial record WeightUnit : Unit
    {
        public static WeightUnit Gram { get; } = new("Gramm", "g", 1m);

        public static WeightUnit Kilogram { get; } = new("Kilogramm", "kg", 1000m);

        public WeightUnit(string name, string abbreviation, decimal factor)
            : base(name, abbreviation, factor)
        {
        }

        public static decimal Convert(decimal amount, WeightUnit from, WeightUnit to)
            => amount * from.Factor / to.Factor;
    }

    public sealed partial record VolumeUnit : Unit
    {
        public static VolumeUnit Milliliter { get; } = new("Milliliter", "ml", 1m);

        public static VolumeUnit Deciliter { get; } = new("Deziliter", "dl", 100m);

        public static VolumeUnit Liter { get; } = new("Liter", "l", 1000m);

        public static VolumeUnit Tablespoon { get; } = new("Esslöffel", "EL", 15m);

        public static VolumeUnit Teaspoon { get; } = new("Teelöffel", "TL", 5m);

        public VolumeUnit(string name, string abbreviation, decimal factor)
            : base(name, abbreviation, factor)
        {
        }

        public static decimal Convert(decimal amount, VolumeUnit from, VolumeUnit to)
            => amount * from.Factor / to.Factor;
    }

    public sealed partial record PieceUnit : Unit
    {
        public static PieceUnit Piece { get; } = new("Stück", "Stück", 1m);

        public static PieceUnit Pinch { get; } = new("Prise", "Prise", 1m);

        public PieceUnit(string name, string abbreviation, decimal factor)
            : base(name, abbreviation, factor)
        {
        }

        public static decimal Convert(decimal amount, PieceUnit from, PieceUnit to)
            => amount * from.Factor / to.Factor;
    }

    public static IReadOnlyList<Unit> All =>
    [
        WeightUnit.Gram, WeightUnit.Kilogram,
        VolumeUnit.Milliliter, VolumeUnit.Deciliter, VolumeUnit.Liter,
        VolumeUnit.Tablespoon, VolumeUnit.Teaspoon,
        PieceUnit.Piece, PieceUnit.Pinch,
    ];

    public static Option<Unit> ParseByAbbreviation(string abbreviation) =>
        All.FirstOrNone(u => u.Abbreviation == abbreviation);
}
