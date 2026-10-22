using System.Globalization;

namespace AndyTours.Daten;

/// <summary>
/// Wandelt ein Datum in die Textform um, in der es in der Datenbank steht.
/// </summary>
public static class DatumAlsText
{
    /// <summary>Speicherformat für Datumswerte (ISO 8601).</summary>
    public const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Liefert das Datum als „JJJJ-MM-TT" bzw. <see cref="DBNull.Value"/>, wenn kein Datum vorhanden ist.
    /// </summary>
    /// <remarks>
    /// Rückgabetyp <c>object</c>, weil SQLite-Parameter für „kein Wert" <see cref="DBNull.Value"/>
    /// statt <c>null</c> erwarten.
    /// </remarks>
    /// <param name="datum">Das Datum oder <c>null</c>.</param>
    /// <returns>Der Parameterwert für die Datenbank.</returns>
    public static object Ausfuehren(DateOnly? datum)
    {
        return datum?.ToString(Format, CultureInfo.InvariantCulture) ?? (object)DBNull.Value;
    }
}
