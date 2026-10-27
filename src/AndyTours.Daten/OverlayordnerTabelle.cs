using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt die Tabelle <c>Overlayordner</c> (Ordner der Overlay-Verwaltung).
/// </summary>
/// <remarks>
/// Jede Änderung ist eine einzelne Anweisung und wird sofort gespeichert. Ob ein Name zulässig ist bzw. ob ein Ordner
/// dorthin verschoben werden darf, prüft die Oberfläche vorher (<c>PruefeOrdnername</c>, <c>PruefeVerschiebeziel</c>).
/// </remarks>
public static class OverlayordnerTabelle
{
    /// <summary>
    /// Liest alle Ordner, sortiert nach Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <returns>Die Ordner.</returns>
    public static List<Overlayordner> Laden(SqliteConnection verbindung)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "SELECT Id, ElternId, Name FROM Overlayordner ORDER BY Id;";

        var ordner = new List<Overlayordner>();
        using SqliteDataReader leser = befehl.ExecuteReader();
        while (leser.Read())
        {
            ordner.Add(new Overlayordner(leser.GetInt64(0), leser.IsDBNull(1) ? null : leser.GetInt64(1), leser.GetString(2)));
        }

        return ordner;
    }

    /// <summary>
    /// Legt einen neuen Ordner an; die Datenbank vergibt die Nummer.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="elternId">Übergeordneter Ordner; <c>null</c> = oberste Ebene.</param>
    /// <param name="name">Name des Ordners (wird ohne Leerzeichen am Rand gespeichert).</param>
    /// <returns>Die vergebene Nummer.</returns>
    public static long Einfuegen(SqliteConnection verbindung, long? elternId, string name)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "INSERT INTO Overlayordner (ElternId, Name) VALUES ($elternId, $name) RETURNING Id;";
        befehl.Parameters.AddWithValue("$elternId", (object?)elternId ?? DBNull.Value);
        befehl.Parameters.AddWithValue("$name", name.Trim());
        return (long)befehl.ExecuteScalar()!;
    }

    /// <summary>
    /// Gibt einem Ordner einen neuen Namen.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Ordners.</param>
    /// <param name="name">Der neue Name (wird ohne Leerzeichen am Rand gespeichert).</param>
    public static void Umbenennen(SqliteConnection verbindung, long id, string name)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "UPDATE Overlayordner SET Name = $name WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.Parameters.AddWithValue("$name", name.Trim());
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Hängt einen Ordner samt Inhalt in einen anderen Ordner.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Ordners.</param>
    /// <param name="elternId">Der neue übergeordnete Ordner; <c>null</c> = oberste Ebene.</param>
    public static void Verschieben(SqliteConnection verbindung, long id, long? elternId)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "UPDATE Overlayordner SET ElternId = $elternId WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.Parameters.AddWithValue("$elternId", (object?)elternId ?? DBNull.Value);
        befehl.ExecuteNonQuery();
    }

    /// <summary>
    /// Löscht einen Ordner mit allen Unterordnern und allen Overlays darin.
    /// </summary>
    /// <remarks>
    /// Die Unterordner und Overlays löscht die Datenbank selbst über <c>ON DELETE CASCADE</c>; dafür müssen die
    /// Fremdschlüssel eingeschaltet sein, was <see cref="OeffneDatenbank"/> erledigt.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung.</param>
    /// <param name="id">Nummer des Ordners.</param>
    public static void Loeschen(SqliteConnection verbindung, long id)
    {
        using SqliteCommand befehl = verbindung.CreateCommand();
        befehl.CommandText = "DELETE FROM Overlayordner WHERE Id = $id;";
        befehl.Parameters.AddWithValue("$id", id);
        befehl.ExecuteNonQuery();
    }
}
