---
name: release
description: NoljiMa 프로젝트의 새 버전을 릴리스합니다. 버전 업데이트, 문서 검증, CHANGELOG 작성, Git 태그 생성을 자동화합니다. "릴리스", "버전 올리기", "새 버전 배포" 등의 요청 시 사용됩니다.
---

# NoljiMa 릴리스 Skill

이 Skill은 NoljiMa 프로젝트의 새 버전을 릴리스하는 전체 프로세스를 자동화합니다.

## 릴리스 프로세스

### 1. 현재 버전 확인 및 검증

#### 1.1 다중 소스에서 버전 확인

다음 파일들에서 현재 버전을 읽어서 비교:

**installer.iss** (주 버전 소스):
```bash
grep "#define MyAppVersion" installer.iss
```

**NoljiMa.csproj**:
```bash
grep "<Version>" NoljiMa.csproj
grep "<AssemblyVersion>" NoljiMa.csproj
grep "<FileVersion>" NoljiMa.csproj
```

**CHANGELOG.md**:
```bash
# 최상단 버전 확인
head -n 20 CHANGELOG.md | grep "^## \["
```

**Git 태그**:
```bash
git tag --sort=-version:refname | head -n 5
```

#### 1.2 버전 일관성 검증

모든 소스에서 읽은 버전이 일치하는지 확인:

- ✅ 일치: 현재 버전으로 확정
- ❌ 불일치: 사용자에게 경고 후 수정 필요

**불일치 예시**:
```
⚠️  버전 불일치 발견:
- installer.iss: 0.1.5
- NoljiMa.csproj: 0.1.4
- CHANGELOG.md: 0.1.5
- Git 태그: v0.1.5

→ NoljiMa.csproj를 0.1.5로 수정해야 합니다.
```

#### 1.3 버전 히스토리 표시

사용자에게 버전 히스토리를 보여줍니다:

```
📋 버전 히스토리:
현재: v0.1.5 (2025-12-29)
이전: v0.1.4 (2025-12-28)
     v0.1.3 (2025-12-27)
     v0.1.2 (2025-12-26)
     v0.1.1 (2025-12-25)
```

#### 1.4 새 버전 번호 결정

사용자에게 새 버전 번호를 물어봅니다:

**Semantic Versioning 규칙 설명**:
- **MAJOR** (X.0.0): Breaking changes (호환성 깨짐)
- **MINOR** (0.X.0): 새 기능 추가 (하위 호환)
- **PATCH** (0.0.X): 버그 수정 (하위 호환)

**추천 버전 제시**:
```
현재 버전: 0.1.5

다음 버전 추천:
- 0.1.6 (패치) - 버그 수정
- 0.2.0 (마이너) - 새 기능 추가
- 1.0.0 (메이저) - 첫 정식 릴리스

새 버전 번호를 입력하세요:
```

#### 1.5 버전 번호 검증

입력받은 버전 번호 검증:

- ✅ Semantic Versioning 형식 (X.Y.Z)
- ✅ 현재 버전보다 높은 버전
- ✅ 논리적 증가 (예: 0.1.5 → 0.3.0은 경고)

**검증 실패 예시**:
```
❌ 잘못된 버전: "0.1.a"
   Semantic Versioning 형식이 아닙니다. (X.Y.Z 형식 필요)

❌ 잘못된 버전: "0.1.3"
   현재 버전 (0.1.5)보다 낮습니다.

⚠️  경고: "0.3.0"
   0.1.5 → 0.3.0로 건너뛰기는 비정상적입니다.
   계속 진행하시겠습니까? (y/n)
```

### 2. 버전 정보 업데이트

다음 파일들의 버전을 업데이트합니다:

#### NoljiMa.csproj
```xml
<Version>새버전</Version>
<AssemblyVersion>새버전.0</AssemblyVersion>
<FileVersion>새버전.0</FileVersion>
```

#### installer.iss
```iss
#define MyAppVersion "새버전"
```

### 3. 변경 사항 확인

사용자에게 이번 릴리스의 변경 사항을 물어봅니다:

```
이번 릴리스에 포함된 변경 사항을 알려주세요:

Added (새로운 기능):
-

Changed (변경된 기능):
-

Fixed (버그 수정):
-

Removed (제거된 기능):
-
```

사용자 입력을 받아서 CHANGELOG.md 업데이트 준비.

### 4. CHANGELOG.md 업데이트

3단계에서 받은 변경 사항을 바탕으로 CHANGELOG.md 상단에 새 버전 섹션 추가:

```markdown
## [새버전] - YYYY-MM-DD

### Added
- 새로운 기능들...

### Changed
- 변경된 사항들...

### Fixed
- 버그 수정들...

### Removed
- 제거된 기능들...
```

**중요**:
- 날짜는 오늘 날짜 사용 (YYYY-MM-DD 형식)
- 변경 사항은 3단계에서 받은 내용 사용
- Keep a Changelog 형식 준수

### 5. 문서 무결성 검증

다음 항목들을 검증:

#### 버전 일관성
- NoljiMa.csproj 버전
- installer.iss 버전
- CHANGELOG.md 버전
- 모두 동일한지 확인

#### README.md 검증 및 갱신

**중요**: 릴리스 전 README.md가 최신 기능을 반영하는지 확인하고, 필요시 갱신합니다.

**검증 항목**:
1. **명령어 옵션 확인**
   - Program.cs의 PrintHelp() 함수와 README.md의 "명령어 옵션" 섹션 비교
   - 누락된 옵션이 있는지 확인 (예: --help, --clear-offset, --wait, --timeout)

2. **기능 설명 확인**
   - Program.cs에 구현된 모든 기능이 README.md에 설명되어 있는지
   - offset.txt 파일 설명 (자동 생성 섹션)
   - 메시지 형식 검증 규칙

3. **사용 예시 확인**
   - 예제 명령어가 실제로 동작하는지 검증
   - Exit Code 테이블 정확성

4. **링크 검증**
   - GitHub 저장소 링크 (예: Releases 페이지)
   - 외부 링크 (BotFather, userinfobot)
   - README.md 내부 참조 링크

**갱신 프로세스**:
```bash
# 1. PrintHelp 함수 내용 읽기
grep -A 10 "static void PrintHelp" Program.cs

# 2. README.md의 "명령어 옵션" 섹션과 비교
grep -A 20 "### 명령어 옵션" README.md

# 3. 불일치 발견 시 README.md 업데이트
# - 누락된 옵션 추가
# - 설명 보강
# - 예제 코드 업데이트
```

**체크리스트**:
- [ ] PrintHelp() 내용이 README.md에 모두 반영되었는지
- [ ] 새로 추가된 기능이 "사용법" 섹션에 설명되었는지
- [ ] offset.txt 설명이 "설정 파일" 섹션에 있는지
- [ ] 빌드 명령어가 실제 사용과 일치하는지
- [ ] Telegram Bot 설정 가이드가 최신인지

#### CLAUDE.md 검증
- 문서 규칙 준수 확인
- Git 규칙 존재 확인

#### docs/NoljiMa.md 검증
- 명세서 내용이 최신인지
- 빌드 방법 확인

#### 프로젝트 구조 검증
```bash
# 필수 파일 존재 확인
- CHANGELOG.md (존재)
- CLAUDE.md (존재)
- README.md (존재)
- NoljiMa.csproj (존재)
- installer.iss (존재)
- Program.cs (존재)
- docs/NoljiMa.md (존재)

# 불필요한 파일 확인
- nul 파일 (없어야 함)
- RELEASE_NOTES_*.md (없어야 함)
```

### 6. 변경 사항 보고

사용자에게 다음 정보를 보고:
- 업데이트된 파일 목록
- 버전 변경 내역 (이전 → 새 버전)
- CHANGELOG 추가 내용
- 검증 결과 요약

### 7. Git 커밋 및 푸시

```bash
git add .
git commit -m "release: prepare v새버전

- Update version to 새버전 in all files
- Update CHANGELOG.md with v새버전 changes
- [변경 사항 요약]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

git push
```

**중요**: 태그는 빌드 성공 후에 생성합니다.

### 8. 빌드 및 테스트

#### 8.1 프로젝트 빌드
```bash
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

#### 8.2 빌드 테스트

빌드된 실행 파일이 정상 동작하는지 확인:
```bash
./publish/NoljiMa.exe
```

예상 출력: "사용법: NoljiMa \"메시지\""

**빌드 실패 시**:
- 문제 수정 후 재빌드
- 태그 생성 안 함
- 사용자에게 오류 보고

#### 8.3 포터블 zip 생성
```bash
cd publish && tar -a -c -f ../NoljiMa-v새버전-portable.zip *
```

#### 8.4 인스톨러 생성
```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

출력: `installer-output/NoljiMa-v새버전-Setup.exe`

#### 8.5 인스톨러 테스트

**중요**: Setup.exe를 직접 실행하여 설치 및 PATH 등록을 테스트합니다.

```bash
# 인스톨러 실행 (GUI 설치 진행)
./installer-output/NoljiMa-v새버전-Setup.exe
```

**테스트 체크리스트**:
1. 인스톨러 UI가 정상적으로 표시되는지 확인
2. .NET 8 Runtime 체크가 동작하는지 확인
3. PATH 추가 옵션이 **기본으로 체크**되어 있는지 확인
4. 설치 완료 후 새 명령 프롬프트에서 `NoljiMa` 실행 확인
5. PATH 등록 확인:
   ```bash
   where NoljiMa
   # 예상: C:\Program Files\NoljiMa\NoljiMa.exe
   ```

**테스트 실패 시**:
- 인스톨러 스크립트 수정
- 재빌드 (`8.4`부터 다시 실행)
- 태그 생성 안 함

**테스트 완료 후 정리**:
- 제어판에서 "NoljiMa" 제거
- 또는 다음 릴리스 시 덮어쓰기 설치

### 9. Git 태그 생성 및 푸시

**빌드 성공 확인 후** Git 태그 생성:

```bash
git tag -a v새버전 -m "NoljiMa v새버전

[CHANGELOG 내용을 여기에 복사]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

git push --tags
```

### 10. GitHub Release 생성

```bash
gh release create v새버전 \
  --title "NoljiMa v새버전" \
  --notes "[CHANGELOG 내용]" \
  NoljiMa-v새버전-portable.zip \
  installer-output/NoljiMa-v새버전-Setup.exe
```

### 11. 다음 단계 안내

릴리스 완료 후 사용자에게 확인사항 안내:

1. **GitHub Release 확인**
   - https://github.com/seeper0/NoljiMa/releases/tag/v새버전
   - 파일 업로드 확인 (portable.zip, Setup.exe)

2. **릴리스 테스트**
   - 포터블 버전 다운로드 및 실행 테스트
   - 인스톨러 설치 및 PATH 등록 테스트

3. **릴리스 공지** (선택)
   - README.md 업데이트 (필요시)
   - 사용자 공지 (필요시)

## 오류 처리

### 버전 형식 오류
- Semantic Versioning (X.Y.Z) 형식이 아닌 경우 재입력 요청
- 이전 버전보다 낮은 버전인 경우 경고

### 파일 누락
- 필수 파일이 없는 경우 오류 메시지 표시
- 계속 진행할지 사용자에게 확인

### Git 오류
- 커밋되지 않은 변경사항이 있는 경우 경고
- 푸시 실패 시 원인 안내

### 검증 실패
- 버전 불일치 발견 시 수정 후 재검증
- 문서 오류 발견 시 사용자에게 알림

### Inno Setup 미설치
- ISCC.exe를 찾을 수 없는 경우:
  - 포터블 버전만 먼저 릴리스
  - 인스톨러는 수동으로 나중에 추가하도록 안내

## 예시

### 사용 시나리오 1: 패치 버전 릴리스

**사용자**: "버그 수정했으니 v0.1.1로 릴리스해줘"

**Skill 실행**:
1. 현재 버전 확인 (0.1.0)
2. 새 버전 확인 (0.1.1)
3. 변경 사항 물어보기
4. 파일 업데이트
5. 검증
6. 커밋 & 푸시
7. 빌드 (portable.zip, Setup.exe)
8. GitHub Release 생성
9. 다음 단계 안내

### 사용 시나리오 2: 마이너 버전 릴리스

**사용자**: "새 기능 추가했으니 0.2.0으로 릴리스"

**Skill 실행**:
1. 버전 0.1.0 → 0.2.0 확인
2. Added 섹션에 새 기능 추가 요청
3. 전체 프로세스 실행

## 주의사항

1. **다중 소스에서 버전 검증**: installer.iss, NoljiMa.csproj, CHANGELOG.md, Git 태그 모두 확인
2. **버전 일관성 유지**: 모든 파일의 버전이 일치해야 함
3. **버전 번호 검증**: Semantic Versioning 형식 준수, 현재 버전보다 높은 버전
4. **CHANGELOG.md 맨 위에 추가**: 역순 정렬 유지
5. **날짜 형식**: YYYY-MM-DD (예: 2025-12-27)
6. **커밋 메시지**: "release: prepare v버전" 형식 사용
7. **태그 메시지**: CHANGELOG 내용 포함
8. **nul 파일 검증**: Git 커밋 전 자동 검증
9. **단일 파일 구조**: Program.cs만 존재, 분리된 파일 없음

## 필수 도구

- Git (커밋, 태그, 푸시)
- GitHub CLI (`gh`) - Release 생성용
- .NET 8.0 SDK (빌드용)
- Inno Setup (인스톨러 생성용, 선택)
- tar (zip 생성용, Windows 10+ 내장)

## 체크리스트

릴리스 전 확인사항:
- [ ] Program.cs 테스트 완료
- [ ] 문서 업데이트 완료 (README.md, docs/NoljiMa.md)
- [ ] Breaking changes 확인
- [ ] 버전 번호 결정 (Semantic Versioning)
- [ ] CHANGELOG 작성 준비

릴리스 후 확인사항:
- [ ] GitHub Release 생성 확인
- [ ] portable.zip 다운로드 테스트
- [ ] Setup.exe 설치 테스트
- [ ] PATH 등록 동작 확인
- [ ] Telegram 메시지 전송 테스트

## NoljiMa 특화 기능

### INI 설정 파일
- 릴리스 시 NoljiMa.ini는 포함하지 않음
- 사용자가 초기 설정 모드로 직접 생성

### 단일 파일 아키텍처
- Program.cs 하나로 모든 로직 구현
- ConfigManager.cs, TelegramSender.cs 등 분리 파일 없음
- 외부 의존성 최소화 (System.Text.Json만 사용)

### PATH 자동 등록
- Setup.exe 설치 시 PATH 옵션 제공
- 포터블 버전은 수동 PATH 등록 필요
