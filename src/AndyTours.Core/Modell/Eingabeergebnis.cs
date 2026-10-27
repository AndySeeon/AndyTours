namespace AndyTours.Core.Modell;

/// <summary>
/// Ergebnis der Prüfung von Eingaben: die erzeugte Trainingseinheit oder die fehlerhaften Felder.
/// </summary>
/// <remarks>
/// Ergebnis von <see cref="Berechnungen.EinheitEingaben.AlsEinheit"/> und
/// <see cref="Berechnungen.UebernimmZellwert"/>. Bei gültigen Eingaben ist <see cref="Einheit"/> belegt
/// und <see cref="Fehler"/> leer, sonst ist <see cref="Einheit"/> <c>null</c>.
/// </remarks>
/// <param name="Einheit">Die erzeugte Einheit oder <c>null</c>.</param>
/// <param name="Fehler">Die Felder mit fehlender oder ungültiger Eingabe.</param>
public record Eingabeergebnis(Trainingseinheit? Einheit, IReadOnlySet<Eingabefeld> Fehler)
{
    /// <summary><c>true</c>, wenn alle Eingaben gültig sind.</summary>
    public bool IstGueltig => Einheit is not null;
}
