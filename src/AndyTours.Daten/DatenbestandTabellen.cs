using System.Globalization;
using AndyTours.Core.Modell;
using Microsoft.Data.Sqlite;

namespace AndyTours.Daten;

/// <summary>
/// Liest und schreibt den gesamten Datenbestand über die einzelnen Tabellenklassen.
/// </summary>
public static class DatenbestandTabellen
{
    /// <summary>
    /// Liest alle Tabellen und setzt daraus einen <see cref="Datenbestand"/> zusammen.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung (siehe <see cref="OeffneDatenbank"/>).</param>
    /// <returns>Der vollständige Datenbestand.</returns>
    public static Datenbestand Laden(SqliteConnection verbindung)
    {
        return new Datenbestand
        {
            Fahrer = FahrerTabelle.Laden(verbindung),
            Aktivitaeten = AktivitaetenTabelle.Laden(verbindung),
            Saisonziele = SaisonzieleTabelle.Laden(verbindung),
            Kategorien = KategorienTabelle.Laden(verbindung),
            Equipments = EquipmentsTabelle.Laden(verbindung),
            Trainingseinheiten = TrainingseinheitenTabelle.Laden(verbindung),
            Wartungen = WartungenTabelle.Laden(verbindung),
            Schwellwerte = SchwellwerteTabelle.Laden(verbindung),
        };
    }

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
    public static void Speichern(SqliteConnection verbindung, Datenbestand bestand)
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

        FahrerTabelle.Speichern(verbindung, transaktion, bestand.Fahrer);
        AktivitaetenTabelle.Speichern(verbindung, transaktion, bestand.Aktivitaeten);
        SaisonzieleTabelle.Speichern(verbindung, transaktion, bestand.Saisonziele);
        KategorienTabelle.Speichern(verbindung, transaktion, bestand.Kategorien);
        EquipmentsTabelle.Speichern(verbindung, transaktion, bestand.Equipments);
        TrainingseinheitenTabelle.Speichern(verbindung, transaktion, bestand.Trainingseinheiten);
        WartungenTabelle.Speichern(verbindung, transaktion, bestand.Wartungen);
        SchwellwerteTabelle.Speichern(verbindung, transaktion, bestand.Schwellwerte);

        // Erst hier wird alles endgültig geschrieben; ohne Commit verwirft Dispose die Änderungen.
        transaktion.Commit();
    }
}
