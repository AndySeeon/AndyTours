using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;

namespace AndyTours.Daten;

/// <summary>
/// Lädt den gesamten Datenbestand aus der Datenbank in den Arbeitsspeicher.
/// </summary>
public static class LadeDatenbestand
{
    /// <summary>
    /// Liest alle Tabellen und setzt daraus einen <see cref="Datenbestand"/> zusammen.
    /// </summary>
    /// <param name="verbindung">Geöffnete Verbindung (siehe <see cref="OeffneDatenbank"/>).</param>
    /// <returns>Der vollständige Datenbestand.</returns>
    public static Datenbestand Ausfuehren(SqliteConnection verbindung)
    {
        return new Datenbestand
        {
            Fahrer = LadeFahrer.Ausfuehren(verbindung),
            Aktivitaeten = LadeAktivitaeten.Ausfuehren(verbindung),
            Kategorien = LadeKategorien.Ausfuehren(verbindung),
            Equipments = LadeEquipments.Ausfuehren(verbindung),
            Trainingseinheiten = LadeTrainingseinheiten.Ausfuehren(verbindung),
            Wartungen = LadeWartungen.Ausfuehren(verbindung),
            Schwellwerte = LadeSchwellwerte.Ausfuehren(verbindung),
        };
    }
}
