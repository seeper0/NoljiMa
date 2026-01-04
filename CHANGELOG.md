# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.2] - 2026-01-04

### Added
- 메시지 전송과 동시에 응답 대기하는 `--wait` 조합 기능
- `--wait "[패턴]"` 사용 시 메시지 앞에 패턴 자동 붙이기

### Changed
- help 메시지에 새로운 사용법 추가

### Documentation
- CLAUDE.md에 세션 시작 시 git 동기화 규칙 추가
- CLAUDE.md에 스킬 자동 실행 규칙 추가

## [0.3.1] - 2026-01-03

### Changed
- 메시지 전송 성공 시 최신 update_id로 offset 자동 갱신
- `--wait` 실행 시 전송한 메시지 이후의 응답만 수신하도록 개선

## [0.3.0] - 2026-01-03

### Added
- `--help`, `-h` 옵션으로 사용법 확인 가능
- `--clear-offset` 옵션으로 offset.txt 초기화 가능
- Global Mutex로 병렬 실행 자동 방지 (시스템 전체에서 단일 인스턴스만 실행)
- InitialSetup에 Telegram Bot 설정 가이드 링크 추가
- README에 핵심 워크플로우 섹션 추가 (Claude와 소통 패턴)
- README에 명령어 옵션 섹션 추가
- README에 병렬 실행 자동 방지 설명 추가
- 릴리스 스킬에 README.md 검증 및 갱신 프로세스 추가

### Changed
- **Breaking Change**: 메시지 전송 시 `[ID]` 형식 필수 요구사항 제거 (모든 텍스트 전송 가능)
- `--wait` 패턴을 선택적으로 변경 (패턴 없으면 첫 번째 새 메시지 자동 수신)
- offset 기반으로 새 메시지 자동 구분
- `PrintHelp()` 함수로 help 메시지 중복 코드 제거
- README 전면 개편 (사용법 간소화, 워크플로우 명확화)

## [0.2.2] - 2026-01-01

### Fixed
- 재시도 중 "폴링 실패" 메시지 출력 개선 → "네트워크 오류 감지, 재시도 중..." 표시
- 재시도 진행 상황 표시 추가 (1/3, 2/3, 3/3)
- 최대 재시도 횟수 초과 시에만 "폴링 실패" 메시지 출력

## [0.2.1] - 2026-01-01

### Added
- 메시지 형식 검증: `[ID] 내용` 형식 필수
- wait 패턴 검증: `[키]`만 허용 (내용 포함 불가)
- 파라미터 검증: 누락된 패턴, 알 수 없는 옵션 감지

### Changed
- 기본 타임아웃: 300초 → 86400초 (24시간)
- Long Polling timeout: 가변 → 고정 30초
- 대기 간격: 5분부터 시작, 지수 증가하여 최대 1시간
- Sleep 전 타임아웃 체크 추가로 정확한 타임아웃 보장

### Fixed
- `--wait` 파라미터만 입력 시 "--wait"을 메시지로 전송하는 버그 수정

## [0.2.0] - 2026-01-01

### Added
- **쌍방향 소통 기능**: 텔레그램 메시지 응답 대기 기능 추가 (`--wait`, `--timeout` 파라미터)
- getUpdates API를 통한 Long Polling 구현 (30초 간격)
- offset 파일 관리로 메시지 중복 방지 (`%LOCALAPPDATA%\NoljiMa\offset.txt`)

## [0.1.5] - 2025-12-29

### Changed
- Config 파일 경로 변경: `%AppData%\NoljiMa\NoljiMa.ini` → `%LocalAppData%\NoljiMa\config.ini`
- Local 폴더 사용으로 로밍 프로필 동기화 제외 및 성능 개선

## [0.1.4] - 2025-12-28

### Fixed
- 설정 파일 저장 실패 시 테스트 메시지가 전송되는 문제 수정
- 설정 파일 저장 실패 시 프로그램이 정상 종료되도록 개선

### Changed
- 설정 파일 위치를 사용자 폴더(`%APPDATA%\NoljiMa\`)로 변경하여 권한 문제 해결
- Program Files 설치 시에도 관리자 권한 없이 설정 저장 가능

## [0.1.3] - 2025-12-28

### Changed
- 인스톨러 설치 완료 후 선택 옵션 추가 (프로그램 실행, GitHub 페이지 방문)
- 시작 메뉴에 README 및 프로젝트 페이지 링크 추가
- 설치 폴더에 README.md 파일 포함

## [0.1.2] - 2025-12-27

### Changed
- 릴리스 프로세스 개선 (빌드 → 태그 순서 변경, 인스톨러 테스트 단계 추가)
- 인스톨러 PATH 등록 시 WM_SETTINGCHANGE 브로드캐스트 추가 (즉시 반영)

## [0.1.1] - 2025-12-27

### Changed
- 인스톨러에서 PATH 환경 변수 추가를 기본값으로 설정

### Documentation
- README.md 및 릴리스 스킬 문서 개선

## [0.1.0] - 2025-12-27

### Added
- Telegram Bot API 연동 (HttpClient 사용)
- INI 파일 설정 관리 (직접 파싱)
- 초기 설정 모드 (BotToken, ChatId 입력 후 테스트 전송)
- 명령줄 인자 처리 (메시지 전송)
- 에러 처리 (네트워크, 토큰, ChatId 검증)
- 단일 파일 구현 (Program.cs, 203줄)

### Technical
- .NET 8.0 Console App
- 외부 의존성 없음 (System.Text.Json만 사용)
- 프레임워크 종속 배포 (~178KB)

---

**Telegram 알림을 간편하게 ⚡**
