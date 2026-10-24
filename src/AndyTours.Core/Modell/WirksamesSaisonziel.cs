namespace AndyTours.Core.Modell;

/// <summary>
/// Das Saisonziel, das für eine Aktivität in einem Jahr gilt, mit seiner Herkunft.
/// </summary>
/// <param name="ZielKm">Ziel Distanz in Kilometern (0 = kein Ziel).</param>
/// <param name="ZielHm">Ziel Höhenmeter (0 = kein Ziel).</param>
/// <param name="ZielAnzahl">Ziel Anzahl Trainingseinheiten (0 = kein Ziel).</param>
/// <param name="IstEigenes"><c>true</c>, wenn für genau dieses Jahr ein eigenes Ziel eingetragen ist.</param>
/// <param name="AusJahr">
/// Jahr, aus dem ein früheres eigenes Ziel übernommen ist; <c>null</c> bei einem eigenen Ziel, beim
/// Grundziel aus BikeXperience und wenn es kein Ziel gibt.
/// </param>
/// <param name="AusBikeXperience"><c>true</c>, wenn das Grundziel aus BikeXperience gilt.</param>
public record WirksamesSaisonziel(int ZielKm, int ZielHm, int ZielAnzahl, bool IstEigenes, int? AusJahr, bool AusBikeXperience)
{
    /// <summary>
    /// Kennzeichnung für die Anzeige: „übernommen aus 2026“, „übernommen aus BikeXperience“ oder leer.
    /// </summary>
    public string Herkunft => AusJahr is int jahr
        ? $"übernommen aus {jahr}"
        : AusBikeXperience ? "übernommen aus BikeXperience" : string.Empty;
}
