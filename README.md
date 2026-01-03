# NoljiMa

Telegram으로 알림을 전송하는 간단한 CLI 도구입니다. 작업 완료 시 호출하여 메시지를 받아보세요.

## 개요

**NoljiMa**는 .NET 8.0으로 제작된 Windows 콘솔 애플리케이션으로 Telegram Bot API를 통해 메시지를 전송합니다. 빌드 완료, 배포 완료 등 작업이 끝났을 때 자동으로 알림을 받을 수 있습니다.

## 기능

### 핵심 기능
- **Telegram 메시지 전송**: Bot API를 통한 간편한 알림 (모든 텍스트 전송 가능)
- **쌍방향 소통**: 텔레그램 응답 대기 기능 (`--wait` 파라미터)
- **offset 기반 메시지 추적**: 이미 읽은 메시지와 새 메시지 자동 구분
- **INI 설정 관리**: `%LocalAppData%\NoljiMa\config.ini`로 설정 관리
- **초기 설정 모드**: 처음 실행 시 대화형으로 Bot Token과 Chat ID 입력
- **자동 테스트**: 설정 완료 시 테스트 메시지 전송으로 검증
- **명령줄 통합**: PATH 등록 시 어디서든 `NoljiMa "메시지"` 실행 가능

### 핵심 워크플로우

**작업 완료 후 Claude와 소통하기**:
```bash
# 1. 작업 완료 후 질문 전송
dotnet build
NoljiMa "빌드 완료. 다음 뭐 할까?"

# 2. 응답 대기 (⚠️ 병렬 실행 금지!)
NoljiMa --wait

# 3. 텔레그램에서 답변
"테스트 돌려봐"

# 4. NoljiMa가 메시지를 받아서 Claude에게 전달
# Claude가 테스트 실행...
```

**중요**: offset 기반으로 새 메시지를 자동으로 구분하므로 `[ID]` 형식이 필요 없습니다. 간단하게 메시지를 보내고 응답을 기다리면 됩니다.

### 에러 처리
- 네트워크 오류 감지 및 자동 재시도 (최대 3회)
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
4. 성공 시 `%LocalAppData%\NoljiMa\config.ini` 자동 생성

### 기본 사용 (메시지 전송)

```bash
# PATH 등록 전
NoljiMa.exe "빌드 완료"
NoljiMa.exe "Spine 최적화 방법 질문"

# PATH 등록 후 (어디서든 실행 가능)
NoljiMa "빌드 완료"
NoljiMa "다음 작업은 뭐 할까?"
```

### 응답 대기 모드

텔레그램에서 사용자 응답을 기다릴 수 있습니다:

```bash
# 다음 새 메시지 대기 (기본 타임아웃: 24시간)
NoljiMa --wait

# 특정 패턴 포함 메시지 대기 (선택적)
NoljiMa --wait "확인"

# 타임아웃 지정 (초 단위)
NoljiMa --wait --timeout 600

# 워크플로우 예시
NoljiMa "코드 리뷰 필요"               # 1. 알림 전송
NoljiMa --wait                         # 2. 응답 대기 (폴링 시작)
# 사용자가 텔레그램에서 "확인완료" 입력
# → NoljiMa 종료 (exit 0)
```

### 명령어 옵션

#### 도움말 보기
```bash
# 사용법 확인
NoljiMa --help
NoljiMa -h
```

#### Offset 클리어
메시지 대기 모드(`--wait`)에서 사용하는 읽기 위치를 초기화합니다. 이전에 읽은 메시지를 다시 처음부터 읽고 싶을 때 사용합니다.

```bash
# offset.txt 파일 삭제 (메시지 읽기 위치 초기화)
NoljiMa --clear-offset
```

**사용 예시**:
```bash
# offset 클리어 후 처음부터 메시지 다시 읽기
NoljiMa --clear-offset
NoljiMa --wait "[작업#001]"  # 모든 메시지를 처음부터 확인
```

### Exit Code

| Code | 의미 | 설명 |
|------|------|------|
| 0 | 성공 | 메시지 전송 성공 또는 대기 중 응답 발견 |
| 1 | 실패 | 전송 실패, 타임아웃, 또는 설정 오류 |

### ⚠️ 주의사항

#### 병렬 실행 금지

**NoljiMa를 동시에 여러 개 실행하지 마세요.** 특히 `--wait` 모드는 `offset.txt` 파일을 공유하므로 병렬 실행 시 메시지 중복 처리나 누락이 발생할 수 있습니다.

**❌ 하지 마세요**:
```bash
NoljiMa --wait &         # 백그라운드
NoljiMa --wait           # 동시 실행 ← offset 충돌!
```

**✅ 올바른 사용**:
```bash
NoljiMa "작업 시작"
# ... 작업 진행 ...
NoljiMa --wait           # 응답 대기 (하나만 실행)
```

**이유**: `offset.txt`는 "어디까지 읽었는지" 저장하는 단일 파일입니다. 여러 프로세스가 동시에 읽고 쓰면 메시지 추적이 꼬일 수 있습니다.

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

<!-- NOTE: 이 섹션의 제목과 구조는 Program.cs의 InitialSetup 함수에서 참조됩니다. -->
<!-- 제목이나 구조 변경 시 Program.cs:500-505의 링크도 함께 업데이트해야 합니다. -->

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

### 1. config.ini (필수)

#### 위치
```
%LocalAppData%\NoljiMa\config.ini
```
예: `C:\Users\YourName\AppData\Local\NoljiMa\config.ini`

#### 형식
```ini
[Telegram]
BotToken=123456789:ABCdefGHIjklMNOpqrsTUVwxyz
ChatId=987654321
```

#### 설정 항목

| 키 | 설명 | 예시 |
|----|------|------|
| BotToken | Telegram Bot API 토큰 (@BotFather에서 발급) | 123456789:ABCdefGHI... |
| ChatId | 메시지 받을 채팅 ID (개인 또는 그룹) | 987654321 |

#### 재설정
`config.ini` 파일을 삭제하고 NoljiMa를 다시 실행하면 초기 설정 모드로 진입합니다.

### 2. offset.txt (자동 생성)

#### 위치
```
%LocalAppData%\NoljiMa\offset.txt
```

#### 용도
- 메시지 대기 모드(`--wait`) 사용 시 자동 생성
- 마지막으로 읽은 Telegram update_id 저장
- 중복 메시지 처리 방지

#### 형식
```
12345678
```
(단일 정수, 마지막 update_id)

#### 관리
- 자동 생성/업데이트: 메시지 수신 시 자동 저장
- 수동 삭제: 처음부터 다시 읽고 싶을 때 삭제 가능
- 파일 없음: offset=0으로 시작 (모든 메시지 읽기)

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

### 소스 코드

```
NoljiMa/
├── Program.cs             # 모든 로직 포함 (단일 파일, ~530줄)
├── NoljiMa.csproj        # 프로젝트 파일
├── NoljiMa.sln           # 솔루션 파일
├── installer.iss         # Inno Setup 스크립트
├── CHANGELOG.md          # 변경 이력
├── CLAUDE.md             # 프로젝트 규칙
└── docs/
    └── NoljiMa.md        # 상세 명세서
```

### 실행 시 생성 파일

```
%LocalAppData%\NoljiMa/
├── config.ini            # Telegram 설정 (BotToken, ChatId)
└── offset.txt            # 마지막 읽은 update_id (메시지 대기 모드 사용 시)
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
