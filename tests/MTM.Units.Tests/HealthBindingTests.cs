using System;
using Battle;
using Core;
using UI;
using Xunit;

namespace MTM.Units.Tests
{
    /// <summary>
    /// 선택 과제(2): 유닛 → HPBar 의존성 없이 HPBar가 임의 대상의 HP를 반영.
    /// </summary>
    public class HealthBindingTests
    {
        [Fact]
        public void Bar_reflects_initial_value_immediately_on_bind()
        {
            var health = new Health(100);
            var bar = new HPBar();

            bar.Bind(health);

            Assert.Equal(100, bar.CurrentHP);
            Assert.Equal(100, bar.MaxHP);
            Assert.True(bar.IsBound);
        }

        [Fact]
        public void Bar_follows_subsequent_hp_changes()
        {
            var health = new Health(100);
            var bar = new HPBar();
            bar.Bind(health);

            health.TakeDamage(30);
            Assert.Equal(70, bar.CurrentHP);

            health.Heal(10);
            Assert.Equal(80, bar.CurrentHP);
            Assert.True(Math.Abs(bar.Fill - 0.8f) < 0.001f);
        }

        [Fact]
        public void Bar_binds_to_arbitrary_unit_without_knowing_its_type()
        {
            // '임의의 대상' = Tower. HPBar는 Tower 타입을 전혀 모른 채 Health 추상만 받는다.
            var tower = new Tower("t", maxHp: 200);
            tower.DummyAwake();
            tower.DummyStart();

            var bar = new HPBar();
            bar.Bind(tower.Health); // 인자 타입은 IHealthReadOnly

            tower.TakeDamage(50);
            Assert.Equal(150, bar.CurrentHP);
        }

        [Fact]
        public void Unbind_stops_further_updates_and_prevents_leak()
        {
            var health = new Health(100);
            var bar = new HPBar();
            bar.Bind(health);

            bar.Unbind();
            health.TakeDamage(40);

            Assert.Equal(100, bar.CurrentHP); // 더 이상 갱신되지 않음
            Assert.False(bar.IsBound);
        }

        [Fact]
        public void Rebind_unsubscribes_previous_source()
        {
            var first = new Health(100);
            var second = new Health(50);
            var bar = new HPBar();

            bar.Bind(first);
            bar.Bind(second); // 재바인딩

            first.TakeDamage(100); // 이전 소스 변화는 무시되어야 함
            Assert.Equal(50, bar.CurrentHP);

            second.TakeDamage(10);
            Assert.Equal(40, bar.CurrentHP);
        }

        [Fact]
        public void Multiple_bars_can_observe_one_source()
        {
            var health = new Health(100);
            var bar1 = new HPBar();
            var bar2 = new HPBar();
            bar1.Bind(health);
            bar2.Bind(health);

            health.TakeDamage(25);

            Assert.Equal(75, bar1.CurrentHP);
            Assert.Equal(75, bar2.CurrentHP);
        }

        [Fact]
        public void Destroying_bar_auto_unbinds()
        {
            var health = new Health(100);
            var bar = new HPBar();
            bar.Bind(health);

            bar.DummyDestroy(); // UI 오브젝트 파괴 → 자동 구독 해제

            health.TakeDamage(40);
            Assert.Equal(100, bar.CurrentHP);
            Assert.False(bar.IsBound);
        }
    }
}
