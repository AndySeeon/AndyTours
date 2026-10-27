namespace AndyTours.Core.Modell;

/// <summary>
/// Eine fertige Anfrage an den Routingdienst, die die App nur noch abschicken muss.
/// </summary>
/// <param name="Dienst">Der Dienst, dessen Antwort zu lesen ist.</param>
/// <param name="Adresse">Vollständige Adresse der Anfrage.</param>
/// <param name="Inhalt">JSON-Inhalt für eine POST-Anfrage oder <c>null</c> für eine GET-Anfrage.</param>
/// <param name="Kopfzeilen">Zusätzliche Kopfzeilen, z. B. der Zugangsschlüssel.</param>
public record Routinganfrage(Routingdienst Dienst, Uri Adresse, string? Inhalt, IReadOnlyDictionary<string, string> Kopfzeilen)
{
    /// <summary>Schützt den Zugangsschlüssel in den Kopfzeilen vor der Ausgabe.</summary>
    /// <returns>Dienst und Adresse ohne Kopfzeilen.</returns>
    public override string ToString() => $"{Dienst}: {Adresse}";
}
