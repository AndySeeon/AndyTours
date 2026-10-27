using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erzeugt die Schwellwerte (Puls-, Geschwindigkeits-, Steigungs- und Leistungsbereiche) aus der INI.
/// </summary>
public static class WandleSchwellwerte
{
    /// <summary>
    /// Zuordnung der INI-Abschnitte des Originals zur jeweiligen <see cref="SchwellwertArt"/>.
    /// </summary>
    private static readonly (string Abschnitt, SchwellwertArt Art)[] Abschnitte =
    {
        ("LimitsPulseTrain", SchwellwertArt.PulsTraining),
        ("LimitsPulseStat", SchwellwertArt.PulsStatistik),
        ("LimitsSpeed", SchwellwertArt.Geschwindigkeit),
        ("LimitsGrad", SchwellwertArt.Steigung),
        ("LimitsPower", SchwellwertArt.Leistung),
    };

    /// <summary>
    /// Liest alle Bereiche aus den Zeilen <c>Name;von,bis;R,G,B</c>.
    /// </summary>
    /// <example><c>Flach;0,3;170,0,0</c> → Bereich „Flach" von 0 bis 3 % in Farbe #AA0000.</example>
    /// <param name="ini">Die gelesene <c>bikexperience.ini</c>.</param>
    /// <returns>Alle Schwellwerte mit fortlaufenden Nummern ab 1.</returns>
    /// <exception cref="FormatException">Eine Zeile hat nicht drei Teile.</exception>
    public static List<Schwellwert> Ausfuehren(IniDatei ini)
    {
        var schwellwerte = new List<Schwellwert>();

        foreach (var (abschnitt, art) in Abschnitte)
        {
            if (!ini.Abschnitte.TryGetValue(abschnitt, out List<string>? zeilen))
            {
                continue;
            }

            for (int reihenfolge = 0; reihenfolge < zeilen.Count; reihenfolge++)
            {
                // Teil 0 = Name, Teil 1 = „von,bis", Teil 2 = „R,G,B"
                string[] teile = zeilen[reihenfolge].Split(';');
                if (teile.Length != 3)
                {
                    throw new FormatException($"Ungültige Schwellwertzeile in [{abschnitt}]: „{zeilen[reihenfolge]}“");
                }

                List<int> grenzen = ParseZahlenliste.Ausfuehren(teile[1]);
                List<int> rgb = ParseZahlenliste.Ausfuehren(teile[2]);

                schwellwerte.Add(new Schwellwert
                {
                    Id = schwellwerte.Count + 1,
                    Art = art,
                    Reihenfolge = reihenfolge,
                    Name = teile[0].Trim(),
                    Von = grenzen.ElementAtOrDefault(0),
                    Bis = grenzen.ElementAtOrDefault(1),
                    // Farbe als „#RRGGBB": jeder Anteil zweistellig hexadezimal (X2)
                    Farbe = $"#{rgb.ElementAtOrDefault(0):X2}{rgb.ElementAtOrDefault(1):X2}{rgb.ElementAtOrDefault(2):X2}",
                });
            }
        }

        return schwellwerte;
    }
}
