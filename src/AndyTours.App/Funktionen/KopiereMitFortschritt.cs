using System.Runtime.ExceptionServices;
using System.Windows;
using AndyTours.App.ViewModels;
using AndyTours.App.Views;
using AndyTours.Daten.Datenordner;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Kopiert einen Datenbestand und zeigt dabei das Fortschrittsfenster an.
/// </summary>
public static class KopiereMitFortschritt
{
    /// <summary>
    /// Zeigt das Fortschrittsfenster modal an, kopiert im Hintergrund und kehrt nach dem Ende zurück.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Das Kopieren läuft in einem Hintergrund-Thread (<c>Task.Run</c>), damit die
    /// Oberfläche nicht einfriert – Fenster lassen sich verschieben und neu zeichnen.</item>
    /// <item>Das Fenster ist modal: Alle übrigen Fenster der App sind währenddessen gesperrt.</item>
    /// <item><see cref="Progress{T}"/> leitet die Meldungen automatisch in den Oberflächen-Thread um,
    /// wo das ViewModel sie anzeigt.</item>
    /// </list>
    /// Wiederverwendbar für jeden Kopiervorgang mit Datenbestand (z. B. die spätere Datensicherung).
    /// </remarks>
    /// <param name="besitzer">Fenster, über dem der Fortschritt erscheint, oder <c>null</c>.</param>
    /// <param name="quellordner">Zu kopierender Datenordner.</param>
    /// <param name="zielordner">Zielordner (muss existieren).</param>
    /// <param name="titel">Fenstertitel.</param>
    /// <exception cref="System.IO.IOException">Das Kopieren ist fehlgeschlagen (bereits kopierte Dateien sind wieder entfernt).</exception>
    /// <exception cref="UnauthorizedAccessException">Fehlendes Zugriffsrecht.</exception>
    public static void Ausfuehren(Window? besitzer, string quellordner, string zielordner, string titel)
    {
        var viewModel = new FortschrittsViewModel(titel);
        var fenster = new FortschrittsFenster(viewModel) { Owner = besitzer };
        if (besitzer is null)
        {
            fenster.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        Exception? fehler = null;

        // Das Kopieren startet, sobald das Fenster sichtbar ist; danach schließt es sich selbst.
        fenster.Loaded += async (_, _) =>
        {
            var fortschritt = new Progress<KopierFortschritt>(viewModel.Aktualisiere);
            try
            {
                await Task.Run(() => KopiereDatenbestand.Ausfuehren(quellordner, zielordner, fortschritt));
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

        // Fehler mit ursprünglichem Stacktrace weiterwerfen
        if (fehler is not null)
        {
            ExceptionDispatchInfo.Throw(fehler);
        }
    }
}
