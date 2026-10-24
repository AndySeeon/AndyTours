using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AndyTours.App.Funktionen;
using AndyTours.App.Views;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Import.Garmin;

namespace AndyTours.App.ViewModels;

/// <summary>
/// Teil des Hauptfenster-ViewModels: neue Touren vom angeschlossenen Garmin-Gerät oder aus FIT-Dateien importieren.
/// </summary>
/// <remarks>
/// Ablauf beider Befehle:
/// <list type="number">
/// <item>Aufzeichnungen im Hintergrund lesen, dabei das Fortschrittsfenster zeigen. Touren, die schon in AndyTours
/// sind (gleiches Datum, gleiche Startzeit), fallen weg; beim Gerät schon vor dem Lesen anhand des Dateinamens.</item>
/// <item>Ohne Gerät bzw. ohne neue Touren einen Hinweis zeigen.</item>
/// <item>Das Fenster „Garmin-Import“ mit den neuen Touren öffnen; das Equipment ist vorbelegt
/// (<see cref="ErmittleImportEquipment"/>).</item>
/// <item>Je angehakter Tour: Aufzeichnung im Datenordner ablegen, Einheit samt Anhängen einfügen, Rad-Kennung und
/// Sensoren beim Equipment merken. Erst nach dem Schreiben ändert sich der Datenbestand im Speicher.</item>
/// <item>Die neuen Einheiten markieren, die Anzahl in der Statusleiste melden und übersprungene Aufzeichnungen
/// in einer Meldung nennen.</item>
/// </list>
/// Vom Gerät wird jede Aufzeichnung zuerst in einen temporären Ordner kopiert, der am Ende wieder gelöscht wird.
/// </remarks>
public partial class HauptfensterViewModel
{
    /// <summary>Titel der Meldungen beim Import.</summary>
    private const string Importtitel = "Garmin-Import";

    /// <summary>Hinweis, wenn kein Gerät angeschlossen ist.</summary>
    private const string KeinGeraet = "Kein Garmin-Gerät gefunden. Bitte den Edge per USB anschließen.";

    /// <summary>Hinweis, wenn alle Touren schon in AndyTours sind.</summary>
    private const string KeineNeuenTouren = "Keine neuen Touren gefunden.";

    /// <summary>
    /// Ergebnis des Lesens: ob ein Gerät gefunden wurde, die neuen Touren und die übersprungenen Aufzeichnungen.
    /// </summary>
    /// <param name="GeraetGefunden"><c>false</c>, wenn kein Garmin-Gerät angeschlossen ist (beim Dateiimport immer <c>true</c>).</param>
    /// <param name="Neue">Die neuen Touren mit der gelesenen Datei.</param>
    /// <param name="Uebersprungen">Nicht lesbare Aufzeichnungen, je „Dateiname: Grund“.</param>
    private sealed record Leseergebnis(bool GeraetGefunden, List<(Aufzeichnung Aufzeichnung, string Quelldatei)> Neue, List<string> Uebersprungen);

    /// <summary>Befehl „Datei → Vom Garmin importieren…“: liest die neuen Touren aller angeschlossenen Garmin-Geräte.</summary>
    [RelayCommand]
    private void VomGarminImportieren()
    {
        Statusmeldung = string.Empty;
        string zwischenordner = Path.Combine(Path.GetTempPath(), "AndyTours_Import_" + Guid.NewGuid().ToString("N"));
        List<Trainingseinheit> vorhanden = [.. _bestand.Trainingseinheiten];
        try
        {
            Leseergebnis ergebnis = LeseMitFortschritt("Garmin-Geräte werden gelesen …", fortschritt => LeseGeraete(zwischenordner, vorhanden, fortschritt));
            if (!ergebnis.GeraetGefunden)
            {
                MessageBox.Show(KeinGeraet, Importtitel, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Importiere(ergebnis);
        }
        catch (Exception fehler) when (fehler is COMException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Garmin-Geräte konnten nicht gelesen werden:\n\n{fehler.Message}", Importtitel, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            LoescheZwischenordner(zwischenordner);
        }
    }

    /// <summary>Befehl „Datei → FIT-Dateien importieren…“: liest die neuen Touren aus gewählten Aufzeichnungsdateien.</summary>
    [RelayCommand]
    private void FitDateienImportieren()
    {
        Statusmeldung = string.Empty;
        var dialog = new OpenFileDialog
        {
            Title = "FIT-Dateien importieren",
            Filter = "FIT-Dateien (*.fit)|*.fit|Alle Dateien (*.*)|*.*",
            Multiselect = true,
        };
        if (dialog.ShowDialog(Application.Current.MainWindow) != true)
        {
            return;
        }

        string[] dateien = dialog.FileNames;
        List<Trainingseinheit> vorhanden = [.. _bestand.Trainingseinheiten];
        Importiere(LeseMitFortschritt("FIT-Dateien werden gelesen …", fortschritt => LeseDateien(dateien, vorhanden, fortschritt)));
    }

    /// <summary>
    /// Sucht die Garmin-Geräte und liest ihre neuen Aufzeichnungen (läuft im Hintergrund).
    /// </summary>
    /// <param name="zwischenordner">Temporärer Ordner für die Kopien vom Gerät.</param>
    /// <param name="vorhanden">Die Einheiten des Datenbestands (Kopie der Liste, für die Duplikatprüfung).</param>
    /// <param name="fortschritt">Meldet die Anzahl der gelesenen Aufzeichnungen.</param>
    private static Leseergebnis LeseGeraete(string zwischenordner, List<Trainingseinheit> vorhanden, IProgress<KopierFortschritt> fortschritt)
    {
        List<Geraeteaufzeichnung> alle = [.. FindeLaufwerksgeraete.Ausfuehren(), .. FindeMtpGeraete.Ausfuehren()];
        if (alle.Count == 0)
        {
            return new Leseergebnis(false, [], []);
        }

        // Vorauswahl über den Dateinamen (lokale Startzeit), damit nicht jede alte Tour vom Gerät kopiert wird
        List<Geraeteaufzeichnung> kandidaten = alle
            .Where(a => ParseAufzeichnungsname.Ausfuehren(a.Dateiname) is not var (datum, startzeit)
                        || !IstTourVorhanden.Ausfuehren(vorhanden, datum, startzeit))
            .ToList();

        var ergebnis = new Leseergebnis(true, [], []);
        for (int i = 0; i < kandidaten.Count; i++)
        {
            Geraeteaufzeichnung aufzeichnung = kandidaten[i];

            // Je Aufzeichnung ein eigener Unterordner: Zwei Geräte können Dateien gleichen Namens haben
            string ziel = Path.Combine(zwischenordner, i.ToString(System.Globalization.CultureInfo.InvariantCulture), aufzeichnung.Dateiname);
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ziel)!);
                aufzeichnung.KopiereNach(ziel);
                Lies(ziel, vorhanden, ergebnis);
            }
            catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
            {
                ergebnis.Uebersprungen.Add($"{aufzeichnung.Dateiname} ({aufzeichnung.Geraet}): {fehler.Message}");
            }

            fortschritt.Report(new KopierFortschritt(i + 1, kandidaten.Count, 0, 0));
        }

        return ergebnis;
    }

    /// <summary>
    /// Liest die gewählten Aufzeichnungsdateien (läuft im Hintergrund).
    /// </summary>
    /// <param name="dateien">Vollständige Pfade der Dateien.</param>
    /// <param name="vorhanden">Die Einheiten des Datenbestands (Kopie der Liste, für die Duplikatprüfung).</param>
    /// <param name="fortschritt">Meldet die Anzahl der gelesenen Dateien.</param>
    private static Leseergebnis LeseDateien(string[] dateien, List<Trainingseinheit> vorhanden, IProgress<KopierFortschritt> fortschritt)
    {
        var ergebnis = new Leseergebnis(true, [], []);
        for (int i = 0; i < dateien.Length; i++)
        {
            Lies(dateien[i], vorhanden, ergebnis);
            fortschritt.Report(new KopierFortschritt(i + 1, dateien.Length, 0, 0));
        }

        return ergebnis;
    }

    /// <summary>
    /// Liest eine Aufzeichnung und nimmt sie als neue Tour auf, wenn sie noch nicht vorhanden ist.
    /// </summary>
    /// <remarks>
    /// Jede aufgenommene Tour kommt auch in <paramref name="vorhanden"/>, damit dieselbe Tour aus einer zweiten
    /// Datei (z. B. von zwei Geräten) nur einmal erscheint.
    /// </remarks>
    private static void Lies(string pfad, List<Trainingseinheit> vorhanden, Leseergebnis ergebnis)
    {
        (Aufzeichnung? aufzeichnung, string? fehler) = LeseFitAufzeichnung.Ausfuehren(pfad);
        if (aufzeichnung is null)
        {
            ergebnis.Uebersprungen.Add($"{Path.GetFileName(pfad)}: {fehler}");
            return;
        }

        if (IstTourVorhanden.Ausfuehren(vorhanden, aufzeichnung.Datum, aufzeichnung.Startzeit))
        {
            return;
        }

        vorhanden.Add(new Trainingseinheit { Datum = aufzeichnung.Datum, Startzeit = aufzeichnung.Startzeit });
        ergebnis.Neue.Add((aufzeichnung, pfad));
    }

    /// <summary>
    /// Zeigt das Fortschrittsfenster modal an, führt die Arbeit im Hintergrund aus und liefert ihr Ergebnis.
    /// </summary>
    /// <remarks>
    /// Wie <see cref="KopiereMitFortschritt"/>: Die Oberfläche bleibt bedienbar, die übrigen Fenster sind gesperrt,
    /// und ein unerwarteter Fehler wird nach dem Schließen mit ursprünglichem Stacktrace weitergegeben.
    /// </remarks>
    private static Leseergebnis LeseMitFortschritt(string titel, Func<IProgress<KopierFortschritt>, Leseergebnis> arbeit)
    {
        var viewModel = new FortschrittsViewModel(titel);
        var fenster = new FortschrittsFenster(viewModel) { Owner = Application.Current.MainWindow };
        Leseergebnis? ergebnis = null;
        Exception? fehler = null;

        fenster.Loaded += async (_, _) =>
        {
            var fortschritt = new Progress<KopierFortschritt>(viewModel.Aktualisiere);
            try
            {
                ergebnis = await Task.Run(() => arbeit(fortschritt));
            }
            catch (Exception f)
            {
                fehler = f; // erst nach dem Schließen an den Aufrufer weitergeben
            }
            finally
            {
                fenster.DarfSchliessen = true;
                fenster.Close();
            }
        };

        fenster.ShowDialog();
        if (fehler is not null)
        {
            ExceptionDispatchInfo.Throw(fehler);
        }

        return ergebnis!;
    }

    /// <summary>
    /// Zeigt die neuen Touren im Fenster „Garmin-Import“ und importiert die angehakten.
    /// </summary>
    private void Importiere(Leseergebnis ergebnis)
    {
        if (ergebnis.Neue.Count == 0)
        {
            // Keine neuen Touren: Gab es nicht lesbare Aufzeichnungen, werden diese genannt
            string? meldung = BeschreibeImportergebnis.Ausfuehren(0, ergebnis.Uebersprungen).Meldung;
            MessageBox.Show(meldung ?? KeineNeuenTouren, Importtitel, MessageBoxButton.OK,
                meldung is null ? MessageBoxImage.Information : MessageBoxImage.Warning);
            return;
        }

        List<Auswahleintrag> equipments = _bestand.Equipments
            .Where(e => !e.Ausgemustert)
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(e => new Auswahleintrag(e.Id, e.Name))
            .ToList();
        List<Importzeile> zeilen = ergebnis.Neue
            .OrderBy(n => n.Aufzeichnung.Datum).ThenBy(n => n.Aufzeichnung.Startzeit)
            .Select(n => new Importzeile(n.Aufzeichnung, n.Quelldatei,
                ErmittleImportEquipment.Ausfuehren(_bestand, n.Aufzeichnung) is Equipment vorbelegt
                    ? new Auswahleintrag(vorbelegt.Id, vorbelegt.Name)
                    : new Auswahleintrag(null, GarminImportViewModel.Keines)))
            .ToList();

        var viewModel = new GarminImportViewModel(zeilen, equipments);
        var fenster = new GarminImportFenster(viewModel) { Owner = Application.Current.MainWindow };
        if (fenster.ShowDialog() != true)
        {
            return;
        }

        List<string> uebersprungen = [.. ergebnis.Uebersprungen];
        List<long> importiert = [];
        foreach (Importzeile zeile in viewModel.Ergebnis)
        {
            if (!ImportiereTour(zeile, uebersprungen, out long? id))
            {
                break; // Datenbankfehler: Meldung kam schon, weitere Versuche wären vergeblich
            }

            if (id is long neu)
            {
                importiert.Add(neu);
            }
        }

        // Anzeige neu aufbauen und die neuen Einheiten markieren, soweit sie in der aktuellen Auswahl liegen
        if (importiert.Count > 0)
        {
            AktualisiereJahre();
            AktualisiereStatustext();
            AktualisiereSummen();
            var ids = importiert.ToHashSet();
            MarkiereEinheiten?.Invoke(Einheitszeilen.Where(z => ids.Contains(z.Id)).Select(z => z.Id).ToList());
        }

        (string statustext, string? hinweis) = BeschreibeImportergebnis.Ausfuehren(importiert.Count, uebersprungen);
        Statusmeldung = statustext;
        if (hinweis is not null)
        {
            MessageBox.Show(hinweis, Importtitel, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Importiert eine Tour: Aufzeichnung ablegen, Einheit einfügen, Kennungen beim Equipment merken.
    /// </summary>
    /// <param name="zeile">Die angehakte Zeile.</param>
    /// <param name="uebersprungen">Nimmt die Aufzeichnung auf, wenn sie nicht abgelegt werden kann.</param>
    /// <param name="id">Nummer der neuen Einheit; <c>null</c>, wenn die Tour übersprungen wurde.</param>
    /// <returns><c>false</c> nach einem Datenbankfehler (der Import wird dann abgebrochen).</returns>
    private bool ImportiereTour(Importzeile zeile, List<string> uebersprungen, out long? id)
    {
        id = null;
        Aufzeichnung aufzeichnung = zeile.Aufzeichnung;
        long? equipmentId = zeile.Equipment.Id;

        string relativerPfad;
        try
        {
            relativerPfad = LegeAufzeichnungAb.Ausfuehren(Datenordner, zeile.Quelldatei, aufzeichnung.Dateiname, aufzeichnung.Datum.Year);
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            uebersprungen.Add($"{aufzeichnung.Dateiname}: {fehler.Message}");
            return true;
        }

        // Einheit samt Anhängen einfügen
        Trainingseinheit einheit = ErstelleImportierteEinheit.Ausfuehren(_bestand, aufzeichnung, equipmentId, relativerPfad);
        (long Id, long FahrerId) neu = default;
        if (!Schreibe(verbindung => neu = TrainingseinheitenTabelle.Einfuegen(verbindung, einheit),
                $"Die Tour {aufzeichnung.Dateiname} konnte nicht gespeichert werden"))
        {
            return false;
        }

        einheit.Id = neu.Id;
        einheit.FahrerId = neu.FahrerId;
        if (_bestand.Fahrer.All(f => f.Id != neu.FahrerId))
        {
            // Der Standardsportler wurde eben erst angelegt (leerer Datenordner)
            _bestand.Fahrer.Add(new Fahrer { Id = neu.FahrerId });
        }

        _bestand.Trainingseinheiten.Add(einheit);
        id = einheit.Id;

        // Rad-Kennung und Sensoren beim Equipment merken (und beim bisherigen entfernen)
        List<Equipment> gemerkt = MerkeRadkennungen.Ausfuehren(_bestand, aufzeichnung, equipmentId);
        if (gemerkt.Count > 0)
        {
            if (!SchreibeInTransaktion((verbindung, transaktion) =>
                {
                    foreach (Equipment equipment in gemerkt)
                    {
                        EquipmentsTabelle.Aendern(verbindung, transaktion, equipment);
                    }
                }, "Die Gerätezuordnung konnte nicht gespeichert werden"))
            {
                return false;
            }

            UebernimmEquipment.Einsetzen(_bestand, gemerkt);
        }

        return true;
    }

    /// <summary>Löscht den temporären Ordner der Gerätekopien; ein Fehler dabei wird ignoriert.</summary>
    private static void LoescheZwischenordner(string ordner)
    {
        try
        {
            if (Directory.Exists(ordner))
            {
                Directory.Delete(ordner, recursive: true);
            }
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
        {
            // bewusst ignoriert: Reste im Temp-Ordner räumt Windows auf
        }
    }
}
