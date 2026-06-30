// BattleManager는 정적(static) 전역 상태(게임 시간/스케줄러)를 갖는다.
// 테스트가 병렬로 돌면 이 상태가 경쟁(race)하므로 어셈블리 전역 병렬화를 끈다.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
