namespace AndyTours.Core.Modell;

/// <summary>
/// Die Filter der Tabelle „Trainingseinheiten“, wie sie in der Filterleiste gesetzt sind.
/// </summary>
/// <remarks>
/// Angewendet von <see cref="Berechnungen.FiltereEinheiten"/>. Kategorie, Equipment und Trainingsbereich
/// werden über ihren Namen angegeben (<c>null</c> = alle), damit die Wahl ein Neuladen des Datenbestands
/// übersteht. Alle gesetzten Bedingungen müssen gleichzeitig erfüllt sein.
/// </remarks>
/// <param name="Kategorie">Name der gewählten Kategorie oder <c>null</c>.</param>
/// <param name="Equipment">Name des gewählten Equipments oder <c>null</c>.</param>
/// <param name="Trainingsbereich">Gewählter Trainingsbereich oder <c>null</c>.</param>
/// <param name="Distanz">Bereich der Distanz in km.</param>
/// <param name="Hoehenmeter">Bereich der Höhenmeter.</param>
/// <param name="Name">Text, den der Name der Einheit enthalten muss; leer = kein Filter.</param>
public record Einheitenfilter(
    string? Kategorie,
    string? Equipment,
    string? Trainingsbereich,
    Filterbereich Distanz,
    Filterbereich Hoehenmeter,
    string Name)
{
    /// <summary>Filter ohne jede Einschränkung.</summary>
    public static Einheitenfilter Leer { get; } = new(null, null, null, Filterbereich.Leer, Filterbereich.Leer, string.Empty);
}
