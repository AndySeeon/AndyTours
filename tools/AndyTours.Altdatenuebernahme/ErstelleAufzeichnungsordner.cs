using System.Text;
using Microsoft.Data.Sqlite;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Garmin;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Baut aus Aufzeichnungen im FIT-Format einen neuen Datenbestand im Zwischenordner auf (Spezifikation 033).
/// </summary>
public static class ErstelleAufzeichnungsordner
{
    /// <summary>Name der Aktivität, die alle übernommenen Touren bekommen.</summary>
    public const string Aktivitaetsname = "Radfahren";

    /// <summary>Grund im Bericht, wenn dieselbe Tour schon aus einer anderen Datei übernommen wurde.</summary>
    public const string GrundDoppelt = "Dieselbe Tour (gleiches Datum, gleiche Startzeit) wurde schon aus einer anderen Datei übernommen.";

    /// <summary>
    /// Liest die Aufzeichnungen und schreibt Datenbank, abgelegte Aufzeichnungen und Bericht in den Zwischenordner.
    /// </summary>
    /// <remarks>
    /// Aus jeder Aufzeichnung entsteht eine Tour mit denselben Fachfunktionen wie beim Import vom Edge
    /// (<see cref="LeseFitAufzeichnung"/>, <see cref="IstTourVorhanden"/>, <see cref="LegeAufzeichnungAb"/>,
    /// <see cref="ErstelleImportierteEinheit"/>, <see cref="TrainingseinheitenTabelle.Einfuegen"/>); die Werte sind
    /// deshalb dieselben.
    /// <list type="bullet">
    /// <item>Rad: keines.</item>
    /// <item>Aktivität: Sie ist in der Datenbank ein Pflichtfeld. Deshalb wird im neuen Bestand die Aktivität
    /// <see cref="Aktivitaetsname"/> als Hauptaktivität angelegt; der Import ordnet ohne Rad die Hauptaktivität zu.</item>
    /// <item>Nicht lesbare Dateien und Dateien mit einer schon übernommenen Tour werden übersprungen und mit Grund
    /// im Bericht <see cref="ErstelleAufzeichnungsbericht.Berichtsdatei"/> genannt.</item>
    /// <item>Die Touren werden nach Datum und Startzeit eingefügt, damit ihre Nummern der zeitlichen Folge entsprechen.</item>
    /// </list>
    /// Wie bei der Umwandlung bleibt das Ziel unberührt, bis alles fertig ist (siehe <see cref="TauscheZielordnerAus"/>).
    /// Bei einem Fehler wird der Zwischenordner wieder gelöscht und der Fehler weitergegeben. Die Aufzeichnungen
    /// werden nur gelesen.
    /// </remarks>
    /// <param name="aufzeichnungen">Vollständige Pfade der FIT-Dateien (siehe <c>ErkenneAltdaten</c>).</param>
    /// <param name="quellordner">Ordner mit den Aufzeichnungen; für die Dateinamen im Bericht.</param>
    /// <param name="zielordner">Späterer Zielordner; bestimmt Ort und Namen des Zwischenordners.</param>
    /// <param name="zeitpunkt">Zeitpunkt der Übernahme für den Bericht.</param>
    /// <returns>Pfad des fertigen Zwischenordners und das Ergebnis.</returns>
    public static (string Zwischenordner, Aufzeichnungsergebnis Ergebnis) Ausfuehren(
        IReadOnlyList<string> aufzeichnungen, string quellordner, string zielordner, DateTime zeitpunkt)
    {
        // Zuerst alles lesen; nicht lesbare und doppelte Aufzeichnungen aussortieren
        var uebersprungen = new List<(string Datei, string Grund)>();
        var gelesen = new List<(Aufzeichnung Aufzeichnung, string Pfad)>();
        foreach (string pfad in aufzeichnungen)
        {
            (Aufzeichnung? aufzeichnung, string? fehler) = LeseFitAufzeichnung.Ausfuehren(pfad);
            if (aufzeichnung is null)
            {
                uebersprungen.Add((Path.GetRelativePath(quellordner, pfad), fehler ?? "nicht lesbar"));
            }
            else
            {
                gelesen.Add((aufzeichnung, pfad));
            }
        }

        string zwischenordner = ErstelleZwischenordner.LegeLeerAn(zielordner);
        try
        {
            int uebernommen = 0;
            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(Path.Combine(zwischenordner, DatenordnerInhalt.Datenbankdatei)))
            {
                Datenbestand bestand = LegeAktivitaetAn(verbindung);
                foreach ((Aufzeichnung aufzeichnung, string pfad) in gelesen
                    .OrderBy(g => g.Aufzeichnung.Datum).ThenBy(g => g.Aufzeichnung.Startzeit).ThenBy(g => g.Pfad, StringComparer.OrdinalIgnoreCase))
                {
                    if (IstTourVorhanden.Ausfuehren(bestand.Trainingseinheiten, aufzeichnung.Datum, aufzeichnung.Startzeit))
                    {
                        uebersprungen.Add((Path.GetRelativePath(quellordner, pfad), GrundDoppelt));
                        continue;
                    }

                    string relativerPfad = LegeAufzeichnungAb.Ausfuehren(zwischenordner, pfad, aufzeichnung.Dateiname, aufzeichnung.Datum.Year);
                    Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(bestand, aufzeichnung, equipmentId: null, relativerPfad);
                    (long id, long fahrerId) = TrainingseinheitenTabelle.Einfuegen(verbindung, einheit);
                    einheit.Id = id;
                    einheit.FahrerId = fahrerId;
                    if (bestand.Fahrer.Count == 0)
                    {
                        // Der Standardsportler wurde mit der ersten Tour angelegt; die folgenden verwenden ihn
                        bestand.Fahrer.Add(new Fahrer { Id = fahrerId });
                    }

                    bestand.Trainingseinheiten.Add(einheit);
                    uebernommen++;
                }
            }

            var ergebnis = new Aufzeichnungsergebnis(aufzeichnungen.Count, uebernommen, uebersprungen);

            // Bericht mit Byte-Order-Mark, damit auch der Windows-Editor die Umlaute richtig anzeigt
            File.WriteAllText(Path.Combine(zwischenordner, ErstelleAufzeichnungsbericht.Berichtsdatei),
                ErstelleAufzeichnungsbericht.Bericht(ergebnis, quellordner, zeitpunkt), Encoding.UTF8);

            return (zwischenordner, ergebnis);
        }
        catch
        {
            // Halbfertigen Zwischenordner nicht liegen lassen
            try
            {
                Directory.Delete(zwischenordner, recursive: true);
            }
            catch (Exception aufraeumen) when (aufraeumen is IOException or UnauthorizedAccessException)
            {
                // Die ursprüngliche Fehlermeldung ist wichtiger; der Rest wird beim nächsten Lauf entfernt.
            }

            throw;
        }
    }

    /// <summary>
    /// Legt in der neuen, leeren Datenbank die Aktivität <see cref="Aktivitaetsname"/> als Hauptaktivität an.
    /// </summary>
    /// <returns>Ein Bestand, der nur diese Aktivität enthält; er nimmt danach die übernommenen Touren auf.</returns>
    private static Datenbestand LegeAktivitaetAn(SqliteConnection verbindung)
    {
        var aktivitaet = new Aktivitaet { Name = Aktivitaetsname, IstHaupt = true };
        using (SqliteTransaction transaktion = verbindung.BeginTransaction())
        {
            aktivitaet.Id = AktivitaetenTabelle.Einfuegen(verbindung, transaktion, aktivitaet);
            transaktion.Commit();
        }

        var bestand = new Datenbestand();
        bestand.Aktivitaeten.Add(aktivitaet);
        return bestand;
    }
}
