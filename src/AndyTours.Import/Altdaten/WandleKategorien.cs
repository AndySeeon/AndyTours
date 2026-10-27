using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erzeugt die Kategorien aus dem INI-Abschnitt <c>[Category]</c>.
/// </summary>
public static class WandleKategorien
{
    /// <summary>
    /// Wandelt die Zeilen <c>Name=Standard</c> (z. B. <c>Trainieren=true</c>) in Kategorien um.
    /// </summary>
    /// <param name="ini">Die gelesene <c>bikexperience.ini</c>.</param>
    /// <returns>Die Kategorien mit fortlaufenden Nummern ab 1, in der Reihenfolge der Datei.</returns>
    public static List<Kategorie> Ausfuehren(IniDatei ini)
    {
        var kategorien = new List<Kategorie>();

        foreach (var (name, wert) in HoleIniSchluesselwerte.Ausfuehren(ini, "Category"))
        {
            kategorien.Add(new Kategorie
            {
                Id = kategorien.Count + 1,
                Name = name,
                IstStandard = wert.Equals("true", StringComparison.OrdinalIgnoreCase),
            });
        }

        return kategorien;
    }
}
