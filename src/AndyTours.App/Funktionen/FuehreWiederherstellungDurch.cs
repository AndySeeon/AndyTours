using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using AndyTours.App.ViewModels;
using AndyTours.App.Views;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Daten.Datenordner;
using AndyTours.Daten.Datensicherung;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Führt den Benutzer durch „Extras → Sicherung wiederherstellen…“: Sicherung wählen, Rückfrage, Zurückspielen.
/// </summary>
public static class FuehreWiederherstellungDurch
{
    /// <summary>Überschrift aller Meldungen dieses Ablaufs.</summary>
    private const string Ueberschrift = "Sicherung wiederherstellen";

    /// <summary>
    /// Lässt den Benutzer eine Sicherung wählen und spielt sie nach Bestätigung zurück.
    /// </summary>
    /// <remarks>
    /// Ablauf:
    /// <list type="number">
    /// <item>Sicherungsordner aus den Einstellungen des Datenordners ermitteln und die Sicherungen darin
    /// lesen – im Hintergrund mit Wartecursor und Zeitlimit, damit ein ausgeschaltetes NAS die App
    /// nicht blockiert.</item>
    /// <item>Keine Sicherungen oder Ordner nicht erreichbar → Hinweis, Ende.</item>
    /// <item>Auswahlfenster; Abbrechen → Ende.</item>
    /// <item>Rückfrage mit Datum und Uhrzeit der Sicherung (vorbelegt „Nein“); Nein → Ende.</item>
    /// <item><see cref="SpieleSicherungZurueck"/> im Hintergrund mit Wartecursor – ohne Zeitlimit, denn ein
    /// aufgegebener, aber weiterlaufender Vorgang könnte die Datenbank sonst noch nach der Meldung ersetzen.</item>
    /// <item>Erfolg → <paramref name="neuLaden"/> aufrufen, dann Erfolgsmeldung mit der zusätzlichen
    /// Sicherung des vorherigen Stands. Sonst → verständliche Meldung; der Bestand ist unverändert.</item>
    /// </list>
    /// </remarks>
    /// <param name="datenordner">Der aktive Datenordner.</param>
    /// <param name="neuLaden">Lädt den Datenbestand im Hauptfenster neu (nur nach Erfolg aufgerufen).</param>
    /// <returns><c>true</c>, wenn eine Sicherung zurückgespielt wurde.</returns>
    public static async Task<bool> Ausfuehren(string datenordner, Action neuLaden)
    {
        Window? besitzer = Application.Current.Windows.OfType<Window>().FirstOrDefault(f => f.IsActive) ?? Application.Current.MainWindow;
        string datenbankdatei = Path.Combine(datenordner, DatenordnerInhalt.Datenbankdatei);

        // 1. Sicherungsordner und Sicherungen ermitteln
        string sicherungsordner;
        List<Sicherungseintrag> sicherungen;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            Sicherungseinstellungen einstellungen;
            using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datenbankdatei))
            {
                einstellungen = LadeSicherungseinstellungen.Ausfuehren(verbindung);
            }

            sicherungsordner = ErmittleSicherungsordner.Ausfuehren(datenordner, einstellungen);
            string ordner = sicherungsordner;
            sicherungen = await Task.Run(() => ErmittleSicherungen.Ausfuehren(ordner))
                .WaitAsync(FuehreStartsicherungDurch.StandardZeitlimit);
        }
        catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException or SqliteException or TimeoutException)
        {
            Meldung(besitzer, $"Die Sicherungen konnten nicht gelesen werden:\n\n" +
                BeschreibeSicherungsfehler.Ausfuehren(fehler, FuehreStartsicherungDurch.StandardZeitlimit) +
                "\n\nDer Datenbestand bleibt unverändert.", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        // 2. Keine Sicherungen
        if (sicherungen.Count == 0)
        {
            Meldung(besitzer, $"Im Sicherungsordner\n\n{sicherungsordner}\n\nwurden keine Sicherungen gefunden. " +
                "Der Ordner ist leer, noch nicht angelegt oder nicht erreichbar.\n\nDer Datenbestand bleibt unverändert.",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        // 3. Auswahl
        var auswahl = new SicherungAuswahlViewModel(sicherungen, sicherungsordner);
        var fenster = new SicherungAuswahlFenster(auswahl) { Owner = besitzer };
        if (fenster.ShowDialog() != true || auswahl.Ergebnis is not Sicherungseintrag gewaehlt)
        {
            return false;
        }

        // 4. Rückfrage
        string stand = SicherungAuswahlViewModel.FormatiereZeitpunkt(gewaehlt.Zeitpunkt);
        MessageBoxResult antwort = Meldung(besitzer,
            $"Soll der Datenbestand auf den Stand der Sicherung vom\n\n{stand}\n\nzurückgesetzt werden?\n\n" +
            "Alle Änderungen nach diesem Zeitpunkt gehen dabei verloren. Der aktuelle Stand wird vorher " +
            "als zusätzliche Sicherung abgelegt.",
            MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
        if (antwort != MessageBoxResult.Yes)
        {
            return false;
        }

        // 5. Zurückspielen
        DateTime jetzt = DateTime.Now;
        Wiederherstellungsergebnis ergebnis;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            ergebnis = await Task.Run(() => SpieleSicherungZurueck.Ausfuehren(datenordner, gewaehlt.Datei, jetzt));
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        // 6. Ergebnis melden
        switch (ergebnis.Status)
        {
            case Wiederherstellungsstatus.Wiederhergestellt:
                neuLaden();
                Meldung(besitzer,
                    $"Der Datenbestand wurde auf den Stand vom\n\n{stand}\n\nzurückgesetzt.\n\n" +
                    $"Der vorherige Stand ist als Sicherung vom {SicherungAuswahlViewModel.FormatiereZeitpunkt(jetzt)} abgelegt " +
                    "und lässt sich ebenso wiederherstellen.",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return true;

            case Wiederherstellungsstatus.Unbrauchbar:
                Meldung(besitzer, $"Die Sicherung vom {stand} ist beschädigt oder enthält keine AndyTours-Daten.\n\n" +
                    "Der Datenbestand bleibt unverändert.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;

            case Wiederherstellungsstatus.NeuereVersion:
                Meldung(besitzer, $"Die Sicherung vom {stand} stammt von einer neueren AndyTours-Version und kann " +
                    "mit dieser Version nicht verwendet werden.\n\nDer Datenbestand bleibt unverändert.",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;

            case Wiederherstellungsstatus.SicherungFehlgeschlagen:
                Meldung(besitzer, $"Der aktuelle Stand konnte nicht gesichert werden:\n\n{ergebnis.Fehlergrund}\n\n" +
                    "Deshalb wurde nichts wiederhergestellt; der Datenbestand bleibt unverändert.",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;

            default: // Fehlgeschlagen
                Meldung(besitzer, $"Das Wiederherstellen ist fehlgeschlagen:\n\n{ergebnis.Fehlergrund}\n\n" +
                    "Der bisherige Datenbestand ist vollständig erhalten.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
        }
    }

    /// <summary>Zeigt eine Meldung – mit Besitzerfenster, falls vorhanden, damit sie über diesem erscheint.</summary>
    private static MessageBoxResult Meldung(Window? besitzer, string text, MessageBoxButton knoepfe, MessageBoxImage symbol,
        MessageBoxResult vorbelegt = MessageBoxResult.None) =>
        besitzer is null
            ? MessageBox.Show(text, Ueberschrift, knoepfe, symbol, vorbelegt)
            : MessageBox.Show(besitzer, text, Ueberschrift, knoepfe, symbol, vorbelegt);
}
