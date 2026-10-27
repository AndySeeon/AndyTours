namespace AndyTours.Core.Modell;

/// <summary>
/// Ergebnis der Prüfung der Eingaben im Wartungsfenster.
/// </summary>
/// <param name="Wartung">Die Wartung aus den Eingaben; <c>null</c>, wenn ein Feld fehlerhaft ist.</param>
/// <param name="Fehler">Die fehlerhaften Felder; leer, wenn alles stimmt.</param>
/// <param name="Hinweis">Text zum Fehler, wenn einer erklärt werden muss (z. B. „Das Datum liegt in der Zukunft.“).</param>
public record Wartungsergebnis(Wartung? Wartung, IReadOnlySet<Wartungsfeld> Fehler, string? Hinweis)
{
    /// <summary><c>true</c>, wenn die Eingaben gültig sind.</summary>
    public bool IstGueltig => Wartung is not null;
}
