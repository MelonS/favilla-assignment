using Battle;
using Xunit;

namespace MTM.Units.Tests
{
    /// <summary>타워: 죽으면 영구 파괴.</summary>
    public class TowerTests
    {
        public TowerTests() => BattleManager.Reset();

        private static Tower Spawn(int maxHp = 100)
        {
            var t = new Tower("tower", maxHp: maxHp);
            t.DummyAwake();
            t.DummyStart();
            return t;
        }

        [Fact]
        public void Permanently_destroyed_on_death()
        {
            var t = Spawn();
            t.TakeDamage(100);

            Assert.Equal(UnitState.Destroyed, t.State);
            Assert.True(t.IsPermanentlyDestroyed);
        }

        [Fact]
        public void Stays_destroyed_forever()
        {
            var t = Spawn();
            t.TakeDamage(100);

            BattleManager.Tick(1000);

            Assert.Equal(UnitState.Destroyed, t.State);
            Assert.True(t.IsPermanentlyDestroyed);
        }

        [Fact]
        public void Ignores_further_damage_after_destruction()
        {
            var t = Spawn();

            // 코어 Health(런타임 타입)에 직접 구독해 '무시'를 관측 가능한 차이로 검증.
            int diedCount = 0, changedCount = 0;
            var health = (Core.Health)t.Health;
            health.Died += () => diedCount++;
            health.Changed += _ => changedCount++;

            t.TakeDamage(100);   // 파괴 (HP 100 → 0): Died/Changed 각 1회
            Assert.Equal(1, diedCount);
            Assert.Equal(1, changedCount);

            t.TakeDamage(100);   // 파괴 후 추가 피해는 무시되어야 함

            // 단순 'HP==0'은 가드가 없어도 항상 참(토톨로지). 대신 재통지가 없음을 단언한다.
            Assert.Equal(1, diedCount);                 // Died 재발화 없음 (가드 제거 시 2가 되어 실패)
            Assert.Equal(1, changedCount);              // 추가 HP 변경 통지 없음
            Assert.Equal(UnitState.Destroyed, t.State);
            Assert.True(t.IsPermanentlyDestroyed);
        }
    }
}
