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
            t.TakeDamage(100);
            t.TakeDamage(100); // 파괴 후 추가 피해는 무시

            Assert.Equal(0, t.Health.Current);
            Assert.True(t.IsPermanentlyDestroyed);
        }
    }
}
