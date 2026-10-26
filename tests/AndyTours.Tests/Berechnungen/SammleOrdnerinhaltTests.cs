using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="SammleOrdnerinhalt"/>.
/// </summary>
public class SammleOrdnerinhaltTests
{
    /// <summary>Alpen (1) › Tirol (2) › Ötztal (3); Chiemgau (4) auf oberster Ebene.</summary>
    private static readonly Overlayordner[] Ordner =
    [
        new(1, null, "Alpen"),
        new(2, 1, "Tirol"),
        new(3, 2, "Ötztal"),
        new(4, null, "Chiemgau"),
    ];

    /// <summary>Je ein Overlay in Alpen, Ötztal, Chiemgau und auf oberster Ebene.</summary>
    private static readonly Overlay[] Overlays =
    [
        Overlay(10, 1),
        Overlay(11, 3),
        Overlay(12, 4),
        Overlay(13, null),
    ];

    private static Overlay Overlay(long id, long? ordnerId) =>
        new(id, ordnerId, $"Overlay {id}", null, false, [[new Geoposition(48, 11)]]);

    [Fact]
    public void Liefert_alle_Unterordner_und_Overlays_auf_allen_Ebenen()
    {
        (IReadOnlySet<long> unterordner, IReadOnlyList<Overlay> overlays) = SammleOrdnerinhalt.Ausfuehren(1, Ordner, Overlays);

        Assert.Equal([2, 3], unterordner.Order());
        Assert.Equal([10, 11], overlays.Select(o => o.Id));
    }

    [Fact]
    public void Leerer_Ordner_ohne_Unterordner()
    {
        Overlayordner[] ordner = [.. Ordner, new(5, null, "Leer")];

        (IReadOnlySet<long> unterordner, IReadOnlyList<Overlay> overlays) = SammleOrdnerinhalt.Ausfuehren(5, ordner, Overlays);

        Assert.Empty(unterordner);
        Assert.Empty(overlays);
    }

    [Fact]
    public void Nur_der_eigene_Teilbaum()
    {
        (IReadOnlySet<long> unterordner, IReadOnlyList<Overlay> overlays) = SammleOrdnerinhalt.Ausfuehren(2, Ordner, Overlays);

        Assert.Equal([3], unterordner);
        Assert.Equal([11], overlays.Select(o => o.Id));
    }
}
