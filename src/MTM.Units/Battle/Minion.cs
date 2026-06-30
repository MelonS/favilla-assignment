namespace Battle
{
    /// <summary>
    /// 미니언 : 죽으면 <b>즉시 소멸</b>(부활 없음).
    /// <b>생성 시점</b>의 게임 경과 시간에 비례해 공격력이 결정된다. (후반 스폰일수록 강함)
    /// </summary>
    public sealed class Minion : Unit
    {
        private readonly int _attackDamage;

        /// <summary>생성 시 캡처한 게임 시간(초). 어떤 시점 미니언인지 추적/검증용.</summary>
        public int SpawnedAtSeconds { get; }

        public Minion(string name, int maxHp = 80) : base(name, maxHp)
        {
            // 생성 시점의 게임 시간을 캡처해 공격력을 '확정'한다. (이후 시간이 흘러도 고정)
            SpawnedAtSeconds = BattleManager.CurrentGameSeconds;
            _attackDamage = GetProperAttackDamageBy(SpawnedAtSeconds);
        }

        public override int AttackDamage => _attackDamage;

        /// <summary>
        /// 경과 시간(초)에 비례한 적절한 공격력 반환.
        /// 기본 10 + 2초당 +1 (데모 가시성을 위해 다소 가파른 예시; 실제 수치는 밸런싱 영역).
        /// </summary>
        private int GetProperAttackDamageBy(int gameSeconds) => 10 + gameSeconds / 2;

        /// <summary>사망 → 소멸. 오브젝트를 파괴하고 다시 등록하지 않는다.</summary>
        protected override void HandleDeath()
        {
            State = UnitState.Destroyed;
            DummyDestroy();   // Unity라면 Object.Destroy(gameObject)
        }
    }
}
