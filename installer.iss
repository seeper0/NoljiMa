; NoljiMa Installer Script for Inno Setup
; 한국어 지원 포함

#define MyAppName "NoljiMa"
#define MyAppVersion "0.2.1"
#define MyAppPublisher "seeper0"
#define MyAppURL "https://github.com/seeper0/NoljiMa"
#define MyAppExeName "NoljiMa.exe"

[Setup]
; 앱 기본 정보
AppId={{C9F6B7E1-2345-6789-ABCD-EF0123456789}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=D:\Project\NoljiMa\installer-output
OutputBaseFilename=NoljiMa-v{#MyAppVersion}-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; 아이콘 및 이미지 (선택사항)
; SetupIconFile=icon.ico

; 권한 설정
PrivilegesRequired=admin

; 언어
ShowLanguageDialog=auto

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "addtopath"; Description: "PATH 환경 변수에 추가 (권장)"; GroupDescription: "추가 옵션:"

[Files]
; Runtime-dependent 버전 - 모든 필수 파일 포함
Source: "D:\Project\NoljiMa\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\Project\NoljiMa\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\README"; Filename: "{app}\README.md"
Name: "{group}\NoljiMa 페이지"; Filename: "https://github.com/seeper0/NoljiMa"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
Filename: "https://github.com/seeper0/NoljiMa"; Description: "NoljiMa 페이지 방문"; Flags: postinstall shellexec skipifsilent

[Code]
#ifdef UNICODE
  #define AW "W"
#else
  #define AW "A"
#endif

// Windows API 함수 선언
function SendNotifyMessage(hWnd: Longint; Msg: Cardinal; wParam: Longint; lParam: Longint): BOOL;
  external 'SendNotifyMessage{#AW}@user32.dll stdcall';

// 환경 변수 변경을 시스템에 알림
procedure RefreshEnvironment;
var
  S: string;
begin
  S := 'Environment';
  // HWND_BROADCAST ($ffff), WM_SETTINGCHANGE ($1A)
  SendNotifyMessage($ffff, $1A, 0, CastStringToInteger(S));
end;

// .NET 8 Runtime 체크 및 설치 안내
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  if not IsDotNetInstalled() then
  begin
    if MsgBox('이 애플리케이션을 실행하려면 .NET 8 Desktop Runtime이 필요합니다.' + #13#10 + #13#10 +
              '지금 .NET 다운로드 페이지를 여시겠습니까?' + #13#10 + #13#10 +
              '"아니오"를 선택하면 설치를 계속하지만, 프로그램 실행을 위해 나중에 .NET을 설치해야 합니다.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0/runtime', '', '', SW_SHOW, ewNoWait, ResultCode);
    end;
  end;
end;

// PATH 환경 변수 추가/제거
procedure CurStepChanged(CurStep: TSetupStep);
var
  Path: string;
  AppDir: string;
begin
  if CurStep = ssPostInstall then
  begin
    if WizardIsTaskSelected('addtopath') then
    begin
      AppDir := ExpandConstant('{app}');

      // 사용자 PATH에 추가
      if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path) then
      begin
        // 이미 PATH에 있는지 확인
        if Pos(Uppercase(AppDir), Uppercase(Path)) = 0 then
        begin
          // 끝에 세미콜론이 없으면 추가
          if (Length(Path) > 0) and (Path[Length(Path)] <> ';') then
            Path := Path + ';';

          Path := Path + AppDir;
          RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path);
          RefreshEnvironment;  // 시스템에 환경 변수 변경 알림
        end;
      end
      else
      begin
        // PATH가 없으면 새로 생성
        RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', AppDir);
        RefreshEnvironment;  // 시스템에 환경 변수 변경 알림
      end;
    end;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Path: string;
  AppDir: string;
  P: Integer;
  PathChanged: Boolean;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    AppDir := ExpandConstant('{app}');
    PathChanged := False;

    // PATH에서 제거
    if RegQueryStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path) then
    begin
      P := Pos(Uppercase(AppDir + ';'), Uppercase(Path));
      if P > 0 then
      begin
        Delete(Path, P, Length(AppDir) + 1);
        RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path);
        PathChanged := True;
      end
      else
      begin
        // 끝에 세미콜론 없이 있는 경우
        P := Pos(Uppercase(';' + AppDir), Uppercase(Path));
        if P > 0 then
        begin
          Delete(Path, P, Length(AppDir) + 1);
          RegWriteStringValue(HKEY_CURRENT_USER, 'Environment', 'Path', Path);
          PathChanged := True;
        end
        else
        begin
          // PATH가 AppDir만 있는 경우
          if Uppercase(Path) = Uppercase(AppDir) then
          begin
            RegDeleteValue(HKEY_CURRENT_USER, 'Environment', 'Path');
            PathChanged := True;
          end;
        end;
      end;
    end;

    // PATH 변경 시 시스템에 알림
    if PathChanged then
      RefreshEnvironment;
  end;
end;
