using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Ermittelt die Abbiegungen einer gefahrenen Strecke mit dem Routingdienst aus den Einstellungen (Spec 032).
/// </summary>
/// <remarks>
/// Die Spur wird auf Stützpunkte ausgedünnt (<see cref="WaehleStuetzpunkte"/>), die Pakete werden nacheinander beim
/// Dienst abgefragt (<see cref="Routingabruf"/>), und die gelieferten Abbiegungen werden auf die Spur übernommen
/// (<see cref="UebernimmAbbiegungen"/>). Fehlt auch nur ein Paket, gibt es gar keine Abbiegungen, damit der Edge nicht
/// nur auf einem Teil der Strecke ansagt; der Grund wird zurückgegeben.
/// </remarks>
public static class Abbiegungsabruf
{
    /// <summary>Grund, wenn der Dienst nicht vollständig eingestellt ist.</summary>
    public const string NichtEingestellt =
        "Die Abbiegungen konnten nicht ermittelt werden, weil der Routingdienst nicht vollständig eingestellt ist " +
        "(Serveradresse oder Zugangsschlüssel unter „Extras → Einstellungen“). Der Course enthält keine Abbiegehinweise.";

    /// <summary>Grund, wenn der Dienst nicht geantwortet hat.</summary>
    public const string KeineAntwort =
        "Die Abbiegungen konnten nicht ermittelt werden, weil der Routingdienst nicht geantwortet hat (keine " +
        "Internetverbindung?). Der Course enthält keine Abbiegehinweise.";

    /// <summary>
    /// Ermittelt die Abbiegungen.
    /// </summary>
    /// <param name="spur">Die Positionen der gefahrenen Strecke (<see cref="BildeCourse.Spur"/>).</param>
    /// <param name="einstellungen">Die Routingeinstellungen.</param>
    /// <param name="profil">Das gewählte Fahrprofil.</param>
    /// <param name="abbruch">Bricht die Abfrage ab (z. B. wenn das Fenster geschlossen wird).</param>
    /// <returns>Die Abbiegungen und, falls sie fehlen, der Grund; sonst <c>null</c> als Grund.</returns>
    /// <exception cref="OperationCanceledException">Die Abfrage wurde über <paramref name="abbruch"/> abgebrochen.</exception>
    public static async Task<(IReadOnlyList<Courseabbiegung> Abbiegungen, string? Grund)> Ermittle(
        IReadOnlyList<Geoposition> spur, Routingeinstellungen einstellungen, Fahrprofil profil, CancellationToken abbruch)
    {
        List<Teilstueck> teilstuecke = [];
        foreach (IReadOnlyList<Geoposition> paket in WaehleStuetzpunkte.Ausfuehren(spur))
        {
            if (BildeRoutinganfrage.Ausfuehren(einstellungen, profil, paket) is not Routinganfrage anfrage)
            {
                return ([], NichtEingestellt);
            }

            if (await Routingabruf.Ermittle(anfrage, abbruch).ConfigureAwait(false) is not Teilstueck teilstueck)
            {
                return ([], KeineAntwort);
            }

            teilstuecke.Add(teilstueck);
        }

        return (UebernimmAbbiegungen.Ausfuehren(spur, teilstuecke), null);
    }
}
