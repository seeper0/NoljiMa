---
name: submit
description: 작업 내용을 원격 저장소에 올립니다. "작업 올려줘", "작업 제출해줘", "커밋하고 푸시해줘", "작업 저장하고 올려줘", "오늘 작업 내용 올리자" 등의 요청 시 사용됩니다.
---

# Submit Skill

이 Skill은 작업한 내용을 원격 저장소(GitHub)에 올리는 전체 프로세스를 자동화합니다.

## 프로세스 순서

### 1. 올릴 내용 정리 및 확인

#### 1.1 변경 파일 확인
```bash
git status
```

다음 정보 표시:
- 수정된 파일 (modified)
- 추가된 파일 (new file)
- 삭제된 파일 (deleted)
- 추적되지 않는 파일 (untracked)

#### 1.2 변경 내용 요약
```bash
git diff --stat
```

파일별 변경량 표시:
```
Program.cs           | 208 +++++++++++++++++++++++
docs/NoljiMa.md      | 145 ++++++++++-------
docs/시나리오.md     | 350 ++++++++++++++++++++++++++++++++++++++++
3 files changed, 612 insertions(+), 91 deletions(-)
```

#### 1.3 nul 파일 검증 (CLAUDE.md 규칙)

Windows에서 잘못 생성될 수 있는 nul 파일을 검색하고 삭제:

```bash
find . -name "nul" -type f -delete 2>/dev/null
```

또는 PowerShell:
```powershell
Get-ChildItem -Path . -Recurse -Force -Filter "nul" -File | Remove-Item -Force
```

검증 결과 표시:
- ✅ nul 파일 없음
- ⚠️ nul 파일 발견 및 삭제: [경로]

#### 1.4 변경 내용 분석

수정된 파일들을 분석하여 커밋 타입 및 메시지 제안:

**분석 기준**:
1. **파일 종류**:
   - `*.md` 파일만 → `docs:`
   - `Program.cs` → `feat:` 또는 `fix:` 또는 `refactor:`
   - `*.csproj`, `installer.iss` → `chore:`
   - `.claude/skills/**` → `chore:` 또는 `feat:`
   - 테스트 파일 → `test:`

2. **변경 패턴**:
   - 새 파일 추가 → `feat: add ...`
   - 버그 수정 (fix, bug 키워드) → `fix: ...`
   - 문서만 수정 → `docs: update ...`
   - 리팩토링 → `refactor: ...`
   - 빌드/배포 관련 → `chore: ...`

3. **git diff 분석**:
   - 추가된 함수/기능 확인
   - 주요 변경 내용 추출
   - 파일별 변경 요약

#### 1.5 커밋 메시지 제안

분석 결과를 바탕으로 커밋 메시지 자동 생성:

**형식**:
```
[타입]: [간단한 설명]

- [주요 변경사항 1]
- [주요 변경사항 2]
- [주요 변경사항 3]
```

**예시 1** (기능 추가):
```
feat: add message waiting feature

- Add --wait and --timeout parameters
- Implement getUpdates API integration
- Add offset management for polling
- Update documentation with new format
```

**예시 2** (문서 업데이트):
```
docs: update message format samples

- Change format from [ID][내용] to [ID] 내용
- Update all examples in NoljiMa.md
- Update scenario examples
```

**예시 3** (스킬 추가):
```
chore: add submit skill for workflow automation

- Create submit skill directory
- Add SKILL.md with workflow documentation
- Define commit message suggestion logic
```

#### 1.6 사용자 확인

제안된 메시지를 표시하고 선택 요청:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
제안된 커밋 메시지:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
feat: add message waiting feature

- Add --wait and --timeout parameters
- Implement getUpdates API integration
- Add offset management for polling
- Update documentation with new format
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

변경된 파일: 5개
추가: +612줄, 삭제: -91줄

이 메시지를 사용하시겠습니까?
  y: 그대로 사용
  n: 취소
  edit: 직접 수정
```

#### 1.7 메시지 수정 (edit 선택 시)

AskUserQuestion 도구를 사용하여 커밋 메시지 입력 받기:

```
커밋 메시지를 입력해주세요:

제목 (필수):
> [사용자 입력]

본문 (선택, 여러 줄 가능):
> [사용자 입력]
```

#### 1.8 최종 확인

확정된 커밋 메시지와 변경 파일 목록을 다시 표시:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
최종 확인
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
커밋 메시지:
  feat: add message waiting feature

  - Add --wait and --timeout parameters
  - Implement getUpdates API integration

변경된 파일:
  M  Program.cs
  M  docs/NoljiMa.md
  A  docs/시나리오.md

계속 진행하시겠습니까? (y/n)
```

### 2. 동기화 (원격 변경사항 가져오기)

#### 2.1 원격 상태 확인
```bash
git fetch origin
```

원격 저장소의 최신 상태를 가져옵니다 (로컬 파일은 변경 안 함).

#### 2.2 로컬/원격 비교

현재 브랜치와 원격 브랜치를 비교:

```bash
# 현재 브랜치 확인
git branch --show-current

# 로컬과 원격 커밋 비교
git rev-list --left-right --count HEAD...origin/main
```

**상태 분석**:
- `0 0`: 동일 (동기화됨)
- `N 0`: 로컬이 N개 커밋 앞섬 (푸시만 하면 됨)
- `0 N`: 로컬이 N개 커밋 뒤처짐 (풀 필요)
- `N M`: 분기됨 (풀 후 병합 필요, 충돌 가능성)

**상태별 메시지**:
```
✅ 로컬과 원격이 동기화되어 있습니다.
→ 바로 커밋 및 푸시 진행

⚠️ 원격에 새로운 커밋이 있습니다 (2개 뒤처짐).
→ 먼저 원격 변경사항을 가져옵니다.

⚠️ 로컬과 원격이 분기되었습니다 (로컬 +3, 원격 +2).
→ 병합이 필요하며 충돌이 발생할 수 있습니다.
```

#### 2.3 원격 변경사항 병합

원격이 앞서있거나 분기된 경우 병합:

```bash
# 옵션 1: rebase (깔끔한 히스토리, 권장)
git pull --rebase origin main

# 옵션 2: merge (병합 커밋 생성)
git pull origin main
```

**권장**: `--rebase` 사용 (선형 히스토리 유지)

#### 2.4 충돌 처리

충돌 발생 시:

```
❌ 병합 중 충돌이 발생했습니다:

충돌 파일:
  Program.cs
  docs/NoljiMa.md

다음 단계:
1. 충돌 파일을 열어서 수동으로 해결하세요
2. 해결 후 다음 명령 실행:
   git add .
   git rebase --continue

또는 병합 취소:
   git rebase --abort
```

**충돌 해결 후**:
- 사용자가 직접 해결
- `git add .` 실행
- `git rebase --continue` 실행
- Skill 재개

**충돌 취소 시**:
- `git rebase --abort`
- 작업 중단, 사용자에게 안내

### 3. 커밋

#### 3.1 스테이징

모든 변경사항을 스테이징:

```bash
git add .
```

**중요**:
- `.gitignore`에 정의된 파일은 자동 제외
- nul 파일은 1.3 단계에서 이미 삭제됨

#### 3.2 커밋 생성

확정된 메시지로 커밋 생성:

```bash
git commit -m "확정된 메시지

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"
```

**형식**:
```
[타입]: [제목]

- [본문 라인 1]
- [본문 라인 2]

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>
```

#### 3.3 커밋 확인

생성된 커밋 확인:

```bash
git log -1 --oneline
git log -1 --stat
```

출력 예시:
```
fb8af4c feat: add message waiting feature
 Program.cs           | 208 +++++++++++++++++
 docs/NoljiMa.md      | 145 +++++++-----
 docs/시나리오.md     | 350 ++++++++++++++++++++++++++++
 3 files changed, 612 insertions(+), 91 deletions(-)
```

### 4. 푸시

#### 4.1 푸시 실행

현재 브랜치를 원격 저장소에 푸시:

```bash
# 현재 브랜치 확인
BRANCH=$(git branch --show-current)

# 푸시
git push origin $BRANCH
```

일반적으로: `git push origin main`

#### 4.2 푸시 결과 확인

푸시 성공 시:
```
✅ 푸시 성공!

브랜치: main
커밋: fb8af4c feat: add message waiting feature
원격: https://github.com/seeper0/NoljiMa
```

푸시 실패 시:
```
❌ 푸시 실패: [오류 메시지]

가능한 원인:
- 권한 없음: GitHub 인증 확인 필요
- 원격이 더 최신: git pull 필요
- 브랜치 보호: Pull Request 필요
```

#### 4.3 GitHub 링크 제공 (선택)

커밋 URL 생성 및 안내:

```
🔗 GitHub에서 보기:
https://github.com/seeper0/NoljiMa/commit/fb8af4c
```

URL 형식:
```
https://github.com/{owner}/{repo}/commit/{commit-hash}
```

## 에러 처리

### 변경사항 없음
```
ℹ️ 커밋할 변경사항이 없습니다.
Working tree is clean.
```

### Git 저장소 아님
```
❌ 현재 디렉토리가 Git 저장소가 아닙니다.
git init으로 저장소를 초기화하거나 Git 저장소로 이동하세요.
```

### 원격 저장소 미설정
```
❌ 원격 저장소가 설정되지 않았습니다.

다음 명령으로 원격 저장소를 추가하세요:
git remote add origin https://github.com/username/repo.git
```

### 병합 충돌
```
⚠️ 병합 충돌이 발생했습니다.
파일을 수동으로 수정한 후:
  git add .
  git rebase --continue

또는 병합 취소:
  git rebase --abort
```

### 인증 실패
```
❌ GitHub 인증 실패

해결 방법:
1. Personal Access Token 확인
2. SSH 키 설정 확인
3. git config credential.helper 설정 확인
```

## 커밋 타입 참고

NoljiMa 프로젝트에서 사용하는 커밋 타입:

| 타입 | 설명 | 예시 |
|------|------|------|
| `feat` | 새로운 기능 추가 | feat: add --wait parameter |
| `fix` | 버그 수정 | fix: resolve timeout issue |
| `docs` | 문서 수정 | docs: update README |
| `refactor` | 리팩토링 (기능 변경 없음) | refactor: simplify error handling |
| `chore` | 빌드/도구 변경 | chore: update .gitignore |
| `test` | 테스트 추가/수정 | test: add unit tests |
| `release` | 릴리스 준비 | release: prepare v0.2.0 |

## 사용 예시

### 예시 1: 기능 추가 후

**사용자**: "작업 올려줘"

**Skill 실행**:
1. 변경 파일 확인: `Program.cs`, `docs/NoljiMa.md`
2. 분석 결과: 새 함수 추가 감지
3. 제안 메시지:
   ```
   feat: add message waiting feature

   - Add WaitForMessage function
   - Implement getUpdates API
   ```
4. 사용자 확인: `y`
5. 동기화 → 커밋 → 푸시
6. 완료!

### 예시 2: 문서만 수정

**사용자**: "문서 수정했으니 올려"

**Skill 실행**:
1. 변경 파일 확인: `docs/NoljiMa.md`, `docs/시나리오.md`
2. 분석 결과: `.md` 파일만 수정
3. 제안 메시지:
   ```
   docs: update message format samples

   - Change format from [ID][내용] to [ID] 내용
   - Update scenario examples
   ```
4. 사용자 확인: `y`
5. 동기화 → 커밋 → 푸시
6. 완료!

### 예시 3: 메시지 수정

**사용자**: "오늘 작업 올리자"

**Skill 실행**:
1. 제안 메시지 표시
2. 사용자 선택: `edit`
3. 메시지 입력 요청:
   ```
   제목: feat: implement polling and documentation
   본문:
   - Add --wait and --timeout parameters
   - Update all documentation
   - Add usage scenarios
   ```
4. 최종 확인: `y`
5. 동기화 → 커밋 → 푸시
6. 완료!

## 체크리스트

실행 전:
- [ ] 작업 내용이 완료되었는가?
- [ ] 테스트를 거쳤는가?
- [ ] 불필요한 파일이 포함되지 않았는가?
- [ ] Breaking changes가 있는가? (있다면 커밋 메시지에 명시)

실행 후:
- [ ] GitHub에서 커밋 확인
- [ ] CI/CD 통과 확인 (있는 경우)
- [ ] 원격 저장소와 동기화 확인

## NoljiMa 특화 규칙

### 1. nul 파일 자동 삭제
CLAUDE.md 규칙에 따라 Windows에서 생성될 수 있는 nul 파일을 자동으로 검색하고 삭제합니다.

### 2. 커밋 메시지 형식
- Conventional Commits 형식 사용
- 제목은 50자 이내
- 본문은 72자마다 줄바꿈
- Co-Authored-By 자동 추가

### 3. 단일 파일 구조
Program.cs 하나로 모든 로직이 구현되어 있으므로, Program.cs 변경 시 영향도가 큼을 고려

### 4. 문서 일관성
docs/ 폴더의 문서들은 항상 일관성을 유지해야 하므로, 관련 문서들이 함께 수정되었는지 확인
