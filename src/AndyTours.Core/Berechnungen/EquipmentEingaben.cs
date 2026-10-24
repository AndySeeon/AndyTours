using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Wandelt ein Equipment in die Eingaben des Fensters „Equipment“ um und zurück.
/// </summary>
/// <remarks>
/// Beide Richtungen stehen zusammen, weil sie dieselben Regeln teilen: Anfangskilometer 0 erscheinen als
/// leeres Feld, ein leeres Feld wird wieder als 0 gespeichert; ein fehlendes Anschaffungsdatum ist ein
/// leeres Feld.
/// </remarks>
public static class EquipmentEingaben
{
    /// <summary>
    /// Liefert die Eingaben, mit denen das Fenster für dieses Equipment gefüllt wird.
    /// </summary>
    /// <remarks>
    /// „Standard“ ist nur angehakt, wenn der Standard wirksam ist (siehe
    /// <see cref="ErmittleStandardEquipment.IstWirksam"/>); ein ausgemustertes Equipment aus dem Altbestand
    /// erscheint also nicht als Standard.
    /// </remarks>
    /// <param name="equipment">Das Equipment; für ein neues ein leeres <see cref="Equipment"/>.</param>
    /// <returns>Die Eingaben.</returns>
    public static Equipmenteingaben AlsEingaben(Equipment equipment) => new()
    {
        Name = equipment.Name,
        AktivitaetId = equipment.AktivitaetId,
        StandardKategorie = equipment.StandardKategorie.Trim(),
        Anschaffungsdatum = Eingabetext.AlsText(equipment.Anschaffungsdatum),
        Anfangskilometer = Eingabetext.AlsText(equipment.AnfangsdistanzKm > 0 ? equipment.AnfangsdistanzKm : (double?)null),
        IstStandard = ErmittleStandardEquipment.IstWirksam(equipment),
        Ausgemustert = equipment.Ausgemustert,
    };

    /// <summary>
    /// Prüft die Eingaben und erzeugt daraus ein Equipment.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Der Name darf nicht leer sein und darf kein anderes Equipment schon tragen (ohne Unterschied
    /// zwischen Groß- und Kleinschreibung, Leerzeichen am Rand zählen nicht; siehe
    /// <see cref="PruefeAuswahleintrag"/>). Das Equipment selbst zählt nicht mit.</item>
    /// <item>Das Anschaffungsdatum muss leer oder ein gültiges Datum sein.</item>
    /// <item>Die Anfangskilometer müssen leer oder eine Zahl von 0 an sein.</item>
    /// <item>Standard ist nur möglich mit Aktivität und ohne „ausgemustert“; sonst wird es abgeschaltet.</item>
    /// <item>Nummer und die Werte, die AndyTours nicht pflegt, werden aus <paramref name="basis"/> übernommen.</item>
    /// </list>
    /// Geprüft wird in dieser Reihenfolge; der Hinweis nennt den ersten Fehler.
    /// </remarks>
    /// <param name="eingaben">Die Eingaben des Fensters.</param>
    /// <param name="basis">Das bisherige Equipment bzw. ein leeres für ein neues (Nummer 0).</param>
    /// <param name="bestand">Der Datenbestand, gegen dessen Namen geprüft wird.</param>
    /// <returns>Das neue Equipment und <c>null</c>, oder <c>null</c> und der Hinweis mit dem Grund.</returns>
    public static (Equipment? Equipment, string? Hinweis) AlsEquipment(Equipmenteingaben eingaben, Equipment basis, Datenbestand bestand)
    {
        List<string> andereNamen = bestand.Equipments.Where(e => e.Id != basis.Id || basis.Id == 0).Select(e => e.Name).ToList();
        if (PruefeAuswahleintrag.Ausfuehren(andereNamen, eingaben.Name) is string grund)
        {
            return (null, grund);
        }

        if (!Eingabetext.LeseDatum(eingaben.Anschaffungsdatum, out DateOnly? datum))
        {
            return (null, "Das Anschaffungsdatum ist ungültig (Form: 24.10.2026).");
        }

        if (!Eingabetext.LeseZahl(eingaben.Anfangskilometer, out double? kilometer) || kilometer < 0)
        {
            return (null, "Die Anfangskilometer müssen eine Zahl von 0 an sein.");
        }

        var equipment = new Equipment
        {
            Id = basis.Id,
            Name = eingaben.Name.Trim(),
            AktivitaetId = eingaben.AktivitaetId,
            GewichtKg = basis.GewichtKg,
            RadumfangMm = basis.RadumfangMm,
            CrCoeff = basis.CrCoeff,
            CmCoeff = basis.CmCoeff,
            CwCoeff = basis.CwCoeff,
            CwaCoeff = basis.CwaCoeff,
            SinCoeff = basis.SinCoeff,
            Kettenblaetter = [.. basis.Kettenblaetter],
            Ritzel = [.. basis.Ritzel],
            Geraetezuordnung = basis.Geraetezuordnung,
            StandardKategorie = eingaben.StandardKategorie.Trim(),
            IstStandard = eingaben.IstStandard && eingaben.AktivitaetId is not null && !eingaben.Ausgemustert,
            Ausgemustert = eingaben.Ausgemustert,
            Anschaffungsdatum = datum,
            Bildpfad = basis.Bildpfad,
            AnfangsdistanzKm = kilometer ?? 0,
        };
        return (equipment, null);
    }
}
