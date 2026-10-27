; Installations- und Deinstallationsprogramm von AndyTours (Spezifikation 033, ADR 0005).
;
; Gebaut wird es mit Erstelle-Installation.ps1; das Skript veröffentlicht App und Umwandlungsprogramm und übergibt
; Version und Quellordner an den Inno-Setup-Compiler (/DAppVersion, /DAppQuelle, /DWerkzeugQuelle).
;
; Ablauf der Installation:
;   1. Läuft AndyTours, wird gebeten, es zu beenden.
;   2. Seiten: Datenordner → Übernahme der bisherigen Daten → Verknüpfungen → Zusammenfassung.
;      Bei einer Aktualisierung (Eintrag unter „Apps“ vorhanden) entfallen Datenordner und Übernahme.
;   3. Programmdateien nach %LOCALAPPDATA%\Programs\AndyTours, Verknüpfungen, Eintrag unter „Apps“.
;   4. Übernahme über das Umwandlungsprogramm, Merkdatei mit dem Datenordner schreiben.
;   5. Jede Änderung kommt in das lesbare Protokoll installationsprotokoll.txt im Programmordner.
; Die Deinstallation entfernt alles, was im Protokoll steht, und fragt, ob der Datenordner gelöscht werden soll.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef AppQuelle
  #define AppQuelle "bin\App"
#endif
#ifndef WerkzeugQuelle
  #define WerkzeugQuelle "bin\Werkzeug"
#endif

#define AppExe "AndyTours.App.exe"
#define Werkzeug "AndyTours.Altdatenuebernahme.exe"

[Setup]
; Fester Schlüssel: Daran erkennt eine spätere Fassung die vorhandene Installation (Eintrag „AndyTours_is1“)
AppId=AndyTours
AppName=AndyTours
AppVersion={#AppVersion}
AppVerName=AndyTours {#AppVersion}
AppPublisher=Andreas Hensel
VersionInfoVersion={#AppVersion}
; Nur für den aktuellen Benutzer, ohne Administratorrechte; {autopf} ist dann %LOCALAPPDATA%\Programs
PrivilegesRequired=lowest
DefaultDirName={autopf}\AndyTours
DisableDirPage=yes
DefaultGroupName=AndyTours
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputBaseFilename=AndyTours-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
; Programmicon für die Setup-Datei und das Fenster des Installationsprogramms (Spezifikation 034);
; Verknüpfungen und der Eintrag unter „Apps“ nehmen das Icon aus der Programmdatei
SetupIconFile=..\..\src\AndyTours.App\Ressourcen\AndyTours.ico
UninstallDisplayName=AndyTours
UninstallDisplayIcon={app}\{#AppExe}
; Eine laufende App erkennt der [Code]-Teil selbst und bittet, sie zu beenden
CloseApplications=no
RestartApplications=no
UsePreviousTasks=yes
ShowLanguageDialog=no

[Languages]
Name: "de"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktop"; Description: "Verknüpfung auf dem Desktop anlegen"; GroupDescription: "Verknüpfungen:"; Flags: unchecked

[Files]
; Die App mit eingebautem .NET; Debug-Informationen und XML-Dokumentation werden nicht gebraucht
Source: "{#AppQuelle}\*"; DestDir: "{app}"; Excludes: "*.pdb,AndyTours.*.xml"; Flags: recursesubdirs ignoreversion; BeforeInstall: VorDatei; AfterInstall: NachDatei
Source: "{#WerkzeugQuelle}\{#Werkzeug}"; DestDir: "{app}"; Flags: ignoreversion; BeforeInstall: VorDatei; AfterInstall: NachDatei
; Dieselbe Datei noch einmal zum Entpacken während der Seitenabfrage (--erkennen); sie wird nur einmal gespeichert
Source: "{#WerkzeugQuelle}\{#Werkzeug}"; Flags: dontcopy

[Icons]
Name: "{autoprograms}\AndyTours"; Filename: "{app}\{#AppExe}"; AfterInstall: NachVerknuepfung('{autoprograms}\AndyTours.lnk')
Name: "{autodesktop}\AndyTours"; Filename: "{app}\{#AppExe}"; Tasks: desktop; AfterInstall: NachVerknuepfung('{autodesktop}\AndyTours.lnk')

[Run]
Filename: "{app}\{#AppExe}"; Description: "AndyTours starten"; Flags: nowait postinstall skipifsilent

[Code]
const
  // Eintrag unter „Apps“; Inno Setup hängt „_is1“ an die AppId an
  Uninstallschluessel = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\AndyTours_is1';
  Protokollname = 'installationsprotokoll.txt';
  Datenbankdatei = 'andytours.db';
  Prozessname = 'AndyTours.App.exe';

  // Rückgabecodes des Umwandlungsprogramms (tools/AndyTours.Altdatenuebernahme/Rueckgabecode.cs)
  RcErfolg = 0;
  RcZielHatDaten = 2;
  RcBikeXperience = 10;
  RcNurAufzeichnungen = 11;
  RcNichts = 12;

var
  // Ist AndyTours schon installiert, ist dies eine Aktualisierung: Datenordner und Daten bleiben unangetastet
  IstAktualisierung: Boolean;
  DatenordnerSeite: TInputDirWizardPage;
  UebernahmeSeite: TWizardPage;
  UebernahmeHaken: TNewCheckBox;
  UebernahmeOrdner: TNewEdit;
  UebernahmeDurchsuchen: TNewButton;
  // Im Datenordner liegen schon AndyTours-Daten oder andere Dateien – dann entfällt die Übernahme
  DatenordnerHatInhalt: Boolean;
  // Was übernommen wird: 0 = nichts, RcBikeXperience oder RcNurAufzeichnungen
  Uebernahmeart: Integer;
  AnzahlAufzeichnungen: Integer;
  WerkzeugEntpackt: Boolean;
  // Gesammelte Zeilen des Protokolls; sie werden am Ende der Installation angehängt
  Protokollzeilen: TArrayOfString;
  DateiWarVorhanden, OrdnerWarVorhanden: Boolean;
  // Deinstallation: Protokoll, Datenordner und die Antwort auf die Rückfrage
  AlteProtokollzeilen: TArrayOfString;
  DatenordnerBeimDeinstallieren: String;
  DatenordnerLoeschen: Boolean;

// ---------------------------------------------------------------------------------------------------------------
// Hilfsfunktionen
// ---------------------------------------------------------------------------------------------------------------

// Pfad der Merkdatei, aus der die App beim Start den Datenordner liest (DatenordnerMerkdatei in AndyTours.Daten)
function Merkdatei: String;
begin
  Result := ExpandConstant('{userappdata}\AndyTours\datenordner.txt');
end;

// Ordner mit den Einstellungen der App außerhalb des Datenordners: Merkdatei und Kartenspeicher
function Einstellungsordner(Index: Integer): String;
begin
  if Index = 0 then
    Result := ExpandConstant('{userappdata}\AndyTours')
  else
    Result := ExpandConstant('{localappdata}\AndyTours');
end;

// true, wenn Ordner gleich Basis ist oder darunter liegt (ohne Rücksicht auf Groß-/Kleinschreibung)
function IstGleichOderDarunter(Ordner, Basis: String): Boolean;
begin
  Result := Pos(Lowercase(AddBackslash(Basis)), Lowercase(AddBackslash(Ordner))) = 1;
end;

// true, wenn einer der beiden Ordner im anderen liegt
function Ueberlappen(Ordner1, Ordner2: String): Boolean;
begin
  Result := IstGleichOderDarunter(Ordner1, Ordner2) or IstGleichOderDarunter(Ordner2, Ordner1);
end;

function OrdnerIstLeer(Ordner: String): Boolean;
var
  Fund: TFindRec;
begin
  Result := True;
  if FindFirst(AddBackslash(Ordner) + '*', Fund) then
  begin
    try
      repeat
        if (Fund.Name <> '.') and (Fund.Name <> '..') then
          Result := False;
      until (not Result) or (not FindNext(Fund));
    finally
      FindClose(Fund);
    end;
  end;
end;

// Pfad für die Befehlszeile: ein abschließender „\“ würde das schließende Anführungszeichen maskieren
function AlsArgument(Pfad: String): String;
begin
  if (Length(Pfad) > 0) and (Pfad[Length(Pfad)] = '\') then
    Pfad := Pfad + '.';
  Result := '"' + Pfad + '"';
end;

// Liest eine UTF-8-Textdatei (mit oder ohne Byte-Order-Mark); fehlt sie, ist das Ergebnis leer
function LiesUtf8(Datei: String): String;
var
  Roh: AnsiString;
begin
  Result := '';
  if LoadStringFromFile(Datei, Roh) then
  begin
    // Byte-Order-Mark entfernen
    if Copy(Roh, 1, 3) = #$EF#$BB#$BF then
      Delete(Roh, 1, 3);
    Result := UTF8Decode(Roh);
  end;
end;

// Zerlegt einen Text in Zeilen (CR LF oder LF)
function InZeilen(Text: String): TArrayOfString;
var
  Ende, Anzahl: Integer;
  Zeile: String;
begin
  Anzahl := 0;
  StringChangeEx(Text, #13#10, #10, True);
  while Length(Text) > 0 do
  begin
    Ende := Pos(#10, Text);
    if Ende = 0 then
      Ende := Length(Text) + 1;
    Zeile := Copy(Text, 1, Ende - 1);
    Delete(Text, 1, Ende);
    SetArrayLength(Result, Anzahl + 1);
    Result[Anzahl] := Zeile;
    Anzahl := Anzahl + 1;
  end;
end;

// Startet das Umwandlungsprogramm unsichtbar und liefert Rückgabecode und Ausgabe (Konsole, UTF-8)
function StarteWerkzeug(Programm, Parameter: String; var Ausgabe: String): Integer;
var
  Ausgabedatei: String;
  Rc: Integer;
begin
  Ausgabedatei := ExpandConstant('{tmp}\werkzeug-ausgabe.txt');
  DeleteFile(Ausgabedatei);
  // cmd leitet Ausgabe und Fehlerausgabe in eine Datei um; /S entfernt nur die äußeren Anführungszeichen
  if not Exec(ExpandConstant('{cmd}'), '/S /C "' + AlsArgument(Programm) + ' ' + Parameter + ' > ' +
      AlsArgument(Ausgabedatei) + ' 2>&1"', '', SW_HIDE, ewWaitUntilTerminated, Rc) then
    Rc := -1;
  Ausgabe := Trim(LiesUtf8(Ausgabedatei));
  Result := Rc;
end;

// ---------------------------------------------------------------------------------------------------------------
// Laufende App
// ---------------------------------------------------------------------------------------------------------------

// Fragt die Prozessliste von Windows ab (WMI), ob AndyTours läuft
function AppLaeuft: Boolean;
var
  Locator, Dienst, Treffer: Variant;
begin
  Result := False;
  try
    Locator := CreateOleObject('WbemScripting.SWbemLocator');
    Dienst := Locator.ConnectServer('.', 'root\CIMV2');
    Treffer := Dienst.ExecQuery('SELECT ProcessId FROM Win32_Process WHERE Name = ''' + Prozessname + '''');
    Result := Treffer.Count > 0;
  except
    // Ohne WMI lässt sich nichts feststellen; dann wie „läuft nicht“ weitermachen
    Result := False;
  end;
end;

// Bittet so lange, AndyTours zu beenden, bis es beendet ist; false, wenn der Benutzer abbricht
function WarteAufBeendeteApp(Was: String): Boolean;
begin
  Result := True;
  while AppLaeuft do
  begin
    if SuppressibleMsgBox('AndyTours läuft noch. Bitte beenden Sie AndyTours und klicken Sie dann auf „Wiederholen“.' + #13#10#13#10 +
        'Mit „Abbrechen“ wird die ' + Was + ' abgebrochen.', mbError, MB_RETRYCANCEL, IDCANCEL) = IDCANCEL then
    begin
      Result := False;
      Exit;
    end;
  end;
end;

// ---------------------------------------------------------------------------------------------------------------
// Protokoll: je Änderung eine Zeile „Zeitpunkt | Aktion | Art | Ziel“
// ---------------------------------------------------------------------------------------------------------------

procedure Protokolliere(Aktion, Art, Ziel: String);
var
  Anzahl: Integer;
begin
  Anzahl := GetArrayLength(Protokollzeilen);
  SetArrayLength(Protokollzeilen, Anzahl + 1);
  Protokollzeilen[Anzahl] := GetDateTimeString('yyyy/mm/dd hh:nn:ss', '-', ':') + ' | ' + Aktion + ' | ' + Art + ' | ' + Ziel;
end;

// Hängt die gesammelten Zeilen an das Protokoll im Programmordner an (bei einer Aktualisierung an das vorhandene)
procedure SchreibeProtokoll;
var
  Datei, Text: String;
  I: Integer;
begin
  Datei := ExpandConstant('{app}\' + Protokollname);
  Text := '';
  if not FileExists(Datei) then
    Text := 'AndyTours – Installationsprotokoll' + #13#10 + 'Zeitpunkt | Aktion | Art | Ziel' + #13#10;
  for I := 0 to GetArrayLength(Protokollzeilen) - 1 do
    Text := Text + Protokollzeilen[I] + #13#10;
  SaveStringToFile(Datei, UTF8Encode(Text), True);
end;

// Vor jeder Programmdatei: merken, ob sie und ihr Ordner schon da sind
procedure VorDatei;
var
  Datei: String;
begin
  Datei := ExpandConstant(CurrentFileName);
  DateiWarVorhanden := FileExists(Datei);
  OrdnerWarVorhanden := DirExists(ExtractFileDir(Datei));
end;

// Nach jeder Programmdatei: Datei (und ggf. neu angelegten Unterordner) protokollieren
procedure NachDatei;
var
  Datei: String;
begin
  Datei := ExpandConstant(CurrentFileName);
  if not OrdnerWarVorhanden then
    Protokolliere('angelegt', 'Ordner', ExtractFileDir(Datei));
  if DateiWarVorhanden then
    Protokolliere('geändert', 'Datei', Datei)
  else
    Protokolliere('angelegt', 'Datei', Datei);
end;

procedure NachVerknuepfung(Verknuepfung: String);
begin
  if IstAktualisierung then
    Protokolliere('geändert', 'Verknüpfung', ExpandConstant(Verknuepfung))
  else
    Protokolliere('angelegt', 'Verknüpfung', ExpandConstant(Verknuepfung));
end;

// ---------------------------------------------------------------------------------------------------------------
// Installation: Start und Seiten
// ---------------------------------------------------------------------------------------------------------------

function InitializeSetup: Boolean;
begin
  IstAktualisierung := RegKeyExists(HKEY_CURRENT_USER, Uninstallschluessel);
  Result := WarteAufBeendeteApp('Installation');
end;

procedure HakenGeklickt(Sender: TObject);
begin
  UebernahmeOrdner.Enabled := UebernahmeHaken.Checked;
  UebernahmeDurchsuchen.Enabled := UebernahmeHaken.Checked;
end;

procedure DurchsuchenGeklickt(Sender: TObject);
var
  Ordner: String;
begin
  Ordner := UebernahmeOrdner.Text;
  if BrowseForFolder('Ordner mit den bisherigen Daten wählen:', Ordner, False) then
    UebernahmeOrdner.Text := Ordner;
end;

procedure InitializeWizard;
var
  Hinweis, Bezeichnung: TNewStaticText;
begin
  // Seite „Datenordner“ mit dem Vorschlag Dokumente\AndyTours
  DatenordnerSeite := CreateInputDirPage(wpWelcome, 'Datenordner', 'Wo sollen Ihre Touren gespeichert werden?',
    'AndyTours speichert Touren, Räder und Aufzeichnungen in diesem Ordner. Beim Deinstallieren bleibt er erhalten, ' +
    'wenn Sie nichts anderes wählen.', False, 'AndyTours');
  DatenordnerSeite.Add('Datenordner:');
  DatenordnerSeite.Values[0] := ExpandConstant('{userdocs}\AndyTours');

  // Seite „Bisherige Daten übernehmen“: vorgewählt, Ordner ohne Vorschlag
  UebernahmeSeite := CreateCustomPage(DatenordnerSeite.ID, 'Bisherige Daten übernehmen',
    'Sollen Ihre bisherigen Touren übernommen werden?');

  UebernahmeHaken := TNewCheckBox.Create(UebernahmeSeite);
  UebernahmeHaken.Parent := UebernahmeSeite.Surface;
  UebernahmeHaken.Caption := 'Bisherige Daten übernehmen';
  UebernahmeHaken.Checked := True;
  UebernahmeHaken.Width := UebernahmeSeite.SurfaceWidth;
  UebernahmeHaken.Top := 0;
  UebernahmeHaken.OnClick := @HakenGeklickt;

  Hinweis := TNewStaticText.Create(UebernahmeSeite);
  Hinweis.Parent := UebernahmeSeite.Surface;
  Hinweis.AutoSize := False;
  Hinweis.WordWrap := True;
  Hinweis.Width := UebernahmeSeite.SurfaceWidth;
  Hinweis.Height := ScaleY(56);
  Hinweis.Top := UebernahmeHaken.Top + UebernahmeHaken.Height + ScaleY(12);
  Hinweis.Caption := 'Geben Sie den Ordner mit Ihren BikeXperience-Daten an. Liegen dort keine BikeXperience-Daten, ' +
    'aber Aufzeichnungen im FIT-Format (auch in Unterordnern), können diese als Touren übernommen werden. ' +
    'Der Ordner wird nur gelesen.';

  Bezeichnung := TNewStaticText.Create(UebernahmeSeite);
  Bezeichnung.Parent := UebernahmeSeite.Surface;
  Bezeichnung.Caption := 'Ordner mit den bisherigen Daten:';
  Bezeichnung.Top := Hinweis.Top + Hinweis.Height + ScaleY(8);

  UebernahmeDurchsuchen := TNewButton.Create(UebernahmeSeite);
  UebernahmeDurchsuchen.Parent := UebernahmeSeite.Surface;
  UebernahmeDurchsuchen.Caption := 'Durchsuchen …';
  UebernahmeDurchsuchen.Width := ScaleX(100);
  UebernahmeDurchsuchen.Height := ScaleY(23);
  UebernahmeDurchsuchen.Left := UebernahmeSeite.SurfaceWidth - UebernahmeDurchsuchen.Width;
  UebernahmeDurchsuchen.Top := Bezeichnung.Top + Bezeichnung.Height + ScaleY(4);
  UebernahmeDurchsuchen.OnClick := @DurchsuchenGeklickt;

  UebernahmeOrdner := TNewEdit.Create(UebernahmeSeite);
  UebernahmeOrdner.Parent := UebernahmeSeite.Surface;
  UebernahmeOrdner.Top := UebernahmeDurchsuchen.Top + ScaleY(1);
  UebernahmeOrdner.Width := UebernahmeDurchsuchen.Left - ScaleX(8);
  UebernahmeOrdner.Text := '';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  // Aktualisierung: Datenordner und Daten bleiben, die Übernahme wird nicht mehr angeboten
  if IstAktualisierung and ((PageID = DatenordnerSeite.ID) or (PageID = UebernahmeSeite.ID)) then
    Result := True
  else if (PageID = UebernahmeSeite.ID) and DatenordnerHatInhalt then
    Result := True;
end;

function Datenordner: String;
begin
  Result := RemoveBackslashUnlessRoot(DatenordnerSeite.Values[0]);
end;

// Prüft den gewählten Datenordner; false = auf der Seite bleiben
function PruefeDatenordner: Boolean;
var
  Ordner: String;
begin
  Result := False;
  Ordner := Datenordner;

  if Length(ExtractFileDir(Ordner)) = 0 then
  begin
    MsgBox('Bitte wählen Sie einen Ordner, kein ganzes Laufwerk.', mbError, MB_OK);
    Exit;
  end;

  // Was beim Deinstallieren entfernt wird, darf den Datenordner nicht enthalten (und umgekehrt)
  if Ueberlappen(Ordner, WizardDirValue) or Ueberlappen(Ordner, Einstellungsordner(0)) or
     Ueberlappen(Ordner, Einstellungsordner(1)) then
  begin
    MsgBox('Der Datenordner darf nicht im Programmordner oder in den Einstellungsordnern von AndyTours liegen, ' +
      'weil diese beim Deinstallieren entfernt werden. Bitte wählen Sie einen anderen Ordner.', mbError, MB_OK);
    Exit;
  end;

  DatenordnerHatInhalt := False;
  if FileExists(AddBackslash(Ordner) + Datenbankdatei) then
  begin
    DatenordnerHatInhalt := True;
    MsgBox('Im Datenordner liegen bereits AndyTours-Daten. Sie bleiben unverändert, und die Übernahme ' +
      'bisheriger Daten entfällt.', mbInformation, MB_OK);
  end
  else if DirExists(Ordner) and not OrdnerIstLeer(Ordner) then
  begin
    if MsgBox('Der Ordner enthält bereits andere Dateien. Bisherige Daten lassen sich nur in einen leeren Ordner ' +
        'übernehmen; die Übernahme entfällt dann.' + #13#10#13#10 + 'Diesen Ordner trotzdem verwenden?',
        mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDNO then
      Exit;
    DatenordnerHatInhalt := True;
  end;

  Result := True;
end;

// Prüft die Seite „Übernahme“ und stellt fest, was im Ordner liegt; false = auf der Seite bleiben
function PruefeUebernahme: Boolean;
var
  Quelle, Ausgabe: String;
  Rc: Integer;
begin
  Result := False;
  Uebernahmeart := 0;
  if not UebernahmeHaken.Checked then
  begin
    Result := True;
    Exit;
  end;

  Quelle := RemoveBackslashUnlessRoot(Trim(UebernahmeOrdner.Text));
  if Quelle = '' then
  begin
    MsgBox('Bitte geben Sie den Ordner mit den bisherigen Daten an, oder entfernen Sie den Haken, um ohne Übernahme ' +
      'fortzufahren.', mbError, MB_OK);
    Exit;
  end;
  if not DirExists(Quelle) then
  begin
    MsgBox('Den Ordner „' + Quelle + '“ gibt es nicht.', mbError, MB_OK);
    Exit;
  end;
  if Ueberlappen(Quelle, Datenordner) then
  begin
    MsgBox('Der Ordner mit den bisherigen Daten darf nicht der Datenordner sein und nicht in ihm liegen.', mbError, MB_OK);
    Exit;
  end;

  // Das Umwandlungsprogramm einmal entpacken und fragen, was im Ordner liegt
  if not WerkzeugEntpackt then
  begin
    WizardForm.NextButton.Enabled := False;
    try
      ExtractTemporaryFile('{#Werkzeug}');
    finally
      WizardForm.NextButton.Enabled := True;
    end;
    WerkzeugEntpackt := True;
  end;
  Rc := StarteWerkzeug(ExpandConstant('{tmp}\{#Werkzeug}'), '--erkennen ' + AlsArgument(Quelle), Ausgabe);

  if Rc = RcBikeXperience then
  begin
    Uebernahmeart := RcBikeXperience;
    Result := True;
  end
  else if Rc = RcNurAufzeichnungen then
  begin
    AnzahlAufzeichnungen := StrToIntDef(Ausgabe, 0);
    if MsgBox('Im Ordner liegen keine BikeXperience-Daten, aber ' + IntToStr(AnzahlAufzeichnungen) +
        ' Aufzeichnungen im FIT-Format (auch in Unterordnern).' + #13#10#13#10 +
        'Sollen diese als Touren übernommen werden?', mbConfirmation, MB_YESNO) = IDYES then
      Uebernahmeart := RcNurAufzeichnungen;
    Result := True;
  end
  else if Rc = RcNichts then
  begin
    if TaskDialogMsgBox('Keine bisherigen Daten gefunden',
        'Im Ordner „' + Quelle + '“ liegen weder BikeXperience-Daten noch Aufzeichnungen im FIT-Format.',
        mbInformation, MB_YESNO, ['Anderen Ordner wählen', 'Ohne Übernahme weiter'], 0) = IDNO then
    begin
      UebernahmeHaken.Checked := False;
      HakenGeklickt(nil);
      Result := True;
    end;
  end
  else
    MsgBox('Der Ordner konnte nicht geprüft werden:' + #13#10#13#10 + Ausgabe, mbError, MB_OK);
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = DatenordnerSeite.ID then
    Result := PruefeDatenordner
  else if CurPageID = UebernahmeSeite.ID then
    Result := PruefeUebernahme;
end;

// Seite „Zusammenfassung“ vor dem Start
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo,
  MemoGroupInfo, MemoTasksInfo: String): String;
var
  Text: String;
begin
  Text := 'Programmordner:' + NewLine + Space + WizardDirValue + NewLine + NewLine;
  if IstAktualisierung then
    Text := Text + 'Aktualisierung auf Version {#AppVersion}:' + NewLine + Space +
      'Datenordner, Daten und Einstellungen bleiben unverändert.' + NewLine + NewLine
  else
  begin
    Text := Text + 'Datenordner:' + NewLine + Space + Datenordner + NewLine + NewLine;
    Text := Text + 'Bisherige Daten:' + NewLine + Space;
    if DatenordnerHatInhalt then
      Text := Text + 'keine Übernahme (der Datenordner ist nicht leer)'
    else if Uebernahmeart = RcBikeXperience then
      Text := Text + 'BikeXperience-Daten aus „' + Trim(UebernahmeOrdner.Text) + '“ umwandeln'
    else if Uebernahmeart = RcNurAufzeichnungen then
      Text := Text + IntToStr(AnzahlAufzeichnungen) + ' Aufzeichnungen aus „' + Trim(UebernahmeOrdner.Text) +
        '“ als Touren übernehmen'
    else
      Text := Text + 'keine Übernahme';
    Text := Text + NewLine + NewLine;
  end;
  Text := Text + 'Verknüpfungen:' + NewLine + Space + 'Startmenü';
  if WizardIsTaskSelected('desktop') then
    Text := Text + ', Desktop';
  Result := Text;
end;

// Kurz vor dem Kopieren noch einmal prüfen – AndyTours könnte inzwischen gestartet worden sein
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not WarteAufBeendeteApp('Installation') then
    Result := 'AndyTours läuft noch. Die Installation wurde abgebrochen.';
end;

// ---------------------------------------------------------------------------------------------------------------
// Installation: Übernahme, Merkdatei, Protokoll
// ---------------------------------------------------------------------------------------------------------------

// Ruft das installierte Umwandlungsprogramm auf und zeigt seine Zusammenfassung oder den Grund des Abbruchs
procedure UebernimmBisherigeDaten;
var
  Parameter, Ausgabe: String;
  Rc: Integer;
begin
  if Uebernahmeart = RcBikeXperience then
    Parameter := AlsArgument(RemoveBackslashUnlessRoot(Trim(UebernahmeOrdner.Text))) + ' ' + AlsArgument(Datenordner)
  else
    Parameter := '--aufzeichnungen ' + AlsArgument(RemoveBackslashUnlessRoot(Trim(UebernahmeOrdner.Text))) + ' ' +
      AlsArgument(Datenordner);

  WizardForm.StatusLabel.Caption := 'Bisherige Daten werden übernommen … Das kann einige Minuten dauern.';
  WizardForm.FilenameLabel.Caption := '';
  Rc := StarteWerkzeug(ExpandConstant('{app}\{#Werkzeug}'), Parameter + ' --ohne-rueckfrage', Ausgabe);

  if Rc = RcErfolg then
    SuppressibleMsgBox(Ausgabe, mbInformation, MB_OK, IDOK)
  else if Rc = RcZielHatDaten then
    SuppressibleMsgBox('Im Datenordner liegen bereits AndyTours-Daten. Sie bleiben unverändert; es wurde nichts ' +
      'übernommen.', mbInformation, MB_OK, IDOK)
  else
    SuppressibleMsgBox('Die Übernahme der bisherigen Daten ist abgebrochen. AndyTours ist trotzdem installiert, und ' +
      'der Datenordner ist unverändert.' + #13#10#13#10 + 'Grund:' + #13#10 + Ausgabe, mbError, MB_OK, IDOK);
end;

// Legt die Einstellungsordner der App an und schreibt den Datenordner in die Merkdatei
procedure SchreibeEinstellungen;
var
  I: Integer;
  MerkdateiWarVorhanden: Boolean;
begin
  for I := 0 to 1 do
    if not DirExists(Einstellungsordner(I)) then
    begin
      ForceDirectories(Einstellungsordner(I));
      Protokolliere('angelegt', 'Ordner', Einstellungsordner(I));
    end;

  MerkdateiWarVorhanden := FileExists(Merkdatei);
  SaveStringToFile(Merkdatei, UTF8Encode(Datenordner), False);
  if MerkdateiWarVorhanden then
    Protokolliere('geändert', 'Datei', Merkdatei)
  else
    Protokolliere('angelegt', 'Datei', Merkdatei);
  Protokolliere('angelegt', 'Einstellung', 'Datenordner ' + Datenordner);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    if not DirExists(WizardDirValue) then
      Protokolliere('angelegt', 'Ordner', WizardDirValue);
  end
  else if CurStep = ssPostInstall then
  begin
    if not IstAktualisierung then
    begin
      if Uebernahmeart <> 0 then
        UebernimmBisherigeDaten;
      // Ohne Übernahme (oder nach einem Abbruch) einen leeren Datenordner anlegen, damit die App ihn vorfindet
      if not DirExists(Datenordner) then
        ForceDirectories(Datenordner);
      SchreibeEinstellungen;
      Protokolliere('angelegt', 'Registry', 'HKCU\' + Uninstallschluessel);
    end
    else
      Protokolliere('geändert', 'Registry', 'HKCU\' + Uninstallschluessel);
    SchreibeProtokoll;
  end;
end;

// ---------------------------------------------------------------------------------------------------------------
// Deinstallation
// ---------------------------------------------------------------------------------------------------------------

function InitializeUninstall: Boolean;
begin
  Result := WarteAufBeendeteApp('Deinstallation');
end;

// Zerlegt eine Protokollzeile; false, wenn sie keine Änderung beschreibt (z. B. die Überschrift)
function LiesProtokollzeile(Zeile: String; var Art, Ziel: String): Boolean;
var
  Teil, Stelle: Integer;
begin
  Result := False;
  // Zeitpunkt und Aktion überspringen
  for Teil := 1 to 2 do
  begin
    Stelle := Pos(' | ', Zeile);
    if Stelle = 0 then
      Exit;
    Delete(Zeile, 1, Stelle + 2);
  end;
  Stelle := Pos(' | ', Zeile);
  if Stelle = 0 then
    Exit;
  Art := Copy(Zeile, 1, Stelle - 1);
  Ziel := Copy(Zeile, Stelle + 3, Length(Zeile));
  Result := (Art = 'Ordner') or (Art = 'Datei') or (Art = 'Verknüpfung') or (Art = 'Registry');
end;

// Löscht einen Ordner samt Inhalt – nie den Datenordner, solange er behalten werden soll
procedure EntferneOrdner(Ordner: String);
begin
  if DirExists(Ordner) and not (Ueberlappen(Ordner, DatenordnerBeimDeinstallieren) and not DatenordnerLoeschen) then
    DelTree(Ordner, True, True, True);
end;

// Arbeitet das Protokoll rückwärts ab und entfernt danach die Einstellungen der App
procedure EntferneNachProtokoll;
var
  I: Integer;
  Art, Ziel: String;
begin
  for I := GetArrayLength(AlteProtokollzeilen) - 1 downto 0 do
    if LiesProtokollzeile(AlteProtokollzeilen[I], Art, Ziel) then
    begin
      if (Art = 'Datei') or (Art = 'Verknüpfung') then
        DeleteFile(Ziel)
      else if Art = 'Ordner' then
        EntferneOrdner(Ziel)
      else if (Art = 'Registry') and (Pos('HKCU\', Ziel) = 1) then
        RegDeleteKeyIncludingSubkeys(HKEY_CURRENT_USER, Copy(Ziel, 6, Length(Ziel)));
    end;

  // Die Einstellungen der App außerhalb des Datenordners, auch wenn die App sie erst später angelegt hat
  EntferneOrdner(Einstellungsordner(0));
  EntferneOrdner(Einstellungsordner(1));
  EntferneOrdner(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    // Vor dem Entfernen lesen: das Protokoll und den aktuell eingestellten Datenordner (aus der Merkdatei)
    AlteProtokollzeilen := InZeilen(LiesUtf8(ExpandConstant('{app}\' + Protokollname)));
    DatenordnerBeimDeinstallieren := RemoveBackslashUnlessRoot(Trim(LiesUtf8(Merkdatei)));
    DatenordnerLoeschen := False;
    if (DatenordnerBeimDeinstallieren <> '') and DirExists(DatenordnerBeimDeinstallieren) then
      DatenordnerLoeschen := SuppressibleTaskDialogMsgBox('Datenordner löschen?',
        'Soll auch der Datenordner mit allen Touren gelöscht werden?' + #13#10#13#10 + DatenordnerBeimDeinstallieren + #13#10#13#10 +
        'Behalten Sie ihn, verwendet eine spätere Installation die Daten wieder.',
        mbConfirmation, MB_YESNO, ['Datenordner behalten', 'Datenordner mit allen Touren löschen'], 0, IDYES) = IDNO;
  end
  else if CurUninstallStep = usPostUninstall then
  begin
    EntferneNachProtokoll;
    if DatenordnerLoeschen then
      DelTree(DatenordnerBeimDeinstallieren, True, True, True);
  end;
end;
