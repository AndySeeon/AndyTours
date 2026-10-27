namespace AndyTours.Core.Modell;

/// <summary>
/// Ein Punkt im Diagramm des Jahresfortschritts: die Jahressumme eines Werts und das Ziel dieses Jahres.
/// </summary>
/// <param name="Jahr">Das Jahr.</param>
/// <param name="Summe">Jahressumme in der Einheit des Werts (km, Anzahl, hm oder Stunden Fahrzeit).</param>
/// <param name="Ziel">Saisonziel in derselben Einheit; <c>null</c>, wenn es keines gibt.</param>
public record Jahrespunkt(int Jahr, double Summe, double? Ziel);
