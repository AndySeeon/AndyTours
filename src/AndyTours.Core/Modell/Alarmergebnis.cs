namespace AndyTours.Core.Modell;

/// <summary>
/// Ergebnis der Prüfung der Eingaben im Alarmfenster.
/// </summary>
/// <param name="Alarm">Der Alarm aus den Eingaben; <c>null</c>, wenn ein Feld fehlerhaft ist.</param>
/// <param name="Fehler">Die fehlerhaften Felder; leer, wenn alles stimmt.</param>
/// <param name="Hinweis">Text zum Fehler, wenn einer erklärt werden muss (z. B. ein schon vorhandener Alarm).</param>
public record Alarmergebnis(Wartung? Alarm, IReadOnlySet<Alarmfeld> Fehler, string? Hinweis)
{
    /// <summary><c>true</c>, wenn die Eingaben gültig sind.</summary>
    public bool IstGueltig => Alarm is not null;
}
