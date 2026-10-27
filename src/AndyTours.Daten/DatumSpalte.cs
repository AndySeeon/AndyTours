using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Wandelt Datumswerte zwischen <see cref="DateOnly"/> und der Textform der Datenbank (ISO 8601) um.
/// </summary>
public static class DatumSpalte
{
    /// <summary>Speicherformat für Datumswerte (ISO 8601).</summary>
    public const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Liest die Spalte als Datum.
    /// </summary>
    /// <param name="leser">Datenleser, der auf der aktuellen Zeile steht.</param>
    /// <param name="spalte">Spaltennummer (0-basiert, in der Reihenfolge des SELECT).</param>
    /// <returns>Das Datum oder <c>null</c>, wenn die Spalte leer (NULL) ist.</returns>
    public static DateOnly? Lesen(SqliteDataReader leser, int spalte)
    {
        if (leser.IsDBNull(spalte))
        {
            return null;
        }

        return DateOnly.ParseExact(leser.GetString(spalte), Format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Liefert das Datum als „JJJJ-MM-TT" bzw. <see cref="DBNull.Value"/>, wenn kein Datum vorhanden ist.
    /// </summary>
    /// <remarks>
    /// Rückgabetyp <c>object</c>, weil SQLite-Parameter für „kein Wert" <see cref="DBNull.Value"/>
    /// statt <c>null</c> erwarten.
    /// </remarks>
    /// <param name="datum">Das Datum oder <c>null</c>.</param>
    /// <returns>Der Parameterwert für die Datenbank.</returns>
    public static object AlsText(DateOnly? datum)
    {
        return datum?.ToString(Format, CultureInfo.InvariantCulture) ?? (object)DBNull.Value;
    }
}
