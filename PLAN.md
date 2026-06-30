# 파빌라(Favilla) 사전과제 — 진행 계획 / 체크리스트

> MTM 프로젝트 인게임 유닛 **설계 능력** 테스트.
> Champion / Minion / Tower 유닛 설계 + 유닛↔HPBar 디커플링 설계.
> 컴파일 안 돼도 OK가 원칙이나 — **본 제출물은 실제로 빌드·실행·테스트까지 통과**시켜 기대 이상으로 검증한다.

---

## 0. 환경 확인 (완료)
- [x] TODO 폴더 4개 파일 정독 (`assignment-brief.txt`, `BattleManager.cs`, `DummyMonoBehaviour.cs`, `HPBar.cs`)
- [x] .NET Core 3.1 SDK / git / GitHub 토큰(repo scope, 계정 `MelonS`) 확인
- [x] MelonS-Agents 저장소 관련성 검토

## 1. 과제 분석 (핵심 요구사항)
- [x] **필수 (1)** — Champion / Minion / Tower 클래스
  - Champion : 죽으면 일정 시간 후 **부활**
  - Minion   : 죽으면 **소멸** + 생성 시 `BattleManager.CurrentGameSeconds`에 비례해 **공격력 세팅**
  - Tower    : 죽으면 **영구 파괴**
- [x] **선택 (2)** — 유닛 → HPBar **의존성 0**인 채로, HPBar가 **임의 대상**의 최신 HP를 반영
  - "보여주는(HPBar)" 객체와 "작동하는(코어 HP)" 객체의 책임 분리

## 2. 설계 (핵심 — 채점 대상)
- [x] `Health` 코어 + `IHealthReadOnly`(관찰 가능 추상) — 조합(composition)으로 HP 분리
- [x] `Unit` 추상 기반 (DummyMonoBehaviour 상속) + `TakeDamage` / 사망 처리
- [x] `Champion` / `Minion` / `Tower` — 사망 정책을 Template Method(`HandleDeath`)로 분기
- [x] `BattleManager` 실시간 클럭 + 부활 스케줄러 (CurrentGameSeconds 실제 구동)
- [x] `HPBar.Bind(IHealthReadOnly)` — Observer + DIP, 유닛은 HPBar를 전혀 모름
- [x] 이벤트 구독/해제 라이프사이클 안전성 (메모리 누수 방지)

## 3. +@ 실제 동작 검증 (기대 이상)
- [x] 콘솔 시뮬레이션: Blue vs Red 전투 루프(Unity Update 모사) + ASCII HP 대시보드
- [x] 세 가지 사망 행동 + 미니언 시간 비례 공격력 + HPBar 디커플링을 **실시간 시연**
- [x] 자동화 테스트로 모든 요구사항 검증 (22개 통과 / 부활·소멸·영구파괴·시간비례·HPBar·구조 디커플링)

## 4. 문서화
- [x] `README.md` — Mermaid 클래스/시퀀스/상태 다이어그램, 설계 의사결정·트레이드오프, 실행법, 결과 캡처
- [x] 폴더 구조 / 확장성 / 빌드·테스트 결과 시각화

## 5. 배포
- [x] GitHub **public** 저장소 생성 + 코드 push → https://github.com/MelonS/favilla-assignment
- [x] 제출용 단일 압축 파일(zip) 생성 + GitHub Release(v1.0) 첨부
- [x] README에 압축 파일 **다운로드 링크** 추가

## 6. 마무리 검증
- [x] `dotnet build` / `dotnet test`(22/22) / 시뮬레이션 실행 전부 통과 (로컬)
- [x] GitHub Actions CI 그린 확인 (windows-latest, 빌드+테스트+시뮬 스모크)
- [x] README 링크·다이어그램(Mermaid)·릴리스 자산 확인

---

## 7. 라이브 시각화 (추가 목표 — 정밀 검증 + 실행 가능)
- [x] C# 설계를 JS로 포팅한 **탑다운 전장 시뮬레이터** — 실제 유닛이 지형(잔디·길·강·베이스) 위 이동·교전
- [x] 디자인 시스템 컴포넌트: 토큰 / HP바(인터랙티브) / 유닛카드
- [x] **Claude Design** 디자인 시스템 프로젝트 생성 + 동기화
- [x] **실제 브라우저(headless Edge) 렌더로 검증** → `VERIFY PASS {5개 전부 true}` (타워 t=51 파괴)
- [x] README: 전장 **GIF**(`docs/battle.gif`) + 스크린샷 + 3가지 실행법
- [x] **실행파일(.exe, win-x64 self-contained)** 빌드 → GitHub Release 자산 업로드
- [x] **GitHub Pages 배포** → 링크 한 번으로 브라우저에서 바로 실행 (랜딩 + 전장)
  - https://melons.github.io/favilla-assignment/

---

## ✅ 최종 산출물
- **저장소(public):** https://github.com/MelonS/favilla-assignment
- **제출 zip:** https://github.com/MelonS/favilla-assignment/releases/latest/download/favilla-assignment-submission.zip
- **핵심 설계:** Champion/Minion/Tower (사망 정책 분기) + 유닛↔HPBar 디커플링(Observer+DIP)
- **+@:** 결정론적 콘솔 시뮬레이션(자기검증), xUnit 22개(행동+구조), CI(windows-latest)
- **라이브 시각화:** `design/simulator/index.html` (브라우저 실행) + Claude Design 동기화
