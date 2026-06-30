namespace Battle
{
    /// <summary>
    /// 챔피언 : 죽으면 <b>일정 시간 후 부활</b>한다. (오브젝트는 파괴하지 않는다)
    /// </summary>
    public sealed class Champion : Unit
    {
        public int Level { get; private set; }

        /// <summary>부활까지 대기 시간(초). 실제 게임이라면 레벨/게임시간에 비례.</summary>
        public double ReviveDelaySeconds { get; }

        public int ReviveCount { get; private set; }

        public Champion(string name, int maxHp = 600, int level = 1, double reviveDelaySeconds = 5)
            : base(name, maxHp)
        {
            Level = level;
            ReviveDelaySeconds = reviveDelaySeconds;
        }

        public override int AttackDamage => GetProperAttackDamageBy(Level);

        /// <summary>레벨에 맞는 적절한 공격력 반환. (수치는 밸런싱 영역)</summary>
        private int GetProperAttackDamageBy(int level) => 60 + (level - 1) * 8;

        /// <summary>사망 → 부활 예약. 일정 시간 뒤 만피로 복귀한다.</summary>
        protected override void HandleDeath()
        {
            State = UnitState.Reviving;
            BattleManager.Schedule(ReviveDelaySeconds, Revive);
        }

        private void Revive()
        {
            RestoreFullHealth();        // HP 만피 → Health.Changed 통지 → HPBar 자동 갱신
            State = UnitState.Alive;
            ReviveCount++;
            // 위치 리스폰/부활 무적 등은 게임 로직에서 처리(설계 범위 밖이라 생략)
        }
    }
}
