namespace Battle
{
    /// <summary>
    /// 타워 : 죽으면 <b>영구 파괴</b>된다. 부활/재생성 경로를 모두 차단한다.
    /// </summary>
    public sealed class Tower : Unit
    {
        private readonly int _attackDamage;

        /// <summary>영구 파괴 여부. 한 번 true가 되면 다시 false로 돌아가지 않는다.</summary>
        public bool IsPermanentlyDestroyed { get; private set; }

        public Tower(string name, int maxHp = 1500, int attackDamage = 120) : base(name, maxHp)
        {
            _attackDamage = attackDamage;
        }

        public override int AttackDamage => _attackDamage;

        /// <summary>사망 → 영구 파괴. 부활/리스폰 로직을 어디에서도 호출하지 않는다.</summary>
        protected override void HandleDeath()
        {
            State = UnitState.Destroyed;
            IsPermanentlyDestroyed = true;
            DummyDestroy();   // Unity라면 Object.Destroy(gameObject); 풀/리스폰에 재등록하지 않음
        }
    }
}
