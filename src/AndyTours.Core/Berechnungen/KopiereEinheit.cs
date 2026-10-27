using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Bereitet die Kopie einer Trainingseinheit für den Befehl „Kopieren“ vor.
/// </summary>
public static class KopiereEinheit
{
    /// <summary>
    /// Liefert eine neue, noch nicht gespeicherte Einheit mit den Werten des Originals.
    /// </summary>
    /// <remarks>
    /// Übernommen werden alle Werte außer der Nummer (0 – vergibt die Datenbank), dem Datum (heute) und den
    /// Anhängen: Eine Aufzeichnung gehört zu genau einer gefahrenen Tour. Das Original bleibt unverändert;
    /// die Kategorien werden in eine eigene Liste kopiert.
    /// </remarks>
    /// <param name="original">Die zu kopierende Einheit.</param>
    /// <param name="heute">Das heutige Datum.</param>
    /// <returns>Die Kopie.</returns>
    public static Trainingseinheit Ausfuehren(Trainingseinheit original, DateOnly heute) => new()
    {
        Id = 0,
        Datum = heute,
        FahrerId = original.FahrerId,
        Startzeit = original.Startzeit,
        Name = original.Name,
        Fahrzeit = original.Fahrzeit,
        DistanzKm = original.DistanzKm,
        Hoehenmeter = original.Hoehenmeter,
        MaxHoehe = original.MaxHoehe,
        StartHoehe = original.StartHoehe,
        MaxGeschwindigkeit = original.MaxGeschwindigkeit,
        HfDurchschnitt = original.HfDurchschnitt,
        HfMax = original.HfMax,
        HfMin = original.HfMin,
        Trittfrequenz = original.Trittfrequenz,
        Temperatur = original.Temperatur,
        LeistungWatt = original.LeistungWatt,
        KoerpergewichtKg = original.KoerpergewichtKg,
        Erholungspuls = original.Erholungspuls,
        Trainingsbereich = original.Trainingsbereich,
        Kommentar = original.Kommentar,
        AktivitaetId = original.AktivitaetId,
        EquipmentId = original.EquipmentId,
        KategorieIds = [.. original.KategorieIds],
        Anhaenge = [],
    };
}
