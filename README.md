# NoljiMa

Telegram으로 알림을 전송하는 간단한 CLI 도구입니다. 작업 완료 시 호출하여 메시지를 받아보세요.

## 개요

**NoljiMa**는 .NET 8.0으로 제작된 Windows 콘솔 애플리케이션으로 Telegram Bot API를 통해 메시지를 전송합니다. 빌드 완료, 배포 완료 등 작업이 끝났을 때 자동으로 알림을 받을 수 있습니다.

## 기능

### 핵심 기능
- **Telegram 메시지 전송**: Bot API를 통한 간편한 알림
- **INI 설정 관리**: 실행 파일과 같은 경로의 `NoljiMa.ini`로 관리
- **초기 설정 모드**: 처음 실행 시 대화형으로 Bot Token과 Chat ID 입력
- **자동 테스트**: 설정 완료 시 테스트 메시지 전송으로 검증
- **명령줄 통합**: PATH 등록 시 어디서든 `NoljiMa "메시지"` 실행 가능

### 에러 처리
- 네트워크 오류 감지
- 잘못된 Bot Token 검증
- 잘못된 Chat ID 검증
- 설정 파일 오류 처리

## 기술 스택

- **플랫폼**: .NET 8.0 Console App
- **언어**: C#
- **HTTP 통신**: HttpClient (내장)
- **설정 파싱**: 직접 구현 (외부 의존성 없음)

## 설치

### 필수 요구사항
- Windows 7 이상
- .NET 8.0 Runtime ([다운로드](https://dotnet.microsoft.com/download/dotnet/8.0/runtime))

### 릴리즈에서 설치

#### 옵션 1: 인스톨러 (권장)
1. [Releases](https://github.com/seeper0/NoljiMa/releases)에서 `NoljiMa-v{버전}-Setup.exe` 다운로드
2. 실행하여 설치
3. 설치 중 "PATH 환경 변수에 추가" 옵션 선택 (권장)

#### 옵션 2: 포터블
1. [Releases](https://github.com/seeper0/NoljiMa/releases)에서 `NoljiMa-v{버전}-portable.zip` 다운로드
2. 원하는 위치에 압축 해제
3. (선택) PATH 환경 변수에 수동 등록

### PATH 환경 변수 등록 (포터블 사용 시)

#### Windows
1. `Win + R` → `sysdm.cpl` 실행
2. "고급" 탭 → "환경 변수" 클릭
3. "시스템 변수" 또는 "사용자 변수"에서 `Path` 선택 → "편집"
4. "새로 만들기" → NoljiMa.exe가 있는 폴더 경로 추가 (예: `C:\Tools\NoljiMa`)
5. 확인 후 새 터미널 열기

## 사용법

### 초기 설정

처음 실행 시 설정이 필요합니다:

```bash
# NoljiMa 실행 (인자 없이)
NoljiMa.exe

# 또는 PATH 등록 후
NoljiMa
```

1. Telegram Bot Token 입력 ([@BotFather](https://t.me/botfather)에서 발급)
2. Chat ID 입력 (개인 또는 그룹)
3. 테스트 메시지 전송으로 검증
4. 성공 시 `NoljiMa.ini` 자동 생성

### 메시지 전송

```bash
# PATH 등록 전
NoljiMa.exe "빌드 완료"
NoljiMa.exe "배포 성공!"

# PATH 등록 후 (어디서든 실행 가능)
NoljiMa "빌드 완료"
NoljiMa "테스트 통과"
```

### 빌드 스크립트에 통합

#### Batch Script
```batch
@echo off
dotnet build -c Release
if %ERRORLEVEL% EQU 0 (
    NoljiMa "빌드 성공"
) else (
    NoljiMa "빌드 실패"
)
```

#### PowerShell
```powershell
dotnet build -c Release
if ($LASTEXITCODE -eq 0) {
    NoljiMa "빌드 성공"
} else {
    NoljiMa "빌드 실패"
}
```

#### Bash (WSL/Git Bash)
```bash
dotnet build -c Release && NoljiMa "빌드 성공" || NoljiMa "빌드 실패"
```

## Telegram Bot 설정

### 1. Bot 생성
1. Telegram에서 [@BotFather](https://t.me/botfather) 검색
2. `/newbot` 명령 입력
3. Bot 이름과 username 설정
4. Bot Token 받기 (예: `123456789:ABCdefGHIjklMNOpqrsTUVwxyz`)

### 2. Chat ID 확인

#### 개인 채팅
1. 생성한 Bot과 대화 시작
2. [@userinfobot](https://t.me/userinfobot)에게 메시지 전송
3. Chat ID 확인

#### 그룹 채팅
1. 그룹에 Bot 추가
2. 브라우저에서 `https://api.telegram.org/bot{BotToken}/getUpdates` 접속
3. `"chat":{"id":-123456789}` 형태로 Chat ID 확인

## 설정 파일

### 위치
`NoljiMa.exe`와 같은 폴더의 `NoljiMa.ini`

### 형식
```ini
[Telegram]
BotToken=123456789:ABCdefGHIjklMNOpqrsTUVwxyz
ChatId=987654321
```

### 재설정
`NoljiMa.ini` 파일을 삭제하고 NoljiMa를 다시 실행하면 초기 설정 모드로 진입합니다.

## 빌드

### 소스에서 빌드
```bash
# 저장소 복제
git clone https://github.com/seeper0/NoljiMa.git
cd NoljiMa

# 프로젝트 빌드
dotnet build -c Release

# 프레임워크 종속 게시
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

## 프로젝트 구조

```
NoljiMa/
├── Program.cs             # 모든 로직 포함 (단일 파일, ~200줄)
├── NoljiMa.csproj        # 프로젝트 파일
├── NoljiMa.sln           # 솔루션 파일
├── installer.iss         # Inno Setup 스크립트
├── CHANGELOG.md          # 변경 이력
├── CLAUDE.md             # 프로젝트 규칙
└── docs/
    └── NoljiMa.md        # 상세 명세서
```

## 라이선스

이 프로젝트는 MIT 라이선스에 따라 라이선스가 부여됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

Copyright (c) 2025 seeper0

## 개발 철학

- **단순성**: 단일 파일 구현, 외부 의존성 최소화
- **가성비**: 필요한 기능만, 오버엔지니어링 지양
- **실용성**: 실제 사용 사례에 최적화

---

**Telegram 알림을 간편하게 ⚡**
