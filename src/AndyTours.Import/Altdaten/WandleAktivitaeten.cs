using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erzeugt die Aktivitäten aus dem INI-Abschnitt <c>[Activity]</c>.
/// </summary>
public static class WandleAktivitaeten
{
    /// <summary>
    /// Wandelt die Zeilen <c>Name=Haupt;Einheit;Ziel km;Ziel hm;Ziel Anzahl</c> in Aktivitäten um.
    /// </summary>
    /// <example><c>Bike=true;km/h;8000;80000;100</c> → Hauptaktivität „Bike", Ziel 8000 km, 80000 hm, 100 Einheiten.</example>
    /// <param name="ini">Die gelesene <c>bikexperience.ini</c>.</param>
    /// <returns>Die Aktivitäten mit fortlaufenden Nummern ab 1, in der Reihenfolge der Datei.</returns>
    public static List<Aktivitaet> Ausfuehren(IniDatei ini)
    {
        var aktivitaeten = new List<Aktivitaet>();

        foreach (var (name, wert) in HoleIniSchluesselwerte.Ausfuehren(ini, "Activity"))
        {
            string[] teile = wert.Split(';');

            // Fehlende Teile am Ende werden toleriert und mit Standardwerten belegt.
            string Teil(int index) => index < teile.Length ? teile[index].Trim() : string.Empty;

            aktivitaeten.Add(new Aktivitaet
            {
                Id = aktivitaeten.Count + 1,
                Name = name,
                IstHaupt = Teil(0).Equals("true", StringComparison.OrdinalIgnoreCase),
                Geschwindigkeitseinheit = Teil(1).Length > 0 ? Teil(1) : "km/h",
                ZielKm = ParseZahl.Ausfuehren(Teil(2)),
                ZielHm = ParseZahl.Ausfuehren(Teil(3)),
                ZielAnzahl = ParseGanzzahl.Ausfuehren(Teil(4)),
            });
        }

        return aktivitaeten;
    }
}
