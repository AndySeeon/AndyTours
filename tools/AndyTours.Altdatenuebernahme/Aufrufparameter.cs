namespace AndyTours.Altdatenuebernahme;

/// <summary>
/// Die beim Aufruf des Programms festgelegten Angaben (siehe <see cref="LeseAufrufparameter"/>).
/// </summary>
/// <param name="Quellordner">
/// Vollständiger Pfad des Ordners mit den bisherigen Daten (wird nur gelesen): der BikeXperience-Datenordner,
/// der zu prüfende Ordner bei <see cref="Programmmodus.Erkennen"/> oder der Ordner mit den Aufzeichnungen.
/// </param>
/// <param name="Zielordner">
/// Vollständiger Pfad des AndyTours-Datenordners, der befüllt wird; leer bei <see cref="Programmmodus.Erkennen"/>.
/// </param>
/// <param name="Modus">Was das Programm tut.</param>
/// <param name="OhneRueckfrage">
/// <c>true</c> = keine Eingabe auf der Konsole: Liegen im Zielordner schon AndyTours-Daten, bleiben sie
/// unverändert, und das Programm endet mit <see cref="Rueckgabecode.ZielHatDaten"/> (für das Installationsprogramm).
/// </param>
public record Aufrufparameter(
    string Quellordner,
    string Zielordner,
    Programmmodus Modus = Programmmodus.Umwandeln,
    bool OhneRueckfrage = false);
