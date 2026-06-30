using Battle;
using Xunit;

namespace MTM.Units.Tests
{
    /// <summary>챔피언: 죽으면 일정 시간 후 부활.</summary>
    public class ChampionTests
    {
        public ChampionTests() => BattleManager.Reset();

        private static Champion Spawn(int maxHp = 100, double reviveDelay = 3)
        {
            var c = new Champion("champ", maxHp: maxHp, level: 1, reviveDelaySeconds: reviveDelay);
            c.DummyAwake();
            c.DummyStart();
            return c;
        }

        [Fact]
        public void Dies_into_Reviving_state_not_destroyed()
        {
            var c = Spawn();
            c.TakeDamage(100);

            Assert.Equal(UnitState.Reviving, c.State);
            Assert.False(c.Health.IsAlive);
        }

        [Fact]
        public void Does_not_revive_before_delay_elapses()
        {
            var c = Spawn(reviveDelay: 3);
            c.TakeDamage(100);

            BattleManager.Tick(2); // 아직 3초 안 됨
            Assert.Equal(UnitState.Reviving, c.State);
        }

        [Fact]
        public void Revives_to_full_hp_after_delay()
        {
            var c = Spawn(maxHp: 100, reviveDelay: 3);
            c.TakeDamage(100);

            BattleManager.Tick(3); // 부활 시점 도달

            Assert.Equal(UnitState.Alive, c.State);
            Assert.Equal(100, c.Health.Current);
            Assert.True(c.Health.IsAlive);
            Assert.Equal(1, c.ReviveCount);
        }

        [Fact]
        public void Can_die_and_revive_repeatedly()
        {
            var c = Spawn(maxHp: 100, reviveDelay: 2);

            c.TakeDamage(100);
            BattleManager.Tick(2);
            c.TakeDamage(100);
            BattleManager.Tick(2);

            Assert.Equal(UnitState.Alive, c.State);
            Assert.Equal(2, c.ReviveCount);
        }
    }
}
