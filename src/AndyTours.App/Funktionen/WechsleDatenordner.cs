using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Führt den Benutzer durch den Wechsel des Datenordners: Ordner wählen, prüfen, nachfragen, ggf. kopieren.
/// </summary>
public static class WechsleDatenordner
{
    /// <summary>Überschrift aller Meldungen dieses Ablaufs.</summary>
    private const string Ueberschrift = "Datenordner ändern";

    /// <summary>
    /// Lässt den Benutzer einen neuen Datenordner wählen und bereitet ihn vor.
    /// </summary>
    /// <remarks>
    /// Ablauf nach der Ordnerwahl, je nach Ergebnis von <see cref="PruefeDatenordner"/>:
    /// <list type="bullet">
    /// <item>Nicht erreichbar / nicht beschreibbar → Meldung, bisheriger Ordner bleibt.</item>
    /// <item>Schon AndyTours-Daten darin → Hinweis mit „OK“ (Daten verwenden) oder „Abbrechen“ (bisheriger Ordner bleibt).</item>
    /// <item>Leer → Rückfrage, ob die bisherigen Daten dorthin kopiert werden sollen (mit Fortschrittsanzeige);
    /// „Nein“ = mit leerem Datenbestand beginnen. Schlägt das Kopieren fehl → Meldung, bisheriger Ordner bleibt.</item>
    /// </list>
    /// Der bisherige Ordner wird nur gelesen und bleibt unverändert. Laden und Merken des neuen
    /// Ordners übernimmt der Aufrufer, damit nur ein erfolgreich geladener Ordner gemerkt wird.
    /// </remarks>
    /// <param name="bisherigerOrdner">Der aktuell verwendete Datenordner.</param>
    /// <param name="kopierenAnbieten"><c>false</c> beim Programmstart mit nicht erreichbarem Ordner – dann gibt es nichts zu kopieren.</param>
    /// <returns>Der neue Datenordner oder <c>null</c>, wenn der bisherige aktiv bleiben soll.</returns>
    public static async Task<string?> Ausfuehren(string bisherigerOrdner, bool kopierenAnbieten)
    {
        Window? besitzer = AktivesFenster();

        var dialog = new OpenFolderDialog
        {
            Title = "Neuen Datenordner für AndyTours wählen",
            InitialDirectory = Directory.Exists(bisherigerOrdner) ? bisherigerOrdner : string.Empty,
        };
        bool? gewaehlt = besitzer is null ? dialog.ShowDialog() : dialog.ShowDialog(besitzer);
        if (gewaehlt != true)
        {
            return null; // Auswahl abgebrochen
        }

        string neu = Path.GetFullPath(dialog.FolderName);
        if (string.Equals(neu.TrimEnd('\\'), Path.GetFullPath(bisherigerOrdner).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
        {
            return null; // derselbe Ordner – nichts zu tun
        }

        DatenordnerZustand zustand;
        Mouse.OverrideCursor = Cursors.Wait; // Prüfung kann bei Netzlaufwerken einige Sekunden dauern
        try
        {
            zustand = await PruefeDatenordner.Ausfuehren(neu);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        switch (zustand)
        {
            case DatenordnerZustand.NichtErreichbar:
                Meldung(besitzer, $"Der Ordner\n\n{neu}\n\nist nicht erreichbar. Der bisherige Datenordner bleibt aktiv.",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            case DatenordnerZustand.NichtBeschreibbar:
                Meldung(besitzer, $"AndyTours darf im Ordner\n\n{neu}\n\nnicht schreiben. Bitte wählen Sie einen anderen Ordner.\n\nDer bisherige Datenordner bleibt aktiv.",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;

            case DatenordnerZustand.MitDaten:
                MessageBoxResult verwenden = Meldung(besitzer,
                    $"Im Ordner\n\n{neu}\n\nliegen bereits AndyTours-Daten. AndyTours verwendet ab jetzt diese Daten.\n\n" +
                    "Die Daten im bisherigen Ordner bleiben unverändert erhalten.",
                    MessageBoxButton.OKCancel, MessageBoxImage.Information);
                return verwenden == MessageBoxResult.OK ? neu : null;

            default: // Leer
                if (!kopierenAnbieten)
                {
                    return neu;
                }

                MessageBoxResult kopieren = Meldung(besitzer,
                    $"Im Ordner\n\n{neu}\n\nliegen noch keine AndyTours-Daten.\n\n" +
                    "Sollen die bisherigen Daten dorthin kopiert werden?\n\n" +
                    "Ja: alle Daten werden kopiert, der bisherige Ordner bleibt unverändert.\n" +
                    "Nein: AndyTours beginnt im neuen Ordner mit einem leeren Datenbestand.",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (kopieren != MessageBoxResult.Yes)
                {
                    return neu;
                }

                try
                {
                    KopiereMitFortschritt.Ausfuehren(besitzer, bisherigerOrdner, neu, "Daten werden kopiert …");
                }
                catch (Exception fehler) when (fehler is IOException or UnauthorizedAccessException)
                {
                    Meldung(besitzer, $"Das Kopieren ist fehlgeschlagen:\n\n{fehler.Message}\n\n" +
                        "Bereits kopierte Dateien wurden wieder entfernt. Der bisherige Datenordner bleibt aktiv.",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                return neu;
        }
    }

    /// <summary>Das gerade aktive Fenster der App (Besitzer für Dialoge) oder <c>null</c> vor dem Öffnen des Hauptfensters.</summary>
    private static Window? AktivesFenster() =>
        Application.Current.Windows.OfType<Window>().FirstOrDefault(f => f.IsActive) ?? Application.Current.MainWindow;

    /// <summary>Zeigt eine Meldung – mit Besitzerfenster, falls vorhanden, damit sie über diesem erscheint.</summary>
    private static MessageBoxResult Meldung(Window? besitzer, string text, MessageBoxButton knoepfe, MessageBoxImage symbol) =>
        besitzer is null
            ? MessageBox.Show(text, Ueberschrift, knoepfe, symbol)
            : MessageBox.Show(besitzer, text, Ueberschrift, knoepfe, symbol);
}
