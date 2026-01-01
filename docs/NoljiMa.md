# NoljiMa

Telegram 알림 전송 및 응답 대기 콘솔 앱. 작업 완료 시 메시지를 전송하고, 응답을 대기할 수 있다.

## 개요

| 항목 | 내용 |
|------|------|
| 플랫폼 | .NET 8 Console App (C#) |
| 용도 | CLI에서 Telegram으로 알림 전송 및 응답 수신 |
| 설정 | ini 파일 (%LocalAppData%\NoljiMa\config.ini) |

## 메시지 포맷

### 기본 규칙
메시지는 `[ID][내용]` 형식을 사용한다.

```
[노티#1234][코드 리뷰 완료, 확인 필요]
[작업#ABC][빌드 실패 확인 요청]
[알림#999][배포 준비 완료]
```

### 구성 요소

| 요소 | 설명 | 예시 |
|------|------|------|
| ID | 고유 식별자 (`[` `]`로 감싸기) | `[노티#1234]` |
| 내용 | 전달할 메시지 (`[` `]`로 감싸기) | `[코드 리뷰 완료]` |

### 사용 목적
- **알림 전송**: Claude Code 작업 완료 후 사용자에게 알림
- **응답 대기**: 특정 ID를 포함한 응답 메시지 대기
- **자동화**: 스크립트에서 작업 완료/재개 시그널로 활용

## 사용법

### 기본 사용 (메시지 전송)

```bash
# PATH 등록 전
NoljiMa.exe "[노티#1234][빌드 완료]"
NoljiMa.exe "[작업#ABC][Spine 최적화 방법 질문]"

# PATH 등록 후 (어디서든 실행 가능)
NoljiMa "[노티#1234][빌드 완료]"
NoljiMa "[작업#ABC][Spine 최적화 방법 질문]"

# 레거시 포맷 (포맷 미사용)
NoljiMa "단순 알림 메시지"
```

### 응답 대기 모드 (신규)

```bash
# 특정 ID를 포함한 메시지 대기 (기본 타임아웃: 300초)
NoljiMa --wait "[노티#1234]"

# 타임아웃 지정 (초 단위)
NoljiMa --wait "[노티#1234]" --timeout 600

# 워크플로우 예시
NoljiMa "[노티#1234][코드 리뷰 필요]"    # 1. 알림 전송
NoljiMa --wait "[노티#1234]"             # 2. 응답 대기 (폴링 시작)
# 사용자가 텔레그램에서 "[노티#1234][확인완료]" 입력
# → NoljiMa 종료 (exit 0)
```

### Exit Code

| Code | 의미 | 설명 |
|------|------|------|
| 0 | 성공 | 메시지 전송 성공 또는 대기 중 응답 발견 |
| 1 | 실패 | 전송 실패, 타임아웃, 또는 설정 오류 |

## 설정 파일

### 1. config.ini (필수)

#### 경로
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

### 2. offset.txt (자동 생성)

#### 경로
```
%LocalAppData%\NoljiMa\offset.txt
```

#### 용도
- 메시지 대기 모드(--wait) 사용 시 자동 생성
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

## 동작 흐름

### 메시지 전송 모드 (기본)
1. exe 실행 시 인자로 메시지 받음
2. config.ini 파일에서 BotToken, ChatId 로드
3. Telegram Bot API (sendMessage)로 메시지 전송
4. 성공/실패 콘솔 출력 후 종료 (exit 0 또는 1)

### 응답 대기 모드 (--wait)
1. `--wait "[ID]"` 옵션으로 실행
2. config.ini 파일에서 BotToken, ChatId 로드
3. offset 파일에서 마지막 update_id 로드 (없으면 0)
4. getUpdates API로 메시지 폴링 시작 (Long Polling, 30초 타임아웃)
5. 수신된 메시지에서 지정된 ID 패턴 검색
6. 패턴 발견 시:
   - 새로운 offset 저장 (중복 처리 방지)
   - 성공 메시지 출력
   - exit 0으로 종료
7. 타임아웃 발생 시:
   - "응답 대기 시간 초과" 출력
   - exit 1로 종료

### 초기 설정 (config.ini 파일 없음)
1. exe 실행
2. "설정 파일이 없습니다. 설정을 시작합니다." 출력
3. BotToken 입력 요청 → 사용자 입력
4. ChatId 입력 요청 → 사용자 입력
5. 테스트 메시지 전송 ("[테스트][NoljiMa 설정 완료!]")
6. 성공 시 config.ini 파일 생성 및 저장
7. 실패 시 오류 출력, ini 저장 안 함

### 인자 없이 실행
- config.ini 파일 있음: 사용법 출력
- config.ini 파일 없음: 초기 설정 모드 진입

### 워크플로우 예시 (Claude Code 연동)

```bash
# Step 1: 작업 완료 후 알림 전송
noljima "[노티#1234][코드 리뷰 완료, 확인 필요]"
# → 텔레그램으로 메시지 전송
# → exit 0

# Step 2: 응답 대기 (최대 10분)
noljima --wait "[노티#1234]" --timeout 600
# → 폴링 시작... (Long Polling, 30초 간격)
# → 사용자가 텔레그램에서 "[노티#1234][확인완료]" 입력
# → 패턴 발견!
# → exit 0

# Step 3: Claude Code 작업 재개
# (자동화 스크립트에서 exit code 0 확인 후 다음 작업 실행)
```

## Telegram API 호출

### 1. sendMessage (메시지 전송)

#### Endpoint
```
POST https://api.telegram.org/bot{BotToken}/sendMessage
```

#### Request Body
```json
{
  "chat_id": "{ChatId}",
  "text": "{메시지}"
}
```

#### Response (성공)
```json
{
  "ok": true,
  "result": {
    "message_id": 123,
    "chat": { "id": 987654321 },
    "text": "[노티#1234][메시지 내용]"
  }
}
```

### 2. getUpdates (메시지 수신)

#### Endpoint
```
GET https://api.telegram.org/bot{BotToken}/getUpdates?offset={update_id}&timeout=30
```

#### Parameters

| 파라미터 | 설명 | 예시 |
|----------|------|------|
| offset | 읽을 첫 번째 update의 ID (이전 update_id + 1) | 12345678 |
| timeout | Long Polling 대기 시간 (초) | 30 |

#### Response (성공)
```json
{
  "ok": true,
  "result": [
    {
      "update_id": 12345678,
      "message": {
        "message_id": 999,
        "from": { "id": 987654321 },
        "chat": { "id": 987654321 },
        "date": 1640000000,
        "text": "[노티#1234][확인완료]"
      }
    }
  ]
}
```

#### Long Polling 동작 원리
1. `timeout=30` 설정 시, 서버는 최대 30초 대기
2. 새 메시지 도착 시 즉시 응답 반환
3. 30초 동안 메시지 없으면 빈 배열 반환
4. `offset`을 업데이트하여 다음 폴링 요청
5. 중복 수신 방지: `offset = last_update_id + 1`

### 구현 세부사항
- **HttpClient** 사용 (외부 패키지 불필요)
- **동기 방식** 구현 (`.Result` 사용)
- **타임아웃**: sendMessage 10초, getUpdates 35초 (Long Polling 30초 + 여유 5초)
- **offset 저장**: `%LocalAppData%\NoljiMa\offset.txt` (단일 숫자)

## 에러 처리

### 메시지 전송 모드

| 상황 | 동작 | Exit Code |
|------|------|-----------|
| 네트워크 오류 | "전송 실패: 네트워크 오류" 출력 | 1 |
| 잘못된 토큰 | "전송 실패: 토큰이 유효하지 않습니다" 출력 | 1 |
| 잘못된 ChatId | "전송 실패: ChatId가 유효하지 않습니다" 출력 | 1 |
| config.ini 파싱 오류 | "설정 파일 오류: 다시 설정해주세요" 출력 후 설정 모드 | 1 |

### 응답 대기 모드

| 상황 | 동작 | Exit Code |
|------|------|-----------|
| 타임아웃 초과 | "응답 대기 시간 초과" 출력 | 1 |
| 네트워크 오류 | "폴링 실패: 네트워크 오류" 출력 후 재시도 (최대 3회) | 1 |
| 잘못된 토큰 | "폴링 실패: 토큰이 유효하지 않습니다" 출력 | 1 |
| 패턴 발견 | "응답 메시지 수신: [내용]" 출력 | 0 |
| offset 파일 오류 | 경고 출력 후 offset=0으로 시작 | 계속 진행 |

## 프로젝트 구조

### 소스 코드

```
NoljiMa/
├── NoljiMa.csproj
├── Program.cs             # 모든 로직 포함 (단일 파일)
├── docs/
│   └── NoljiMa.md
├── CHANGELOG.md
└── CLAUDE.md
```

### 실행 시 생성 파일

```
%LocalAppData%\NoljiMa/
├── config.ini             # Telegram 설정 (BotToken, ChatId)
└── offset.txt             # 마지막 읽은 update_id (메시지 대기 모드 사용 시)
```

### Program.cs 구성 (예상)

```csharp
// 설정 관리
- GetConfigPath()          // config.ini 경로 반환
- LoadConfig()             // BotToken, ChatId 로드
- SaveConfig()             // config.ini 저장

// Telegram API
- SendTelegramMessage()    // sendMessage API 호출
- GetTelegramUpdates()     // getUpdates API 호출 (신규)

// Offset 관리 (신규)
- LoadOffset()             // offset.txt 읽기
- SaveOffset()             // offset.txt 저장

// 메시지 처리 (신규)
- WaitForMessage()         // 메시지 폴링 및 패턴 매칭

// 메인 로직
- InitialSetup()           // 초기 설정 모드
- Main()                   // 인자 파싱, 모드 전환
```

**코드 규모**: 약 400~450줄 예상 (기존 220줄 + 신규 200줄)
**외부 의존성**: 없음 (System.Text.Json, HttpClient 내장)

## 빌드

```bash
# 프레임워크 종속 배포 (~작은 크기, .NET Runtime 필요)
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish

# 독립 실행형 배포 (~큰 크기, Runtime 포함)
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish
```

## 설치

### PATH 환경 변수 등록

어디서든 `NoljiMa "메시지"` 형태로 실행하려면 PATH에 등록해야 합니다.

#### Windows
1. `NoljiMa.exe`를 원하는 위치에 배치 (예: `C:\Tools\NoljiMa\`)
2. 시스템 환경 변수 설정
   - `Win + R` → `sysdm.cpl` 실행
   - "고급" 탭 → "환경 변수" 클릭
   - "시스템 변수" 또는 "사용자 변수"에서 `Path` 선택 → "편집"
   - "새로 만들기" → `C:\Tools\NoljiMa` 추가
   - 확인 후 새 터미널 열기

#### 확인
```bash
NoljiMa "테스트 메시지"
```

## 배포

### 인스톨러 빌드 (Inno Setup)

#### 1. Inno Setup 설치
[Inno Setup](https://jrsoftware.org/isinfo.php) 다운로드 및 설치

#### 2. 프로젝트 빌드
```bash
# 버전이 포함된 빌드
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

#### 3. 인스톨러 컴파일
```bash
# Inno Setup Compiler로 빌드
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

출력 파일: `installer-output/NoljiMa-v0.1.0-Setup.exe`

**인스톤러 특징:**
- .NET 8 Runtime 자동 체크
- PATH 환경 변수 자동 등록 옵션
- 시작 메뉴 바로가기 생성
- 언어: 한국어/영어 지원

### 배포 파일 생성 (수동)

#### 1. 프레임워크 종속 배포 (권장)
```bash
# 빌드
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish

# 압축 (PowerShell)
Compress-Archive -Path ./publish/* -DestinationPath NoljiMa-v0.1.0-portable.zip
```

**특징:**
- 파일 크기: ~1-2MB
- .NET 8 Runtime 필요
- 사용자: [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) 설치 필요

#### 2. 독립 실행형 배포
```bash
# 빌드
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish

# 압축 (PowerShell)
Compress-Archive -Path ./publish/* -DestinationPath NoljiMa-v0.1.0-standalone.zip
```

**특징:**
- 파일 크기: ~60-80MB
- .NET Runtime 불필요
- 다운로드 후 바로 실행 가능

### 배포 파일 명명 규칙

- **포터블**: `NoljiMa-v{버전}-portable.zip`
- **독립형**: `NoljiMa-v{버전}-standalone.zip`
- **예시**: `NoljiMa-v0.1.0-portable.zip`

### 릴리스 체크리스트

1. 버전 정보 업데이트
   - `CHANGELOG.md`에 변경사항 기록
   - 프로젝트 파일에 버전 정보 추가 (선택)

2. 빌드 및 테스트
   ```bash
   dotnet build -c Release
   dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
   ```

3. 배포 파일 검증
   - `publish/NoljiMa.exe` 실행 확인
   - 설정 모드 테스트
   - 메시지 전송 테스트

4. 압축 및 배포
   ```bash
   Compress-Archive -Path ./publish/* -DestinationPath NoljiMa-v0.1.0-portable.zip
   ```

5. Git 정리
   ```bash
   # nul 파일 검색 및 삭제
   find . -name "nul" -type f -delete 2>/dev/null
   git status
   git add .
   git commit -m "release: v0.1.0"
   git tag v0.1.0
   git push origin main --tags
   ```
