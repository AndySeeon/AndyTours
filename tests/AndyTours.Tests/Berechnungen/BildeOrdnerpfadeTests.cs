using AndyTours.Core.Berechnungen;
using AndyTours.Core.Modell;

namespace AndyTours.Tests.Berechnungen;

/// <summary>
/// Tests für <see cref="BildeOrdnerpfade"/>.
/// </summary>
public class BildeOrdnerpfadeTests
{
    /// <summary>Alpen (1) › Tirol (2) › Ötztal (3); Chiemgau (4) und allgäu (5) auf oberster Ebene.</summary>
    private static readonly Overlayordner[] Ordner =
    [
        new(1, null, "Alpen"),
        new(2, 1, "Tirol"),
        new(3, 2, "Ötztal"),
        new(4, null, "Chiemgau"),
        new(5, null, "allgäu"),
    ];

    [Fact]
    public void Oberste_Ebene_zuerst_dann_Pfade_alphabetisch()
    {
        IReadOnlyList<Auswahleintrag> eintraege = BildeOrdnerpfade.Ausfuehren(Ordner);

        Assert.Equal(
            [
                new Auswahleintrag(null, "(oberste Ebene)"),
                new Auswahleintrag(5, "allgäu"),
                new Auswahleintrag(1, "Alpen"),
                new Auswahleintrag(2, "Alpen › Tirol"),
                new Auswahleintrag(3, "Alpen › Tirol › Ötztal"),
                new Auswahleintrag(4, "Chiemgau"),
            ],
            eintraege);
    }

    [Fact]
    public void Beim_Verschieben_eines_Ordners_fehlen_er_und_seine_Unterordner()
    {
        IReadOnlyList<Auswahleintrag> eintraege = BildeOrdnerpfade.Ausfuehren(Ordner, verschobenerOrdnerId: 2);

        Assert.Equal([null, 5, 1, 4], eintraege.Select(e => e.Id));
    }

    [Fact]
    public void Ohne_Ordner_nur_die_oberste_Ebene()
    {
        Assert.Equal([new Auswahleintrag(null, "(oberste Ebene)")], BildeOrdnerpfade.Ausfuehren([]));
    }
}
