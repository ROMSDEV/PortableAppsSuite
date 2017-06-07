unit InstFunc;

{
  Inno Setup
  Copyright (C) 1997-2004 Jordan Russell
  Portions by Martijn Laan
  For conditions of distribution and use, see LICENSE.TXT.

  Misc. installation functions

  $jrsoftware: issrc/Projects/InstFunc.pas,v 1.30 2004/02/16 02:58:30 jr Exp $
}

interface

uses
  Windows, SysUtils, Struct, Int64Em;

{$I VERSION.INC}

type
  PSimpleStringListArray = ^TSimpleStringListArray;
  TSimpleStringListArray = array[0..$1FFFFFFE] of String;
  TSimpleStringList = class
  private
    FList: PSimpleStringListArray;
    FCount, FCapacity: Integer;
    function Get(Index: Integer): String;
    procedure SetCapacity(NewCapacity: Integer);
  public
    destructor Destroy; override;
    procedure Add(const S: String);
    procedure AddIfDoesntExist(const S: String);
    procedure Clear;
    function IndexOf(const S: String): Integer;

    property Count: Integer read FCount;
    property Items[Index: Integer]: String read Get; default;
  end;

  TDeleteDirProc = function(const DirName: String; Param: Pointer): Boolean;

  TFROChecksum = record
    Size: Cardinal;
    CRC: Longint;
  end;

  TEnumFROFilenamesProc = procedure(const Filename: String; Param: Pointer);

const
  RegRootKeyNames: array[HKEY_CLASSES_ROOT..HKEY_DYN_DATA] of PChar = (
    'HKEY_CLASSES_ROOT', 'HKEY_CURRENT_USER', 'HKEY_LOCAL_MACHINE',
    'HKEY_USERS', 'HKEY_PERFORMANCE_DATA', 'HKEY_CURRENT_CONFIG',
    'HKEY_DYN_DATA');

function CheckForMutexes(Mutexes: String): Boolean;
function CompareFileRenameOperationsChecksums(const S1, S2: TFROChecksum): Boolean;
procedure DelayDeleteFile(const Filename: String; const Tries: Integer);
function DelTree(const Path: String; const IsDir, DeleteFiles, DeleteSubdirsAlso: Boolean;
  const DeleteDirProc: TDeleteDirProc; const Param: Pointer): Boolean;
procedure EnumFileReplaceOperationsFilenames(const EnumFunc: TEnumFROFilenamesProc;
  Param: Pointer);
function GenerateUniqueName(Path: String; const Extension: String): String;
function GetComputerNameString: String;
function GetFileDateTime(const Filename: string; var DateTime: TFileTime): Boolean;
function GetSpaceOnDisk(const DriveRoot: String;
  var FreeBytes, TotalBytes: Integer64): Boolean;
function GetUserNameString: String;
function GrantPermissionOnFile(const Filename: String;
  const Entries: TGrantPermissionEntry; const EntryCount: Integer): Boolean;
function GrantPermissionOnKey(const RootKey: HKEY; const Subkey: String;
  const Entries: TGrantPermissionEntry; const EntryCount: Integer): Boolean;
procedure IncrementSharedCount(const Filename: String;
  const AlreadyExisted: Boolean);
function DecrementSharedCount(const Filename: String): Boolean;
function InstExec(const Filename, Params: String; WorkingDir: String;
  const WaitUntilTerminated, WaitUntilIdle: Boolean; const ShowCmd: Integer;
  const ProcessMessagesProc: TProcedure; var ResultCode: Integer): Boolean;
function InstShellExec(const Filename, Params: String; WorkingDir: String;
  const ShowCmd: Integer; var ErrorCode: Integer): Boolean;
procedure MakeFileRenameOperationsChecksum(var Checksum: TFROChecksum);
function ModifyPifFile(const Filename: String; const CloseOnExit: Boolean): Boolean;
procedure RegisterServer(const Filename: String; const FailCriticalErrors: Boolean);
function UnregisterServer(const Filename: String; const FailCriticalErrors: Boolean): Boolean;
procedure UnregisterFont(const FontName, FontFilename: String);
procedure RestartComputer;
procedure RestartReplace(const TempFile, DestFile: String);
procedure Win32ErrorMsg(const FunctionName: String);

implementation

uses
  Messages, ShellApi, PathFunc, CmnFunc2, Msgs, MsgIDs, Compress;

procedure Win32ErrorMsg(const FunctionName: String);
var
  LastError: DWORD;
begin
  LastError := GetLastError;
  raise Exception.Create(FmtSetupMessage(msgErrorFunctionFailedWithMessage,
    [FunctionName, IntToStr(LastError), SysErrorMessage(LastError)]));
end;

function GenerateUniqueName(Path: String; const Extension: String): String;
  function IntToBase32(Number: Longint): String;
  const
    Table: array[0..31] of Char = '0123456789ABCDEFGHIJKLMNOPQRSTUV';
  var
    I: Integer;
  begin
    Result := '';
    for I := 0 to 4 do begin
      Insert(Table[Number and 31], Result, 1);
      Number := Number shr 5;
    end;
  end;
var
  Rand, RandOrig: Longint;
begin
  Path := AddBackslash(Path);
  RandOrig := Random($2000000);
  Rand := RandOrig;
  repeat
    Inc(Rand);
    if Rand > $1FFFFFF then Rand := 0;
    if Rand = RandOrig then
      { practically impossible to go through 33 million possibilities,
        but check "just in case"... }
      raise Exception.Create(FmtSetupMessage1(msgErrorTooManyFilesInDir,
        RemoveBackslashUnlessRoot(Path)));
    { Generate a random name }
    Result := Path + 'is-' + IntToBase32(Rand) + Extension;
  until not FileOrDirExists(Result);
end;

procedure RestartReplace(const TempFile, DestFile: String);
{ Renames TempFile to DestFile the next time Windows is started. If DestFile
  already existed, it will be overwritten. If DestFile is '' then TempFile
  will be deleted, however this is only supported by 95/98 and NT, not
  Windows 3.1x. }
var
  WinDir, WinInitFile, TempWinInitFile: String;
  OldF, NewF: TextFile;
  OldFOpened, NewFOpened: Boolean;
  L, L2: String;
  RenameSectionFound, WriteLastLine: Boolean;
  NewDestFile: PChar;
begin
  if not UsingWinNT then begin
    { Because WININIT.INI allows multiple entries with the same name,
      it must manually parse the file instead of using
      WritePrivateProfileString }
    WinDir := GetWinDir;
    WinInitFile := AddBackslash(WinDir) + 'WININIT.INI';
    OldFOpened := False;
    NewFOpened := False;
    try
      try
        if NewFileExists(WinInitFile) then begin
          TempWinInitFile := GenerateUniqueName(WinDir, '.tmp');
          { Flush Windows' cache for the file first }
          WritePrivateProfileString(nil, nil, nil, PChar(WinInitFile));
          AssignFile(OldF, WinInitFile);
          FileMode := fmOpenRead or fmShareDenyWrite;  Reset(OldF);
          OldFOpened := True;
        end
        else
          TempWinInitFile := WinInitFile;
        AssignFile(NewF, TempWinInitFile);
        FileMode := fmOpenWrite or fmShareExclusive;  Rewrite(NewF);
        NewFOpened := True;
        RenameSectionFound := False;
        WriteLastLine := False;
        if OldFOpened then
          while not Eof(OldF) do begin
            Readln(OldF, L);
            WriteLastLine := True;
            L2 := Trim(L);
            if (L2 <> '') and (L2[1] = '[') then begin
              if CompareText(L, '[rename]') = 0 then
                RenameSectionFound := True
              else
              if RenameSectionFound then
                Break;
            end;
            Writeln(NewF, L);
            WriteLastLine := False;
          end;
        if not RenameSectionFound then
          Writeln(NewF, '[rename]');
        if DestFile <> '' then
          L2 := GetShortName(DestFile)
        else
          L2 := 'NUL';
        Writeln(NewF, L2 + '=' + GetShortName(TempFile));
        if OldFOpened then begin
          if WriteLastLine then
            Writeln(NewF, L);
          while not Eof(OldF) do begin
            Readln(OldF, L);
            Writeln(NewF, L);
          end;
        end;
      finally
        if NewFOpened then CloseFile(NewF);
        if OldFOpened then CloseFile(OldF);
      end;
    except
      if OldFOpened then DeleteFile(TempWinInitFile);
      raise;
    end;
    if OldFOpened then begin
      if not DeleteFile(WinInitFile) then
        Win32ErrorMsg('DeleteFile');
      if not MoveFile(PChar(TempWinInitFile), PChar(WinInitFile)) then
        Win32ErrorMsg('MoveFile');
    end;
  end
  else begin
    if DestFile <> '' then
      NewDestFile := PChar(DestFile)
    else
      NewDestFile := nil;
    if not MoveFileEx(PChar(TempFile), NewDestFile,
       MOVEFILE_DELAY_UNTIL_REBOOT or MOVEFILE_REPLACE_EXISTING) then
      Win32ErrorMsg('MoveFileEx');
  end;
end;

function DelTree(const Path: String; const IsDir, DeleteFiles, DeleteSubdirsAlso: Boolean;
  const DeleteDirProc: TDeleteDirProc; const Param: Pointer): Boolean;
{ Deletes the specified directory including all files and subdirectories in
  it (including those with hidden, system, and read-only attributes). Returns
  True if it was able to successfully remove everything. }
var
  BasePath, FindSpec: String;
  H: THandle;
  FindData: TWin32FindData;
  S: String;
begin
  Result := True;
  if DeleteFiles then begin
    if IsDir then begin
      BasePath := AddBackslash(Path);
      FindSpec := BasePath + '*';
    end
    else begin
      BasePath := PathExtractPath(Path);
      FindSpec := Path;
    end;
    H := FindFirstFile(PChar(FindSpec), FindData);
    if H <> INVALID_HANDLE_VALUE then begin
      repeat
        S := FindData.cFileName;
        if FindData.dwFileAttributes and FILE_ATTRIBUTE_READONLY <> 0 then
          SetFileAttributes(PChar(BasePath + S), FindData.dwFileAttributes and
            not FILE_ATTRIBUTE_READONLY);
        if FindData.dwFileAttributes and FILE_ATTRIBUTE_DIRECTORY = 0 then
          Windows.DeleteFile(PChar(BasePath + S))
        else begin
          if DeleteSubdirsAlso and (S <> '.') and (S <> '..') then
            if not DelTree(BasePath + S, True, True, True, DeleteDirProc, Param) then
              Result := False;
        end;
      until not FindNextFile(H, FindData);
      Windows.FindClose(H);
    end;
  end;
  if IsDir then begin
    if Assigned(DeleteDirProc) then begin
      if not DeleteDirProc(Path, Param) then
        Result := False;
    end
    else begin
      if not RemoveDirectory(PChar(Path)) then
        Result := False;
    end;
  end;
end;

procedure IncrementSharedCount(const Filename: String;
  const AlreadyExisted: Boolean);
const
  SharedDLLsKey = NEWREGSTR_PATH_SETUP + '\SharedDLLs';  {don't localize}
var
  K: HKEY;
  Disp, Size, Count, CurType, NewType: DWORD;
  CountStr: String;
  FilenameP: PChar;
begin
  if RegCreateKeyEx(HKEY_LOCAL_MACHINE, SharedDLLsKey, 0, nil, REG_OPTION_NON_VOLATILE,
     KEY_QUERY_VALUE or KEY_SET_VALUE, nil, K, @Disp) <> ERROR_SUCCESS then
    raise Exception.Create(FmtSetupMessage(msgErrorRegOpenKey,
      [RegRootKeyNames[HKEY_LOCAL_MACHINE], SharedDLLsKey]));
  FilenameP := PChar(Filename);
  Count := 0;
  NewType := REG_DWORD;
  try
    if RegQueryValueEx(K, FilenameP, nil, @CurType, nil, @Size) = ERROR_SUCCESS then
      case CurType of
        REG_SZ:
          if RegQueryStringValue(K, FilenameP, CountStr) then begin
            Count := StrToInt(CountStr);
            NewType := REG_SZ;
          end;
        REG_BINARY: begin
            if (Size >= 1) and (Size <= 4) then begin
              if RegQueryValueEx(K, FilenameP, nil, nil, @Count, @Size) <> ERROR_SUCCESS then
                { ^ relies on the high 3 bytes of Count being initialized to 0 }
                Abort;
              NewType := REG_BINARY;
            end;
          end;
        REG_DWORD: begin
            Size := SizeOf(DWORD);
            if RegQueryValueEx(K, FilenameP, nil, nil, @Count, @Size) <> ERROR_SUCCESS then
              Abort;
          end;
      end;
  except
    Count := 0;
  end;
  if Integer(Count) < 0 then Count := 0;  { just in case... }
  if (Count = 0) and AlreadyExisted then
    Inc(Count);
  Inc(Count);
  case NewType of
    REG_SZ: begin
        CountStr := IntToStr(Count);
        RegSetValueEx(K, FilenameP, 0, NewType, PChar(CountStr), Length(CountStr)+1);
      end;
    REG_BINARY, REG_DWORD:
      RegSetValueEx(K, FilenameP, 0, NewType, @Count, SizeOf(Count));
  end;
  RegCloseKey(K);
end;

function DecrementSharedCount(const Filename: String): Boolean;
{ Returns True if OK to delete }
const
  SharedDLLsKey = NEWREGSTR_PATH_SETUP + '\SharedDLLs';  {don't localize}
var
  K: HKEY;
  Count, CurType, NewType, Size, Disp: DWORD;
  CountStr: String;
begin
  Result := False;

  if RegCreateKeyEx(HKEY_LOCAL_MACHINE, SharedDLLsKey, 0, nil, REG_OPTION_NON_VOLATILE,
     KEY_QUERY_VALUE or KEY_SET_VALUE, nil, K, @Disp) <> ERROR_SUCCESS then
    raise Exception.Create(FmtSetupMessage(msgErrorRegOpenKey,
      [RegRootKeyNames[HKEY_LOCAL_MACHINE], SharedDLLsKey]));
  if RegQueryValueEx(K, PChar(Filename), nil, @CurType, nil, @Size) <> ERROR_SUCCESS then begin
    RegCloseKey(K);
    Exit;
  end;

  Count := 2;
  NewType := REG_DWORD;
  try
    case CurType of
      REG_SZ:
        if RegQueryStringValue(K, PChar(Filename), CountStr) then begin
          Count := StrToInt(CountStr);
          NewType := REG_SZ;
        end;
      REG_BINARY: begin
          if (Size >= 1) and (Size <= 4) then begin
            if RegQueryValueEx(K, PChar(Filename), nil, nil, @Count, @Size) <> ERROR_SUCCESS then
              { ^ relies on the high 3 bytes of Count being initialized to 0 }
              Abort;
            NewType := REG_BINARY;
          end;
        end;
      REG_DWORD: begin
          Size := SizeOf(DWORD);
          if RegQueryValueEx(K, PChar(Filename), nil, nil, @Count, @Size) <> ERROR_SUCCESS then
            Abort;
        end;
    end;
  except
    Count := 2;  { default to 2 if an error occurred }
  end;
  Dec(Count);
  if Count <= 0 then begin
    Result := True;
    RegDeleteValue(K, PChar(Filename));
  end
  else begin
    case NewType of
      REG_SZ: begin
          CountStr := IntToStr(Count);
          RegSetValueEx(K, PChar(Filename), 0, NewType, PChar(CountStr), Length(CountStr)+1);
        end;
      REG_BINARY, REG_DWORD:
        RegSetValueEx(K, PChar(Filename), 0, NewType, @Count, SizeOf(Count));
    end;
  end;
  RegCloseKey(K);
end;

function GetFileDateTime(const Filename: string; var DateTime: TFileTime): Boolean;
var
  Handle: THandle;
  FindData: TWin32FindData;
begin
  Handle := FindFirstFile(PChar(Filename), FindData);
  if Handle <> INVALID_HANDLE_VALUE then begin
    Windows.FindClose(Handle);
    if FindData.dwFileAttributes and FILE_ATTRIBUTE_DIRECTORY = 0 then begin
      DateTime := FindData.ftLastWriteTime;
      Result := True;
      Exit;
    end;
  end;
  Result := False;
  DateTime.dwLowDateTime := 0;
  DateTime.dwHighDateTime := 0;
end;

function InstExec(const Filename, Params: String; WorkingDir: String;
  const WaitUntilTerminated, WaitUntilIdle: Boolean; const ShowCmd: Integer;
  const ProcessMessagesProc: TProcedure; var ResultCode: Integer): Boolean;
var
  CmdLine: String;
  WorkingDirP: PChar;
  StartupInfo: TStartupInfo;
  ProcessInfo: TProcessInformation;
begin
  Result := True;
  CmdLine := '"' + Filename + '"';
  if Params <> '' then
    CmdLine := CmdLine + ' ' + Params;
  if (CompareText(PathExtractExt(Filename), '.bat') = 0) or
     (CompareText(PathExtractExt(Filename), '.cmd') = 0) then begin
    { Use our own handling for .bat and .cmd files since passing them straight
      to CreateProcess on Windows NT 4.0 has problems: it doesn't properly
      quote the command line it passes to cmd.exe. This didn't work before:
        Filename: "c:\batch.bat"; Parameters: """abc"""
      And other Windows versions might have unknown quirks too, since
      CreateProcess isn't documented to accept .bat files in the first place. }
    if UsingWinNT then
      { With cmd.exe, the whole command line must be quoted for quoted
        parameters to work }
      CmdLine := '"' + AddBackslash(GetSystemDir) + 'cmd.exe" /C "' + CmdLine + '"'
    else
      CmdLine := '"' + AddBackslash(GetWinDir) + 'COMMAND.COM" /C ' + CmdLine;
  end;
  if WorkingDir = '' then
    WorkingDir := PathExtractDir(Filename);
  FillChar(StartupInfo, SizeOf(StartupInfo), 0);
  StartupInfo.cb := SizeOf(StartupInfo);
  StartupInfo.dwFlags := STARTF_USESHOWWINDOW;
  StartupInfo.wShowWindow := ShowCmd;
  if WorkingDir <> '' then
    WorkingDirP := PChar(WorkingDir)
  else
    WorkingDirP := nil;
  if not CreateProcess(nil, PChar(CmdLine), nil, nil, False, 0, nil,
     WorkingDirP, StartupInfo, ProcessInfo) then begin
    Result := False;
    ResultCode := GetLastError;
    Exit;
  end;
  with ProcessInfo do begin
    { Don't need the thread handle, so close it now }
    CloseHandle(hThread);
    if WaitUntilIdle then
      WaitForInputIdle(hProcess, INFINITE);
    if WaitUntilTerminated then
      { Wait until the process returns, but still process any messages that
        arrive. }
      repeat
        { Process any pending messages first because MsgWaitForMultipleObjects
          (called below) only returns when *new* messages arrive }
        if Assigned(ProcessMessagesProc) then
          ProcessMessagesProc;
      until MsgWaitForMultipleObjects(1, hProcess, False, INFINITE, QS_ALLINPUT) <> WAIT_OBJECT_0+1;
    { Get the exit code. Will be set to STILL_ACTIVE if not yet available }
    GetExitCodeProcess(hProcess, DWORD(ResultCode));
    { Then close the process handle }
    CloseHandle(hProcess);
  end;
end;

function InstShellExec(const Filename, Params: String; WorkingDir: String;
  const ShowCmd: Integer; var ErrorCode: Integer): Boolean;
var
  WorkingDirP: PChar;
  E: Integer;
begin
  if WorkingDir = '' then
    WorkingDir := PathExtractDir(Filename);
  if WorkingDir <> '' then
    WorkingDirP := PChar(WorkingDir)
  else
    WorkingDirP := nil;
  E := ShellExecute(0, 'open', PChar(Filename), PChar(Params), WorkingDirP,
    ShowCmd);
  Result := E > 32;
  if not Result then
    ErrorCode := E;
end;

function CheckForMutexes(Mutexes: String): Boolean;
{ Returns True if any of the mutexes in the comma-separated Mutexes string
  exist }
var
  I: Integer;
  M: String;
  H: THandle;
begin
  Result := False;
  repeat
    I := Pos(',', Mutexes);
    if I = 0 then I := Maxint;
    M := Trim(Copy(Mutexes, 1, I-1));
    if M <> '' then begin
      H := OpenMutex(SYNCHRONIZE, False, PChar(M));
      if H <> 0 then begin
        CloseHandle(H);
        Result := True;
        Break;
      end;
    end;
    Delete(Mutexes, 1, I);
  until Mutexes = '';
end;

function ModifyPifFile(const Filename: String; const CloseOnExit: Boolean): Boolean;
{ Changes the "Close on exit" setting of a .pif file. Returns True if it was
  able to make the change. }
var
  F: File;
  B: Byte;
begin
  { Note: Specs on the .pif format were taken from
    http://smsoft.chat.ru/en/pifdoc.htm }
  Result := False;
  AssignFile(F, Filename);
  FileMode := fmOpenReadWrite or fmShareExclusive;
  Reset(F, 1);
  try
    { Is it a valid .pif file? }
    if FileSize(F) >= $171 then begin
      Seek(F, $63);
      BlockRead(F, B, SizeOf(B));
      { Toggle the "Close on exit" bit }
      if (B and $10 <> 0) <> CloseOnExit then begin
        B := B xor $10;
        Seek(F, $63);
        BlockWrite(F, B, SizeOf(B));
      end;
      Result := True;
    end;
  finally
    CloseFile(F);
  end;
end;

function GetComputerNameString: String;
var
  Buf: array[0..MAX_COMPUTERNAME_LENGTH] of Char;
  Size: DWORD;
begin
  Size := SizeOf(Buf);
  if GetComputerName(Buf, Size) then
    Result := Buf
  else
    Result := '';
end;

function GetUserNameString: String;
var
  Buf: array[0..255] of Char;
  BufSize: DWORD;
begin
  BufSize := SizeOf(Buf);
  if GetUserName(Buf, BufSize) then
    Result := Buf
  else
    Result := '';
end;

{ Work around problem in D2's declaration of the function }
function NewAdjustTokenPrivileges(TokenHandle: THandle; DisableAllPrivileges: BOOL;
  const NewState: TTokenPrivileges; BufferLength: DWORD;
  PreviousState: PTokenPrivileges; ReturnLength: PDWORD): BOOL; stdcall;
  external advapi32 name 'AdjustTokenPrivileges';

procedure RestartComputer;
{ Restarts the computer. The function will NOT return if it is successful,
  since Windows kills the process immediately after sending it a WM_ENDSESSION
  message. }

  procedure RestartErrorMessage;
  begin
    MessageBox(0, PChar(SetupMessages[msgErrorRestartingComputer]),
      PChar(SetupMessages[msgErrorTitle]), MB_OK or MB_ICONEXCLAMATION);
  end;

var
  Token: THandle;
  TokenPriv: TTokenPrivileges;
const
  SE_SHUTDOWN_NAME = 'SeShutdownPrivilege';  { don't localize }
begin
  if Win32Platform = VER_PLATFORM_WIN32_NT then begin
    if not OpenProcessToken(GetCurrentProcess, TOKEN_ADJUST_PRIVILEGES or TOKEN_QUERY,
       {$IFNDEF Delphi3orHigher} @Token {$ELSE} Token {$ENDIF}) then begin
      RestartErrorMessage;
      Exit;
    end;

    LookupPrivilegeValue(nil, SE_SHUTDOWN_NAME, TokenPriv.Privileges[0].Luid);

    TokenPriv.PrivilegeCount := 1;
    TokenPriv.Privileges[0].Attributes := SE_PRIVILEGE_ENABLED;

    NewAdjustTokenPrivileges(Token, False, TokenPriv, 0, nil, nil);

    { Cannot test the return value of AdjustTokenPrivileges. }
    if GetLastError <> ERROR_SUCCESS then begin
      RestartErrorMessage;
      Exit;
    end;
  end;
  if not ExitWindowsEx(EWX_REBOOT, 0) then
    RestartErrorMessage;

  { If ExitWindows/ExitWindowsEx were successful, program execution halts here
    (at least on Win95) }
end;

procedure DelayDeleteFile(const Filename: String; const Tries: Integer);
{ Attempts to delete Filename, retrying up to Tries times if the file is in use.
  It delays 250 msec between tries. }
var
  I: Integer;
begin
  for I := 0 to Tries-1 do begin
    if I <> 0 then Sleep(250);
    if Windows.DeleteFile(PChar(Filename)) or
       (GetLastError = ERROR_FILE_NOT_FOUND) or
       (GetLastError = ERROR_PATH_NOT_FOUND) then
      Break;
  end;
end;

procedure MakeFileRenameOperationsChecksum(var Checksum: TFROChecksum);
{ Calculates a checksum of the current PendingFileRenameOperations registry
  value (on NT 4+ platforms) or of the current WININIT.INI file (on non-NT
  platforms). The caller can use this checksum to determine if
  PendingFileRenameOperations or WININIT.INI was changed (perhaps by another
  program). }
var
  K: HKEY;
  S: String;
  WinInitFile: String;
  F: File;
  Buf: array[0..4095] of Byte;
  BytesRead: Integer;
begin
  Checksum.Size := 0;
  Checksum.CRC := $12345678;  { ...it hasn't been calculated yet }
  if UsingWinNT then begin
    if RegOpenKeyEx(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager',
       0, KEY_QUERY_VALUE, K) = ERROR_SUCCESS then begin
      if RegQueryMultiStringValue(K, 'PendingFileRenameOperations', S) then begin
        Checksum.Size := Length(S);
        Checksum.CRC := GetCRC32(S[1], Length(S));
      end;
      RegCloseKey(K);
    end;
  end
  else begin
    WinInitFile := AddBackslash(GetWinDir) + 'WININIT.INI';
    AssignFile(F, WinInitFile);
    {$I-}
    FileMode := fmOpenRead or fmShareDenyWrite;  Reset(F, 1);
    {$I+}
    if IOResult = 0 then begin
      try
        Checksum.CRC := Longint($FFFFFFFF);
        while True do begin
          {$I-}
          BlockRead(F, Buf, SizeOf(Buf), BytesRead);
          {$I+}
          if (IOResult <> 0) or (BytesRead = 0) then Break;
          Inc(Checksum.Size, BytesRead);
          Checksum.CRC := UpdateCRC32(Checksum.CRC, Buf, BytesRead);
        end;
        Checksum.CRC := not Checksum.CRC;
      finally
        CloseFile(F);
      end;
    end;
  end;
end;

function CompareFileRenameOperationsChecksums(const S1, S2: TFROChecksum): Boolean;
{ Returns True if two TFROChecksum records are equal }
begin
  Result := (S1.Size = S2.Size) and (S1.CRC = S2.CRC);
end;

procedure EnumFileReplaceOperationsFilenames(const EnumFunc: TEnumFROFilenamesProc;
  Param: Pointer);
{ Enumerates all the filenames in the current PendingFileRenameOperations
  registry value or WININIT.INI file. The function does not distinguish between
  source and destination filenames; it enumerates both. }

  procedure DoNT;
  var
    K: HKEY;
    S: String;
    P, PEnd: PChar;
  begin
    if RegOpenKeyEx(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager',
       0, KEY_QUERY_VALUE, K) = ERROR_SUCCESS then begin
      RegQueryMultiStringValue(K, 'PendingFileRenameOperations', S);
      RegCloseKey(K);
      P := PChar(S);
      PEnd := P + Length(S);
      while P < PEnd do begin
        if P[0] = '!' then
          { Note: '!' means that MoveFileEx was called with the
            MOVEFILE_REPLACE_EXISTING flag }
          Inc(P);
        if StrLComp(P, '\??\', 4) = 0 then
          Inc(P, 4);
        if P[0] <> #0 then
          EnumFunc(P, Param);
        Inc(P, StrLen(P) + 1);
      end;
    end;
  end;

  procedure DoNonNT;
  var
    WinInitFile: String;
    F: TextFile;
    Line, Filename: String;
    InRenameSection: Boolean;
    P: Integer;
  begin
    WinInitFile := AddBackslash(GetWinDir) + 'WININIT.INI';
    AssignFile(F, WinInitFile);
    {$I-}
    FileMode := fmOpenRead or fmShareDenyWrite;  Reset(F);
    {$I+}
    if IOResult = 0 then begin
      try
        InRenameSection := False;
        while True do begin
          {$I-}
          if Eof(F) or (IOResult <> 0) then Break;
          Readln(F, Line);
          if IOResult <> 0 then Break;
          {$I+}
          Line := Trim(Line);
          if (Line = '') or (Line[1] = ';') then
            Continue;
          if Line[1] = '[' then begin
            InRenameSection := (CompareText(Line, '[rename]') = 0);
          end
          else if InRenameSection then begin
            P := Pos('=', Line);
            if P > 0 then begin
              Filename := Copy(Line, 1, P-1);
              if (Filename <> '') and (CompareText(Filename, 'NUL') <> 0) then
                EnumFunc(Filename, Param);
              Filename := Copy(Line, P+1, Maxint);
              if (Filename <> '') and (CompareText(Filename, 'NUL') <> 0) then
                EnumFunc(Filename, Param);
            end;
          end;
        end;
      finally
        CloseFile(F);
      end;
    end;
  end;

begin
  if UsingWinNT then
    DoNT
  else
    DoNonNT;
end;

procedure RegisterServer(const Filename: String; const FailCriticalErrors: Boolean);
var
  SaveCurrentDir: String;
  SaveCursor: HCURSOR;
  NewErrorMode, SaveErrorMode: UINT;
  LibHandle: THandle;
  RegisterServerProc: function: HRESULT; stdcall;
  RegisterCode: HRESULT;
begin
  SaveCurrentDir := GetCurrentDir;
  SaveCursor := SetCursor(LoadCursor(0, IDC_WAIT));  { show the 'hourglass' cursor }
  if FailCriticalErrors then
    NewErrorMode := SEM_NOOPENFILEERRORBOX or SEM_FAILCRITICALERRORS
  else
    NewErrorMode := SEM_NOOPENFILEERRORBOX;
  SaveErrorMode := SetErrorMode(NewErrorMode);
  try
    SetCurrentDir(PathExtractDir(Filename));
    LibHandle := SafeLoadLibrary(Filename, NewErrorMode);
    if LibHandle = 0 then
      Win32ErrorMsg('LoadLibrary');
    try
      @RegisterServerProc := GetProcAddress(LibHandle, 'DllRegisterServer');
      if @RegisterServerProc = nil then
        raise Exception.Create(SetupMessages[msgErrorRegisterServerMissingExport]);
      RegisterCode := RegisterServerProc;
      if FAILED(RegisterCode) then
        raise Exception.Create(FmtSetupMessage(msgErrorFunctionFailed,
          ['DllRegisterServer', IntToHexStr8(RegisterCode)]));
    finally
      FreeLibrary(LibHandle);
    end;
  finally
    SetCurrentDir(SaveCurrentDir);
    SetErrorMode(SaveErrorMode);
    SetCursor(SaveCursor);
  end;
end;

function UnregisterServer(const Filename: String; const FailCriticalErrors: Boolean): Boolean;
var
  SaveCurrentDir: String;
  NewErrorMode, SaveErrorMode: UINT;
  LibHandle: THandle;
  UnregisterServerProc: function: HRESULT; stdcall;
begin
  Result := True;
  try
    SaveCurrentDir := GetCurrentDir;
    if FailCriticalErrors then
      NewErrorMode := SEM_NOOPENFILEERRORBOX or SEM_FAILCRITICALERRORS
    else
      NewErrorMode := SEM_NOOPENFILEERRORBOX;
    SaveErrorMode := SetErrorMode(NewErrorMode);
    try
      SetCurrentDir(PathExtractDir(Filename));
      LibHandle := SafeLoadLibrary(Filename, NewErrorMode);
      if LibHandle <> 0 then begin
        try
          @UnregisterServerProc := GetProcAddress(LibHandle, 'DllUnregisterServer');
          if Assigned(@UnregisterServerProc) and SUCCEEDED(UnregisterServerProc) then
            Exit;
        finally
          FreeLibrary(LibHandle);
        end;
      end;
    finally
      SetCurrentDir(SaveCurrentDir);
      SetErrorMode(SaveErrorMode);
    end;
  except
  end;
  Result := False;
end;

procedure UnregisterFont(const FontName, FontFilename: String);
var
  K: HKEY;
begin
  if Win32Platform <> VER_PLATFORM_WIN32_WINDOWS then begin
    WriteProfileString('Fonts', PChar(FontName), nil);
  end
  else begin
    if RegOpenKeyEx(HKEY_LOCAL_MACHINE, NEWREGSTR_PATH_SETUP + '\Fonts',
       0, KEY_SET_VALUE, K) = ERROR_SUCCESS then begin
      RegDeleteValue(K, PChar(FontName));
      RegCloseKey(K);
    end;
  end;
  if RemoveFontResource(PChar(FontFilename)) then
    SendNotifyMessage(HWND_BROADCAST, WM_FONTCHANGE, 0, 0);
end;

function GetSpaceOnDisk(const DriveRoot: String;
  var FreeBytes, TotalBytes: Integer64): Boolean;
type
  TLargeIntegerRec = record
    Lo, Hi: Cardinal;
  end;
var
  GetDiskFreeSpaceExFunc: function(lpDirectoryName: PAnsiChar;
    lpFreeBytesAvailable: PLargeInteger; lpTotalNumberOfBytes: PLargeInteger;
    lpTotalNumberOfFreeBytes: PLargeInteger): BOOL; stdcall;
  SectorsPerCluster, BytesPerSector, FreeClusters, TotalClusters: Cardinal;
begin
  { NOTE: The docs claim that GetDiskFreeSpace supports UNC paths on
    Windows 95 OSR2 and later. But that does not seem to be the case in my
    tests; it fails with error 50 on Windows 95 through Me.
    GetDiskFreeSpaceEx, however, *does* succeed with UNC paths, so use it
    if available. }
  GetDiskFreeSpaceExFunc := GetProcAddress(GetModuleHandle(kernel32),
    'GetDiskFreeSpaceExA');
  if Assigned(@GetDiskFreeSpaceExFunc) then begin
    Result := GetDiskFreeSpaceExFunc(PChar(DriveRoot),
      @TLargeInteger(FreeBytes), @TLargeInteger(TotalBytes), nil);
  end
  else begin
    Result := GetDiskFreeSpace(PChar(DriveRoot), DWORD(SectorsPerCluster),
      DWORD(BytesPerSector), DWORD(FreeClusters), DWORD(TotalClusters));
    if Result then begin
      { Windows 95/98 cap the result of GetDiskFreeSpace at 2GB, but NT 4.0
        does not, so we must use a 64-bit multiply operation to avoid an
        overflow. }
      Multiply32x32to64(BytesPerSector * SectorsPerCluster, FreeClusters,
        FreeBytes);
      Multiply32x32to64(BytesPerSector * SectorsPerCluster, TotalClusters,
        TotalBytes);
    end;
  end;
end;

function GrantPermission(const ObjectType: DWORD; const ObjectName: String;
  const Entries: TGrantPermissionEntry; const EntryCount: Integer;
  const Inheritance: DWORD): Boolean;
{ Grants the specified access to the specified object. Returns True if
  successful. Always fails on Windows 9x/Me and NT 4.0. }
type
  PPSID = ^PSID;
  PPACL = ^PACL;
  PTrusteeW = ^TTrusteeW;
  TTrusteeW = record
    pMultipleTrustee: PTrusteeW;
    MultipleTrusteeOperation: DWORD;  { MULTIPLE_TRUSTEE_OPERATION }
    TrusteeForm: DWORD;  { TRUSTEE_FORM }
    TrusteeType: DWORD;  { TRUSTEE_TYPE }
    ptstrName: PWideChar;
  end;
  TExplicitAccessW = record
    grfAccessPermissions: DWORD;
    grfAccessMode: DWORD;  { ACCESS_MODE }
    grfInheritance: DWORD;
    Trustee: TTrusteeW;
  end;
  PArrayOfExplicitAccessW = ^TArrayOfExplicitAccessW;
  TArrayOfExplicitAccessW = array[0..999999] of TExplicitAccessW;
const
  GRANT_ACCESS = 1;
  TRUSTEE_IS_SID = 0;
  TRUSTEE_IS_UNKNOWN = 0;
var
  AdvApiHandle: THandle;
  GetNamedSecurityInfoA: function(pObjectName: PAnsiChar; ObjectType: DWORD;
    SecurityInfo: SECURITY_INFORMATION; ppsidOwner, ppsidGroup: PPSID;
    ppDacl, ppSacl: PPACL; var ppSecurityDescriptor: PSECURITY_DESCRIPTOR): DWORD;
    stdcall;
  SetNamedSecurityInfoA: function(pObjectName: PAnsiChar; ObjectType: DWORD;
    SecurityInfo: SECURITY_INFORMATION; ppsidOwner, ppsidGroup: PSID;
    ppDacl, ppSacl: PACL): DWORD; stdcall;
  SetEntriesInAclW: function(cCountOfExplicitEntries: ULONG;
    const pListOfExplicitEntries: TExplicitAccessW; OldAcl: PACL;
    var NewAcl: PACL): DWORD; stdcall;
  SD: PSECURITY_DESCRIPTOR;
  Dacl, NewDacl: PACL;
  ExplicitAccess: PArrayOfExplicitAccessW;
  E: ^TGrantPermissionEntry;
  I: Integer;
  Sid: PSID;
begin
  Result := False;
  if Win32Platform <> VER_PLATFORM_WIN32_NT then
    Exit;
  if Lo(GetVersion) < 5 then
    Exit;  { GetNamedSecurityInfo and SetEntriesInACL are buggy on NT 4 }

  AdvApiHandle := GetModuleHandle(advapi32);
  GetNamedSecurityInfoA := GetProcAddress(AdvApiHandle, 'GetNamedSecurityInfoA');
  SetNamedSecurityInfoA := GetProcAddress(AdvApiHandle, 'SetNamedSecurityInfoA');
  SetEntriesInAclW := GetProcAddress(AdvApiHandle, 'SetEntriesInAclW');
  if (@GetNamedSecurityInfoA = nil) or (@SetNamedSecurityInfoA = nil) or
     (@SetEntriesInAclW = nil) then
    Exit;

  ExplicitAccess := nil;
  if GetNamedSecurityInfoA(PChar(ObjectName), ObjectType, DACL_SECURITY_INFORMATION,
     nil, nil, @Dacl, nil, SD) <> ERROR_SUCCESS then
    Exit;
  try
    { Note: Dacl will be nil if GetNamedSecurityInfo is called on a FAT partition.
      Be careful not to dereference a nil pointer. }
    ExplicitAccess := AllocMem(EntryCount * SizeOf(ExplicitAccess[0]));
    E := @Entries;
    for I := 0 to EntryCount-1 do begin
      if not AllocateAndInitializeSid(E.Sid.Authority, E.Sid.SubAuthCount,
         E.Sid.SubAuth[0], E.Sid.SubAuth[1], 0, 0, 0, 0, 0, 0, Sid) then
        Exit;
      ExplicitAccess[I].grfAccessPermissions := E.AccessMask;
      ExplicitAccess[I].grfAccessMode := GRANT_ACCESS;
      ExplicitAccess[I].grfInheritance := Inheritance;
      ExplicitAccess[I].Trustee.TrusteeForm := TRUSTEE_IS_SID;
      ExplicitAccess[I].Trustee.TrusteeType := TRUSTEE_IS_UNKNOWN;
      PSID(ExplicitAccess[I].Trustee.ptstrName) := Sid;
      Inc(E);
    end;
    if SetEntriesInAclW(EntryCount, ExplicitAccess[0], Dacl, NewDacl) <> ERROR_SUCCESS then
      Exit;
    try
      if SetNamedSecurityInfoA(PChar(ObjectName), ObjectType,
         DACL_SECURITY_INFORMATION, nil, nil, NewDacl, nil) <> ERROR_SUCCESS then
        Exit;
    finally
      LocalFree(HLOCAL(NewDacl));
    end;
  finally
    if Assigned(ExplicitAccess) then begin
      for I := EntryCount-1 downto 0 do begin
        Sid := PSID(ExplicitAccess[I].Trustee.ptstrName);
        if Assigned(Sid) then
          FreeSid(Sid);
      end;
      FreeMem(ExplicitAccess);
    end;
    LocalFree(HLOCAL(SD));
  end;
  Result := True;
end;

const
  OBJECT_INHERIT_ACE    = 1;
  CONTAINER_INHERIT_ACE = 2;

function GrantPermissionOnFile(const Filename: String;
  const Entries: TGrantPermissionEntry; const EntryCount: Integer): Boolean;
{ Grants the specified access to the specified file/directory. Returns True if
  successful. Always fails on Windows 9x/Me and NT 4.0. }
const
  SE_FILE_OBJECT = 1;
var
  Attr, Inheritance: DWORD;
begin
  Attr := GetFileAttributes(PChar(Filename));
  if Attr = $FFFFFFFF then begin
    Result := False;
    Exit;
  end;
  if Attr and FILE_ATTRIBUTE_DIRECTORY <> 0 then
    Inheritance := OBJECT_INHERIT_ACE or CONTAINER_INHERIT_ACE
  else
    Inheritance := 0;
  Result := GrantPermission(SE_FILE_OBJECT, Filename, Entries, EntryCount,
    Inheritance);
end;

function GrantPermissionOnKey(const RootKey: HKEY; const Subkey: String;
  const Entries: TGrantPermissionEntry; const EntryCount: Integer): Boolean;
{ Grants the specified access to the specified registry key. Returns True if
  successful. Always fails on Windows 9x/Me and NT 4.0. }
const
  SE_REGISTRY_KEY = 4;
var
  ObjName: String;
begin
  case RootKey of
    HKEY_CLASSES_ROOT: ObjName := 'CLASSES_ROOT';
    HKEY_CURRENT_USER: ObjName := 'CURRENT_USER';
    HKEY_LOCAL_MACHINE: ObjName := 'MACHINE';
    HKEY_USERS: ObjName := 'USERS';
  else
    { Other root keys are not supported by Get/SetNamedSecurityInfo }
    Result := False;
    Exit;
  end;
  ObjName := ObjName + '\' + Subkey;
  Result := GrantPermission(SE_REGISTRY_KEY, ObjName, Entries, EntryCount,
    CONTAINER_INHERIT_ACE);
end;

{ TSimpleStringList }

procedure TSimpleStringList.Add(const S: String);
begin
  if FCount = FCapacity then
    SetCapacity(FCapacity + 8);
  FList^[FCount] := S;
  Inc(FCount);
end;

procedure TSimpleStringList.AddIfDoesntExist(const S: String);
begin
  if IndexOf(S) = -1 then
    Add(S);
end;

procedure TSimpleStringList.SetCapacity(NewCapacity: Integer);
begin
  ReallocMem(FList, NewCapacity * SizeOf(Pointer));
  if NewCapacity > FCapacity then
    FillChar(FList^[FCapacity], (NewCapacity - FCapacity) * SizeOf(Pointer), 0);
  FCapacity := NewCapacity;
end;

procedure TSimpleStringList.Clear;
begin
  if FCount <> 0 then Finalize(FList^[0], FCount);
  FCount := 0;
  SetCapacity(0);
end;

function TSimpleStringList.Get(Index: Integer): String;
begin
  Result := FList^[Index];
end;

function TSimpleStringList.IndexOf(const S: String): Integer;
{ Note: This is case-sensitive, unlike TStringList.IndexOf }
var
  I: Integer;
begin
  Result := -1;
  for I := 0 to FCount-1 do
    if FList^[I] = S then begin
      Result := I;
      Break;
    end;
end;

destructor TSimpleStringList.Destroy;
begin
  Clear;
  inherited Destroy;
end;

end.
