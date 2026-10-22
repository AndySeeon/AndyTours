using System.Globalization;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest ein Datum aus einer Spalte der Datenbank (Gegenstück zu <see cref="DatumAlsText"/>).
/// </summary>
public static class TextAlsDatum
{
    /// <summary>
    /// Liest die Spalte als Datum.
    /// </summary>
    /// <param name="leser">Datenleser, der auf der aktuellen Zeile steht.</param>
    /// <param name="spalte">Spaltennummer (0-basiert, in der Reihenfolge des SELECT).</param>
    /// <returns>Das Datum oder <c>null</c>, wenn die Spalte leer (NULL) ist.</returns>
    public static DateOnly? Ausfuehren(SqliteDataReader leser, int spalte)
    {
        if (leser.IsDBNull(spalte))
        {
            return null;
        }

        return DateOnly.ParseExact(leser.GetString(spalte), DatumAlsText.Format, CultureInfo.InvariantCulture);
    }
}
