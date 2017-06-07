unit Compress;

{
  Inno Setup
  Copyright (C) 1997-2004 Jordan Russell
  Portions by Martijn Laan
  For conditions of distribution and use, see LICENSE.TXT.

  Abstract compression classes, and some generic compression-related functions

  $jrsoftware: issrc/Projects/Compress.pas,v 1.6 2004/02/28 02:16:16 jr Exp $
}

interface

uses
  SysUtils, Int64Em, FileClass;

type
  ECompressError = class(Exception);
  ECompressDataError = class(ECompressError);
  ECompressInternalError = class(ECompressError);

  TCompressorProgressProc = procedure(BytesProcessed: Cardinal) of object;
  TCompressorWriteProc = procedure(const Buffer; Count: Longint) of object;
  TCustomCompressorClass = class of TCustomCompressor;
  TCustomCompressor = class
  private
    FProgressProc: TCompressorProgressProc;
    FWriteProc: TCompressorWriteProc;
  protected
    property ProgressProc: TCompressorProgressProc read FProgressProc;
    property WriteProc: TCompressorWriteProc read FWriteProc;
  public
    constructor Create(AWriteProc: TCompressorWriteProc;
      AProgressProc: TCompressorProgressProc; CompressionLevel: Integer); virtual;
    procedure Compress(const Buffer; Count: Longint); virtual; abstract;
    procedure Finish; virtual; abstract;
  end;

  TDecompressorReadProc = function(var Buffer; Count: Longint): Longint of object;
  TCustomDecompressorClass = class of TCustomDecompressor;
  TCustomDecompressor = class
  private
    FReadProc: TDecompressorReadProc;
  protected
    property ReadProc: TDecompressorReadProc read FReadProc;
  public
    constructor Create(AReadProc: TDecompressorReadProc); virtual;
    procedure DecompressInto(var Buffer; Count: Longint); virtual; abstract;
    procedure Reset; virtual; abstract;
  end;

  TCompressedBlockWriter = class
  private
    FCompressor: TCustomCompressor;
    FFile: TFile;
    FStartPos: Integer64;
    FTotalBytesStored: Cardinal;
    FInBufferCount, FOutBufferCount: Cardinal;
    FInBuffer, FOutBuffer: array[0..4095] of Byte;
    procedure CompressorWriteProc(const Buffer; Count: Longint);
    procedure DoCompress(const Buf; var Count: Cardinal);
    procedure FlushOutputBuffer;
  public
    constructor Create(AFile: TFile; ACompressorClass: TCustomCompressorClass;
      CompressionLevel: Integer);
    destructor Destroy; override;
    procedure Finish;
    procedure Write(const Buffer; Count: Cardinal);
  end;

  TStoredDecompressor = class(TCustomDecompressor)
  public
    procedure DecompressInto(var Buffer; Count: Longint); override;
    procedure Reset; override;
  end;

  TAbstractBlockReader = class
  public
    constructor Create(AFile: TFile; ADecompressorClass: TCustomDecompressorClass); virtual;
    constructor CreateCache(ABlockReader: TAbstractBlockReader); virtual; abstract;
    destructor Destroy; override;
    procedure Read(var Buffer; Count: Cardinal); virtual; abstract;
    procedure Skip(Count: Cardinal);
  end;

  TCompressedBlockReader = class(TAbstractBlockReader)
  private
    FDecompressor: TCustomDecompressor;
    FFile: TFile;
    FInBytesLeft: Cardinal;
    FInitialized: Boolean;
    FInBufferNext: Cardinal;
    FInBufferAvail: Cardinal;
    FInBuffer: array[0..4095] of Byte;
    function DecompressorReadProc(var Buffer; Count: Longint): Longint;
    procedure ReadChunk;
  public
    constructor Create(AFile: TFile; ADecompressorClass: TCustomDecompressorClass); override;
    destructor Destroy; override;
    procedure Read(var Buffer; Count: Cardinal); override;
  end;

  TCacheReader = class(TAbstractBlockReader)
  protected
    FBlockReader: TAbstractBlockReader;
    FCacheEnabled: boolean;
    AllocationGranularity: integer;
    FBuffer: pointer;
    BufCapacity: Cardinal;
    BufSize: Cardinal;
    CurPos: Cardinal; // must be updated on every read/seek when caching is enabled
    procedure SetCapacity(ACapacity:integer);
  public
    constructor CreateCache(ABlockReader: TAbstractBlockReader); override;
    destructor Destroy; override;
    procedure Read(var ABuffer; Count: Cardinal); override;
    property CacheEnabled: boolean read FCacheEnabled write FCacheEnabled;
    property Bookmark: Cardinal read CurPos; // only valid when cache is enabled
    procedure Rewind; // to the start of buffer, wherever it happens to be
    procedure Seek(ABookmark: Cardinal);
  end;

function GetCRC32(const Buf; BufSize: Cardinal): Longint;
procedure TransformCallInstructions(var Buf; Size: Integer; const Encode: Boolean);
function UpdateCRC32(CurCRC: Longint; const Buf; BufSize: Cardinal): Longint;

implementation

const
  SStoredDataError = 'Unexpected end of stream';
  SCompressedBlockDataError = 'Compressed block is corrupted';

var
  CRC32TableInited: Boolean;
  CRC32Table: array[Byte] of Longint;

procedure InitCRC32Table;
var
  CRC: Longint;
  I, N: Integer;
begin
  for I := 0 to 255 do begin
    CRC := I;
    for N := 0 to 7 do begin
      if Odd(CRC) then
        CRC := (CRC shr 1) xor Longint($EDB88320)
      else
        CRC := CRC shr 1;
    end;
    Crc32Table[I] := CRC;
  end;
end;

function UpdateCRC32(CurCRC: Longint; const Buf; BufSize: Cardinal): Longint;
var
  P: ^Byte;
begin
  if not CRC32TableInited then begin
    InitCRC32Table;
    CRC32TableInited := True;
  end;
  P := @Buf;
  while BufSize <> 0 do begin
    CurCRC := CRC32Table[Lo(CurCRC) xor P^] xor (CurCRC shr 8);
    Dec(BufSize);
    Inc(P);
  end;
  Result := CurCRC;
end;

function GetCRC32(const Buf; BufSize: Cardinal): Longint;
begin
  Result := UpdateCRC32(Longint($FFFFFFFF), Buf, BufSize) xor Longint($FFFFFFFF);
end;

procedure TransformCallInstructions(var Buf; Size: Integer; const Encode: Boolean);
{ Transforms addresses in relative CALL or JMP instructions to absolute ones
  if Encode is True, or the inverse if Encode is False.
  This transformation can lead to a higher compression ratio when compressing
  32-bit x86 code. }
type
  PByteArray = ^TByteArray;
  TByteArray = array[0..$7FFFFFFE] of Byte;
var
  P: PByteArray;
  I: Integer;
begin
  if Size < 5 then
    Exit;
  Dec(Size, 4);
  P := @Buf;
  I := 0;
  while I < Size do begin
    { Does it appear to be a CALL or JMP instruction with a relative 32-bit
      address? }
    if (P[I] = $E8) or (P[I] = $E9) then begin
      { Change the address to be relative to the beginning of the buffer,
        instead of relative to the next instruction. If decoding, do the
        opposite. }
      Inc(I, 5);
      if Encode then
        Inc(Longint((@P[I-4])^), I)
      else
        Dec(Longint((@P[I-4])^), I);
    end
    else
      Inc(I);
  end;
end;

{ TCustomCompressor }

constructor TCustomCompressor.Create(AWriteProc: TCompressorWriteProc;
  AProgressProc: TCompressorProgressProc; CompressionLevel: Integer);
begin
  inherited Create;
  FWriteProc := AWriteProc;
  FProgressProc := AProgressProc;
end;

{ TCustomDecompressor }

constructor TCustomDecompressor.Create(AReadProc: TDecompressorReadProc);
begin
  inherited Create;
  FReadProc := AReadProc;
end;

{ TCompressedBlockWriter }

type
  TCompressedBlockHeader = packed record
    StoredSize: LongWord;   { Total bytes written, including the CRCs }
    Compressed: Boolean;    { True if data is compressed, False if not }
  end;

constructor TCompressedBlockWriter.Create(AFile: TFile;
  ACompressorClass: TCustomCompressorClass; CompressionLevel: Integer);
var
  HdrCRC: Longint;
  Hdr: TCompressedBlockHeader;
begin
  inherited Create;

  FFile := AFile;
  if Assigned(ACompressorClass) and (CompressionLevel <> 0) then
    FCompressor := ACompressorClass.Create(CompressorWriteProc, nil, CompressionLevel);
  FStartPos := AFile.Position;

  { Note: These will be overwritten by Finish }
  HdrCRC := 0;
  AFile.WriteBuffer(HdrCRC, SizeOf(HdrCRC));
  Hdr.StoredSize := 0;
  Hdr.Compressed := False;
  AFile.WriteBuffer(Hdr, SizeOf(Hdr));
end;

destructor TCompressedBlockWriter.Destroy;
begin
  FCompressor.Free;
  inherited;
end;

procedure TCompressedBlockWriter.FlushOutputBuffer;
{ Flushes contents of FOutBuffer into the file, with a preceding CRC }
var
  CRC: Longint;
begin
  CRC := GetCRC32(FOutBuffer, FOutBufferCount);
  FFile.WriteBuffer(CRC, SizeOf(CRC));
  Inc(FTotalBytesStored, SizeOf(CRC));
  FFile.WriteBuffer(FOutBuffer, FOutBufferCount);
  Inc(FTotalBytesStored, FOutBufferCount);
  FOutBufferCount := 0;
end;

procedure TCompressedBlockWriter.CompressorWriteProc(const Buffer; Count: Longint);
var
  P: ^Byte;
  Bytes: Cardinal;
begin
  P := @Buffer;
  while Count > 0 do begin
    Bytes := Count;
    if Bytes > SizeOf(FOutBuffer) - FOutBufferCount then
      Bytes := SizeOf(FOutBuffer) - FOutBufferCount;
    Move(P^, FOutBuffer[FOutBufferCount], Bytes);
    Inc(FOutBufferCount, Bytes);
    if FOutBufferCount = SizeOf(FOutBuffer) then
      FlushOutputBuffer;
    Dec(Count, Bytes);
    Inc(P, Bytes);
  end;
end;

procedure TCompressedBlockWriter.DoCompress(const Buf; var Count: Cardinal);
begin
  if Count > 0 then begin
    if Assigned(FCompressor) then
      FCompressor.Compress(Buf, Count)
    else
      CompressorWriteProc(Buf, Count);
  end;
  Count := 0;
end;

procedure TCompressedBlockWriter.Write(const Buffer; Count: Cardinal);
var
  P: ^Byte;
  Bytes: Cardinal;
begin
  { Writes are buffered strictly as an optimization, to avoid feeding tiny
    blocks to the compressor }
  P := @Buffer;
  while Count > 0 do begin
    Bytes := Count;
    if Bytes > SizeOf(FInBuffer) - FInBufferCount then
      Bytes := SizeOf(FInBuffer) - FInBufferCount;
    Move(P^, FInBuffer[FInBufferCount], Bytes);
    Inc(FInBufferCount, Bytes);
    if FInBufferCount = SizeOf(FInBuffer) then
      DoCompress(FInBuffer, FInBufferCount);
    Dec(Count, Bytes);
    Inc(P, Bytes);
  end;
end;

procedure TCompressedBlockWriter.Finish;
var
  Pos: Integer64;
  HdrCRC: Longint;
  Hdr: TCompressedBlockHeader;
begin
  DoCompress(FInBuffer, FInBufferCount);
  if Assigned(FCompressor) then
    FCompressor.Finish;
  if FOutBufferCount > 0 then
    FlushOutputBuffer;

  Pos := FFile.Position;
  FFile.Seek64(FStartPos);
  Hdr.StoredSize := FTotalBytesStored;
  Hdr.Compressed := Assigned(FCompressor);
  HdrCRC := GetCRC32(Hdr, SizeOf(Hdr));
  FFile.WriteBuffer(HdrCRC, SizeOf(HdrCRC));
  FFile.WriteBuffer(Hdr, SizeOf(Hdr));
  FFile.Seek64(Pos);
end;

{ TStoredDecompressor }

procedure TStoredDecompressor.DecompressInto(var Buffer; Count: Longint);
var
  P: ^Byte;
  NumRead: Longint;
begin
  P := @Buffer;
  while Count > 0 do begin
    NumRead := ReadProc(P^, Count);
    if NumRead = 0 then
      raise ECompressDataError.Create(SStoredDataError);
    Inc(P, NumRead);
    Dec(Count, NumRead);
  end;
end;

procedure TStoredDecompressor.Reset;
begin
end;



constructor TAbstractBlockReader.Create(AFile: TFile; ADecompressorClass: TCustomDecompressorClass);
begin
  inherited Create;
end;

destructor TAbstractBlockReader.Destroy;
begin
  inherited Destroy;
end;

procedure TAbstractBlockReader.Skip(Count:Cardinal);
var p:pointer;
begin
  GetMem(p,Count);
  Read(p^,Count);
  FreeMem(p);
end;


{ TCompressedBlockReader }

constructor TCompressedBlockReader.Create(AFile: TFile;
  ADecompressorClass: TCustomDecompressorClass);
var
  HdrCRC: Longint;
  Hdr: TCompressedBlockHeader;
  P: Integer64;
begin
  inherited;

  FFile := AFile;

  if (AFile.Read(HdrCRC, SizeOf(HdrCRC)) <> SizeOf(HdrCRC)) or
     (AFile.Read(Hdr, SizeOf(Hdr)) <> SizeOf(Hdr)) then
    raise ECompressDataError.Create(SCompressedBlockDataError);
  if HdrCRC <> GetCRC32(Hdr, SizeOf(Hdr)) then
    raise ECompressDataError.Create(SCompressedBlockDataError);
  P := AFile.Position;
  Inc64(P, Hdr.StoredSize);
  if Compare64(P, AFile.Size) > 0 then
    raise ECompressDataError.Create(SCompressedBlockDataError);
  if Hdr.Compressed then
    FDecompressor := ADecompressorClass.Create(DecompressorReadProc);
  FInBytesLeft := Hdr.StoredSize;
  FInitialized := True;
end;

destructor TCompressedBlockReader.Destroy;
var
  P: Integer64;
begin
  FDecompressor.Free;
  if FInitialized then begin
    { Must seek ahead if the caller didn't read everything that was originally
      compressed, or if it did read everything but zlib is in a "CHECK" state
      (i.e. it didn't read and verify the trailing adler32 yet due to lack of
      input bytes). }
    P := FFile.Position;
    Inc64(P, FInBytesLeft);
    FFile.Seek64(P);
  end;
  inherited;
end;

procedure TCompressedBlockReader.ReadChunk;
var
  CRC: Longint;
  Len: Cardinal;
begin
  { Read chunk CRC }
  if FInBytesLeft < SizeOf(CRC) + 1 then
    raise ECompressDataError.Create(SCompressedBlockDataError);
  FFile.ReadBuffer(CRC, SizeOf(CRC));
  Dec(FInBytesLeft, SizeOf(CRC));

  { Read chunk data }
  Len := FInBytesLeft;
  if Len > SizeOf(FInBuffer) then
    Len := SizeOf(FInBuffer);
  FFile.ReadBuffer(FInBuffer, Len);
  Dec(FInBytesLeft, Len);
  FInBufferNext := 0;
  FInBufferAvail := Len;
  if CRC <> GetCRC32(FInBuffer, Len) then
    raise ECompressDataError.Create(SCompressedBlockDataError);
end;

function TCompressedBlockReader.DecompressorReadProc(var Buffer;
  Count: Longint): Longint;
var
  P: ^Byte;
  Bytes: Cardinal;
begin
  Result := 0;
  P := @Buffer;
  while Count > 0 do begin
    if FInBufferAvail = 0 then begin
      if FInBytesLeft = 0 then
        Break;
      ReadChunk;
    end;
    Bytes := Count;
    if Bytes > FInBufferAvail then
      Bytes := FInBufferAvail;
    Move(FInBuffer[FInBufferNext], P^, Bytes);
    Inc(FInBufferNext, Bytes);
    Dec(FInBufferAvail, Bytes);
    Inc(P, Bytes);
    Dec(Count, Bytes);
    Inc(Result, Bytes);
  end;
end;

procedure TCompressedBlockReader.Read(var Buffer; Count: Cardinal);
begin
  if Assigned(FDecompressor) then
    FDecompressor.DecompressInto(Buffer, Count)
  else begin
    { Not compressed -- call DecompressorReadProc directly }
    if Cardinal(DecompressorReadProc(Buffer, Count)) <> Count then
      raise ECompressDataError.Create(SCompressedBlockDataError);
  end;
end;


////////////// TCacheReader

constructor TCacheReader.CreateCache(ABlockReader: TAbstractBlockReader);
begin
  FBlockReader:=ABlockReader;
  AllocationGranularity:=4096; FBuffer:=nil; BufSize:=0; BufCapacity:=0; CurPos:=0;
end;

destructor TCacheReader.Destroy;
begin
//  FBlockReader.Free; // ???
  if FBuffer<>nil then FreeMem(FBuffer);
  inherited;
end;

procedure TCacheReader.Read(var ABuffer; Count: Cardinal);
var t:Cardinal; ABuf:pointer;
begin
  if Count=0 then exit;
  ABuf:=@ABuffer;
  if CurPos<BufSize then begin // read from the buffer
    t:=BufSize-CurPos; if t>Count then t:=Count;
    Move(pointer(cardinal(FBuffer)+CurPos)^,ABuf^,t);
    Inc(cardinal(ABuf),t); Dec(Count,t);
    Inc(CurPos,t);
  end;
  if Count>0 then begin // read from the underlying object
    FBlockReader.Read(ABuf^,Count);
    if FCacheEnabled then begin // append the read bytes to the buffer
      if BufSize+Count>BufCapacity then SetCapacity(BufSize+Count);
      Move(ABuf^,pointer(cardinal(FBuffer)+CurPos)^,Count);
      Inc(BufSize,Count); Inc(CurPos,Count);
    end;
  end;
  if not FCacheEnabled and (CurPos=BufSize) and (CurPos>0) then begin // clean the buffer
    CurPos:=0; BufSize:=0; BufCapacity:=0; FreeMem(FBuffer); FBuffer:=nil;
  end;
end;

procedure TCacheReader.Seek(ABookmark:Cardinal);
begin
  if FCacheEnabled and (ABookmark<BufSize) then CurPos:=ABookmark;
end;

procedure TCacheReader.Rewind;
begin
  if FCacheEnabled then CurPos:=0;
end;

procedure TCacheReader.SetCapacity(ACapacity:integer);
var t:integer;
begin
  t:=ACapacity mod AllocationGranularity;
  if t>0 then Inc(ACapacity, AllocationGranularity-t);
  ReallocMem(FBuffer,ACapacity);
  BufCapacity:=ACapacity;
end;

end.
