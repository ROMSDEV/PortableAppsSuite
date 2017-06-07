unit RebuildScript;

interface

function GetInstallScript:string;
function GetLanguageFile(i: Integer) : AnsiString;
function GetCompressMethodName:string;
procedure AddEmbeddedFiles;

implementation

uses Windows, SysUtils, Main, PathFunc, MD5, SHA1, MyTypes, Struct;

type
  TScriptBuilder = class
  private
    Res: TBigString;
    CurSlice: Integer;

    procedure Print(s: AnsiString);
    procedure PrintLn(s: AnsiString);

    procedure StrParam(DisplayName, Value:string; UseQuotes: boolean = true);
    procedure IntParam(DisplayName:string; Value:Int64);
    procedure StrConst(DisplayName, Value:string; ShowAlways:boolean=false);
    procedure IntConst(DisplayName:string; Value:Int64);
    procedure NewLine();
    procedure PrintComment(Text: AnsiString);
    procedure PrintSectionHeader(SectionName: AnsiString; PrependEmptyLine: boolean = true);
    procedure PrintFlagsParam(FlagsStr: AnsiString);
    procedure PrintVersions(MinVersion, OnlyBelowVersion: TMySetupVersionData);

    procedure PrintSetupHeader;
    procedure PrintRegistryEntry(const re: TSetupRegistryEntry);
    procedure PrintRunEntry(const re:TSetupRunEntry);
    procedure PrintTaskEntry(const te:TSetupTaskEntry);
    procedure PrintComponentEntry(const ce:TSetupComponentEntry);
    procedure PrintTypeEntry(const te:TSetupTypeEntry);
    procedure PrintFileEntry(const fe:TSetupFileEntry);
    procedure PrintDeleteEntry(const de: TSetupDeleteEntry);
    procedure PrintCustomMessageEntry(const ce:TSetupCustomMessageEntry);
    procedure PrintIconEntry(const ie:TSetupIconEntry);
    procedure PrintDirEntry(const de: TSetupDirEntry);
    procedure PrintIniEntry(const ie: TSetupIniEntry);
    procedure PrintLanguageEntry(const le:TSetupLanguageEntry);

    procedure PrintLangOptions(const le:TSetupLanguageEntry);
  public
    function RebuildScript() : AnsiString;
    function GetLanguageFile(i: Integer) : AnsiString;
  end;

function VerToStr(Ver: Cardinal; ServicePack: Word): String;
var
  Digits: Integer;
begin
  with TSetupVersionDataVersion(Ver) do begin
    Digits := 2;
    if Minor mod 10 = 0 then begin
      Dec (Digits);
      Minor := Minor div 10;
    end;
    FmtStr (Result, '%d.%.*d', [Major, Digits, Minor]);
    if Build <> 0 then
      Result := Result + Format('.%d', [Build]);
    if ServicePack <> 0 then begin
      Result := Result + ' Service Pack ' + IntToStr(Hi(ServicePack));
      if Lo(ServicePack) <> 0 then
        Result := Result + Format('.%d', [Lo(ServicePack)]);
    end;
  end;
end;

function RegRootToStr(const re:TSetupRegistryEntry):string;
begin
  with re do begin
    if rootkey=HKEY_CLASSES_ROOT then Result:='HKCR'
    else if rootkey=HKEY_CURRENT_USER then Result:='HKCU'
    else if rootkey=HKEY_LOCAL_MACHINE then Result:='HKLM'
    else if rootkey=HKEY_USERS then Result:='HKU'
    else if rootkey=HKEY_CURRENT_CONFIG then Result:='HKCC'
    else Result:='';
    if Result<>'' then
      if ro32bit in Options then Result:=Result+'32'
      else if ro64bit in Options then Result:=Result+'64';
  end;
end;

function DoubleQuotes(s:string):string;
var i:integer;
begin
  for i:=length(s) downto 1 do
    if s[i]='"' then Insert('"',s,i);
  Result:=s;
end;

function UnUnicode(s:string):string;
var
  i,n:integer;
begin
  i:=1; Result:='';
  while i<=length(s) do begin
    if Ver>=4202 then n:=(byte(s[i+1]) shl 8) or byte(s[i])
    else n:=byte(s[i]);//(byte(s[i]) shl 8) or byte(s[i+1]);
    if (n>=32) and (n<=127) then Result:=Result+char(n)
    else Result:=Result+'<'+{IntToStr(n)} Format('%4.4x', [n])+'>';
    if Ver>=4202 then inc(i,2) else inc(i);
  end;
end;

function Hash2Str(Hash: TSetupHash) : string;
begin
  case Hash.HashType of
    htMD5:  Result := MD5DigestToString(Hash.MD5);
    htSHA1: Result := SHA1DigestToString(Hash.SHA1);
  else
    Result := '<Unknown>';  
  end;
end;

function SaltToStr(Salt: TSetupSalt) : string;
var
  i: integer;
begin
  Result := '';
  for i := Low(Salt) to High(Salt) do
  begin
    Result := Result + IntToHex(Salt[i], 2);
  end;
  Result := LowerCase(Result);
end;

function ProcArcsToStr(Arcs: TMySetupProcessorArchitectures) : String;
  function ArchStr(a: TSetupProcessorArchitecture):string;
  begin
    case a of
      paX86         : Result:='x86';
      paAMD64       : Result:='x64';
      paIA64        : Result:='ia64';
    else Result:='';
    end;
  end;
var
  pa: TSetupProcessorArchitecture;
  t: String;
begin
  Result := '';
  for pa:=Low(pa) to High(pa) do
    if pa in Arcs then begin
      t := ArchStr(pa);
      if t<>'' then Result := Result + t + ' ';
    end;
  Result := Trim(Result);
end;

function GetCompressMethodName:string;
begin
  case SetupHeader.CompressMethod of
    cmStored: Result:='none';
    cmZip: Result:='zip';
    cmBzip: Result:='bzip2';
    cmLZMA: Result:='lzma';
    cmLZMA2: Result:='lzma2';
  end;
end;

function GetInnoVersionStr() : string;
var
  Ver1, Ver2, Ver3: Integer;
begin
  Ver1 := Ver div 1000;
  Ver2 := (Ver div 100) - (Ver1 * 10);
  Ver3 := Ver mod 100; 

  Result := Format('%d.%d.%d', [Ver1, Ver2, Ver3]);
  if (VerIsUnicode) then
    Result := Result + ' (Unicode)';
end;

function GetBaseFileName() : string;
var
  name: string;
begin
  name := SetupHeader.BaseFilename;
  if (name = '') then
  begin
    name := ExtractFileName(SetupLdrOriginalFilename);
	// Remove .exe extenstion if have one
	if (Pos('.exe', name) = Length(name) - 3) then
	  name := Copy(name, 1, Length(name) - 4);
  end;
  Result := name;
end;

function MaybeToRtf(Name, Contents:string):string;
begin
  if copy(Contents, 1, 5)='{\rtf' then Name:=ChangeFileExt(Name, '.rtf');
  Result:=Name;
end;

procedure AddFakeRtfOrTxtFile(Name, Contents:string);
begin
  AddFakeFile(MaybeToRtf(Name, Contents), Contents);
end;

procedure AddEmbeddedFiles;
var i:integer;
begin
  with SetupHeader do begin      // mirror these filenames in procedures above
    AddFakeRtfOrTxtFile('embedded\License.txt', LicenseText);
    AddFakeRtfOrTxtFile('embedded\InfoBefore.txt', InfoBeforeText);
    AddFakeRtfOrTxtFile('embedded\InfoAfter.txt', InfoAfterText);
    AddFakeFile('embedded\CompiledCode.bin', CompiledCodeText);
  end;
  AddFakeFile('embedded\WizardImage.bmp', WizardImage);
  AddFakeFile('embedded\WizardSmallImage.bmp', WizardSmallImage);
  AddFakeFile('embedded\decompressor.dll', DecompDll);

  for i:=0 to SetupHeader.NumLanguageEntries-1 do
    with PSetupLanguageEntry(Entries[SeLanguage][i])^ do begin
      AddFakeFile('embedded\'+Name+'.isl', GetLanguageFile(i));
      AddFakeRtfOrTxtFile('embedded\'+Name+'License.txt', LicenseText);
      AddFakeRtfOrTxtFile('embedded\'+Name+'InfoBefore.txt', InfoBeforeText);
      AddFakeRtfOrTxtFile('embedded\'+Name+'InfoAfter.txt', InfoAfterText);
    end;
end;

function GetInstallScript : AnsiString;
var
  Builder: TScriptBuilder;
begin
  Builder := TScriptBuilder.Create;
  try
    Result := Builder.RebuildScript();
  finally
    Builder.Free;
  end;    
end;

function GetLanguageFile(i: Integer) : AnsiString;
var
  Builder: TScriptBuilder;
begin
  Builder := TScriptBuilder.Create;
  try
    Result := Builder.GetLanguageFile(i);
  finally
    Builder.Free;
  end;
end;

{ TScriptBuilder }

procedure TScriptBuilder.Print(s: AnsiString);
begin
  Res.AppendString(s);
end;

procedure TScriptBuilder.PrintLn(s: AnsiString);
begin
  Res.AppendString(s+#13#10);
end;

procedure TScriptBuilder.StrParam(DisplayName, Value:string; UseQuotes: boolean = true);
begin
  if Value<>'' then
  begin
    if UseQuotes then
      Print(DisplayName + ': "' + DoubleQuotes(Value) + '"; ')
    else
      Print(DisplayName + ': ' + Value + '; ');  
  end;
end;

procedure TScriptBuilder.IntParam(DisplayName:string; Value:Int64);
begin
  if Value <> 0 then StrParam(DisplayName, IntToStr(Value), True);
end;

procedure TScriptBuilder.StrConst(DisplayName, Value:string; ShowAlways:boolean=false);
begin
  if ShowAlways or (Value<>'') then PrintLn(DisplayName+'='+Value);
end;

procedure TScriptBuilder.IntConst(DisplayName:string; Value:Int64);
begin
  StrConst(DisplayName, IntToStr(Value));
end;

procedure TScriptBuilder.NewLine();
begin
  PrintLn('');
end;

procedure TScriptBuilder.PrintComment(Text: AnsiString);
begin
  PrintLn('; ' + Text);
end;

procedure TScriptBuilder.PrintSectionHeader(SectionName: AnsiString; PrependEmptyLine: boolean = true);
begin
  if (PrependEmptyLine) then NewLine();
  PrintLn('[' + SectionName + ']');
end;

procedure TScriptBuilder.PrintFlagsParam(FlagsStr: AnsiString);
begin
  if (FlagsStr <> '') then
    Print('Flags: ' + FlagsStr);
  NewLine();  
end;

procedure TScriptBuilder.PrintVersions(MinVersion, OnlyBelowVersion: TMySetupVersionData);
begin
  with MinVersion do
    if (WinVersion>$04000000) or (NTVersion>$04000000) then
      StrParam('MinVersion', VerToStr(WinVersion,0) + ',' + VerToStr(NTVersion,NTServicePack), False);
  with OnlyBelowVersion do
    if (WinVersion<>0) or (NTVersion<>0) then
      StrParam('OnlyBelowVersion', VerToStr(WinVersion,0) + ',' + VerToStr(NTVersion,NTServicePack), False);
end;

function TScriptBuilder.RebuildScript: AnsiString;
var
  i:integer;
begin
  Res.Init;

  // Add UTF-8 BOM to script start for Unicode versions.
  if (VerIsUnicode) then Print(#$EF#$BB#$BF);

  PrintSetupHeader;

  CurSlice:=0;
  if Entries[seFile].Count>0 then begin
    PrintSectionHeader('Files');
    for i:=0 to Entries[seFile].Count-1 do
      PrintFileEntry(PSetupFileEntry(Entries[seFile][i])^);
  end;
  if Entries[seDir].Count>0 then begin
    PrintSectionHeader('Dirs');
    for i:=0 to Entries[seDir].Count-1 do
      PrintDirEntry(PSetupDirEntry(Entries[seDir][i])^);
  end;
  if Entries[seRegistry].Count>0 then begin
    PrintSectionHeader('Registry');
    for i:=0 to Entries[seRegistry].Count-1 do
      PrintRegistryEntry(PSetupRegistryEntry(Entries[seRegistry][i])^);
  end;
  if Entries[seIni].Count>0 then begin
    PrintSectionHeader('INI');
    for i:=0 to Entries[seIni].Count-1 do
      PrintIniEntry(PSetupIniEntry(Entries[seIni][i])^);
  end;
  if Entries[seRun].Count>0 then begin
    PrintSectionHeader('Run');
    for i:=0 to Entries[seRun].Count-1 do
      PrintRunEntry(PSetupRunEntry(Entries[seRun][i])^);
  end;
  if Entries[seUninstallRun].Count>0 then begin
    PrintSectionHeader('UninstallRun');
    for i:=0 to Entries[seUninstallRun].Count-1 do
      PrintRunEntry(PSetupRunEntry(Entries[seUninstallRun][i])^);
  end;
  if Entries[seIcon].Count>0 then begin
    PrintSectionHeader('Icons');
    for i:=0 to Entries[seIcon].Count-1 do
      PrintIconEntry(PSetupIconEntry(Entries[seIcon][i])^);
  end;
  if Entries[seTask].Count>0 then begin
    PrintSectionHeader('Tasks');
    for i:=0 to Entries[seTask].Count-1 do
      PrintTaskEntry(PSetupTaskEntry(Entries[seTask][i])^);
  end;
  if Entries[seComponent].Count>0 then begin
    PrintSectionHeader('Components');
    for i:=0 to Entries[seComponent].Count-1 do
      PrintComponentEntry(PSetupComponentEntry(Entries[seComponent][i])^);
  end;
  if Entries[seInstallDelete].Count>0 then begin
    PrintSectionHeader('InstallDelete');
    for i:=0 to Entries[seInstallDelete].Count-1 do
      PrintDeleteEntry(PSetupDeleteEntry(Entries[seInstallDelete][i])^);
  end;
  if Entries[seUninstallDelete].Count>0 then begin
    PrintSectionHeader('UninstallDelete');
    for i:=0 to Entries[seUninstallDelete].Count-1 do
      PrintDeleteEntry(PSetupDeleteEntry(Entries[seUninstallDelete][i])^);
  end;
  if Entries[seType].Count>0 then begin
    PrintSectionHeader('Types');
    for i:=0 to Entries[seType].Count-1 do
      PrintTypeEntry(PSetupTypeEntry(Entries[seType][i])^);
  end;
  if Entries[seCustomMessage].Count>0 then begin
    PrintSectionHeader('CustomMessages');
    for i:=0 to Entries[seCustomMessage].Count-1 do
      PrintCustomMessageEntry(PSetupCustomMessageEntry(Entries[seCustomMessage][i])^);
  end;
  if Entries[seLanguage].Count>0 then begin
    PrintSectionHeader('Languages');
    PrintComment('These files are stubs');
    PrintComment('To achieve better results after recompilation, use the real language files');
    for i:=0 to Entries[seLanguage].Count-1 do
      PrintLanguageEntry(PSetupLanguageEntry(Entries[seLanguage][i])^);
  end;
  
  Result := Res.CopyAsString;
  Res.Destroy;
end;

procedure TScriptBuilder.PrintSetupHeader;
  function Priv2Str(Priv: TMySetupPrivileges) : string;
  begin
    case Priv of
      prNone        : Result:='none';
      prPowerUser   : Result:='poweruser';
      prLowest      : Result:='lowest';
    else Result:='';
    end;
  end;
  function DisPage2Str(a: TMySetupDisablePage):string;
  begin
    case a of
      dpAuto   : Result:='auto';
      dpNo     : Result:='no';
      dpYes    : Result:='yes';
    else Result:='';
    end;
  end;
begin
  StrConst(';InnoSetupVersion', GetInnoVersionStr());

  PrintSectionHeader('Setup');
  StrConst('AppName', SetupHeader.AppName, True);
  StrConst('AppVerName', SetupHeader.AppVerName);
  StrConst('AppVersion', SetupHeader.AppVersion);
  StrConst('AppPublisher', SetupHeader.AppPublisher);
  StrConst('AppPublisherURL', SetupHeader.AppPublisherURL);
  StrConst('AppSupportPhone', SetupHeader.AppSupportPhone);
  StrConst('AppSupportURL', SetupHeader.AppSupportURL);
  StrConst('AppUpdatesURL', SetupHeader.AppUpdatesURL);
  StrConst('AppMutex', SetupHeader.AppMutex);
  StrConst('AppComments', SetupHeader.AppComments);
  StrConst('AppModifyPath', SetupHeader.AppModifyPath);
  if (shCreateAppDir in SetupHeader.Options) then
    StrConst('DefaultDirName', SetupHeader.DefaultDirName)
  else
    StrConst('CreateAppDir', 'no');
  if (SetupHeader.DefaultGroupName <> '(Default)') then
    StrConst('DefaultGroupName', SetupHeader.DefaultGroupName);
  StrConst('UninstallDisplayIcon', SetupHeader.UninstallDisplayIcon);
  StrConst('UninstallDisplayName', SetupHeader.UninstallDisplayName);
  if (SetupHeader.UninstallFilesDir <> '{app}') then
    StrConst('UninstallFilesDir', SetupHeader.UninstallFilesDir);
  StrConst('OutputBaseFilename',  GetBaseFilename());
  StrConst('Compression', GetCompressMethodName);
  if SetupHeader.EncryptionUsed then
    StrConst('; Encryption', 'yes');
  if (Ver > 4202) and (SetupHeader.EncryptionUsed or (shPassword in SetupHeader.Options)) then
  begin
    StrConst('; PasswordHash', Hash2Str(SetupHeader.PasswordHash));
    StrConst('; PasswordSalt', SaltToStr(SetupHeader.PasswordSalt));
  end;  
  StrConst('ArchitecturesAllowed', ProcArcsToStr(SetupHeader.ArchitecturesAllowed));
  StrConst('ArchitecturesInstallIn64BitMode', ProcArcsToStr(SetupHeader.ArchitecturesInstallIn64BitMode));
  if (Ver<5310) and not(shUninstallable in SetupHeader.Options) then
    StrConst('Uninstallable', 'no')
  else if not SameText(SetupHeader.Uninstallable, 'yes') then
    StrConst('Uninstallable', SetupHeader.Uninstallable);
  if (SetupHeader.PrivilegesRequired <> prAdmin) then
    StrConst('PrivilegesRequired', Priv2Str(SetupHeader.PrivilegesRequired));
  if (Int64(SetupHeader.ExtraDiskSpaceRequired) > 0) then
    IntConst('ExtraDiskSpaceRequired', Int64(SetupHeader.ExtraDiskSpaceRequired));
  if (SetupHeader.DisableDirPage <> dpNo) then
    StrConst('DisableDirPage', DisPage2Str(SetupHeader.DisableDirPage));
  if (SetupHeader.DisableProgramGroupPage <> dpNo) then
    StrConst('DisableProgramGroupPage', DisPage2Str(SetupHeader.DisableProgramGroupPage));
  if (shChangesAssociations in SetupHeader.Options) then
    StrConst('ChangesAssociations', 'yes');
  if (shAllowNoIcons in SetupHeader.Options) then
    StrConst('AllowNoIcons', 'yes');
  // mirror these filenames in AddEmbeddedFiles()
  with SetupHeader do begin
    if LicenseText<>'' then StrConst('LicenseFile', MaybeToRtf('embedded\License.txt', LicenseText));
    if InfoBeforeText<>'' then StrConst('InfoBeforeFile', MaybeToRtf('embedded\InfoBefore.txt', InfoBeforeText));
    if InfoAfterText<>'' then StrConst('InfoAfterFile', MaybeToRtf('embedded\InfoAfter.txt', InfoAfterText));
  end;
  if WizardImage<>'' then StrConst('WizardImageFile', 'embedded\WizardImage.bmp');
  if WizardSmallImage<>'' then StrConst('WizardSmallImageFile', 'embedded\WizardSmallImage.bmp');
  if TimeStampsInUTC then StrConst(';TimeStampsInUTC', 'yes');
end;

procedure TScriptBuilder.PrintRegistryEntry(const re: TSetupRegistryEntry);
  function OptStr(Opt: TSetupRegistryOption):string;
  begin
    case Opt of
      roCreateValueIfDoesntExist         : Result:='createvalueifdoesntexist';
      roUninsDeleteValue                 : Result:='uninsdeletevalue';
      roUninsClearValue                  : Result:='uninsclearvalue';
      roUninsDeleteEntireKey             : Result:='uninsdeletekey';
      roUninsDeleteEntireKeyIfEmpty      : Result:='uninsdeletekeyifempty';
      roPreserveStringType               : Result:='preservestringtype';
      roDeleteKey                        : Result:='deletekey';
      roDeleteValue                      : Result:='deletevalue';
      roNoError                          : Result:='noerror';
      roDontCreateKey                    : Result:='dontcreatekey';
    else Result:='';
    end;
  end;
var
  s,t:string;
  i:dword;
  o:TSetupRegistryOption;
  vType: String;
begin
  with re do begin
    StrParam('Root', RegRootToStr(re), false);
    StrParam('Subkey', Subkey);
    StrParam('ValueName', ValueName);
    t:='';
    if Typ<>rtNone then begin
      case Typ of
        rtString: begin vType:='String'; t:=ValueData; end;
        rtExpandString: begin vType:='ExpandSZ'; t:=ValueData; end;
        rtDword: begin vType:='Dword';
          if Ver>=4104 then t:=ValueData else begin
            Move(ValueData[1],i,4); t:=Format('$%x', [i]);
          end;
        end;
        rtQWord: begin vType:='Qword'; t := ValueData; end;
        rtBinary: begin vType:='Binary';
          t:='';
          for i:=1 to length(ValueData) do t:=t+IntToHex(byte(ValueData[i]),2)+' ';
          if length(t)>0 then SetLength(t,length(t)-1);
        end;
        rtMultiString: begin vType:='MultiSZ'; t:=ValueData;
          repeat
            i:=pos(#0,t);
            if i=0 then break;
            t[i]:='{'; insert('break}',s,i+1);
          until false;
        end;
        else vType:='Unknown';
      end;
    end;
    StrParam('ValueType', vType, False);
    StrParam('ValueData',t);
    StrParam('Components',Components, false);
    StrParam('Tasks',Tasks, false);
    StrParam('Languages',Languages);
    StrParam('Check',Check);
    StrParam('BeforeInstall',BeforeInstall);
    StrParam('AfterInstall',AfterInstall);

    PrintVersions(MinVersion, OnlyBelowVersion);

    s:='';
    for o:=Low(o) to High(o) do if o in Options then begin
      t:=OptStr(o);
      if t<>'' then s:=s+t+' ';
    end;
    PrintFlagsParam(s);
  end;
end;

procedure TScriptBuilder.PrintRunEntry(const re:TSetupRunEntry);
  function OptStr(Opt: TSetupRunOption):string;
  begin
    case Opt of
      roShellExec           : Result:='shellexec';
      roSkipIfDoesntExist   : Result:='skipifdoesntexist';
      roPostInstall         : Result:='postinstall';
      roUnchecked           : Result:='unchecked';
      roSkipIfSilent        : Result:='skipifsilent';
      roSkipIfNotSilent     : Result:='skipifnotsilent';
      roHideWizard          : Result:='hidewizard';
      roRun32Bit            : Result:='32bit';
      roRun64Bit            : Result:='64bit';
      roRunAsOriginalUser   : Result:='';
    else Result:='';
    end;
  end;
var
  s, t: string;
  o:TSetupRunOption;
begin
  with re do begin
    StrParam('Filename',Name);
    StrParam('Parameters',Parameters);
    StrParam('WorkingDir',WorkingDir);
    StrParam('RunOnceId',RunOnceId);
    StrParam('StatusMsg',StatusMsg);
    StrParam('Description',Description);
    StrParam('Components',Components, false);
    StrParam('Tasks',Tasks, false);
    StrParam('Languages',Languages);
    StrParam('Check',Check);
    StrParam('BeforeInstall',BeforeInstall);
    StrParam('AfterInstall',AfterInstall);

    PrintVersions(MinVersion, OnlyBelowVersion);

    for o:=Low(o) to High(o) do if o in Options then begin
      t:=OptStr(o);
      if t<>'' then s:=s+t+' ';
    end;
    case Wait of
      rwNoWait: s := s + 'nowait';
      rwWaitUntilIdle: s := s + 'waituntilidle';
    end;
    PrintFlagsParam(s);
  end;
end;

procedure TScriptBuilder.PrintTaskEntry(const te:TSetupTaskEntry);
begin
  with te do begin
    StrParam('Name',Name);
    StrParam('Description',Description);
    StrParam('GroupDescription',GroupDescription);
    StrParam('Components',Components);
    StrParam('Languages',Languages);
    StrParam('Check',Check);

    PrintVersions(MinVersion, OnlyBelowVersion);

    NewLine();
  end;
end;

procedure TScriptBuilder.PrintComponentEntry(const ce:TSetupComponentEntry);
var
  s:string;
  i:integer;
begin
  with ce do begin
    StrParam('Name',Name);
    StrParam('Description',Description);
    s:=Types;
    for i:=1 to length(s) do if s[i]=',' then s[i]:=' ';
    StrParam('Types',s);
    IntParam('ExtraDiskSpaceRequired',Int64(ExtraDiskSpaceRequired));
    StrParam('Languages',Languages);
    StrParam('Check',Check);

    PrintVersions(MinVersion, OnlyBelowVersion);

    NewLine();
  end;
end;

procedure TScriptBuilder.PrintTypeEntry(const te:TSetupTypeEntry);
begin
  with te do begin
    StrParam('Name',Name);
    StrParam('Description',Description);
    StrParam('Languages',Languages);
    StrParam('Check',Check);

    PrintVersions(MinVersion, OnlyBelowVersion);

    NewLine();
  end;
end;

procedure TScriptBuilder.PrintFileEntry(const fe:TSetupFileEntry);
  function OptStr(Opt: TSetupFileOption):string;
  begin
    case Opt of
      foConfirmOverwrite         : Result:='confirmoverwrite';
      foUninsNeverUninstall      : Result:='uninsneveruninstall';
      foRestartReplace           : Result:='restartreplace';
      foDeleteAfterInstall       : Result:='deleteafterinstall';
      foRegisterServer           : Result:='regserver';
      foRegisterTypeLib          : Result:='regtypelib';
      foSharedFile               : Result:='sharedfile';
      foCompareTimeStamp         : Result:='comparetimestamp';
      foFontIsntTrueType         : Result:='fontisnttruetype';
      foSkipIfSourceDoesntExist  : Result:='skipifsourcedoesntexist';
      foOverwriteReadOnly        : Result:='overwritereadonly';
      foOverwriteSameVersion     : Result:='';
      foCustomDestName           : Result:='';
      foOnlyIfDestFileExists     : Result:='onlyifdestfileexists';
      foNoRegError               : Result:='noregerror';
      foUninsRestartDelete       : Result:='uninsrestartdelete';
      foOnlyIfDoesntExist        : Result:='onlyifdoesntexist';
      foIgnoreVersion            : Result:='ignoreversion';
      foPromptIfOlder            : Result:='promptifolder';
      foDontCopy                 : Result:='dontcopy';
      foUninsRemoveReadOnly      : Result:='uninsremovereadonly';
      foRecurseSubDirsExternal   : Result:='';
      fo32bit                    : Result:='32bit';
      fo64bit                    : Result:='64bit';
      foExternalSizePreset       : Result:='';
      foSetNTFSCompression       : Result:='setntfscomptression';
      foUnsetNTFSCompression     : Result:='unsetntfscomptression';
      foGacInstall               : Result:='gacinstall';
    else Result:='';
    end;
  end;
var
  s,t:string;
  o:TSetupFileOption;
  Opts:TMySetupFileOptions;
begin
  if (fe.FileType <> ftUserFile) then Exit;

  with fe do begin
    if LocationEntry<>-1 then
      with PSetupFileLocationEntry(Entries[seFileLocation][LocationEntry])^ do begin
        if FirstSlice<>LastSlice then
          PrintComment('the following file spans ' + GetSliceName(FirstSlice) + ' to ' + GetSliceName(LastSlice))
        else if FirstSlice<>CurSlice then
          PrintComment('the following file starts on ' + GetSliceName(FirstSlice));
        CurSlice:=LastSlice;
      end;
    if (LocationEntry=-1) then Print('; ');
    StrParam('Source',SourceFilename);
    StrParam('DestDir',RemoveBackslashUnlessRoot(DestDir));
    StrParam('DestName',DestName);
    StrParam('FontInstall',InstallFontName);
    StrParam('Components',Components, false);
    StrParam('Tasks',Tasks, false);
    StrParam('Languages',Languages);
    StrParam('Check',Check);
    StrParam('BeforeInstall',BeforeInstall);
    StrParam('AfterInstall',AfterInstall);

    PrintVersions(MinVersion, OnlyBelowVersion);

    s:='';
    Opts:=Options;
    if foDeleteAfterInstall in Opts then Exclude(Opts, foUninsNeverUninstall);
    for o:=Low(o) to High(o) do if o in Opts then begin
      t:=OptStr(o);
      if t<>'' then s:=s+t+' ';
    end;
    PrintFlagsParam(s);
  end;
end;

procedure TScriptBuilder.PrintDeleteEntry(const de: TSetupDeleteEntry);
  function TypeStr(Typ: TSetupDeleteType):string;
  begin
    case Typ of
      dfFiles             : Result:='files';
      dfFilesAndOrSubdirs : Result:='filesandordirs';
      dfDirIfEmpty        : Result:='dirifempty';
    end;  
  end;
begin
  with de do begin
    StrParam('Type', TypeStr(DeleteType), false);
    StrParam('Name',Name);
    StrParam('Components',Components, false);
    StrParam('Tasks',Tasks, false);
    StrParam('Languages',Languages);
    StrParam('Check',Check, false);
    StrParam('BeforeInstall',BeforeInstall);
    StrParam('AfterInstall',AfterInstall);
    NewLine();
  end;
end;

procedure TScriptBuilder.PrintCustomMessageEntry(const ce:TSetupCustomMessageEntry);
var
  s,v:string;
begin
  with ce do begin
    s := Name;
    if (LangIndex >= 0) then
      s:=PSetupLanguageEntry(Entries[seLanguage][LangIndex])^.Name+'.'+s;
    v := StringReplace(Value, #13#10, '%n', [rfReplaceAll]);
    StrConst(s, v);
  end;
end;

procedure TScriptBuilder.PrintIconEntry(const ie:TSetupIconEntry);
var
  s:string;
begin
  with ie do begin
    StrParam('Name',IconName);
    StrParam('Filename',Filename);
    StrParam('Parameters',Parameters);
    StrParam('WorkingDir',WorkingDir);
    StrParam('IconFilename',IconFilename);
    if IconIndex<>0 then StrParam('IconIndex', IntToStr(IconIndex), false);
    StrParam('Comment',Comment);
    StrParam('Components',Components, false);
    StrParam('Tasks',Tasks, false);
    StrParam('Languages',Languages);
    StrParam('Check',Check);
    StrParam('BeforeInstall',BeforeInstall);
    StrParam('AfterInstall',AfterInstall);

    PrintVersions(MinVersion, OnlyBelowVersion);

    s:='';
    case CloseOnExit of
      icYes: s:=s+'closeonexit ';
      icNo: s:=s+'dontcloseonexit ';
    end;
    case ShowCmd of
      SW_SHOWMAXIMIZED: s:=s+'runmaximized ';
      SW_SHOWMINNOACTIVE: s:=s+'runminimized ';
    end;
    PrintFlagsParam(s);
  end;
end;

procedure TScriptBuilder.PrintDirEntry(const de: TSetupDirEntry);
  function OptStr(Opt: TSetupDirOption):string;
  begin
    case Opt of
      doUninsNeverUninstall      : Result:='uninsneveruninstall';
      doDeleteAfterInstall       : Result:='deleteafterinstall';
      doUninsAlwaysUninstall     : Result:='uninsalwaysuninstall';
      doSetNTFSCompression       : Result:='setntfscomptression';
      doUnsetNTFSCompression     : Result:='unsetntfscomptression';
    else Result:='';
    end;
  end;
var
  s,t:string;
  Opts: TMySetupDirOptions;
  o: TSetupDirOption;
begin
  StrParam('Name', de.DirName);
  Opts := de.Options;
  for o:=Low(o) to High(o) do if o in Opts then begin
    t := OptStr(o);
    if t<>'' then s := s + t + ' ';
  end;
  PrintFlagsParam(s);
end;

procedure TScriptBuilder.PrintIniEntry(const ie: TSetupIniEntry);
  function OptStr(Opt: TSetupIniOption):string;
  begin
    case Opt of
      ioCreateKeyIfDoesntExist      : Result:='createkeyifdoesntexist';
      ioUninsDeleteEntry            : Result:='uninsdeleteentry';
      ioUninsDeleteEntireSection    : Result:='uninsdeletesection';
      ioUninsDeleteSectionIfEmpty   : Result:='uninsdeletesectionifempty';
    else Result:='';
    end;
  end;
var
  s,t:string;
  Opts: TMySetupIniOptions;
  o: TSetupIniOption;
begin
  StrParam('FileName', ie.Filename);
  StrParam('Section', ie.Section);
  StrParam('Key', ie.Entry);
  StrParam('String', ie.Value);

  Opts := ie.Options;
  for o:=Low(o) to High(o) do if o in Opts then begin
    t := OptStr(o);
    if t<>'' then s := s + t + ' ';
  end;
  PrintFlagsParam(s);
end;

procedure TScriptBuilder.PrintLanguageEntry(const le:TSetupLanguageEntry);
begin
  with le do begin                                      // these filenames must be mirrored in AddEmbeddedFiles()
    StrParam('Name', Name);
    StrParam('MessagesFile', 'embedded\'+Name+'.isl');
    if LicenseText<>'' then StrParam('LicenseFile', MaybeToRtf('embedded\'+Name+'License.txt', LicenseText));
    if InfoBeforeText<>'' then StrParam('InfoBeforeFile', MaybeToRtf('embedded\'+Name+'InfoBefore.txt', InfoBeforeText));
    if InfoAfterText<>'' then StrParam('InfoAfterFile', MaybeToRtf('embedded\'+Name+'InfoAfter.txt', InfoAfterText));
  end;
  NewLine();
end;

procedure TScriptBuilder.PrintLangOptions(const le:TSetupLanguageEntry);
begin
  with le do begin
    StrConst('LanguageName', UnUnicode(LanguageName));
    StrConst('LanguageID', '$'+IntToHex(LanguageID,4));
    IntConst('LanguageCodePage', LanguageCodePage);
    StrConst('DialogFontName', DialogFontName);
    StrConst('TitleFontName', TitleFontName);
    StrConst('WelcomeFontName', WelcomeFontName);
    StrConst('CopyrightFontName', CopyrightFontName);
    IntConst('DialogFontSize', DialogFontSize);
    IntConst('TitleFontSize', TitleFontSize);
    IntConst('WelcomeFontSize', WelcomeFontSize);
    IntConst('CopyrightFontSize', CopyrightFontSize);
    if (RightToLeft) then StrConst('RightToLeft', 'yes');
  end;
end;

function TScriptBuilder.GetLanguageFile(i: Integer) : AnsiString;
begin
  Res.Init;
  PrintSectionHeader('LangOptions', False);
  PrintLangOptions(PSetupLanguageEntry(Entries[seLanguage][i])^);
  Result := Res.CopyAsString;
  Res.Destroy;
end;

end.
