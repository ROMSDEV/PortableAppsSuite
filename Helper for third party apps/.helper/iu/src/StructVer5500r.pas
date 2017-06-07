unit StructVer5500r;

interface
uses
  MyTypes, Int64Em, struct5500r;

{$DEFINE ARCHr}
// If unit for non-unicode version then previous define will be just ARCH
// so UNICODE will not be defined 
{$IF Defined(ARCHu) or Defined(ARCHur)}
  {$DEFINE UNICODE}
{$IFEND}

{$DEFINE EXTr}
// Same trick as with Unicode but for RT versions
{$IF Defined(EXTr) or Defined(EXTur)}
  {$DEFINE RTVER}
{$IFEND}

const UNI_FIRST = 5205;  

type
  TAnInnoVer = class(TInnoVer)
  public
    constructor Create; override;
    procedure SetupSizes; override;
    procedure UnifySetupLdrOffsetTable(const p; var ot:TMySetupLdrOffsetTable); override;
    procedure UnifySetupHeader(const p; var sh:TMySetupHeader); override;
    procedure UnifyFileEntry(const p; var fe:TMySetupFileEntry); override;
    procedure UnifyFileLocationEntry(const p; var fl:TMySetupFileLocationEntry); override;
    procedure UnifyRegistryEntry(const p; var re:TMySetupRegistryEntry); override;
    procedure UnifyRunEntry(const p; var re:TMySetupRunEntry); override;
    procedure UnifyIconEntry(const p; var ie:TMySetupIconEntry); override;
    procedure UnifyTaskEntry(const p; var te:TMySetupTaskEntry); override;
    procedure UnifyComponentEntry(const p; var ce:TMySetupComponentEntry); override;
    procedure UnifyTypeEntry(const p; var te:TMySetupTypeEntry); override;
    procedure UnifyCustomMessageEntry(const p; var ce:TMySetupCustomMessageEntry); override;
    procedure UnifyLanguageEntry(const p; var le:TMySetupLanguageEntry); override;
    procedure UnifyDirEntry(const p; var de: TMySetupDirEntry); override;
    procedure UnifyIniEntry(const p; var ie: TMySetupIniEntry); override;
    procedure UnifyDeleteEntry(const p; var de: TMySetupDeleteEntry); override;
  end;

implementation

const
  SetupFileOptionTable: array [0..MySetupFileOptionLast] of byte = (
  ord(struct5500r.foConfirmOverwrite)                                           ,  {foConfirmOverwrite        }
  ord(struct5500r.foUninsNeverUninstall)                                        ,  {foUninsNeverUninstall     }
  ord(struct5500r.foRestartReplace)                                             ,  {foRestartReplace          }
  ord(struct5500r.foDeleteAfterInstall)                                         ,  {foDeleteAfterInstall      }
  ord(struct5500r.foRegisterServer)                                             ,  {foRegisterServer          }
  ord(struct5500r.foRegisterTypeLib)                                            ,  {foRegisterTypeLib         }
  ord(struct5500r.foSharedFile)                                                 ,  {foSharedFile              }
  {$IF 5500>=3005}
  ord(struct5500r.foCompareTimeStamp)                                           ,  {foCompareTimeStamp        }
  {$ELSE}
  ord(struct5500r.foCompareTimeStampAlso)                                       ,  {foCompareTimeStamp        }
  {$IFEND}
  ord(struct5500r.foFontIsntTrueType)                                           ,  {foFontIsntTrueType        }
  ord(struct5500r.foSkipIfSourceDoesntExist)                                    ,  {foSkipIfSourceDoesntExist }
  ord(struct5500r.foOverwriteReadOnly)                                          ,  {foOverwriteReadOnly       }
  ord(struct5500r.foOverwriteSameVersion)                                       ,  {foOverwriteSameVersion    }
  ord(struct5500r.foCustomDestName)                                             ,  {foCustomDestName          }
  {$IF 5500>=1325}ord(struct5500r.foOnlyIfDestFileExists)    {$ELSE}255{$IFEND} ,  {foOnlyIfDestFileExists    }
  {$IF 5500>=2001}ord(struct5500r.foNoRegError)              {$ELSE}255{$IFEND} ,  {foNoRegError              }
  {$IF 5500>=3001}ord(struct5500r.foUninsRestartDelete)      {$ELSE}255{$IFEND} ,  {foUninsRestartDelete      }
  {$IF 5500>=3005}ord(struct5500r.foOnlyIfDoesntExist)       {$ELSE}255{$IFEND} ,  {foOnlyIfDoesntExist       }
  {$IF 5500>=3005}ord(struct5500r.foIgnoreVersion)           {$ELSE}255{$IFEND} ,  {foIgnoreVersion           }
  {$IF 5500>=3005}ord(struct5500r.foPromptIfOlder)           {$ELSE}255{$IFEND} ,  {foPromptIfOlder           }
  {$IF 5500>=4000}ord(struct5500r.foDontCopy)                {$ELSE}255{$IFEND} ,  {foDontCopy                }
  {$IF 5500>=4005}ord(struct5500r.foUninsRemoveReadOnly)     {$ELSE}255{$IFEND} ,  {foUninsRemoveReadOnly     }
  {$IF 5500>=4108}ord(struct5500r.foRecurseSubDirsExternal)  {$ELSE}255{$IFEND} ,  {foRecurseSubDirsExternal  }
  {$IF 5500>=4201}ord(struct5500r.foReplaceSameVersionIfContentsDiffer){$ELSE}255{$IFEND} ,
  {$IF 5500>=4205}ord(struct5500r.foDontVerifyChecksum)      {$ELSE}255{$IFEND} ,
  {$IF 5500>=5003}ord(struct5500r.foUninsNoSharedFilePrompt) {$ELSE}255{$IFEND} ,
  {$IF 5500>=5100}ord(struct5500r.foCreateAllSubDirs)        {$ELSE}255{$IFEND} ,
  {$IF 5500>=5102}ord(struct5500r.fo32bit)                   {$ELSE}255{$IFEND} ,  {fo32bit                   }
  {$IF 5500>=5102}ord(struct5500r.fo64bit)                   {$ELSE}255{$IFEND} ,  {fo64bit                   }
  {$IF 5500>=5200}ord(struct5500r.foExternalSizePreset)      {$ELSE}255{$IFEND} ,  {foExternalSizePreset      }
  {$IF 5500>=5200}ord(struct5500r.foSetNTFSCompression)      {$ELSE}255{$IFEND} ,  {foSetNTFSCompression      }
  {$IF 5500>=5200}ord(struct5500r.foUnsetNTFSCompression)    {$ELSE}255{$IFEND} ,  {foUnsetNTFSCompression    }
  {$IF 5500>=5300}ord(struct5500r.foGacInstall)              {$ELSE}255{$IFEND}    {foGacInstall              }
);

  SetupFileLocationFlagTable: array[0..MySetupFileLocationFlagLast] of byte = (
    ord(struct5500r.foVersionInfoValid)                                              ,
    ord(struct5500r.foVersionInfoNotValid)                                           ,
    {$IF 5500>=4010}ord(struct5500r.foTimeStampInUTC)           {$ELSE}255{$IFEND} ,
    {$IF 5500>=4100}ord(struct5500r.foIsUninstExe)              {$ELSE}255{$IFEND} ,
    {$IF 5500>=4108}ord(struct5500r.foCallInstructionOptimized) {$ELSE}255{$IFEND} ,
    {$IF (5500>=4200) AND (5500<5507)}ord(struct5500r.foTouch){$ELSE}255{$IFEND} ,
    {$IF 5500>=4202}ord(struct5500r.foChunkEncrypted)           {$ELSE}255{$IFEND} ,
    {$IF 5500>=4205}ord(struct5500r.foChunkCompressed)          {$ELSE}255{$IFEND} ,
    {$IF 5500>=5113}ord(struct5500r.foSolidBreak)               {$ELSE}255{$IFEND}
  );

  SetupRegistryOptionTable: array[0..MySetupRegistryOptionLast] of byte = (
  ord(struct5500r.roCreateValueIfDoesntExist)                                   ,  {roCreateValueIfDoesntExist    }
  ord(struct5500r.roUninsDeleteValue)                                           ,  {roUninsDeleteValue            }
  ord(struct5500r.roUninsClearValue)                                            ,  {roUninsClearValue             }
  ord(struct5500r.roUninsDeleteEntireKey)                                       ,  {roUninsDeleteEntireKey        }
  ord(struct5500r.roUninsDeleteEntireKeyIfEmpty)                                ,  {roUninsDeleteEntireKeyIfEmpty }
  ord(struct5500r.roPreserveStringType)                                         ,  {roPreserveStringType          }
  ord(struct5500r.roDeleteKey)                                                  ,  {roDeleteKey                   }
  ord(struct5500r.roDeleteValue)                                                ,  {roDeleteValue                 }
  ord(struct5500r.roNoError)                                                    ,  {roNoError                     }
  ord(struct5500r.roDontCreateKey)                                              ,  {roDontCreateKey               }
  {$IF 5500>=5100}ord(struct5500r.ro32bit)                 {$ELSE}255{$IFEND} ,  {ro32bit                       }
  {$IF 5500>=5100}ord(struct5500r.ro64bit)                 {$ELSE}255{$IFEND}    {ro64bit                       }
);

  SetupDirOptionTable: array[0..MySetupDirOptionLast] of byte = (
  ord(struct5500r.doUninsNeverUninstall)                                          ,  {doUninsNeverUninstall   }
  ord(struct5500r.doDeleteAfterInstall)                                           ,  {doDeleteAfterInstall    }
  ord(struct5500r.doUninsAlwaysUninstall)                                         ,  {doUninsAlwaysUninstall  }
  {$IF 5500>=5200}ord(struct5500r.doSetNTFSCompression)      {$ELSE}255{$IFEND} ,  {doSetNTFSCompression    }
  {$IF 5500>=5200}ord(struct5500r.doUnsetNTFSCompression)    {$ELSE}255{$IFEND}    {doUnsetNTFSCompression  }
);

  SetupIniOptionTable: array[0..MySetupIniOptionLast] of byte = (
  ord(struct5500r.ioCreateKeyIfDoesntExist)                       ,  {ioCreateKeyIfDoesntExist     }
  ord(struct5500r.ioUninsDeleteEntry)                             ,  {ioUninsDeleteEntry           }
  ord(struct5500r.ioUninsDeleteEntireSection)                     ,  {ioUninsDeleteEntireSection   }
  ord(struct5500r.ioUninsDeleteSectionIfEmpty)                    ,  {ioUninsDeleteSectionIfEmpty  }
  ord(struct5500r.ioHasValue)                                        {ioHasValue                   }
);

  SetupHeaderOptionTable: array[0..MySetupHeaderOptionLast] of byte = (
    ord(struct5500r.shDisableStartupPrompt),
    {$IF 5500<5310}ord(struct5500r.shUninstallable)             {$ELSE}255{$IFEND},
    ord(struct5500r.shCreateAppDir),
    ord(struct5500r.shAllowNoIcons),
    {$IF (5500>=3003) AND (5500 < 3000)}ord(struct5500r.shAlwaysRestart) {$ELSE}255{$IFEND},
    ord(struct5500r.shAlwaysUsePersonalGroup),
    ord(struct5500r.shWindowVisible),
    ord(struct5500r.shWindowShowCaption),
    ord(struct5500r.shWindowResizable),
    ord(struct5500r.shWindowStartMaximized),
    ord(struct5500r.shEnableDirDoesntExistWarning),
    ord(struct5500r.shPassword),
    ord(struct5500r.shAllowRootDirectory),
    ord(struct5500r.shDisableFinishedPage),
    ord(struct5500r.shChangesAssociations),
    ord(struct5500r.shUsePreviousAppDir),
    ord(struct5500r.shBackColorHorizontal),
    ord(struct5500r.shUsePreviousGroup),
    ord(struct5500r.shUpdateUninstallLogAppName),
    {$IF 5500>=2001}ord(struct5500r.shUsePreviousSetupType)      {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shDisableReadyMemo)          {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shAlwaysShowComponentsList)  {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shFlatComponentsList)        {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shShowComponentSizes)        {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shUsePreviousTasks)          {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shDisableReadyPage)          {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shAlwaysShowDirOnReadyPage)  {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.shAlwaysShowGroupOnReadyPage){$ELSE}255{$IFEND},
    {$IF 5500>=2018}ord(struct5500r.shAllowUNCPath)              {$ELSE}255{$IFEND},
    {$IF 5500>=3000}ord(struct5500r.shUserInfoPage)              {$ELSE}255{$IFEND},
    {$IF 5500>=3000}ord(struct5500r.shUsePreviousUserInfo)       {$ELSE}255{$IFEND},
    {$IF 5500>=3001}ord(struct5500r.shUninstallRestartComputer)  {$ELSE}255{$IFEND},
    {$IF 5500>=3003}ord(struct5500r.shRestartIfNeededByRun)      {$ELSE}255{$IFEND},
    {$IF 5500>=3008}ord(struct5500r.shShowTasksTreeLines)        {$ELSE}255{$IFEND},
    {$IF 5500>=4009}ord(struct5500r.shAllowCancelDuringInstall)  {$ELSE}255{$IFEND},
    {$IF 5500>=4103}ord(struct5500r.shWizardImageStretch)        {$ELSE}255{$IFEND},
    {$IF 5500>=4108}ord(struct5500r.shAppendDefaultDirName)      {$ELSE}255{$IFEND},
    {$IF 5500>=4108}ord(struct5500r.shAppendDefaultGroupName)    {$ELSE}255{$IFEND},
    {$IF 5500>=4202}ord(struct5500r.shEncryptionUsed)            {$ELSE}255{$IFEND},
    {$IF 5500>=5004}ord(struct5500r.shChangesEnvironment)        {$ELSE}255{$IFEND},
    {$IF (5500>=5107) AND (NOT DEFINED(UNICODE))}
      ord(struct5500r.shShowUndisplayableLanguages)
    {$ELSE}255{$IFEND},
    {$IF 5500>=5113}ord(struct5500r.shSetupLogging)             {$ELSE}255{$IFEND},
    {$IF 5500>=5201}ord(struct5500r.shSignedUninstaller)        {$ELSE}255{$IFEND},
    {$IF 5500>=5308}ord(struct5500r.shUsePreviousLanguage)      {$ELSE}255{$IFEND},
    {$IF 5500>=5309}ord(struct5500r.shDisableWelcomePage)       {$ELSE}255{$IFEND},
    {$IF 5500>=5500}ord(struct5500r.shCloseApplications)       {$ELSE}255{$IFEND},
    {$IF 5500>=5500}ord(struct5500r.shRestartApplications)       {$ELSE}255{$IFEND},
    {$IF 5500>=5500}ord(struct5500r.shAllowNetworkDrive)       {$ELSE}255{$IFEND}
  );

  SetupRunOptionTable: array[0..MySetupRunOptionLast] of byte = (
    ord(struct5500r.roShellExec),
    ord(struct5500r.roSkipIfDoesntExist),
    {$IF 5500>=2001}ord(struct5500r.roPostInstall)         {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.roUnchecked)           {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.roSkipIfSilent)        {$ELSE}255{$IFEND},
    {$IF 5500>=2001}ord(struct5500r.roSkipIfNotSilent)     {$ELSE}255{$IFEND},
    {$IF 5500>=2008}ord(struct5500r.roHideWizard)          {$ELSE}255{$IFEND},
    {$IF 5500>=5110}ord(struct5500r.roRun32Bit)            {$ELSE}255{$IFEND},
    {$IF 5500>=5110}ord(struct5500r.roRun64Bit)            {$ELSE}255{$IFEND},
    {$IF 5500>=5200}ord(struct5500r.roRunAsOriginalUser)   {$ELSE}255{$IFEND}
  );

constructor TAnInnoVer.Create;
begin
  VerSupported:=5500;
  IsUnicode:={$IFDEF UNICODE}true{$ELSE}false{$ENDIF};
  IsRT:={$IFDEF RTVER}true{$ELSE}false{$ENDIF};
  SetupID:=SetupLdrOffsetTableId;
  OfsTabSize:=sizeof(TSetupLdrOffsetTable);
end;

procedure TAnInnoVer.SetupSizes;
begin
  MyTypes.SetupHeaderSize:=sizeof(TSetupHeader);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupHeaderStrings:=SetupHeaderStrings;
    MyTypes.SetupHeaderAnsiStrings:=SetupHeaderAnsiStrings;
    {$ELSE}
    MyTypes.SetupHeaderStrings:=0;
    MyTypes.SetupHeaderAnsiStrings:=SetupHeaderStrings+SetupHeaderAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupHeaderStrings:=0;
  MyTypes.SetupHeaderAnsiStrings:=SetupHeaderStrings;
  {$IFEND}

  {$IF 5500>=4000}
  MyTypes.SetupLanguageEntrySize:=sizeof(TSetupLanguageEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupLanguageEntryStrings:=SetupLanguageEntryStrings;
    MyTypes.SetupLanguageEntryAnsiStrings:=SetupLanguageEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupLanguageEntryStrings:=0;
    MyTypes.SetupLanguageEntryAnsiStrings:=SetupLanguageEntryStrings+SetupLanguageEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupLanguageEntryStrings:=0;
  MyTypes.SetupLanguageEntryAnsiStrings:=SetupLanguageEntryStrings;
  {$IFEND}
  {$ELSEIF 5500 >= 2001}
  MyTypes.SetupLanguageEntrySize:=sizeof(TSetupLangOptions);
  MyTypes.SetupLanguageEntryStrings:=0;
  MyTypes.SetupLanguageEntryAnsiStrings:=SetupLangOptionsStrings;
  {$ELSE}
  MyTypes.SetupLanguageEntrySize:=0;
  MyTypes.SetupLanguageEntryStrings:=0;
  MyTypes.SetupLanguageEntryAnsiStrings:=0;
  {$IFEND}

  {$IF 5500>=4201}
  MyTypes.SetupCustomMessageEntrySize:=sizeof(TSetupCustomMessageEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupCustomMessageEntryStrings:=SetupCustomMessageEntryStrings;
    MyTypes.SetupCustomMessageEntryAnsiStrings:=SetupCustomMessageEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupCustomMessageEntryStrings:=0;
    MyTypes.SetupCustomMessageEntryAnsiStrings:=SetupCustomMessageEntryStrings+SetupCustomMessageEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupCustomMessageEntryStrings:=0;
  MyTypes.SetupCustomMessageEntryAnsiStrings:=SetupCustomMessageEntryStrings;
  {$IFEND}
  {$ELSE}
  MyTypes.SetupCustomMessageEntrySize:=0;
  MyTypes.SetupCustomMessageEntryStrings:=0;
  MyTypes.SetupCustomMessageEntryAnsiStrings:=0;
  {$IFEND}

  {$IF 5500>=4100}
  MyTypes.SetupPermissionEntrySize:=sizeof(TSetupPermissionEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupPermissionEntryStrings:=SetupPermissionEntryStrings;
    MyTypes.SetupPermissionEntryAnsiStrings:=SetupPermissionEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupPermissionEntryStrings:=0;
    MyTypes.SetupPermissionEntryAnsiStrings:=SetupPermissionEntryStrings+SetupPermissionEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupPermissionEntryStrings:=0;
  MyTypes.SetupPermissionEntryAnsiStrings:=SetupPermissionEntryStrings;
  {$IFEND}
  {$ELSE}
  MyTypes.SetupPermissionEntrySize:=0;
  MyTypes.SetupPermissionEntryStrings:=0;
  MyTypes.SetupPermissionEntryAnsiStrings:=0;
  {$IFEND}

{$IF 5500 >= 2001}
  MyTypes.SetupTypeEntrySize:=sizeof(TSetupTypeEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
      MyTypes.SetupTypeEntryStrings:=SetupTypeEntryStrings;
      MyTypes.SetupTypeEntryAnsiStrings:=SetupTypeEntryAnsiStrings;
    {$ELSE}
      MyTypes.SetupTypeEntryStrings:=0;
      MyTypes.SetupTypeEntryAnsiStrings:=SetupTypeEntryStrings+SetupTypeEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
    MyTypes.SetupTypeEntryStrings:=0;
    MyTypes.SetupTypeEntryAnsiStrings:=SetupTypeEntryStrings;
  {$IFEND}
{$ELSE}
  MyTypes.SetupTypeEntrySize:=0;
  MyTypes.SetupTypeEntryStrings:=0;
  MyTypes.SetupTypeEntryAnsiStrings:=0;
{$IFEND}

{$IF 5500 >= 2001}
  MyTypes.SetupComponentEntrySize:=sizeof(TSetupComponentEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
      MyTypes.SetupComponentEntryStrings:=SetupComponentEntryStrings;
      MyTypes.SetupComponentEntryAnsiStrings:=SetupComponentEntryAnsiStrings;
    {$ELSE}
      MyTypes.SetupComponentEntryStrings:=0;
      MyTypes.SetupComponentEntryAnsiStrings:=SetupComponentEntryStrings+SetupComponentEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
    MyTypes.SetupComponentEntryStrings:=0;
    MyTypes.SetupComponentEntryAnsiStrings:=SetupComponentEntryStrings;
  {$IFEND}
{$ELSE}
  MyTypes.SetupComponentEntrySize:=0;
  MyTypes.SetupComponentEntryStrings:=0;
  MyTypes.SetupComponentEntryAnsiStrings:=0;
{$IFEND}

{$IF 5500 >= 2001}
  MyTypes.SetupTaskEntrySize:=sizeof(TSetupTaskEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
      MyTypes.SetupTaskEntryStrings:=SetupTaskEntryStrings;
      MyTypes.SetupTaskEntryAnsiStrings:=SetupTaskEntryAnsiStrings;
    {$ELSE}
      MyTypes.SetupTaskEntryStrings:=0;
      MyTypes.SetupTaskEntryAnsiStrings:=SetupTaskEntryStrings+SetupTaskEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
    MyTypes.SetupTaskEntryStrings:=0;
    MyTypes.SetupTaskEntryAnsiStrings:=SetupTaskEntryStrings;
  {$IFEND}
{$ELSE}
  MyTypes.SetupTaskEntrySize:=0;
  MyTypes.SetupTaskEntryStrings:=0;
  MyTypes.SetupTaskEntryAnsiStrings:=0;
{$IFEND}

  MyTypes.SetupDirEntrySize:=sizeof(TSetupDirEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupDirEntryStrings:=SetupDirEntryStrings;
    MyTypes.SetupDirEntryAnsiStrings:=SetupDirEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupDirEntryStrings:=0;
    MyTypes.SetupDirEntryAnsiStrings:=SetupDirEntryStrings+SetupDirEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupDirEntryStrings:=0;
  MyTypes.SetupDirEntryAnsiStrings:=SetupDirEntryStrings;
  {$IFEND}

  MyTypes.SetupFileEntrySize:=sizeof(TSetupFileEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupFileEntryStrings:=SetupFileEntryStrings;
    MyTypes.SetupFileEntryAnsiStrings:=SetupFileEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupFileEntryStrings:=0;
    MyTypes.SetupFileEntryAnsiStrings:=SetupFileEntryStrings+SetupFileEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupFileEntryStrings:=0;
  MyTypes.SetupFileEntryAnsiStrings:=SetupFileEntryStrings;
  {$IFEND}

  MyTypes.SetupIconEntrySize:=sizeof(TSetupIconEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupIconEntryStrings:=SetupIconEntryStrings;
    MyTypes.SetupIconEntryAnsiStrings:=SetupIconEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupIconEntryStrings:=0;
    MyTypes.SetupIconEntryAnsiStrings:=SetupIconEntryStrings+SetupIconEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupIconEntryStrings:=0;
  MyTypes.SetupIconEntryAnsiStrings:=SetupIconEntryStrings;
  {$IFEND}

  MyTypes.SetupIniEntrySize:=sizeof(TSetupIniEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupIniEntryStrings:=SetupIniEntryStrings;
    MyTypes.SetupIniEntryAnsiStrings:=SetupIniEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupIniEntryStrings:=0;
    MyTypes.SetupIniEntryAnsiStrings:=SetupIniEntryStrings+SetupIniEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupIniEntryStrings:=0;
  MyTypes.SetupIniEntryAnsiStrings:=SetupIniEntryStrings;
  {$IFEND}

  MyTypes.SetupRegistryEntrySize:=sizeof(TSetupRegistryEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupRegistryEntryStrings:=SetupRegistryEntryStrings;
    MyTypes.SetupRegistryEntryAnsiStrings:=SetupRegistryEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupRegistryEntryStrings:=0;
    MyTypes.SetupRegistryEntryAnsiStrings:=SetupRegistryEntryStrings+SetupRegistryEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupRegistryEntryStrings:=0;
  MyTypes.SetupRegistryEntryAnsiStrings:=SetupRegistryEntryStrings;
  {$IFEND}

  MyTypes.SetupDeleteEntrySize:=sizeof(TSetupDeleteEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupDeleteEntryStrings:=SetupDeleteEntryStrings;
    MyTypes.SetupDeleteEntryAnsiStrings:=SetupDeleteEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupDeleteEntryStrings:=0;
    MyTypes.SetupDeleteEntryAnsiStrings:=SetupDeleteEntryStrings+SetupDeleteEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupDeleteEntryStrings:=0;
  MyTypes.SetupDeleteEntryAnsiStrings:=SetupDeleteEntryStrings;
  {$IFEND}

  MyTypes.SetupRunEntrySize:=sizeof(TSetupRunEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupRunEntryStrings:=SetupRunEntryStrings;
    MyTypes.SetupRunEntryAnsiStrings:=SetupRunEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupRunEntryStrings:=0;
    MyTypes.SetupRunEntryAnsiStrings:=SetupRunEntryStrings+SetupRunEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupRunEntryStrings:=0;
  MyTypes.SetupRunEntryAnsiStrings:=SetupRunEntryStrings;
  {$IFEND}

  MyTypes.SetupFileLocationEntrySize:=sizeof(TSetupFileLocationEntry);
  {$IF 5500>=UNI_FIRST}
    {$IFDEF UNICODE}
    MyTypes.SetupFileLocationEntryStrings:=SetupFileLocationEntryStrings;
    MyTypes.SetupFileLocationEntryAnsiStrings:=SetupFileLocationEntryAnsiStrings;
    {$ELSE}
    MyTypes.SetupFileLocationEntryStrings:=0;
    MyTypes.SetupFileLocationEntryAnsiStrings:=SetupFileLocationEntryStrings+SetupFileLocationEntryAnsiStrings;
    {$ENDIF}
  {$ELSE}
  MyTypes.SetupFileLocationEntryStrings:=0;
  MyTypes.SetupFileLocationEntryAnsiStrings:=SetupFileLocationEntryStrings;
  {$IFEND}
end;

// the following procedures extract information from important
// version-specific data structures. they are applied to whatever
// data is read from the installation file. all the other extraction
// code operates on the produced unified version-independent structures

procedure TAnInnoVer.UnifySetupLdrOffsetTable(const p; var ot:TMySetupLdrOffsetTable);
var
  oot:TSetupLdrOffsetTable absolute p;
begin
  with ot do begin
    ID                          :=oot.ID;
    TotalSize                   :=oot.TotalSize;
    OffsetEXE                   :=oot.OffsetEXE;
//    {$IF 5500<=4106} // since the main installer exe is not unpacked, there's no need for its size
//    CompressedSizeEXE           :=oot.CompressedSizeEXE;
//    {$IFEND}
    UncompressedSizeEXE         :=oot.UncompressedSizeEXE;
    {$IF 5500>=4003}
      CRCEXE                    :=oot.CRCEXE;
    {$ELSE}
      CRCEXE                    :=oot.AdlerEXE;
    {$IFEND}
    Offset0                     :=oot.Offset0;
    Offset1                     :=oot.Offset1;
    {$IF 5500>=4010}
      TableCRC                  :=oot.TableCRC;
      TableCRCUsed:=true;
    {$ELSE}
      TableCRCUsed:=false;
    {$IFEND}
  end;
end;

procedure TAnInnoVer.UnifySetupHeader(const p; var sh:TMySetupHeader);
var
  osh:TSetupHeader absolute p;
begin
  with sh do begin
    AppName                     :=NormalizeStringVal(osh.AppName);
    AppVerName                  :=NormalizeStringVal(osh.AppVerName);
    AppId                       :=osh.AppId;
    AppCopyright                :=osh.AppCopyright;
    AppPublisher                :=osh.AppPublisher;
    AppPublisherURL             :=osh.AppPublisherURL;
{$IF 5500>=5113}
    AppSupportPhone             :=osh.AppSupportPhone;
{$ELSE}
    AppSupportPhone             :='';
{$IFEND}    
    AppSupportURL               :=osh.AppSupportURL;
    AppUpdatesURL               :=osh.AppUpdatesURL;
    AppVersion                  :=osh.AppVersion;
    DefaultDirName              :=osh.DefaultDirName;
    DefaultGroupName            :=osh.DefaultGroupName;
    BaseFilename                :=osh.BaseFilename;
{$IF 5500>=4000}
    NumLanguageEntries          :=osh.NumLanguageEntries;
    CompiledCodeText            :=osh.CompiledCodeText;
{$ELSE}
    NumLanguageEntries          :=0;
    CompiledCodeText            :='';
{$IFEND}
    LicenseText                 :=osh.LicenseText;
    InfoBeforeText              :=osh.InfoBeforeText;
    InfoAfterText               :=osh.InfoAfterText;
{$IF 5500>=4201}
    NumCustomMessageEntries     :=osh.NumCustomMessageEntries;
{$ELSE}
    NumCustomMessageEntries     :=0;
{$IFEND}
{$IF 5500>=4100}
    NumPermissionEntries        :=osh.NumPermissionEntries;
{$ELSE}
    NumPermissionEntries        :=0;
{$IFEND}
{$IF 5500>=2001}
    NumTypeEntries              :=osh.NumTypeEntries;
    NumComponentEntries         :=osh.NumComponentEntries;
    NumTaskEntries              :=osh.NumTaskEntries;
{$ELSE}
    NumTypeEntries              :=0;
    NumComponentEntries         :=0;
    NumTaskEntries              :=0;
{$IFEND}
    NumDirEntries               :=osh.NumDirEntries;
    NumFileEntries              :=osh.NumFileEntries;
    NumFileLocationEntries      :=osh.NumFileLocationEntries;
    NumIconEntries              :=osh.NumIconEntries;
    NumIniEntries               :=osh.NumIniEntries;
    NumRegistryEntries          :=osh.NumRegistryEntries;
    NumInstallDeleteEntries     :=osh.NumInstallDeleteEntries;
    NumUninstallDeleteEntries   :=osh.NumUninstallDeleteEntries;
    NumRunEntries               :=osh.NumRunEntries;
    NumUninstallRunEntries      :=osh.NumUninstallRunEntries;
    MinVersion                  :=TMySetupVersionData(osh.MinVersion);
    OnlyBelowVersion            :=TMySetupVersionData(osh.OnlyBelowVersion);
    ExtraDiskSpaceRequired      :=Integer64(Int64(osh.ExtraDiskSpaceRequired));
{$IF 5500>=4000}
    SlicesPerDisk               :=osh.SlicesPerDisk;
{$ELSE}
    SlicesPerDisk               :=1;
{$IFEND}
{$IF 5500>=4105}
    case osh.CompressMethod of
      {$IF 5500>=4205}
      cmStored:                 CompressMethod:=MyTypes.cmStored;
      {$IFEND}
      {$IF 5500<>4205}
      cmZip:                    CompressMethod:=MyTypes.cmZip;
      {$IFEND}
      cmBzip:                   CompressMethod:=MyTypes.cmBzip;
      cmLZMA:                   CompressMethod:=MyTypes.cmLZMA;
	    {$IF 5500>=5309}
	    cmLZMA2:                  CompressMethod:=MyTypes.cmLZMA2;
	    {$IFEND}
    end;
{$ELSEIF 5500 >= 2017}
    if shBzipUsed in osh.Options then CompressMethod:=MyTypes.cmBzip
    else CompressMethod:=MyTypes.cmZip;
{$ELSE}
    CompressMethod:=MyTypes.cmZip;
{$IFEND}
{$IF 5500>=4202}
    EncryptionUsed := shEncryptionUsed in osh.Options;
    {$IF 5500>=5309}
    PasswordHash.HashType := htSHA1;
    Move(osh.PasswordHash, PasswordHash.SHA1, sizeof(PasswordHash.SHA1));
    {$ELSE}
    PasswordHash.HashType := htMD5;
    Move(osh.PasswordHash, PasswordHash.MD5, sizeof(PasswordHash.MD5));
    {$IFEND}
    Move(osh.PasswordSalt, PasswordSalt, sizeof(PasswordSalt));
{$ELSE}
    EncryptionUsed := False;
    FillChar(PasswordSalt, SizeOf(PasswordSalt), 0);
{$IFEND}
{$IF 5500>=5100}
    Move(osh.ArchitecturesAllowed, ArchitecturesAllowed, sizeof(ArchitecturesAllowed));
    Move(osh.ArchitecturesInstallIn64BitMode, ArchitecturesInstallIn64BitMode, sizeof(ArchitecturesInstallIn64BitMode));
{$ELSE}
    ArchitecturesAllowed:=[];
    ArchitecturesInstallIn64BitMode:=[];
{$IFEND}
{$IF 5500>=3004}
    case osh.PrivilegesRequired of
      prNone:       PrivilegesRequired := MyTypes.prNone;
      prPowerUser:  PrivilegesRequired := MyTypes.prPowerUser;
      prAdmin:      PrivilegesRequired := MyTypes.prAdmin;
      {$IF 5500>=5307}
      prLowest:     PrivilegesRequired := MyTypes.prLowest;
      {$IFEND}
    end;
{$ELSE}
    if (shAdminPrivilegesRequired in osh.Options) then
      PrivilegesRequired := MyTypes.prAdmin
    else
      PrivilegesRequired := MyTypes.prNone;
{$IFEND}
    UninstallFilesDir     := osh.UninstallFilesDir;
    UninstallDisplayName  := osh.UninstallDisplayName;
    UninstallDisplayIcon  := osh.UninstallDisplayIcon;
    AppMutex              := osh.AppMutex;
{$IF 5500>=4204}
    AppReadmeFile         := osh.AppReadmeFile;
    AppContact            := osh.AppContact;
    AppComments           := NormalizeStringVal(osh.AppComments);
    AppModifyPath         := osh.AppModifyPath;
{$ELSE}
    AppReadmeFile         := '';
    AppContact            := '';
    AppComments           := '';
    AppModifyPath         := '';
{$IFEND}
{$IF 5500>=5308}
    CreateUninstallRegKey := osh.CreateUninstallRegKey;
{$ELSE}
    if (shCreateUninstallRegKey in osh.Options) then
      CreateUninstallRegKey := 'yes'
    else
      CreateUninstallRegKey := 'no';
{$IFEND}
{$IF 5500>=5310}
    Uninstallable := osh.Uninstallable;
{$ELSE}
    Uninstallable := '';
{$IFEND}
{$IF 5500>=5500}
    CloseApplicationsFilter := osh.CloseApplicationsFilter;
{$ELSE}
    CloseApplicationsFilter := '';
{$IFEND}
{$IF 5500>=5506}
    SetupMutex := osh.SetupMutex;
{$ELSE}
    SetupMutex := '';
{$IFEND}
{$IF 5500>=5303}
    case osh.DisableDirPage of
      dpAuto: DisableDirPage := MyTypes.dpAuto;
      dpNo:   DisableDirPage := MyTypes.dpNo;
      dpYes:  DisableDirPage := MyTypes.dpYes;
    end;
    case osh.DisableProgramGroupPage of
      dpAuto: DisableProgramGroupPage := MyTypes.dpAuto;
      dpNo:   DisableProgramGroupPage := MyTypes.dpNo;
      dpYes:  DisableProgramGroupPage := MyTypes.dpYes;
    end;
{$ELSE}
    if (shDisableDirPage in osh.Options) then
      DisableDirPage := MyTypes.dpYes
    else
      DisableDirPage := MyTypes.dpNo;
    if (shDisableProgramGroupPage in osh.Options) then
      DisableProgramGroupPage := MyTypes.dpYes
    else
      DisableProgramGroupPage := MyTypes.dpNo;
{$IFEND}
{$IF 5500>=4010}
    case osh.LanguageDetectionMethod of
      ldUILanguage: LanguageDetectionMethod := MyTypes.ldUILanguage;
      ldLocale:     LanguageDetectionMethod := MyTypes.ldLocale;
      ldNone:       LanguageDetectionMethod := MyTypes.ldNone;
    end;
{$ELSE}
    LanguageDetectionMethod := MyTypes.ldNone;
{$IFEND}
{$IF 5500>=5306}
    UninstallDisplaySize := SetInteger64(osh.UninstallDisplaySize);
{$ELSE}
    UninstallDisplaySize := SetInteger64(0);
{$IFEND}
  end;
  TranslateSet(osh.Options, sh.Options, PByteArray(@SetupHeaderOptionTable)^, MySetupHeaderOptionLast);
end;

procedure TAnInnoVer.UnifyFileEntry(const p; var fe:TMySetupFileEntry);
var
  ofe: TSetupFileEntry absolute p;
begin
  with fe do begin
    SourceFilename              := ofe.SourceFilename;
    DestName                    := ofe.DestName;
    {$IF 5500>=2001}
    Components                  := ofe.Components;
    Tasks                       := ofe.Tasks;
    {$ELSE}
    Components                  := '';
    Tasks                       := '';
    {$IFEND}
    {$IF 5500>=4000}
    Check                       := ofe.Check;
    {$ELSE}
    Check                       := '';
    {$IFEND}
    {$IF 5500>=4001}
    Languages                   := ofe.Languages;
    {$ELSE}
    Languages                   := '';
    {$IFEND}
    {$IF 5500>=4100}
    AfterInstall                := ofe.AfterInstall;
    BeforeInstall               := ofe.BeforeInstall;
    PermissionsEntry            := ofe.PermissionsEntry;
    {$ELSE}
    AfterInstall                := '';
    BeforeInstall               := '';
    PermissionsEntry            := 0;
    {$IFEND}
    MinVersion                  := TMySetupVersionData(ofe.MinVersion);
    OnlyBelowVersion            := TMySetupVersionData(ofe.OnlyBelowVersion);
    LocationEntry               := ofe.LocationEntry;
    ExternalSize                := Integer64(Int64(ofe.ExternalSize));
    Attribs                     := ofe.Attribs;
    case ofe.FileType of
      ftUserFile: FileType  := MyTypes.ftUserFile;
      ftUninstExe: FileType := MyTypes.ftUninstExe;
      {$IF 5500<5000}
      ftRegSvrExe: FileType := MyTypes.ftRegSvrExe;
      {$IFEND}
    end;
  end;
  TranslateSet(ofe.Options, fe.Options, PByteArray(@SetupFileOptionTable)^, MySetupFileOptionLast);
end;

procedure TAnInnoVer.UnifyFileLocationEntry(const p; var fl:TMySetupFileLocationEntry);
var
  ofl: TSetupFileLocationEntry absolute p;
begin
  with fl do begin
{$IF 5500>=4000}
    FirstSlice                  :=ofl.FirstSlice;
    LastSlice                   :=ofl.LastSlice;
{$ELSE}
    FirstSlice                  :=ofl.FirstDisk;
    LastSlice                   :=ofl.LastDisk;
{$IFEND}
    StartOffset                 :=ofl.StartOffset;
{$IF 5500<=4000}
    ChunkSuboffset.Hi:=0; ChunkSuboffset.Lo:=0;
    ChunkCompressedSize         :=Integer64(Int64(ofl.CompressedSize));
{$ELSE}
    ChunkSuboffset              :=ofl.ChunkSuboffset;
    ChunkCompressedSize         :=ofl.ChunkCompressedSize;
{$IFEND}
    OriginalSize                :=Integer64(Int64(ofl.OriginalSize));

{$IF 5500>=5309}
    SHA1Sum                     := ofl.SHA1Sum;
    HashType                    := htSHA1;
{$ELSEIF 5500>=4200}
    MD5Sum                      := ofl.MD5Sum;
    HashType                    := htMD5;
{$ELSEIF 5500>=4001}
    CRC                         := ofl.CRC;
    HashType                    := htCRC32;
{$ELSE}
    CRC                         := ofl.Adler;
    HashType                    := htAdler;
{$IFEND}

{$IF 5500<4010}
    TimeStamp                   :=ofl.Date;
{$ELSEIF 5500<5507}
    TimeStamp                   :=ofl.TimeStamp;
{$ELSE}
    TimeStamp                   :=ofl.SourceTimeStamp;
{$IFEND}
    FileVersionMS               :=ofl.FileVersionMS;
    FileVersionLS               :=ofl.FileVersionLS;
    Contents:='';
    PrimaryFileEntry:=-1;
  end;
  TranslateSet(ofl.Flags, fl.Flags, PByteArray(@SetupFileLocationFlagTable)^, MySetupFileLocationFlagLast);
{$IF 5500<4205}
  Include(fl.Flags, MyTypes.foChunkCompressed);
{$IFEND}
end;

procedure TAnInnoVer.UnifyRegistryEntry(const p; var re:TMySetupRegistryEntry);
var
  ore: TSetupRegistryEntry absolute p;
begin
  with re do begin
    RootKey                       :=ore.RootKey;
    Subkey                        :=ore.Subkey;
    ValueName                     :=ore.ValueName;
    ValueData                     :=ore.ValueData;
    {$IF 5500>=2001}
    Components                    :=ore.Components;
    Tasks                         :=ore.Tasks;
    {$ELSE}
    Components                    := '';
    Tasks                         := '';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=ore.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    {$IF 5500>=4001}
    Languages                     :=ore.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4100}
    AfterInstall                  :=ore.AfterInstall;
    BeforeInstall                 :=ore.BeforeInstall;
    {$ELSE}
    AfterInstall                  :='';
    BeforeInstall                 :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(ore.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(ore.OnlyBelowVersion);

    case ore.Typ of
      rtNone         : Typ:=MyTypes.rtNone;
      rtString       : Typ:=MyTypes.rtString;
      rtExpandString : Typ:=MyTypes.rtExpandString;
      rtDWord        : Typ:=MyTypes.rtDWord;
      rtBinary       : Typ:=MyTypes.rtBinary;
      rtMultiString  : Typ:=MyTypes.rtMultiString;
      {$IF 5500>=5205}
      rtQWord        : Typ:=MyTypes.rtQWord;
      {$IFEND}
    end;
  end;
  TranslateSet(ore.Options, re.Options, PByteArray(@SetupRegistryOptionTable)^, MySetupRegistryOptionLast);
end;

procedure TAnInnoVer.UnifyRunEntry(const p; var re:TMySetupRunEntry);
var
  ore: TSetupRunEntry absolute p;
begin
  with re do begin
    Name                          :=ore.Name;
    Parameters                    :=ore.Parameters;
    WorkingDir                    :=ore.WorkingDir;
    RunOnceId                     :=ore.RunOnceId;
    {$IF 5500>=5113}
    Verb                          :=ore.Verb;
    {$ELSE}
    Verb                          :='';
    {$IFEND}
    {$IF 5500>=2001}
    StatusMsg                     :=ore.StatusMsg;
    Description                   :=NormalizeStringVal(ore.Description);
    Components                    :=ore.Components;
    Tasks                         :=ore.Tasks;
    {$ELSE}
    StatusMsg                     :='';
    Description                   :='';
    Components                    :='';
    Tasks                         :='';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=ore.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    {$IF 5500>=4001}
    Languages                     :=ore.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4100}
    AfterInstall                  :=ore.AfterInstall;
    BeforeInstall                 :=ore.BeforeInstall;
    {$ELSE}
    AfterInstall                  :='';
    BeforeInstall                 :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(ore.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(ore.OnlyBelowVersion);

    case ore.Wait of
      rwWaitUntilTerminated : Wait := MyTypes.rwWaitUntilTerminated;
      rwNoWait              : Wait := MyTypes.rwNoWait;
      rwWaitUntilIdle       : Wait := MyTypes.rwWaitUntilIdle;
    end;
  end;
  TranslateSet(ore.Options, re.Options, PByteArray(@SetupRunOptionTable)^, MySetupRunOptionLast);
end;

procedure TAnInnoVer.UnifyIconEntry(const p; var ie:TMySetupIconEntry);
var
  oie: TSetupIconEntry absolute p;
begin
  with ie do begin
    IconName                      :=NormalizeStringVal(oie.IconName);
    Filename                      :=oie.Filename;
    Parameters                    :=oie.Parameters;
    WorkingDir                    :=oie.WorkingDir;
    IconFilename                  :=oie.IconFilename;
    Comment                       :=NormalizeStringVal(oie.Comment);
    IconIndex                     :=oie.IconIndex;
    {$IF 5500>=1325}
    ShowCmd                       :=oie.ShowCmd;
    {$ELSE}
    ShowCmd                       :=0;
    {$IFEND}
    CloseOnExit                   :=TMySetupIconCloseOnExit(ord(oie.CloseOnExit));
    {$IF 5500>=2001}
    HotKey                        :=oie.HotKey;
    Components                    :=oie.Components;
    Tasks                         :=oie.Tasks;
    {$ELSE}
    HotKey                        :=0;
    Components                    :='';
    Tasks                         :='';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=oie.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    {$IF 5500>=4001}
    Languages                     :=oie.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4100}
    AfterInstall                  :=oie.AfterInstall;
    BeforeInstall                 :=oie.BeforeInstall;
    {$ELSE}
    AfterInstall                  :='';
    BeforeInstall                 :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(oie.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(oie.OnlyBelowVersion);
  end;
end;

procedure TAnInnoVer.UnifyTaskEntry(const p; var te:TMySetupTaskEntry);
{$IF 5500>=2001}
var
  ote: TSetupTaskEntry absolute p;
begin
  with te do begin
    Name                          :=ote.Name;
    Description                   :=NormalizeStringVal(ote.Description);
    GroupDescription              :=NormalizeStringVal(ote.GroupDescription);
    Components                    :=ote.Components;
    {$IF 5500>=4001}
    Languages                     :=ote.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=ote.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(ote.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(ote.OnlyBelowVersion);
    Move(ote.Options,Options,sizeof(ote.Options));
  end;
{$ELSE}
begin
{$IFEND}
end;

procedure TAnInnoVer.UnifyComponentEntry(const p; var ce:TMySetupComponentEntry);
{$IF 5500>=2001}
var
  oce: TSetupComponentEntry absolute p;
begin
  with ce do begin
    Name                          :=oce.Name;
    Description                   :=NormalizeStringVal(oce.Description);
    Types                         :=oce.Types;
    ExtraDiskSpaceRequired        :=Integer64(Int64(oce.ExtraDiskSpaceRequired));
    {$IF 5500>=4001}
    Languages                     :=oce.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=oce.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(oce.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(oce.OnlyBelowVersion);
    Move(oce.Options,Options,sizeof(oce.Options));
  end;
{$ELSE}
begin
{$IFEND}  
end;

procedure TAnInnoVer.UnifyTypeEntry(const p; var te:TMySetupTypeEntry);
{$IF 5500>=2001}
var
  ote: TSetupTypeEntry absolute p;
begin
  with te do begin
    Name                          :=ote.Name;
    Description                   :=NormalizeStringVal(ote.Description);
    {$IF 5500>=4001}
    Languages                     :=ote.Languages;
    {$ELSE}
    Languages                     :='';
    {$IFEND}
    {$IF 5500>=4000}
    Check                         :=ote.Check;
    {$ELSE}
    Check                         :='';
    {$IFEND}
    MinVersion                    :=TMySetupVersionData(ote.MinVersion);
    OnlyBelowVersion              :=TMySetupVersionData(ote.OnlyBelowVersion);
    Move(ote.Options,Options,sizeof(ote.Options));
    {$IF 5500>=4003}
    Typ                           :=TMySetupTypeType(ote.Typ);
    {$ELSE}
    Typ                           :=Low(TMySetupTypeType);
    {$IFEND}
  end;
{$ELSE}
begin
{$IFEND}
end;

procedure TAnInnoVer.UnifyCustomMessageEntry(const p; var ce:TMySetupCustomMessageEntry);
{$IF 5500>=4201}
var
  oce: TSetupCustomMessageEntry absolute p;
begin
  with ce do begin
    Name                          :=NormalizeStringVal(oce.Name);
    Value                         :=NormalizeStringVal(oce.Value);
    LangIndex                     :=oce.LangIndex;
  end;
{$ELSE}
begin
{$IFEND}
end;

procedure TAnInnoVer.UnifyLanguageEntry(const p; var le:TMySetupLanguageEntry);
{$IF 5500>=4000}
var
  ole: TSetupLanguageEntry absolute p;
begin
  with le do begin
    Name                          :=ole.Name;
    LanguageName                  :=CopyStringVal(ole.LanguageName);
    DialogFontName                :=ole.DialogFontName;
    TitleFontName                 :=ole.TitleFontName;
    WelcomeFontName               :=ole.WelcomeFontName;
    CopyrightFontName             :=ole.CopyrightFontName;
    Data                          :=ole.Data;
{$IF 5500>=4001}
    LicenseText                   :=ole.LicenseText;
    InfoBeforeText                :=ole.InfoBeforeText;
    InfoAfterText                 :=ole.InfoAfterText;
{$ELSE}
    LicenseText                   :='';
    InfoBeforeText                :='';
    InfoAfterText                 :='';
{$IFEND}
{$IF (5500>=4202) AND not Defined(UNICODE)}
    LanguageCodePage              :=ole.LanguageCodePage;
{$ELSE}
    LanguageCodePage              :=0;
{$IFEND}
    LanguageID                    :=ole.LanguageID;
    DialogFontSize                :=ole.DialogFontSize;
    TitleFontSize                 :=ole.TitleFontSize;
    WelcomeFontSize               :=ole.WelcomeFontSize;
    CopyrightFontSize             :=ole.CopyrightFontSize;
    RightToLeft                   :={$IF 5500>=5203} ole.RightToLeft {$ELSE} false {$IFEND};
  end;
{$ELSE}
begin
{$IFEND}
end;

procedure TAnInnoVer.UnifyDirEntry(const p; var de: TMySetupDirEntry);
var
  ode: TSetupDirEntry absolute p;
begin
  with de do begin
    DirName             := ode.DirName;
    {$IF 5500>=2001}
    Components          :=ode.Components;
    Tasks               :=ode.Tasks;
    {$ELSE}
    Components          :='';
    Tasks               :='';
    {$IFEND}
    Languages           := {$IF 5500>=4001} ode.Languages {$ELSE} '' {$IFEND};
    Check               := {$IF 5500>=3008} ode.Check {$ELSE} '' {$IFEND};
{$IF 5500>=4100}
    AfterInstall        := ode.AfterInstall;
    BeforeInstall       := ode.BeforeInstall;
{$ELSE}
    AfterInstall        := '';
    BeforeInstall       := '';
{$IFEND}
    Attribs             := {$IF 5500>=2011} ode.Attribs {$ELSE} 0 {$IFEND}; 
    MinVersion          := TMySetupVersionData(ode.MinVersion);
    OnlyBelowVersion    := TMySetupVersionData(ode.OnlyBelowVersion);
  end;
  TranslateSet(ode.Options, de.Options, PByteArray(@SetupDirOptionTable)^, MySetupDirOptionLast);
end;

procedure TAnInnoVer.UnifyIniEntry(const p; var ie: TMySetupIniEntry);
var
  oie: TSetupIniEntry absolute p;
begin
  with ie do begin
    Filename            := oie.Filename;
    Section             := oie.Section;
    Entry               := oie.Entry;
    Value               := oie.Value;
    {$IF 5500>=2001}
    Components          := oie.Components;
    Tasks               := oie.Tasks;
    {$ELSE}
    Components          := '';
    Tasks               := '';
    {$IFEND}
    Languages           := {$IF 5500>=4001} oie.Languages {$ELSE} '' {$IFEND};
    Check               := {$IF 5500>=3008} oie.Check {$ELSE} '' {$IFEND};
{$IF 5500>=4100}
    AfterInstall        := oie.AfterInstall;
    BeforeInstall       := oie.BeforeInstall;
{$ELSE}
    AfterInstall        := '';
    BeforeInstall       := '';
{$IFEND}
    MinVersion          := TMySetupVersionData(oie.MinVersion);
    OnlyBelowVersion    := TMySetupVersionData(oie.OnlyBelowVersion);
  end;
  TranslateSet(oie.Options, ie.Options, PByteArray(@SetupIniOptionTable)^, MySetupIniOptionLast);
end;

procedure TAnInnoVer.UnifyDeleteEntry(const p; var de: TMySetupDeleteEntry);
var
  ode: TSetupDeleteEntry absolute p;
begin
  with de do begin
    Name                := ode.Name;
    {$IF 5500>=2001}
    Components          :=ode.Components;
    Tasks               :=ode.Tasks;
    {$ELSE}
    Components          :='';
    Tasks               :='';
    {$IFEND}
{$IF 5500>=4001}
    Languages           := ode.Languages;
{$ELSE}
    Languages           := '';
{$IFEND}
{$IF 5500>=3008}
    Check               := ode.Check;
{$ELSE}
    Check               := '';
{$IFEND}
{$IF 5500>=4100}
    AfterInstall        := ode.AfterInstall;
    BeforeInstall       := ode.BeforeInstall;
{$ELSE}
    AfterInstall        := '';
    BeforeInstall       := '';
{$IFEND}
    MinVersion          := TMySetupVersionData(ode.MinVersion);
    OnlyBelowVersion    := TMySetupVersionData(ode.OnlyBelowVersion);

    case ode.DeleteType of
      dfFiles:              DeleteType := MyTypes.dfFiles;
      dfFilesAndOrSubdirs:  DeleteType := MyTypes.dfFilesAndOrSubdirs;
      dfDirIfEmpty:         DeleteType := MyTypes.dfDirIfEmpty;
    end;
  end;  
end;

begin
  SetLength(VerList, Length(VerList)+1);
  VerList[High(VerList)] := TAnInnoVer.Create;
end.