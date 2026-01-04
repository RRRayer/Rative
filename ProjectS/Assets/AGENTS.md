# Agent.md (Repository Guidelines)

본 문서는 팀의 개발 속도와 안정성을 위해 **규칙을 명문화**합니다.
특히 본 프로젝트는 **Photon PUN 기반 멀티플레이**가 핵심이므로, 네트워크 관련 규칙은 반드시 준수합니다.

---

## 0. Quick Start
1. Unity Hub에서 프로젝트 열기
2. `Assets/00.Scenes/Launcher.unity` 오픈
3. Play

### 로컬 멀티 테스트 (권장 루틴)
- 에디터 1개 + 빌드 1개(또는 ParrelSync 클론 1개)로 2클라 접속 테스트
- 룸 입장/퇴장/재입장, 씬 동기화, 플레이어 중복 생성 여부 확인

---

## 1. Project Structure & Module Organization

### Core Folders
- `Assets/00.Scenes`: Unity scenes (`Launcher.unity`, 룸/인게임 씬 등)
- `Assets/01.Scripts`: 게임플레이/네트워크/UI 스크립트
- `Assets/02_Prefabs`: 재사용 prefab
- `Assets/03_Arts`: 머티리얼/아트 에셋
- `Assets/Photon`, `Assets/StarterAssets`, `Assets/TextMesh Pro`: third-party packages
- `Packages/`, `ProjectSettings/`: Unity 패키지/에디터 설정
- `Docs/`: 팀 문서

### 권장 Script 세분화(확장 시 유지보수용)
- `Assets/01.Scripts/Networking`
- `Assets/01.Scripts/Gameplay`
- `Assets/01.Scripts/Systems`
- `Assets/01.Scripts/UI`
- `Assets/01.Scripts/Editor`

(기존 구조를 바로 바꾸기 어렵다면, 새 코드부터 위 구조로 점진 이동)

---

## 2. Onboarding / Environment Setup (필수)

### Unity Version
- Unity: `6000.2.x` (프로젝트에 맞게 정확 버전 명시 권장)

### Photon PUN Setup
- Photon 설정 에셋: `PhotonServerSettings` (Project 창에서 검색)
- AppId 관리 정책(둘 중 하나 선택해서 명문화):
  - (A) AppId는 저장소에 커밋한다 (팀 내부 프로젝트용)
  - (B) AppId는 커밋하지 않는다 (개인 로컬에서만 설정)
    - 이 경우 `Docs/SETUP_PHOTON.md`에 설정 방법과 체크리스트를 작성하고,
      팀원이 AppId를 입력하지 않아 생기는 오류를 줄인다.

### Project Settings (Merge/협업 안정화)
- Editor > Version Control: **Visible Meta Files**
- Editor > Asset Serialization: **Force Text**
- `.meta` 파일은 에셋 변경 시 항상 함께 커밋

---

## 3. Build, Test, and Development Commands

### Run locally
- Unity Hub → 프로젝트 오픈 → `Assets/00.Scenes/Launcher.unity` → Play

### Build
- Unity Build Profiles 사용
  - `Assets/Settings/Build Profiles/Windows.asset`
- 또는 Editor 메뉴: File > Build Settings

### Tests
- 현재 전용 테스트 커맨드 없음
- 테스트 추가 시:
  - `Assets/Tests/` 하위에 명확히 배치
  - Unity Test Runner로 실행 (Edit > Test Runner)

---

## 4. Coding Style & Naming Conventions
- C# 4-space indentation
- Types/Methods: `PascalCase`
- Fields/Locals: `camelCase`
- Unity `.meta` 파일은 항상 함께 커밋
- 자동 생성 `.sln`/`.csproj`는 직접 수정 금지

---

## 5. Networking Rules (Photon PUN) - 필수 준수

### 5.1 Authority Model (PvE 기준 권장)
- 플레이어 입력/카메라/오디오는 **본인 오브젝트에서만**
  - `if (!photonView.IsMine) return;` 패턴 준수
- **MasterClient가 “게임 결과에 영향을 주는 결정”을 확정**
  - 웨이브 시작/종료, 드랍 생성, 경험치/레벨업 처리, 클리어/실패 판정 등

> PUN은 서버 권위가 아니라 클라 중 1명이 MasterClient이므로,
> MasterClient 변경(호스트 마이그레이션)에 대한 최소 정책을 갖는다.

### 5.2 Instantiate Policy
- 네트워크 Instantiate는 아래만 허용(기본 원칙):
  - 플레이어
  - 보스/엘리트(소수)
  - 팀 전체에 영향 주는 오브젝트(힐존/포탑/목표 등) 중 필요한 것만
- **잡몹은 원칙적으로 네트워크 Instantiate 금지**
  - 수십~수백 개체를 PhotonView로 동기화하지 않는다

### 5.3 “잡몹 로컬 생성” 표준 패턴
- MasterClient가 아래 이벤트만 전파:
  - `WaveStart(waveIndex, seed, difficulty, timeStamp)`
  - `EliteSpawn(eliteType, seedOrFixedId, spawnRule)`
- 각 클라:
  - 동일 규칙(시드 기반)으로 로컬 스폰/이동/피격 이펙트 처리
- MasterClient가 “합의가 필요한 결과”만 확정하여 브로드캐스트:
  - 사망 확정, 드랍 생성, 경험치 지급, 웨이브 클리어 등

### 5.4 State vs Event
- State(현재값): `IPunObservable` 또는 TransformView로 동기화
  - 예: 체력, 페이즈, 무기 상태, 로비 준비 상태 등
- Event(1회성): RPC 또는 RaiseEvent로 동기화
  - 예: 스킬 발동, 보스 패턴 트리거, 웨이브 시작, 드랍 생성 알림 등
- 금지:
  - 매 프레임 RPC 호출
  - 연사/이펙트 목적 RPC 남발

### 5.5 Event Code 중앙관리 (권장)
- RaiseEvent를 쓸 경우, 이벤트 코드는 중앙에서만 정의:
  - `Assets/01.Scripts/Networking/NetworkEventCodes.cs`
  - enum 또는 const byte로 관리
- PR 시 이벤트 추가/변경은 반드시 문서화

### 5.6 Room / Player Custom Properties 규약 (로비/매칭 안정화)
- PlayerProperties 키 예시(통일):
  - `ready` (bool), `classId` (int), `loadDone` (bool)
- RoomProperties 키 예시(통일):
  - `gameState` (int), `seed` (int), `wave` (int), `difficulty` (int)
- 키 이름은 팀에서 확정 후 변경 최소화

### 5.7 Scene Sync
- `PhotonNetwork.AutomaticallySyncScene = true` 기본 사용
- 씬 로드는 MasterClient만 수행:
  - `PhotonNetwork.LoadLevel(sceneName)`
- 씬 전환 시 플레이어/싱글톤 중복 생성 여부를 항상 확인

### 5.8 DontDestroyOnLoad 사용 규칙
- 네트워크로 생성되는 Player 프리팹에 무분별 적용 금지
- 싱글톤은 “중복 시 파괴” 템플릿을 통일하고,
  씬 이동/룸 이탈 시 상태 초기화 루틴을 명확히 둔다

---

## 6. Local Multiplayer Test Routine (PR 전 필수 확인)

### 최소 체크리스트
- [ ] 2클라(에디터+빌드 또는 에디터+클론) 접속 성공
- [ ] 룸 입장/퇴장/재입장 시 플레이어 중복 생성 없음
- [ ] MasterClient가 씬 로드하면 다른 클라가 정상 동기화됨
- [ ] 로컬/리모트 분리 정상(`photonView.IsMine` 기준 카메라/입력)
- [ ] RPC/Serialize 스팸 로그 없음(과도한 호출/지연 유발)

---

## 7. Asset & Scene Hygiene
- 씬은 `Assets/00.Scenes`, 프리팹은 `Assets/02_Prefabs` 중심 편집
- 가능한 로직은 프리팹/프리셋/ScriptableObject로 빼서 씬 변경을 최소화
- Third-party(Photon/StarterAssets) 수정 시:
  - PR에 변경 사유/내용을 반드시 기록

---

## 8. Commit & Pull Request Guidelines

### Commit Messages
- Conventional Commits + 한국어 요약
  - `feat: 로비 레디 상태 동기화 추가`
  - `fix: 룸 재입장 시 플레이어 중복 생성 수정`
  - `perf: 네트워크 동기화 빈도 최적화`

### PR Requirements
- 요약(무엇을/왜)
- 관련 이슈/작업 링크(있다면)
- 씬/UI 변경 시 스크린샷 또는 GIF

### PR Networking Checklist (네트워크 변경 시 필수)
- [ ] RPC 추가/변경됨: 목적/호출 빈도/대상 명시
- [ ] RaiseEvent 추가/변경됨: 이벤트 코드/페이로드 명시
- [ ] Serialize(IPunObservable) 변경됨: 변수 목록/주기 명시
- [ ] Room/Player Properties 키 추가/변경됨: 키 목록 명시
- [ ] 2클라 로컬 테스트 완료

---

## 9. Troubleshooting (자주 터지는 문제)
- 플레이어 중복 생성:
  - 씬 전환/룸 재입장 루틴, DontDestroyOnLoad, 싱글톤 중복 확인
- 리모트도 입력이 먹는 현상:
  - `photonView.IsMine` 가드 누락 확인
- 씬 동기화 안 됨:
  - `AutomaticallySyncScene` 설정과 LoadLevel 주체(Master) 확인
- 네트워크 끊김/렉:
  - RPC 남발 여부, Serialize 빈도, 동기화 대상 개수(특히 몹) 확인