using Microsoft.Data.Sqlite;
using AndyTours.Core.Modell;
using AndyTours.Daten;
using AndyTours.Tests.Datenordner;

namespace AndyTours.Tests.Daten;

/// <summary>
/// Tests für <see cref="OverlayordnerTabelle"/>, <see cref="OverlaysTabelle"/> und <see cref="OverlayPunkte"/>.
/// </summary>
public class OverlaysTests
{
    private static readonly IReadOnlyList<IReadOnlyList<Geoposition>> Abschnitte =
    [
        [new Geoposition(47.123456789, 11.987654321), new Geoposition(-33.5, -70.25)],
        [new Geoposition(48, 12)],
    ];

    private static Overlay Neu(long? ordnerId, string name = "Chiemsee-Runde", bool eingeschaltet = false) =>
        new(0, ordnerId, name, null, eingeschaltet, Abschnitte);

    [Fact]
    public void Punkte_bleiben_beim_Verpacken_und_Entpacken_genau_erhalten()
    {
        IReadOnlyList<IReadOnlyList<Geoposition>> wieder = OverlayPunkte.Entpacken(OverlayPunkte.Verpacken(Abschnitte));

        Assert.Equal(Abschnitte.Count, wieder.Count);
        Assert.Equal(Abschnitte[0], wieder[0]);
        Assert.Equal(Abschnitte[1], wieder[1]);
    }

    [Theory]
    [InlineData(new byte[] { })]
    [InlineData(new byte[] { 2, 0, 0, 0, 0 })]
    [InlineData(new byte[] { 1, 1, 0, 0, 0, 5, 0, 0, 0 })]
    public void Unlesbare_Punkte_ergeben_InvalidDataException(byte[] daten)
    {
        Assert.Throws<InvalidDataException>(() => OverlayPunkte.Entpacken(daten));
    }

    [Fact]
    public void Overlay_bleibt_mit_Punkten_Zustand_und_Ordner_nach_erneutem_Oeffnen_erhalten()
    {
        using var temp = new TempOrdner();
        string datei = Path.Combine(temp.Pfad, "andytours.db");
        long ordner;
        long id;
        using (SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(datei))
        {
            ordner = OverlayordnerTabelle.Einfuegen(verbindung, null, " Chiemgau ");
            id = OverlaysTabelle.Einfuegen(verbindung, Neu(ordner, eingeschaltet: true));
        }

        using SqliteConnection wieder = OeffneDatenbank.Ausfuehren(datei);
        Assert.Equal([new Overlayordner(ordner, null, "Chiemgau")], OverlayordnerTabelle.Laden(wieder));
        Overlay overlay = Assert.Single(OverlaysTabelle.Laden(wieder));
        Assert.Equal((id, (long?)ordner, "Chiemsee-Runde", (Overlayfarbe?)null, true), (overlay.Id, overlay.OrdnerId, overlay.Name, overlay.Farbe, overlay.IstEingeschaltet));
        Assert.Equal(Abschnitte[0], overlay.Abschnitte[0]);
        Assert.Equal(Abschnitte[1], overlay.Abschnitte[1]);
    }

    [Fact]
    public void Ordner_anlegen_im_Ordner_und_auf_oberster_Ebene()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");

        long alpen = OverlayordnerTabelle.Einfuegen(verbindung, null, "Alpen");
        long tirol = OverlayordnerTabelle.Einfuegen(verbindung, alpen, "Tirol");

        Assert.Equal([new Overlayordner(alpen, null, "Alpen"), new Overlayordner(tirol, alpen, "Tirol")], OverlayordnerTabelle.Laden(verbindung));
    }

    [Fact]
    public void Ordner_umbenennen_und_verschieben()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long alpen = OverlayordnerTabelle.Einfuegen(verbindung, null, "Alpen");
        long tirol = OverlayordnerTabelle.Einfuegen(verbindung, alpen, "Tirol");

        OverlayordnerTabelle.Umbenennen(verbindung, tirol, "Nordtirol ");
        OverlayordnerTabelle.Verschieben(verbindung, tirol, null);

        Assert.Contains(new Overlayordner(tirol, null, "Nordtirol"), OverlayordnerTabelle.Laden(verbindung));
    }

    [Fact]
    public void Ordner_loeschen_loescht_Unterordner_und_alle_Overlays_darin()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long alpen = OverlayordnerTabelle.Einfuegen(verbindung, null, "Alpen");
        long tirol = OverlayordnerTabelle.Einfuegen(verbindung, alpen, "Tirol");
        long oetztal = OverlayordnerTabelle.Einfuegen(verbindung, tirol, "Ötztal");
        long chiemgau = OverlayordnerTabelle.Einfuegen(verbindung, null, "Chiemgau");
        OverlaysTabelle.Einfuegen(verbindung, Neu(alpen));
        OverlaysTabelle.Einfuegen(verbindung, Neu(oetztal));
        long bleibt = OverlaysTabelle.Einfuegen(verbindung, Neu(chiemgau));
        long obenBleibt = OverlaysTabelle.Einfuegen(verbindung, Neu(null));

        OverlayordnerTabelle.Loeschen(verbindung, alpen);

        Assert.Equal([chiemgau], OverlayordnerTabelle.Laden(verbindung).Select(o => o.Id));
        Assert.Equal([bleibt, obenBleibt], OverlaysTabelle.Laden(verbindung).Select(o => o.Id));
    }

    [Fact]
    public void Overlay_umbenennen_verschieben_und_loeschen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long ordner = OverlayordnerTabelle.Einfuegen(verbindung, null, "Chiemgau");
        long id = OverlaysTabelle.Einfuegen(verbindung, Neu(null));
        long anderes = OverlaysTabelle.Einfuegen(verbindung, Neu(null, "Anderes"));

        OverlaysTabelle.Umbenennen(verbindung, id, " Kampenwand ");
        OverlaysTabelle.Verschieben(verbindung, id, ordner);
        Overlay geaendert = OverlaysTabelle.Laden(verbindung).First(o => o.Id == id);
        Assert.Equal(("Kampenwand", (long?)ordner), (geaendert.Name, geaendert.OrdnerId));

        OverlaysTabelle.Loeschen(verbindung, id);
        Assert.Equal([anderes], OverlaysTabelle.Laden(verbindung).Select(o => o.Id));
    }

    [Fact]
    public void Eigene_Farbe_setzen_und_zuruecksetzen()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long id = OverlaysTabelle.Einfuegen(verbindung, Neu(null));

        OverlaysTabelle.SetzeFarbe(verbindung, id, Overlayfarbe.Orange);
        Assert.Equal(Overlayfarbe.Orange, Assert.Single(OverlaysTabelle.Laden(verbindung)).Farbe);

        OverlaysTabelle.SetzeFarbe(verbindung, id, null);
        Assert.Null(Assert.Single(OverlaysTabelle.Laden(verbindung)).Farbe);
    }

    [Fact]
    public void Mehrere_Overlays_auf_einmal_ein_und_ausschalten()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long a = OverlaysTabelle.Einfuegen(verbindung, Neu(null, "A"));
        long b = OverlaysTabelle.Einfuegen(verbindung, Neu(null, "B"));
        long c = OverlaysTabelle.Einfuegen(verbindung, Neu(null, "C"));

        OverlaysTabelle.SetzeEingeschaltet(verbindung, [a, c], true);
        Assert.Equal([true, false, true], OverlaysTabelle.Laden(verbindung).Select(o => o.IstEingeschaltet));

        OverlaysTabelle.SetzeEingeschaltet(verbindung, [a, b], false);
        Assert.Equal([false, false, true], OverlaysTabelle.Laden(verbindung).Select(o => o.IstEingeschaltet));
    }

    [Fact]
    public void Unbekannte_Farbe_gilt_als_keine_eigene_Farbe()
    {
        using SqliteConnection verbindung = OeffneDatenbank.Ausfuehren(":memory:");
        long id = OverlaysTabelle.Einfuegen(verbindung, Neu(null));
        using (SqliteCommand befehl = verbindung.CreateCommand())
        {
            befehl.CommandText = $"UPDATE Overlay SET Farbe = 'Lila' WHERE Id = {id};";
            befehl.ExecuteNonQuery();
        }

        Assert.Null(Assert.Single(OverlaysTabelle.Laden(verbindung)).Farbe);
    }
}
