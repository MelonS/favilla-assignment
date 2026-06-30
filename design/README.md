# MTM — 라이브 시각화 (Claude Design 디자인 시스템)

핵심 C# 설계(`../src/MTM.Units`)를 **JS로 그대로 포팅**해, 설치 없이 브라우저에서 직접 조작·검증하는 인터랙티브 시각화입니다.
[Claude Design](https://claude.ai/design) 디자인 시스템 프로젝트로 제작/동기화되었습니다.

## 실행 (설치 불필요)

파일을 브라우저로 열기만 하면 됩니다.

| 파일 | 설명 |
|---|---|
| [`simulator/index.html`](simulator/index.html) | **인터랙티브 전투 시뮬레이터** — ▶재생/한 틱/리셋/속도 + 라이브 자기검증 패널 |
| [`components/hp-bar.html`](components/hp-bar.html) | HP바 컴포넌트 — `HPBar.bind(IHealthReadOnly)` 관찰자, 슬라이더로 직접 데미지 |
| [`components/unit-card.html`](components/unit-card.html) | 유닛 카드 — Champion/Minion/Tower × (ALIVE/REVIVING/DESTROYED) |
| [`foundations/tokens.html`](foundations/tokens.html) | 색·타이포 토큰 + 의존 방향(DIP) 다이어그램 |

## 무엇을 검증하나

시뮬레이터는 .NET 콘솔 시뮬레이션과 **동일한 설계·동일한 공식**(챔피언 ATK=60+(레벨−1)*8, 미니언 ATK=10+생성초/2, Health/HPBar 관찰자)을 공유하되, **튜닝 상수·웨이브 수·유닛 HP·전투 모델·시나리오는 서로 다른** 별개의 전장을 결정론적으로 돌리며 5개 요구사항을 라이브로 확인합니다.

- 챔피언: 죽으면 일정 시간 후 **부활** (Reviving → Alive)
- 미니언: 죽으면 **소멸**(전장(actors 목록)에서 제거, 재등록 없음) + **생성 시간 비례 공격력**
- 타워: 죽으면 **영구 파괴**
- HPBar: 유닛 의존성 0으로 임의 대상 HP 실시간 반영 (매 틱 `bar == health`)

> 실제 브라우저(headless Edge) 렌더에서 `VERIFY PASS {revive, despawn, scaled, tower, hpbar: 전부 true}` 확인 완료.
> 동일 설계가 **.NET**과 **웹**에서 똑같이 동작 — 설계 이식성(portability)의 증거.

## Claude Design 연동

이 폴더는 Claude Design 프로젝트 **"Favilla MTM — Battle Visualizer"** 와 동기화됩니다.
Claude Design에서 컴포넌트를 이어서 디자인하고, `/design-sync`로 코드베이스와 양방향 동기화할 수 있습니다.
