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
}
