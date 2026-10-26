using System.Globalization;
using System.Text;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Erstellt den Bericht und die Zusammenfassung der Übernahme von Aufzeichnungen.
/// </summary>
/// <remarks>
/// Beides gehört zusammen: Der Bericht liegt ausführlich im Datenordner, die Zusammenfassung gibt das Programm
/// auf der Konsole aus, und das Installationsprogramm zeigt sie an.
/// </remarks>
public static class ErstelleAufzeichnungsbericht
{
    /// <summary>Dateiname des Berichts im Datenordner.</summary>
    public const string Berichtsdatei = "aufzeichnungsbericht.txt";

    /// <summary>
    /// Baut den ausführlichen Bericht: Herkunft, Mengen und jede übersprungene Datei mit Grund.
    /// </summary>
    /// <param name="ergebnis">Ergebnis der Übernahme.</param>
    /// <param name="quellordner">Ordner mit den Aufzeichnungen.</param>
    /// <param name="zeitpunkt">Zeitpunkt der Übernahme.</param>
    /// <returns>Mehrzeiliger Berichtstext.</returns>
    public static string Bericht(Aufzeichnungsergebnis ergebnis, string quellordner, DateTime zeitpunkt)
    {
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        var text = new StringBuilder();

        text.AppendLine("AndyTours – Bericht zur Übernahme der Aufzeichnungen");
        text.AppendLine(de, $"Erstellt: {zeitpunkt.ToString("dd.MM.yyyy HH:mm", de)}");
        text.AppendLine(de, $"Ordner mit den Aufzeichnungen: {quellordner}");
        text.AppendLine();
        text.AppendLine(de, $"Gefundene Aufzeichnungen: {ergebnis.Gefunden:N0}");
        text.AppendLine(de, $"Übernommene Touren:       {ergebnis.Uebernommen:N0}");
        text.AppendLine(de, $"Übersprungene Dateien:    {ergebnis.Uebersprungen.Count:N0}");
        text.AppendLine();
        text.AppendLine("Die Touren haben kein Rad und die Aktivität „Radfahren“; beides lässt sich in AndyTours ändern.");

        if (ergebnis.Uebersprungen.Count > 0)
        {
            text.AppendLine();
            text.AppendLine("Übersprungene Dateien:");
            foreach ((string datei, string grund) in ergebnis.Uebersprungen)
            {
                text.AppendLine(de, $"  {datei}: {grund}");
            }
        }

        return text.ToString();
    }

    /// <summary>
    /// Fasst Mengen und den Ort des Berichts kurz zusammen.
    /// </summary>
    /// <param name="ergebnis">Ergebnis der Übernahme.</param>
    /// <param name="zielordner">Der befüllte Datenordner.</param>
    /// <returns>Mehrzeiliger Text.</returns>
    public static string Zusammenfassung(Aufzeichnungsergebnis ergebnis, string zielordner)
    {
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        var text = new StringBuilder();

        text.AppendLine("Übernahme der Aufzeichnungen abgeschlossen.");
        text.AppendLine(de, $"  Übernommene Touren:    {ergebnis.Uebernommen:N0}");
        text.AppendLine(de, $"  Übersprungene Dateien: {ergebnis.Uebersprungen.Count:N0}");
        text.AppendLine(de, $"Bericht: {Path.Combine(zielordner, Berichtsdatei)}");

        return text.ToString();
    }
}
