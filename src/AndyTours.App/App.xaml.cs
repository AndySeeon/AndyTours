using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.Sqlite;
using AndyTours.App.Funktionen;
using AndyTours.App.ViewModels;
using AndyTours.App.Views;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App;

/// <summary>
/// Anwendungsklasse von AndyTours: steuert den Programmstart.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Programmstart: Datenordner ermitteln und prüfen, bei Bedarf nachfragen, dann das Hauptfenster öffnen.
    /// </summary>
    /// <remarks>
    /// Ablauf:
    /// <list type="number">
    /// <item>Datenordner ermitteln: Entwicklung → <c>AppDaten</c>, sonst gemerkter Ordner oder <c>Dokumente\AndyTours</c>.</item>
    /// <item>Ordner prüfen (ein fehlender Ordner wird dabei angelegt).</item>
    /// <item>Nicht erreichbar oder nicht beschreibbar → Meldung mit „Erneut versuchen / Anderen Ordner wählen / Beenden“,
    /// so lange, bis ein brauchbarer Ordner vorliegt oder der Benutzer beendet.</item>
    /// <item>Hauptfenster mit dem Ordner öffnen.</item>
    /// </list>
    /// <c>async void</c> ist hier zulässig, weil es sich um einen Ereignishandler handelt.
    /// </remarks>
    private async void AnwendungStartet(object sender, StartupEventArgs e)
    {
        string ordner;
        try
        {
            ordner = ErmittleStartDatenordner.Ausfuehren(Programmpfade.IstEntwicklung, Programmpfade.Programmordner,
                LeseGemerktenDatenordner.Ausfuehren(Programmpfade.Merkdatei), Programmpfade.Standardordner);
        }
        catch (InvalidOperationException fehler)
        {
            MessageBox.Show(fehler.Message, "AndyTours", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        bool anderenOrdnerGewaehlt = false;
        while (true)
        {
            DatenordnerZustand zustand;
            Mouse.OverrideCursor = Cursors.Wait; // Prüfung kann bei Netzlaufwerken einige Sekunden dauern
            try
            {
                zustand = await PruefeDatenordner.Ausfuehren(ordner);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            if (zustand is DatenordnerZustand.MitDaten or DatenordnerZustand.Leer)
            {
                break;
            }

            string meldung = zustand == DatenordnerZustand.NichtBeschreibbar
                ? $"AndyTours darf im Datenordner\n\n{ordner}\n\nnicht schreiben."
                : $"Der Datenordner\n\n{ordner}\n\nist nicht erreichbar. Ist z. B. das NAS ausgeschaltet oder das Laufwerk getrennt?";
            var rueckfrage = new DatenordnerNichtErreichbarFenster(meldung);
            rueckfrage.ShowDialog();

            if (rueckfrage.Gewaehlt == DatenordnerNichtErreichbarFenster.Aktion.Beenden)
            {
                Shutdown();
                return;
            }

            if (rueckfrage.Gewaehlt == DatenordnerNichtErreichbarFenster.Aktion.AndererOrdner)
            {
                // Keine Kopier-Rückfrage: Der bisherige Ordner ist ja nicht erreichbar.
                string? neu = await WechsleDatenordner.Ausfuehren(ordner, kopierenAnbieten: false);
                if (neu is not null)
                {
                    ordner = neu;
                    anderenOrdnerGewaehlt = true;
                }
            }

            // „Erneut versuchen“ und ein abgebrochener Ordnerwechsel: Schleife prüft erneut
        }

        HauptfensterViewModel viewModel;
        try
        {
            viewModel = new HauptfensterViewModel(ordner);
        }
        catch (Exception fehler) when (fehler is SqliteException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Die Daten im Ordner\n{ordner}\nkonnten nicht geladen werden:\n\n{fehler.Message}",
                "AndyTours", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Ein beim Start neu gewählter Ordner gilt auch für die nächsten Starts (nicht in der Entwicklung).
        if (anderenOrdnerGewaehlt && !Programmpfade.IstEntwicklung)
        {
            MerkeDatenordner.Ausfuehren(Programmpfade.Merkdatei, ordner);
        }

        var hauptfenster = new Hauptfenster(viewModel);
        MainWindow = hauptfenster;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        hauptfenster.Show();
    }
}
