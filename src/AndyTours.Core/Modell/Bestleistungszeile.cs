namespace AndyTours.Core.Modell;

/// <summary>
/// Eine angezeigte Zeile im Bereich „Bestleistungen“ der Karteikarte „Aktuell“.
/// </summary>
/// <param name="Bezeichnung">z. B. „Kilometer“ oder „Längste Tour“.</param>
/// <param name="Wert">Der formatierte Wert, z. B. „8.123,4 km“.</param>
/// <param name="Herkunft">Rekordjahr („2019“) bzw. Datum und Name der Einheit („14.06.2021 Alpenrunde“).</param>
/// <param name="IstLaufendesJahr"><c>true</c>, wenn der Rekord aus dem laufenden Jahr stammt (wird hervorgehoben).</param>
/// <param name="EinheitId">Nummer der Einheit eines Einzelrekords (Doppelklick öffnet sie); <c>null</c> bei Jahresrekorden.</param>
public record Bestleistungszeile(string Bezeichnung, string Wert, string Herkunft, bool IstLaufendesJahr, long? EinheitId = null);
