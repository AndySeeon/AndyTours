// WPF-Projekte binden System.IO nicht automatisch ein
using System.IO;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Tiling.Layers;
using AndyTours.Core.Modell;

namespace AndyTours.App.Funktionen;

/// <summary>
/// Erzeugt die unterste Ebene einer Karte – die Kartenausschnitte einer Kartenart – für Tourdetail und Tourenplanung.
/// </summary>
/// <remarks>
/// Siehe <c>docs/adr/0003-kartenbibliothek.md</c>. Die Ausschnitte werden je Kartenart in einem eigenen Unterordner des
/// Kartenspeichers (<see cref="Programmpfade.Kartenspeicher"/>) abgelegt und von dort gelesen, sobald sie einmal geladen
/// sind – auch ohne Internet. Der Zwischenspeicher verfällt nicht. OpenStreetMap liefert bis Stufe 19, OpenTopoMap bis
/// Stufe 17. Jede Anfrage trägt die Kennung der App (<see cref="Kartenabruf"/>).
/// </remarks>
public static class Kartenebenen
{
    /// <summary>
    /// Erzeugt die Kartenebene einer Kartenart.
    /// </summary>
    /// <param name="kartenart">Die gewünschte Kartenart.</param>
    /// <returns>Die neue Ebene; sie heißt wie die Kartenart.</returns>
    public static TileLayer Erzeuge(Kartenart kartenart)
    {
        var zwischenspeicher = new FileCache(Path.Combine(Programmpfade.Kartenspeicher, kartenart.ToString()), "png");
        HttpTileSource quelle = kartenart switch
        {
            Kartenart.OpenTopoMap => new HttpTileSource(
                new GlobalSphericalMercator(YAxis.OSM, 0, 17), "https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png",
                ["a", "b", "c"], name: kartenart.ToString(), persistentCache: zwischenspeicher,
                configureHttpRequestMessage: Kartenabruf.SetzeKennung),
            _ => new HttpTileSource(
                new GlobalSphericalMercator(YAxis.OSM, 0, 19), "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                name: kartenart.ToString(), persistentCache: zwischenspeicher,
                configureHttpRequestMessage: Kartenabruf.SetzeKennung),
        };

        return new TileLayer(quelle, httpClient: Kartenabruf.Verbindung) { Name = kartenart.ToString() };
    }
}
