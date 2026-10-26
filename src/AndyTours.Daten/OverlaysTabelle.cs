using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Overlay</c> (gespeicherte Strecken, die auf den Karten eingeblendet werden).
/// </summary>
/// <remarks>
/// Jede Änderung wird sofort gespeichert. Die Farbe steht als Name im Klartext (z. B. „Blau“), ohne eigene Farbe als
/// NULL; die Punkte als Binärwert (<see cref="OverlayPunkte"/>).
/// </remarks>
public static class OverlaysTabelle
{
    /// <summary>
    /// Liest alle Overlays mit ihren Punkten, sortiert nach Nummer.
    /// </summary>
    /// <remarks>
    /// Ein unbekannter Farbname (z. B. aus einer späteren Programmversion) gilt als „keine eigene Farbe“.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Overlays.</returns>
    /// <exception cref="InvalidDataException">Die Punkte eines Overlays sind nicht lesbar.</exception>
    public static List<Overlay> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Id, OrdnerId, Name, Farbe, Eingeschaltet, Punkte FROM Overlay ORDER BY Id;";

        var overlays = new List<Overlay>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            Overlayfarbe? farbe = !leser.IsDBNull(3) && Enum.TryParse(leser.GetString(3), out Overlayfarbe f) && Enum.IsDefined(f) ? f : null;
            overlays.Add(new Overlay(
                leser.GetInt64(0),
                leser.IsDBNull(1) ? null : leser.GetInt64(1),
                leser.GetString(2),
                farbe,
                leser.GetBoolean(4),
                OverlayPunkte.Entpacken((byte[])leser.GetValue(5))));
        }

        return overlays;
    }

    /// <summary>
    /// Fügt ein neues Overlay ein; die Datenbank vergibt die Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="overlay">Das Overlay (seine <see cref="Overlay.Id"/> wird nicht verwendet).</param>
    /// <returns>Die vergebene Nummer.</returns>
    public static long Einfuegen(SqliteConnection verbindung, Overlay overlay)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = """
            INSERT INTO Overlay (OrdnerId, Name, Farbe, Eingeschaltet, Punkte)
            VALUES ($ordnerId, $name, $farbe, $eingeschaltet, $punkte) RETURNING Id;
            """;
        befehl.Parameters.AddWithValue("$ordnerId", (object?)overlay.OrdnerId ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$name", overlay.Name.Trim());
        befehl.Parameters.AddWithValue("$farbe", (object?)overlay.Farbe?.ToString() ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$eingeschaltet", overlay.IstEingeschaltet);
        befehl.Parameters.AddWithValue("$punkte", OverlayPunkte.Verpacken(overlay.Abschnitte));
        return (long)befehl.ExecuteScalar()!;
    }

    /// <summary>
    /// Gibt einem Overlay einen neuen Namen.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Overlays.</param>
    /// <param name="name">Der neue Name (wird ohne Leerzeichen am Rand gespeichert).</param>
    public static void Umbenennen(SqliteConnection verbindung, long id, string name) =>
        Setze(verbindung, id, "Name", name.Trim());

    /// <summary>
    /// Legt ein Overlay in einen anderen Ordner.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Overlays.</param>
    /// <param name="ordnerId">Der neue Ordner; <c>null</c> = oberste Ebene.</param>
    public static void Verschieben(SqliteConnection verbindung, long id, long? ordnerId) =>
        Setze(verbindung, id, "OrdnerId", ordnerId);

    /// <summary>
    /// Setzt oder entfernt die eigene Farbe eines Overlays.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Overlays.</param>
    /// <param name="farbe">Die eigene Farbe; <c>null</c> = wieder die gemeinsame Overlay-Farbe.</param>
    public static void SetzeFarbe(SqliteConnection verbindung, long id, Overlayfarbe? farbe) =>
        Setze(verbindung, id, "Farbe", farbe?.ToString());

    /// <summary>
    /// Schaltet mehrere Overlays auf einmal ein oder aus (z. B. alle Overlays eines Ordners).
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="ids">Die Nummern der Overlays.</param>
    /// <param name="eingeschaltet"><c>true</c> = einblenden.</param>
    public static void SetzeEingeschaltet(SqliteConnection verbindung, IEnumerable<long> ids, bool eingeschaltet)
    {
        using SqliteTransaction transaktion = verbindung.BeginTransaction();
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.Transaction = transaktion;
        befehl.CommandText = "UPDATE Overlay SET Eingeschaltet = $eingeschaltet WHERE Id = $id;";
        SqliteParameter idParameter = befehl.Parameters.Add("$id", SqliteType.Integer);
        befehl.Parameters.AddWithValue("$eingeschaltet", eingeschaltet);
        foreach (long id in ids)
        {
            idParameter.Value = id;
            befehl.ExecuteNonQuery();
        }

        transaktion.Commit();
    }

    /// <summary>
    /// Löscht ein Overlay.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Overlays.</param>
    public static void Loeschen(SqliteConnection verbindung, long id)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "DELETE FROM Overlay WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.ExecuteNonQuery();
    }

    /// <summary>Schreibt eine Spalte eines Overlays; der Spaltenname stammt nur aus dieser Klasse.</summary>
    private static void Setze(SqliteConnection verbindung, long id, string spalte, object? wert)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = $"UPDATE Overlay SET {spalte} = $wert WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.Parameters.AddWithValue("$wert", wert ?? DBNull.Value);
        befehl.ExecuteNonQuery();
    }
}
