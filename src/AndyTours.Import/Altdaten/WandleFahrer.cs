using AndyTours.Core.Modell;

namespace AndyTours.Import.Altdaten;

/// <summary>
/// Erzeugt den Sportler aus dem INI-Abschnitt <c>[Biker]</c>.
/// </summary>
public static class WandleFahrer
{
    /// <summary>
    /// Liest die persönlichen Daten des Sportlers.
    /// </summary>
    /// <remarks>
    /// Die Koordinaten des Heimatorts stehen im Original nicht bei den persönlichen Daten,
    /// sondern im Abschnitt <c>[GPSViewer]</c> (<c>homeLat</c>, <c>homeLon</c>).
    /// </remarks>
    /// <param name="ini">Die gelesene <c>bikexperience.ini</c>.</param>
    /// <returns>Der Sportler mit der Nummer 1.</returns>
    public static Fahrer Ausfuehren(IniDatei ini)
    {
        var biker = HoleIniSchluesselwerte.Ausfuehren(ini, "Biker");
        var gps = HoleIniSchluesselwerte.Ausfuehren(ini, "GPSViewer");

        // Fehlt ein Schlüssel, wird ein leerer Text verwendet (ergibt bei Zahlen 0).
        string Wert(Dictionary<string, string> abschnitt, string schluessel) =>
            abschnitt.TryGetValue(schluessel, out string? wert) ? wert : string.Empty;

        // Koordinaten sind optional; nur übernehmen, wenn wirklich ein Wert dasteht.
        double? Koordinate(string schluessel) =>
            Wert(gps, schluessel).Length > 0 ? ParseZahl.Ausfuehren(Wert(gps, schluessel)) : null;

        return new Fahrer
        {
            Id = 1,
            Vorname = Wert(biker, "firstname"),
            Nachname = Wert(biker, "lastname"),
            Geburtstag = ParseAltesDatum.Ausfuehren(Wert(biker, "birthday")),
            GroesseCm = ParseGanzzahl.Ausfuehren(Wert(biker, "height")),
            GewichtKg = ParseZahl.Ausfuehren(Wert(biker, "weight")),
            Ruhepuls = ParseGanzzahl.Ausfuehren(Wert(biker, "pulsemin")),
            Maxpuls = ParseGanzzahl.Ausfuehren(Wert(biker, "pulsemax")),
            TrainingspulsVon = ParseGanzzahl.Ausfuehren(Wert(biker, "hfMin")),
            TrainingspulsBis = ParseGanzzahl.Ausfuehren(Wert(biker, "hfMax")),
            Heimatort = Wert(biker, "homelocation"),
            HeimatBreitengrad = Koordinate("homeLat"),
            HeimatLaengengrad = Koordinate("homeLon"),
        };
    }
}
