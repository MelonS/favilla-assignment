using System;
using System.Collections.Generic;

namespace Battle
{
    /// <summary>
    /// 전역 전투 컨텍스트. (과제 제공 스텁을 '실제 동작'하도록 확장)
    ///  - 게임 시간(초) 진행
    ///  - 지연 실행 예약(챔피언 부활 등) 스케줄러
    /// 명세가 정적 접근(BattleManager.CurrentGameSeconds)을 전제하므로 static을 유지한다.
    /// (운영 코드라면 IGameClock 주입을 택했을 것 — README의 '설계 결정' 참고)
    /// </summary>
    public static class BattleManager
    {
        private static double _elapsedSeconds;
        private static readonly List<Scheduled> _scheduled = new List<Scheduled>();

        /// <summary>현재 게임 시간(초).</summary>
        public static int CurrentGameSeconds => (int)_elapsedSeconds;

        /// <summary>한 프레임 진행: deltaSeconds만큼 시간을 흘리고, 만기된 예약 콜백을 발화한다.</summary>
        public static void Tick(double deltaSeconds)
        {
            if (deltaSeconds <= 0) return;
            _elapsedSeconds += deltaSeconds;

            // 만기된 예약만 골라 발화. (콜백이 새 예약을 추가해도 이번 프레임엔 발화되지 않도록 역순 순회)
            for (int i = _scheduled.Count - 1; i >= 0; i--)
            {
                if (_scheduled[i].DueAt <= _elapsedSeconds)
                {
                    Action callback = _scheduled[i].Callback;
                    _scheduled.RemoveAt(i);
                    callback?.Invoke();
                }
            }
        }

        /// <summary>delaySeconds 후 callback 실행을 예약한다. (예: 챔피언 부활)</summary>
        public static void Schedule(double delaySeconds, Action callback)
        {
            _scheduled.Add(new Scheduled(_elapsedSeconds + Math.Max(0, delaySeconds), callback));
        }

        /// <summary>테스트/리스타트용 초기화.</summary>
        public static void Reset()
        {
            _elapsedSeconds = 0;
            _scheduled.Clear();
        }

        private readonly struct Scheduled
        {
            public readonly double DueAt;
            public readonly Action Callback;
            public Scheduled(double dueAt, Action callback)
            {
                DueAt = dueAt;
                Callback = callback;
            }
        }
    }
}
