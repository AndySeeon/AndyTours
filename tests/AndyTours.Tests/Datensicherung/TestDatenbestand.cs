using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;

namespace AndyTours.Tests.Datensicherung;

/// <summary>
/// Legt für die Sicherungstests einen Datenordner mit einer kleinen, vollständigen Datenbank an.
/// </summary>
public static class TestDatenbestand
{
    /// <summary>
    /// Erzeugt einen Datenbestand mit einem Rad, einer Wartung und der angegebenen Zahl von Einheiten.
    /// </summary>
    /// <param name="anzahlEinheiten">Anzahl der Trainingseinheiten (0 = Datenbank ohne Touren).</param>
    /// <param name="name">Name der Einheiten; unterscheidet die Bestände verschiedener Datenordner.</param>
    public static Datenbestand Erzeuge(int anzahlEinheiten, string name = "Bründlingalm")
    {
        var bestand = new Datenbestand
        {
            Fahrer = { new Fahrer { Id = 1, Vorname = "Andy" } },
            Aktivitaeten = { new Aktivitaet { Id = 1, Name = "Bike", IstHaupt = true } },
            Equipments = { new Equipment { Id = 1, Name = "Genius", AktivitaetId = 1 } },
            Wartungen = { new Wartung { Id = 1, Datum = new DateOnly(2026, 9, 6), EquipmentId = 1, Taetigkeit = "Kette gewechselt" } },
        };

        for (int i = 1; i <= anzahlEinheiten; i++)
        {
            bestand.Trainingseinheiten.Add(new Trainingseinheit
            {
                Id = i, FahrerId = 1, AktivitaetId = 1, EquipmentId = 1,
                Datum = new DateOnly(2026, 8, i), Name = name, DistanzKm = 50 + i, Hoehenmeter = 500,
            });
        }

        return bestand;
    }

    /// <summary>
    /// Schreibt den Bestand als Datenbank in den Datenordner.
    /// </summary>
    /// <returns>Vollständiger Pfad der Datenbankdatei.</returns>
    public static string SpeichereIn(string datenordner, Datenbestand bestand)
    {
        string datei = Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei);
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei);
        SpeichereDatenbestand.Ausfuehren(verbindung, bestand);
        return datei;
    }

    /// <summary>Lädt den Datenbestand einer Datenbankdatei (z. B. einer Sicherung).</summary>
    public static Datenbestand Lade(string datei)
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei);
        return LadeDatenbestand.Ausfuehren(verbindung);
    }
}
