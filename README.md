# 프로젝트명
Project S

## 개요
1인칭 멀티 하이퍼 서바이벌 게임

## 기술 스택
- Unity C#
- Photon PUN
- AI Tools: ChatGPT(Codex), Gemini

## 주간 진행 상황

### Week 1 (25.12.22 - 12.28)
**작업 내역**
- 프로젝트 초기 구조/모듈 정리(Classes/Gameplay/Progression/UI 등)
- 기본 플레이어/씬/입력 스캐폴딩

**AI 활용**
- 설계 정리 및 코드 스캐폴딩 보조

**완료 기능**
- 기본 씬/플레이 플로우 진입

**커밋 로그**
- feat(#1): 구조 확정
- FEAT/2 (#1)

**링크**
- https://github.com/RRRayer/Rative/pull/2

**다음 주 계획**
- 클래스/스킬 시스템 기반 구현 및 테스트 씬 보강

### Week 2 (25.12.29 - 26.1.4)
**작업 내역**
- 클래스 시스템(ClassDefinition/PlayerClassState/TestClassFactory)
- 스킬 시스템(SkillSlot/SkillExecutor/SkillBehaviour)
- PlayerManager 통합
- TestLee 씬 및 기본 프리팹 추가
- 1인칭 컨트롤러 및 입력 확장
- 메인메뉴/로비/대기실 UI 및 네트워크 플로우 구축
- GameRoom/InGame/MainMenu 씬 추가 및 연결
- 클래스/스킬 ScriptableObject 테스트 데이터 추가
- 빌드 설정 업데이트(창 모드, 씬 포함)

**AI 활용**
- 스킬 실행 구조/전투 보정 설계 및 코드 적용
- 로비/대기실 UI 구조 설계 및 스크립트 골격 정리

**완료 기능**
- 기본 스킬 실행(콤보/대시/채널/콘)
- 클래스 스탯 적용 및 이동속도 반영
- 메인메뉴/로비/대기실 동작
- 게임룸/인게임 테스트 씬 구성
- 클래스 선택 UI 및 인게임 클래스 표시

**커밋 로그**
- feat(#3): 플레이어 구조 확장
- feat(#3): 클래스, 스킬 테스트 코드 추가
- feat(#3): 예시 클래스 생성
- FEAT/8 (#3)
- feat(#5): 메인메뉴 구현
- feat(#5): 대기실 구현
- feat(#5) 테스트 케이스 추가
- chore(#5): 빌드 설정 변경
- FEAT/10 (#5)

**테스트 결과**
- 플레이어 테스트 씬
<img width="1458" height="436" alt="image" src="https://github.com/user-attachments/assets/d0cf8f2c-27a6-4018-8643-b2c2a9e6aa23" />
- 온라인 로비 테스트 씬
<img width="3439" height="1439" alt="image" src="https://github.com/user-attachments/assets/77cee21e-cb04-4935-99a3-c31599a89e7f" />


**링크**
- https://github.com/RRRayer/Rative/pull/8
- https://github.com/RRRayer/Rative/pull/10

**다음 주 계획**
- 전사 스킬/업그레이드 디테일 구현, 전투 피드백 보강

### Week 3 (1.4 - 1.11)
**작업 내역**
- 전사 클래스 1차 구현 및 업그레이드 트랙/패시브 구조 도입
- 전투 시스템 확장: Airborne, Projectile, WeaponHitbox, SkillBehaviour 보강
- 공유 경험치/레벨링 및 업그레이드 UI/HUD 추가
- TestLee 씬/프리팹 보강 (전사/적/투사체/XP 픽업)
- 플레이어 프리팹 경로 정리

**AI 활용**
- 전사 업그레이드/전투 로직 설계 및 코드 보조
- UI/HUD/진행 시스템 설계 보조

**완료 기능**
- 전사 스킬 업그레이드 트랙 및 패시브 동작
- Airborne/투사체/히트박스 기반 전투 처리
- 공유 XP 및 업그레이드 선택 UI/HUD 동작
- TestLee 전투/프리팹 구성 보강

**커밋 로그**
- feat(#12): 전사 클래스 1차 구현
- feat(#12): 전사, 적 구현
- feat(#12): 플레이어 프리팹 위치 이동
- art(#9): 맵, 플레이어 에셋 추가
- feat(#9): 테스트 씬 생성
- feat: 프로빌더 패키지 추가
- art: 몬스터 추가
- feat(#9): 템페스트 애니메이션 추가

**테스트 결과**
- 플레이어 테스트 씬
<img width="1286" height="873" alt="스크린샷 2026-01-15 144940" src="https://github.com/user-attachments/assets/e63715f1-83a4-466b-b7f4-31035cefc7fa" />
<img width="917" height="514" alt="스크린샷 2026-01-15 145020" src="https://github.com/user-attachments/assets/891ac8e1-161f-46eb-b9fe-936aae14d897" />

**링크**
- https://github.com/RRRayer/Rative/pull/13
- https://github.com/RRRayer/Rative/pull/14

**다음 주 계획**
- GaneManager, 팀 공동 업그레이드 구현
- 실제 아트와 플레이어 연동
