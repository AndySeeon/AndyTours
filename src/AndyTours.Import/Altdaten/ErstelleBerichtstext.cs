using System.Globalization;
using System.Text;
using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erstellt aus einem <see cref="Migrationsergebnis"/> einen lesbaren Bericht als Text.
/// </summary>
public static class ErstelleBerichtstext
{
    /// <summary>
    /// Baut den Bericht auf: Mengengerüst, Jahressummen zum Abgleich mit dem Original,
    /// Hinweise, Warnungen und nicht reparierte Umlaute.
    /// </summary>
    /// <param name="ergebnis">Ergebnis der Übernahme.</param>
    /// <returns>Mehrzeiliger Berichtstext.</returns>
    public static string Ausfuehren(Migrationsergebnis ergebnis)
    {
        Datenbestand bestand = ergebnis.Bestand;
        Migrationsbericht bericht = ergebnis.Bericht;

        // Deutsche Zahlendarstellung (1.234,56) für den Bericht
        CultureInfo de = CultureInfo.GetCultureInfo("de-DE");
        var text = new StringBuilder();

        text.AppendLine("AndyTours – Bericht zur Übernahme der BikeXperience-Altdaten");
        text.AppendLine($"Erstellt: {DateTime.Now.ToString("dd.MM.yyyy HH:mm", de)}");
        text.AppendLine();

        text.AppendLine("Übernommen:");
        text.AppendLine($"  Trainingseinheiten: {bestand.Trainingseinheiten.Count}");
        text.AppendLine($"  Equipments:         {bestand.Equipments.Count}");
        text.AppendLine($"  Wartungen:          {bestand.Wartungen.Count}");
        text.AppendLine($"  Aktivitäten:        {bestand.Aktivitaeten.Count}");
        text.AppendLine($"  Kategorien:         {bestand.Kategorien.Count}");
        text.AppendLine($"  Schwellwerte:       {bestand.Schwellwerte.Count}");
        text.AppendLine($"  Sportler:           {bestand.Fahrer.Count}");
        text.AppendLine();

        // Jahressummen zum Vergleich mit der Statistik-Karteikarte des Originals
        text.AppendLine("Jahressummen (zum Abgleich mit BikeXperience):");
        text.AppendLine("  Jahr  Anzahl        km        hm   Fahrzeit");
        foreach (Jahressumme summe in BerechneJahressummen.Ausfuehren(bestand.Trainingseinheiten))
        {
            // Fahrzeit als Gesamtstunden:Minuten, da Jahreswerte weit über 24 Stunden liegen
            string fahrzeit = $"{(int)summe.Fahrzeit.TotalHours}:{summe.Fahrzeit.Minutes:00}";
            text.AppendLine(string.Format(de, "  {0}  {1,6}  {2,10:N2}  {3,8:N0}  {4,9}",
                summe.Jahr, summe.Anzahl, summe.DistanzKm, summe.Hoehenmeter, fahrzeit));
        }

        text.AppendLine();

        text.AppendLine("Umlaute:");
        text.AppendLine($"  {bericht.AnzahlLatin1Zeichen} Zeichen exakt wiederhergestellt (Latin-1-Kodierung erkannt)");
        text.AppendLine($"  {bericht.AnzahlReparierteTexte} Texte über die Korrekturliste repariert");
        text.AppendLine($"  {bericht.NichtReparierteTexte.Count} Texte mit nicht reparierbaren Zeichen (siehe unten)");
        text.AppendLine();

        AbschnittAnhaengen(text, "Hinweise", bericht.Hinweise);
        AbschnittAnhaengen(text, "Warnungen", bericht.Warnungen);
        AbschnittAnhaengen(text, "Nicht reparierte Umlaute (bitte manuell korrigieren)", bericht.NichtReparierteTexte);

        return text.ToString();
    }

    /// <summary>Hängt eine Überschrift und die Einträge als Aufzählung an; leere Listen erscheinen als „keine".</summary>
    private static void AbschnittAnhaengen(StringBuilder text, string ueberschrift, List<string> eintraege)
    {
        text.AppendLine($"{ueberschrift} ({eintraege.Count}):");
        if (eintraege.Count == 0)
        {
            text.AppendLine("  keine");
        }

        foreach (string eintrag in eintraege)
        {
            text.AppendLine($"  - {eintrag}");
        }

        text.AppendLine();
    }
}
