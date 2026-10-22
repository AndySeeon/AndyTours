using System.Globalization;
using System.Text;
using AndyTours.Core.Modell;
using AndyTours.Import.Altdaten;

namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Erstellt die kurze Zusammenfassung, die das Programm nach der Umwandlung auf der Konsole ausgibt.
/// </summary>
public static class ErstelleZusammenfassung
{
    /// <summary>
    /// Fasst Mengen, Anzahl der Warnungen und den Ort des Berichts zusammen.
    /// </summary>
    /// <remarks>Die Einzelheiten stehen im Bericht; hier nur das Wichtigste auf einen Blick.</remarks>
    /// <param name="ergebnis">Ergebnis der Umwandlung.</param>
    /// <param name="zielordner">Der befüllte Zielordner.</param>
    /// <returns>Mehrzeiliger Text.</returns>
    public static string Ausfuehren(Migrationsergebnis ergebnis, string zielordner)
    {
        Datenbestand bestand = ergebnis.Bestand;
        Migrationsbericht bericht = ergebnis.Bericht;
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        var text = new StringBuilder();

        text.AppendLine("Umwandlung abgeschlossen.");
        text.AppendLine(de, $"  Trainingseinheiten: {bestand.Trainingseinheiten.Count:N0}");
        text.AppendLine(de, $"  Räder/Equipments:   {bestand.Equipments.Count:N0}");
        text.AppendLine(de, $"  Wartungen:          {bestand.Wartungen.Count:N0}");
        text.AppendLine(de, $"  Aktivitäten:        {bestand.Aktivitaeten.Count:N0}");
        text.AppendLine(de, $"  Kategorien:         {bestand.Kategorien.Count:N0}");
        text.AppendLine(de, $"  Schwellwerte:       {bestand.Schwellwerte.Count:N0}");
        text.AppendLine(de, $"  Sportler:           {bestand.Fahrer.Count:N0}");
        text.AppendLine(de, $"  Warnungen: {bericht.Warnungen.Count:N0}, nicht reparierte Umlaute: {bericht.NichtReparierteTexte.Count:N0}");
        text.AppendLine(de, $"Bericht: {Path.Combine(zielordner, ErstelleZwischenordner.Berichtsdatei)}");

        return text.ToString();
    }
}
