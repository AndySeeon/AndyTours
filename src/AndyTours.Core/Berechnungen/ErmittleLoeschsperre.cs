using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt, ob eine Aktivität, Kategorie oder ein Equipment gelöscht werden darf, und liefert sonst den
/// Hinweis mit dem Grund.
/// </summary>
/// <remarks>
/// Gesperrt ist das Löschen, solange Einheiten oder Equipments den Eintrag verwenden – sonst stünden
/// sie ohne Aktivität bzw. mit einer verschwundenen Standardkategorie da. Bei Aktivitäten zusätzlich
/// für die Hauptaktivität und die letzte verbleibende Aktivität, weil jede Einheit eine Aktivität braucht.
/// Ein Equipment ist gesperrt, solange Einheiten es verwenden oder Wartungen dazu erfasst sind; der Hinweis
/// empfiehlt dann das Ausmustern, das die Zuordnung erhält.
/// </remarks>
public static class ErmittleLoeschsperre
{
    /// <summary>Zahlenformat der Hinweise (Tausenderpunkt).</summary>
    private static readonly CultureInfo Deutsch = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>Prüft, ob eine Aktivität gelöscht werden darf.</summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aktivitaetId">Id der Aktivität.</param>
    /// <returns>Der Hinweis mit dem Grund, oder <c>null</c>, wenn gelöscht werden darf.</returns>
    public static string? Aktivitaet(Datenbestand bestand, long aktivitaetId)
    {
        Aktivitaet? aktivitaet = bestand.Aktivitaeten.FirstOrDefault(a => a.Id == aktivitaetId);
        if (aktivitaet is null)
        {
            return null;
        }

        if (aktivitaet.IstHaupt)
        {
            return $"‚{aktivitaet.Name}‘ ist die Hauptaktivität und kann nicht gelöscht werden. Bitte zuerst eine andere Aktivität als Hauptaktivität festlegen.";
        }

        if (bestand.Aktivitaeten.Count == 1)
        {
            return $"‚{aktivitaet.Name}‘ ist die letzte Aktivität und kann nicht gelöscht werden.";
        }

        (int einheiten, int equipments) = ZaehleVerwendung.Aktivitaet(bestand, aktivitaetId);
        var teile = new List<string>();
        if (einheiten > 0)
        {
            teile.Add(Anzahl(einheiten, "Einheit", "Einheiten"));
        }

        if (equipments > 0)
        {
            teile.Add(Anzahl(equipments, "Equipment", "Equipments"));
        }

        return teile.Count == 0
            ? null
            : $"‚{aktivitaet.Name}‘ wird von {string.Join(" und ", teile)} verwendet und kann nicht gelöscht werden.";
    }

    /// <summary>Prüft, ob eine Kategorie gelöscht werden darf.</summary>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="kategorieId">Id der Kategorie.</param>
    /// <returns>Der Hinweis mit dem Grund, oder <c>null</c>, wenn gelöscht werden darf.</returns>
    public static string? Kategorie(Datenbestand bestand, long kategorieId)
    {
        Kategorie? kategorie = bestand.Kategorien.FirstOrDefault(k => k.Id == kategorieId);
        if (kategorie is null)
        {
            return null;
        }

        (int einheiten, List<string> equipments) = ZaehleVerwendung.Kategorie(bestand, kategorieId);
        var teile = new List<string>();
        if (einheiten > 0)
        {
            teile.Add($"wird in {Anzahl(einheiten, "Einheit", "Einheiten")} verwendet");
        }

        if (equipments.Count > 0)
        {
            string namen = string.Join(", ", equipments.Select(n => $"‚{n}‘"));
            teile.Add(equipments.Count == 1
                ? $"ist beim Equipment {namen} als Standardkategorie eingetragen"
                : $"ist bei den Equipments {namen} als Standardkategorie eingetragen");
        }

        return teile.Count == 0
            ? null
            : $"‚{kategorie.Name}‘ {string.Join(" und ", teile)}; sie kann daher nicht gelöscht werden.";
    }

    /// <summary>Anzahl mit passender Einzahl oder Mehrzahl, z. B. „1 Einheit“, „1.234 Einheiten“.</summary>
    private static string Anzahl(int anzahl, string einzahl, string mehrzahl) =>
        string.Format(Deutsch, "{0:N0} {1}", anzahl, anzahl == 1 ? einzahl : mehrzahl);

    /// <summary>Prüft, ob ein Equipment gelöscht werden darf.</summary>
    /// <remarks>
    /// Beispiel: „‚Genius‘ wird in 1 Einheit verwendet und hat 1 Wartung; es kann daher nicht gelöscht
    /// werden. Bitte stattdessen als ausgemustert kennzeichnen.“ Ist es bereits ausgemustert, entfällt der
    /// letzte Satz.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="equipmentId">Id des Equipments.</param>
    /// <returns>Der Hinweis oder <c>null</c>, wenn gelöscht werden darf (auch bei unbekannter Id).</returns>
    public static string? Equipment(Datenbestand bestand, long equipmentId)
    {
        Equipment? equipment = bestand.Equipments.FirstOrDefault(e => e.Id == equipmentId);
        if (equipment is null)
        {
            return null;
        }

        (int einheiten, int wartungen) = ZaehleVerwendung.Equipment(bestand, equipmentId);
        var teile = new List<string>();
        if (einheiten > 0)
        {
            teile.Add($"wird in {Anzahl(einheiten, "Einheit", "Einheiten")} verwendet");
        }

        if (wartungen > 0)
        {
            teile.Add($"hat {Anzahl(wartungen, "Wartung", "Wartungen")}");
        }

        if (teile.Count == 0)
        {
            return null;
        }

        // Die Empfehlung entfällt, wenn das Equipment schon ausgemustert ist
        string hinweis = $"‚{equipment.Name}‘ {string.Join(" und ", teile)}; es kann daher nicht gelöscht werden.";
        return equipment.Ausgemustert ? hinweis : hinweis + " Bitte stattdessen als ausgemustert kennzeichnen.";
    }
}
