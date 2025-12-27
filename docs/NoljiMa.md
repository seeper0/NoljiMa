# NoljiMa

Telegram 알림 전송 콘솔 앱. 작업 완료 시 호출하여 메시지를 전송한다.

## 개요

| 항목 | 내용 |
|------|------|
| 플랫폼 | .NET 8 Console App (C#) |
| 용도 | CLI에서 Telegram으로 알림 전송 |
| 설정 | ini 파일 (exe와 동일 경로) |

## 사용법

```bash
# PATH 등록 전
NoljiMa.exe "빌드 완료"
NoljiMa.exe "Spine 최적화 방법 질문"

# PATH 등록 후 (어디서든 실행 가능)
NoljiMa "빌드 완료"
NoljiMa "Spine 최적화 방법 질문"
```

## 설정 파일

### 파일명
`NoljiMa.ini` (exe와 같은 디렉토리)

### 형식
```ini
[Telegram]
BotToken=123456789:ABCdefGHIjklMNOpqrsTUVwxyz
ChatId=987654321
```

### 설정 항목

| 키 | 설명 | 예시 |
|----|------|------|
| BotToken | Telegram Bot API 토큰 (@BotFather에서 발급) | 123456789:ABCdefGHI... |
| ChatId | 메시지 받을 채팅 ID (개인 또는 그룹) | 987654321 |

## 동작 흐름

### 정상 실행 (ini 파일 존재)
1. exe 실행 시 인자로 메시지 받음
2. ini 파일에서 BotToken, ChatId 로드
3. Telegram Bot API로 메시지 전송
4. 성공/실패 콘솔 출력 후 종료

### 초기 설정 (ini 파일 없음)
1. exe 실행
2. "설정 파일이 없습니다. 설정을 시작합니다." 출력
3. BotToken 입력 요청 → 사용자 입력
4. ChatId 입력 요청 → 사용자 입력
5. 테스트 메시지 전송 ("NoljiMa 설정 완료!")
6. 성공 시 ini 파일 생성 및 저장
7. 실패 시 오류 출력, ini 저장 안 함

### 인자 없이 실행
- ini 파일 있음: "사용법: NoljiMa \"메시지\"" 출력
- ini 파일 없음: 초기 설정 모드 진입

## Telegram API 호출

### Endpoint
```
https://api.telegram.org/bot{BotToken}/sendMessage
```

### Request (POST)
```json
{
  "chat_id": "{ChatId}",
  "text": "{메시지}"
}
```

### 구현
- HttpClient 사용 (외부 패키지 불필요)
- 동기 방식으로 단순하게 구현

## 에러 처리

| 상황 | 동작 |
|------|------|
| 네트워크 오류 | "전송 실패: 네트워크 오류" 출력 |
| 잘못된 토큰 | "전송 실패: 토큰이 유효하지 않습니다" 출력 |
| 잘못된 ChatId | "전송 실패: ChatId가 유효하지 않습니다" 출력 |
| ini 파싱 오류 | "설정 파일 오류: 다시 설정해주세요" 출력 후 설정 모드 |

## 프로젝트 구조

```
NoljiMa/
├── NoljiMa.csproj
├── Program.cs             # 모든 로직 포함 (단일 파일)
├── docs/
│   └── NoljiMa.md
├── CHANGELOG.md
└── CLAUDE.md
```

### Program.cs 구성
- INI 파일 읽기/쓰기
- Telegram API 호출 (HttpClient 사용)
- 메인 로직 (인자 처리, 초기 설정 모드)
- 약 200줄, 외부 의존성 없음

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

### 배포 파일 생성

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
