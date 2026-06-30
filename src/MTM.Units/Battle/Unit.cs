using System;
using Core;

namespace Battle
{
    /// <summary>유닛의 생존 상태.</summary>
    public enum UnitState
    {
        Spawning,   // 생성됨, 아직 전투 진입 전
        Alive,      // 살아서 활동
        Reviving,   // 사망했으나 부활 대기 중 (챔피언)
        Destroyed   // 소멸/영구 파괴 (미니언·타워)
    }

    /// <summary>
    /// 모든 인게임 유닛의 공통 기반.
    ///  - HP는 '상속'이 아니라 '조합(Health)'으로 보유 → HP 표시/관찰 책임을 유닛에서 분리.
    ///  - 사망 시 '행동'만 하위 타입이 Template Method(HandleDeath)로 다르게 구현한다.
    ///  - HPBar 등 UI에 대한 의존성은 전혀 없다. (유닛은 자신을 누가 그리는지 모른다)
    /// </summary>
    public abstract class Unit : DummyMonoBehaviour, IDamageable
    {
        private readonly Core.Health _health;   // '작동하는 코어' 객체

        /// <summary>외부에 노출하는 읽기전용 HP 관찰 창구. (HPBar는 이 추상에만 붙는다)</summary>
        public IHealthReadOnly Health => _health;

        public UnitState State { get; protected set; } = UnitState.Spawning;
        public string Name { get; protected set; }

        /// <summary>해당 유닛의 적절한 공격력. (구체 수치는 타입별로 결정)</summary>
        public abstract int AttackDamage { get; }

        protected Unit(string name, int maxHp)
        {
            Name = name;
            _health = new Core.Health(maxHp);
            _health.Died += OnDied;   // 코어가 사망을 알리면 유닛이 사망 정책을 수행
        }

        /// <summary>전투 진입. (Unity의 Start에 대응)</summary>
        public override void DummyStart()
        {
            if (State == UnitState.Spawning) State = UnitState.Alive;
        }

        /// <summary>피해를 받는다. (전투 코드가 IDamageable로 호출 — 유닛 타입을 몰라도 됨)</summary>
        public void TakeDamage(int amount)
        {
            if (State != UnitState.Alive) return;   // Alive 상태에서만 피해를 받는다 (Spawning/Reviving/Destroyed는 모두 무시)
            _health.TakeDamage(amount);
        }

        /// <summary>대상에게 공격력만큼 피해를 가한다.</summary>
        public void Attack(IDamageable target)
        {
            if (State != UnitState.Alive || target == null) return;
            target.TakeDamage(AttackDamage);
        }

        private void OnDied()
        {
            // 사망 순간의 '행동'과 '최종 상태'를 모두 타입별 HandleDeath가 소유한다.
            HandleDeath();   // ← 챔피언=부활(Reviving) / 미니언=소멸(Destroyed) / 타워=영구파괴(Destroyed)
        }

        /// <summary>사망 시 행동. 하위 타입이 반드시 구현한다.</summary>
        protected abstract void HandleDeath();

        /// <summary>코어 HP를 만점으로 되살린다. (부활 구현 보조)</summary>
        protected void RestoreFullHealth() => _health.ResetToFull();
    }
}
