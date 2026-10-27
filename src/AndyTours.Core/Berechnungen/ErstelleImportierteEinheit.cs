using AndyTours.Core.Modell;

namespace AndyTours.Core.Berechnungen;

/// <summary>
/// Erzeugt beim Import aus einer Aufzeichnung die neue Trainingseinheit.
/// </summary>
public static class ErstelleImportierteEinheit
{
    /// <summary>
    /// Liefert die neue, noch nicht gespeicherte Einheit.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Datum, Startzeit, Fahrzeit, Distanz, Höhenmeter, Höhen, Höchstgeschwindigkeit, Puls, Trittfrequenz,
    /// Temperatur und Leistung kommen aus der Aufzeichnung; der Name bleibt leer.</item>
    /// <item>Equipment: das in der Importliste gewählte.</item>
    /// <item>Aktivität: die des Equipments; ohne Equipment oder ohne Aktivität beim Equipment die Hauptaktivität.</item>
    /// <item>Kategorien: alle Standardkategorien und zusätzlich die Standardkategorie des Equipments
    /// (siehe <see cref="FindeStandardkategorie"/>).</item>
    /// <item>Fahrer: der erste vorhandene, wie bei <see cref="ErstelleNeueEinheit"/>.</item>
    /// <item>Anhänge: Höhenprofil und GPS-Track verweisen beide auf die abgelegte Aufzeichnung – so wie bei den
    /// aus BikeXperience übernommenen Einheiten.</item>
    /// </list>
    /// Die Nummer ist 0; die Datenbank vergibt sie beim Speichern.
    /// </remarks>
    /// <param name="bestand">Der Datenbestand.</param>
    /// <param name="aufzeichnung">Die gelesene Aufzeichnung.</param>
    /// <param name="equipmentId">Das gewählte Equipment oder <c>null</c> für keines.</param>
    /// <param name="relativerPfad">Pfad der abgelegten Aufzeichnung relativ zum Datenordner, z. B. „2026/2026-09-26-08-02-49.fit“.</param>
    /// <returns>Die vorbereitete Einheit.</returns>
    public static Trainingseinheit Ausfuehren(Datenbestand bestand, Aufzeichnung aufzeichnung, long? equipmentId, string relativerPfad)
    {
        Equipment? equipment = bestand.Equipments.FirstOrDefault(e => e.Id == equipmentId);
        long aktivitaet = equipment?.AktivitaetId ?? bestand.Aktivitaeten.FirstOrDefault(a => a.IstHaupt)?.Id ?? 0;

        List<long> kategorien = bestand.Kategorien.Where(k => k.IstStandard).Select(k => k.Id).ToList();
        if (FindeStandardkategorie.Ausfuehren(bestand, equipment?.Id) is long kategorieId && !kategorien.Contains(kategorieId))
        {
            kategorien.Add(kategorieId);
        }

        return new Trainingseinheit
        {
            FahrerId = bestand.Fahrer.FirstOrDefault()?.Id ?? 0,
            Datum = aufzeichnung.Datum,
            Startzeit = aufzeichnung.Startzeit,
            Fahrzeit = aufzeichnung.Fahrzeit,
            DistanzKm = aufzeichnung.DistanzKm,
            Hoehenmeter = aufzeichnung.Hoehenmeter,
            MaxHoehe = aufzeichnung.MaxHoehe,
            StartHoehe = aufzeichnung.StartHoehe,
            MaxGeschwindigkeit = aufzeichnung.MaxGeschwindigkeit,
            HfDurchschnitt = aufzeichnung.HfDurchschnitt,
            HfMax = aufzeichnung.HfMax,
            HfMin = aufzeichnung.HfMin,
            Trittfrequenz = aufzeichnung.Trittfrequenz,
            Temperatur = aufzeichnung.Temperatur,
            LeistungWatt = aufzeichnung.LeistungWatt,
            AktivitaetId = aktivitaet,
            EquipmentId = equipment?.Id,
            KategorieIds = kategorien,
            Anhaenge =
            [
                new Anhang { Typ = AnhangTyp.Hoehenprofil, RelativerPfad = relativerPfad },
                new Anhang { Typ = AnhangTyp.GpsTrack, RelativerPfad = relativerPfad },
            ],
        };
    }
}
