unit Struct;

//////// only version-independent types and constants
//////// a separate file is used to keep version-specific definitions off the project namespace

interface

uses Windows, MyTypes;

type
  TSetupLdrOffsetTable = TMySetupLdrOffsetTable;
  PSetupLdrOffsetTable = ^TSetupLdrOffsetTable;
  TSetupHeader = TMySetupHeader;
  PSetupHeader = ^TSetupHeader;
  TSetupFileEntry = TMySetupFileEntry;
  PSetupFileEntry = ^TSetupFileEntry;
  TSetupFileLocationEntry = TMySetupFileLocationEntry;
  PSetupFileLocationEntry = ^TSetupFileLocationEntry;
  TSetupRegistryEntry = TMySetupRegistryEntry;
  PSetupRegistryEntry = ^TSetupRegistryEntry;
  TSetupRunEntry = TMySetupRunEntry;
  PSetupRunEntry = ^TSetupRunEntry;
  TSetupIconEntry = TMySetupIconEntry;
  PSetupIconEntry = ^TSetupIconEntry;
  TSetupTaskEntry = TMySetupTaskEntry;
  PSetupTaskEntry = ^TSetupTaskEntry;
  TSetupComponentEntry = TMySetupComponentEntry;
  PSetupComponentEntry = ^TSetupComponentEntry;
  TSetupTypeEntry = TMySetupTypeEntry;
  PSetupTypeEntry = ^TMySetupTypeEntry;
  TSetupCustomMessageEntry = TMySetupCustomMessageEntry;
  PSetupCustomMessageEntry = ^TSetupCustomMessageEntry;
  TSetupLanguageEntry = TMySetupLanguageEntry;
  PSetupLanguageEntry = ^TMySetupLanguageEntry;
  TSetupDirEntry = TMySetupDirEntry;
  PSetupDirEntry = ^TMySetupDirEntry;
  TSetupIniEntry = TMySetupIniEntry;
  PSetupIniEntry = ^TMySetupIniEntry;
  TSetupDeleteEntry = TMySetupDeleteEntry;
  PSetupDeleteEntry = ^TMySetupDeleteEntry;

  TSetupFileOption = TMySetupFileOption;
  TSetupRegistryOption = TMySetupRegistryOption;
  TSetupDirOption = TMySetupDirOption;
  TSetupIniOption = TMySetupIniOption;
  TSetupRunOption = TMySetupRunOption;
  TSetupFileLocationFlag = TMySetupFileLocationFlag;

  TSetupProcessorArchitecture = TMySetupProcessorArchitecture;
  TSetupProcessorArchitectures = TMySetupProcessorArchitectures;
  TSetupDeleteType = TMySetupDeleteType;


  TSetupID = array[0..63] of Char;
//  TUninstallLogID = array[0..63] of Char;
  TMessagesHdrID = array[0..63] of Char;
  TUninstLangOptionsID = array[1..8] of Char;
  TCompID = array[1..4] of Char;
  TDiskSliceID = array[1..8] of Char;
const
  { SetupID is used by the Setup program to check if the SETUP.0 file is
    compatible with with it. If you make any modifications to the records in
    this file it's recommended you change SetupID. Any change will do (like
    changing the letters or numbers), as long as your format is
    unrecognizable by the standard Inno Setup. }
//  SetupID: TSetupID = 'Inno Setup Setup Data (4.1.5)';
//  UninstallLogID: TUninstallLogID = 'Inno Setup Uninstall Log (b)';
  MessagesHdrID: TMessagesHdrID = 'Inno Setup Messages (4.1.4)';
  UninstLangOptionsID: TUninstLangOptionsID = '!ulo!000';
  ZLIBID: TCompID = 'zlb'#26;
  DiskSliceID: TDiskSliceID = 'idska32'#26;
type
  TSetupVersionDataVersion = packed record
    Build: Word;
    Minor, Major: Byte;
  end;
  TSetupVersionData = packed record
    WinVersion, NTVersion: Cardinal;
    NTServicePack: Word;
  end;

  { A TDiskSliceHeader record follows DiskSliceID in a SETUP-*.BIN file }
  TDiskSliceHeader = packed record
    TotalSize: Cardinal;
  end;

  { A TMessageHeader record follows MessagesHdrID in a SETUP.MSG file }
  TMessagesHeader = packed record
    NumMessages: Cardinal;
    TotalSize: Cardinal;
    NotTotalSize: Cardinal;
    CRCMessages: Longint;
  end;


  { TUninstLangOptions is a simplified version of TSetupLangOptions that is
    used by the uninstaller }
  TUninstLangOptions = packed record
    ID: TUninstLangOptionsID;
    DialogFontName: String[31];
    DialogFontSize: Integer;
  end;

  { TGrantPermissionEntry is stored inside string fields named 'Permissions' }
  TGrantPermissionSid = record
    Authority: TSIDIdentifierAuthority;
    SubAuthCount: Byte;
    SubAuth: array[0..1] of DWORD;
  end;
  TGrantPermissionEntry = record
    Sid: TGrantPermissionSid;
    AccessMask: DWORD;
  end;

  TSetupLdrExeHeader = packed record
    ID: Longint;
    OffsetTableOffset, NotOffsetTableOffset: Longint;
  end;

  TSetupLdrOffsetTable4010 = packed record // valid since v4.0.10
    ID: array[1..12] of Char;
    TotalSize,
    OffsetEXE, CompressedSizeEXE, UncompressedSizeEXE, CRCEXE,
    Offset0, Offset1: Longint;
    TableCRC: Longint;  { CRC of all prior fields in this record }
  end;

//  TSetupLdrOffsetTable = TSetupLdrOffsetTable4010;
// since there is no mechanism that chooses the right version automatically,
// it's better to know which structure you use

  TSetupLdrOffsetTable4000 = packed record // in vv.4.0.0-4.0.2 field CRCEXE is named AdlerEXE
    ID: array[1..12] of Char;
    TotalSize,
    OffsetEXE, CompressedSizeEXE, UncompressedSizeEXE, CRCEXE,
    Offset0, Offset1: Longint;
  end;

const
  SetupLdrExeHeaderOffset = $30;
  SetupLdrExeHeaderID = $6F6E6E49;
  SetupLdrOffsetTableResID = 11111;
{  SetupLdrOffsetTableID4010 = 'rDlPtS06'#$87#$65#$56#$78;
  SetupLdrOffsetTableID4003 = 'rDlPtS05'#$87#$65#$56#$78;
  SetupLdrOffsetTableID4000 = 'rDlPtS04'#$87#$65#$56#$78;
}
  UninstallerMsgTailID = $67734D49;

implementation

end.
