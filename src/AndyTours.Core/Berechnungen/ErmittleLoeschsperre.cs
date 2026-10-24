using System.Globalization;
using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Ermittelt, ob eine Aktivität oder Kategorie gelöscht werden darf, und liefert sonst den Hinweis
/// mit dem Grund.
/// </summary>
/// <remarks>
/// Gesperrt ist das Löschen, solange Einheiten oder Equipments den Eintrag verwenden – sonst stünden
/// sie ohne Aktivität bzw. mit einer verschwundenen Standardkategorie da. Bei Aktivitäten zusätzlich
/// für die Hauptaktivität und die letzte verbleibende Aktivität, weil jede Einheit eine Aktivität braucht.
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
}
