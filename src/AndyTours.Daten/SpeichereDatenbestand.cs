using System.Globalization;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Schreibt einen vollständigen <see cref="Datenbestand"/> in eine leere Datenbank.
/// </summary>
/// <remarks>
/// Gedacht für die Erstbefüllung nach der Übernahme der Altdaten. Die Nummern (Ids) aus dem
/// Datenbestand werden unverändert übernommen, damit die Verweise zwischen den Tabellen stimmen.
/// </remarks>
public static class SpeichereDatenbestand
{
    /// <summary>
    /// Speichert alle Tabellen in einer einzigen Transaktion.
    /// </summary>
    /// <remarks>
    /// Die Transaktion sorgt dafür, dass bei einem Fehler gar nichts gespeichert wird (statt eines
    /// halb gefüllten Bestands). Nebenbei ist das Einfügen in einer Transaktion bei SQLite um ein
    /// Vielfaches schneller, weil nicht nach jeder Zeile auf die Festplatte geschrieben wird.
    /// Die Reihenfolge folgt den Verweisen: zuerst Tabellen, auf die andere verweisen.
    /// </remarks>
    /// <param name="verbindung">Geöffnete Verbindung (siehe <see cref="OeffneDatenbank"/>).</param>
    /// <param name="bestand">Der zu speichernde Datenbestand.</param>
    /// <exception cref="InvalidOperationException">Die Datenbank enthält bereits Trainingseinheiten.</exception>
    public static void Ausfuehren(SqliteConnection verbindung, Datenbestand bestand)
    {
        // Schutz vor doppeltem Einspielen: Ids würden sonst kollidieren.
        using (SqliteCommand pruefung = verbindung.CreateCommand())
        {
            pruefung.CommandText = "SELECT COUNT(*) FROM Trainingseinheit;";
            if (Convert.ToInt64(pruefung.ExecuteScalar(), CultureInfo.InvariantCulture) > 0)
            {
                throw new InvalidOperationException("Die Datenbank enthält bereits Trainingseinheiten.");
            }
        }

        using SqliteTransaction transaktion = verbindung.BeginTransaction();

        SpeichereFahrer.Ausfuehren(verbindung, transaktion, bestand.Fahrer);
        SpeichereAktivitaeten.Ausfuehren(verbindung, transaktion, bestand.Aktivitaeten);
        SpeichereKategorien.Ausfuehren(verbindung, transaktion, bestand.Kategorien);
        SpeichereEquipments.Ausfuehren(verbindung, transaktion, bestand.Equipments);
        SpeichereTrainingseinheiten.Ausfuehren(verbindung, transaktion, bestand.Trainingseinheiten);
        SpeichereWartungen.Ausfuehren(verbindung, transaktion, bestand.Wartungen);
        SpeichereSchwellwerte.Ausfuehren(verbindung, transaktion, bestand.Schwellwerte);

        // Erst hier wird alles endgültig geschrieben; ohne Commit verwirft Dispose die Änderungen.
        transaktion.Commit();
    }
}
