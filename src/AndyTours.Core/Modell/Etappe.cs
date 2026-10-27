namespace AndyTours.Core.Modell;

/// <summary>
/// Eine Etappe im Tourdetail: eine Einheit und der Verlauf ihrer Aufzeichnung.
/// </summary>
/// <remarks>
/// Das Tourdetail einer einzelnen Einheit hat genau eine Etappe, eine Mehrtagestour mehrere. Der Verlauf fehlt
/// (<c>null</c>), wenn die Einheit keine Aufzeichnung hat oder ihre Datei fehlt bzw. nicht gelesen werden kann;
/// die Einheit zählt dann zu den Summen der Kopfzeile, fehlt aber in Profil und Karte.
/// </remarks>
/// <param name="Einheit">Die Einheit (für Kopfzeile, Summen und Hinweise).</param>
/// <param name="Verlauf">Die gelesenen Messpunkte ihrer Aufzeichnung oder <c>null</c>.</param>
public record Etappe(Trainingseinheit Einheit, IReadOnlyList<Verlaufspunkt>? Verlauf);
