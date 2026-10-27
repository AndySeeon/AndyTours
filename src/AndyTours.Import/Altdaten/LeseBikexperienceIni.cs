namespace AndyTours.Import.Altdaten;

/// <summary>
/// Liest die Stammdaten aus der Einstellungsdatei <c>bikexperience.ini</c> des Originals.
/// </summary>
public static class LeseBikexperienceIni
{
    /// <summary>
    /// Liest Sportler, Aktivitäten, Kategorien, Equipments und Schwellwerte.
    /// </summary>
    /// <param name="pfad">Vollständiger Pfad zur <c>bikexperience.ini</c>.</param>
    /// <returns>Die übernommenen Stammdaten.</returns>
    public static AltEinstellungen Ausfuehren(string pfad)
    {
        // Die INI enthält keine Umlaute in Bytes > 127, wird aber zur Sicherheit genauso tolerant gelesen.
        IniDatei ini = LeseIniDatei.Ausfuehren(LeseTextdateiTolerant.Ausfuehren(pfad, out _));

        // Aktivitäten zuerst, weil die Equipments darauf verweisen.
        var aktivitaeten = WandleAktivitaeten.Ausfuehren(ini);

        return new AltEinstellungen
        {
            Fahrer = WandleFahrer.Ausfuehren(ini),
            Aktivitaeten = aktivitaeten,
            Kategorien = WandleKategorien.Ausfuehren(ini),
            Equipments = WandleEquipments.Ausfuehren(ini, aktivitaeten),
            Schwellwerte = WandleSchwellwerte.Ausfuehren(ini),
        };
    }
}
