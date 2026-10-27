using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Kleiner Datenbestand für die Tests der Aktivitäten und Kategorien.
/// </summary>
/// <remarks>
/// Aktivitäten: Bike (1, Hauptaktivität), Laufen (2), Gravel (3, unbenutzt).
/// Kategorien: Urlaub (1), Trainieren (2, Standard), Training (3), Leer (4, unbenutzt).
/// Einheiten: 10 Bike/Trainieren, 11 Bike/Trainieren+Training, 12 Laufen/Training, 13 Bike/Urlaub.
/// Equipments: Genius (Bike, Standardkategorie „trainieren “), Schuhe (Laufen), Cube (ohne Aktivität).
/// <para>
/// <see cref="MitEquipments"/> ergänzt für die Tests der Equipments: Altrad (4, Bike, Standard, aber
/// ausgemustert – wie „Namur“ im Altbestand), Rennrad (5, Bike, Standard), Spikes (6, Laufen, ausgemustert),
/// Neu (7, Laufen, unbenutzt). Einheit 10 fährt Genius, 11 Altrad, 12 Schuhe. Wartungen: zwei für Rennrad,
/// eine für Genius.
/// </para>
/// </remarks>
internal static class Auswahlbestand
{
    /// <summary>Erzeugt den Bestand jedes Mal neu, damit Tests ihn ändern dürfen.</summary>
    public static Datenbestand Erzeugen() => new()
    {
        Aktivitaeten =
        [
            new() { Id = 1, Name = "Bike", IstHaupt = true },
            new() { Id = 2, Name = "Laufen" },
            new() { Id = 3, Name = "Gravel" },
        ],
        Kategorien =
        [
            new() { Id = 1, Name = "Urlaub" },
            new() { Id = 2, Name = "Trainieren", IstStandard = true },
            new() { Id = 3, Name = "Training" },
            new() { Id = 4, Name = "Leer" },
        ],
        Trainingseinheiten =
        [
            new() { Id = 10, AktivitaetId = 1, KategorieIds = [2] },
            new() { Id = 11, AktivitaetId = 1, KategorieIds = [2, 3] },
            new() { Id = 12, AktivitaetId = 2, KategorieIds = [3] },
            new() { Id = 13, AktivitaetId = 1, KategorieIds = [1] },
        ],
        Equipments =
        [
            new() { Id = 1, Name = "Genius", AktivitaetId = 1, StandardKategorie = "trainieren " },
            new() { Id = 2, Name = "Schuhe", AktivitaetId = 2, StandardKategorie = "" },
            new() { Id = 3, Name = "Cube", AktivitaetId = null, StandardKategorie = "Urlaub" },
        ],
    };

    /// <summary>Erzeugt den Bestand mit weiteren Equipments, Equipment-Zuordnungen und Wartungen.</summary>
    public static Datenbestand MitEquipments()
    {
        Datenbestand bestand = Erzeugen();
        bestand.Equipments.AddRange(
        [
            new() { Id = 4, Name = "Altrad", AktivitaetId = 1, IstStandard = true, Ausgemustert = true },
            new() { Id = 5, Name = "Rennrad", AktivitaetId = 1, IstStandard = true },
            new() { Id = 6, Name = "Spikes", AktivitaetId = 2, Ausgemustert = true },
            new() { Id = 7, Name = "Neu", AktivitaetId = 2 },
        ]);
        bestand.Trainingseinheiten[0].EquipmentId = 1;
        bestand.Trainingseinheiten[1].EquipmentId = 4;
        bestand.Trainingseinheiten[2].EquipmentId = 2;
        bestand.Wartungen =
        [
            new() { Id = 1, EquipmentId = 5, Taetigkeit = "Kette" },
            new() { Id = 2, EquipmentId = 5, Taetigkeit = "Reifen" },
            new() { Id = 3, EquipmentId = 1, Taetigkeit = "Bremse" },
        ];
        return bestand;
    }
}
