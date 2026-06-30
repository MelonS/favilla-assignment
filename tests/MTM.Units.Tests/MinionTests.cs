using Battle;
using Xunit;

namespace MTM.Units.Tests
{
    /// <summary>미니언: 죽으면 소멸 + 생성 시 게임시간 비례 공격력.</summary>
    public class MinionTests
    {
        public MinionTests() => BattleManager.Reset();

        private static Minion Spawn(int maxHp = 50)
        {
            var m = new Minion("minion", maxHp: maxHp);
            m.DummyAwake();
            m.DummyStart();
            return m;
        }

        [Fact]
        public void Despawns_permanently_on_death()
        {
            var m = Spawn();
            m.TakeDamage(50);

            Assert.Equal(UnitState.Destroyed, m.State);

            BattleManager.Tick(100); // 시간이 지나도
            Assert.Equal(UnitState.Destroyed, m.State); // 절대 부활하지 않음
        }

        [Fact]
        public void Attack_is_set_proportional_to_game_time_at_spawn()
        {
            var early = Spawn();           // t=0 생성
            BattleManager.Tick(60);        // 게임시간 60초 경과
            var late = Spawn();            // t=60 생성

            Assert.True(late.AttackDamage > early.AttackDamage);
            Assert.Equal(0, early.SpawnedAtSeconds);
            Assert.Equal(60, late.SpawnedAtSeconds);
        }

        [Fact]
        public void Attack_is_frozen_at_spawn_time_even_if_time_passes()
        {
            var m = Spawn();
            int atSpawn = m.AttackDamage;

            BattleManager.Tick(120); // 생성 후 시간이 흘러도
            Assert.Equal(atSpawn, m.AttackDamage); // 공격력은 생성 시점 값으로 고정
        }
    }
}
