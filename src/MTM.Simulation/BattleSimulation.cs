using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Battle;
using Core;
using UI;

namespace MTM.Simulation
{
    public enum Side { Blue, Red }

    /// <summary>
    /// 유닛 + 그 유닛을 '표시'하는 HPBar 묶음.
    /// 중요한 점: HPBar는 Unit이 아니라 Unit.Health(IHealthReadOnly)에만 Bind되어 있고,
    /// Unit은 자신을 그리는 HPBar의 존재를 전혀 모른다. (디커플링 증명)
    /// </summary>
    internal sealed class Actor
    {
        public Side Side;
        public string Kind;     // "Champion" / "Minion" / "Tower"
        public Unit Unit;
        public HPBar Bar;       // Unit.Health에 Bind
        public UnitState PrevState;
        public int? ReviveAtSecond;
    }

    /// <summary>
    /// 설계가 '실제로' 돌아가는지 검증하는 결정론적(난수 없음) 라인 교전 시뮬레이션.
    /// Unity의 Awake/Start/Update/Destroy 라이프사이클을 더미로 펌프하며,
    /// 세 가지 사망 행동 + 미니언 시간비례 공격력 + HPBar 디커플링을 한 번에 시연한다.
    /// </summary>
    internal sealed class BattleSimulation
    {
        // ── 튜닝 상수 (데모 가시성을 위해 조정된 값) ─────────────────────────────
        private const int MaxSeconds = 46;
        private static readonly int[] WaveSeconds = { 0, 8, 16, 24, 32 };
        private const int MinionsPerWave = 2;
        private const int BarWidth = 22;

        private readonly List<Actor> _actors = new List<Actor>();
        private readonly List<string> _transcript = new List<string>();
        private int _minionCounter;

        // ── 검증 트래커 ─────────────────────────────────────────────────────────
        private bool _championRevived;
        private bool _minionDespawned;
        private bool _towerPermanentlyDestroyed;
        private bool _hpBarAlwaysMatched = true;
        private int _firstMinionAttack = int.MinValue;
        private int _lastMinionAttack = int.MinValue;

        public void Run()
        {
            Header();
            BattleManager.Reset();
            SpawnTowersAndChampions();
            SpawnMinionWave(0);
            RenderDashboard($"초기 배치 (t=0s)");

            bool snapshotAfterReviveDone = false;

            for (int t = 1; t <= MaxSeconds; t++)
            {
                BattleManager.Tick(1);                 // 시간 진행 + 부활 예약 발화
                if (WaveSeconds.Contains(t)) SpawnMinionWave(t);

                PumpUpdate();                          // 살아있는 유닛 DummyUpdate
                ResolveCombat();                       // 공격 해소 → 데미지 → 사망 이벤트
                AssertHpBarsMatch();                   // 매 틱 HPBar==Health 검증
                DetectAndLogTransitions(t);

                if (!snapshotAfterReviveDone && _championRevived)
                {
                    RenderDashboard($"챔피언 부활 직후 (t={t}s)");
                    snapshotAfterReviveDone = true;
                }

                if (_towerPermanentlyDestroyed && t > FirstTowerDeathSecond + 3) break;
            }

            RenderDashboard($"종료 시점 (t={BattleManager.CurrentGameSeconds}s)");
            Verify();
            Dump();
        }

        private int FirstTowerDeathSecond = int.MaxValue;

        // ── 스폰 ─────────────────────────────────────────────────────────────────
        private void SpawnTowersAndChampions()
        {
            // 타워는 영구 파괴 시연을 위해 데모용으로 HP를 낮춘다. (기본값은 1500)
            Add(Side.Blue, "Tower", new Tower("Blue Outer Tower", maxHp: 720, attackDamage: 34));
            Add(Side.Red, "Tower", new Tower("Red Outer Tower", maxHp: 560, attackDamage: 34));

            // Red 챔피언을 약간 약하게 → 먼저 죽고 부활하는 장면을 보장.
            Add(Side.Blue, "Champion", new Champion("Garen (Blue)", maxHp: 380, level: 3, reviveDelaySeconds: 5));
            Add(Side.Red, "Champion", new Champion("Riven (Red)", maxHp: 300, level: 3, reviveDelaySeconds: 5));
        }

        private void SpawnMinionWave(int second)
        {
            foreach (Side side in new[] { Side.Blue, Side.Red })
            {
                for (int i = 0; i < MinionsPerWave; i++)
                {
                    _minionCounter++;
                    var m = new Minion($"Minion#{_minionCounter} ({side})", maxHp: 70);
                    Add(side, "Minion", m);

                    if (_firstMinionAttack == int.MinValue) _firstMinionAttack = m.AttackDamage;
                    _lastMinionAttack = m.AttackDamage;
                }
            }
            LogEvent(second, $"[SPAWN] 미니언 웨이브 — 진영별 {MinionsPerWave}기, ATK={10 + second / 2} (게임시간 {second}s 비례)");
        }

        private void Add(Side side, string kind, Unit unit)
        {
            // Unity 라이프사이클 펌프: 생성 → Awake → Start
            unit.DummyAwake();
            unit.DummyStart();

            // HPBar를 만들어 '임의 대상'(unit.Health)에 Bind.
            //   - HPBar는 Unit/Champion/Tower 타입을 전혀 참조하지 않는다.
            //   - Bind 인자 타입은 IHealthReadOnly 추상뿐.
            var bar = new HPBar();
            bar.Bind(unit.Health);

            _actors.Add(new Actor
            {
                Side = side,
                Kind = kind,
                Unit = unit,
                Bar = bar,
                PrevState = unit.State,
                ReviveAtSecond = null
            });
        }

        // ── 진행 ─────────────────────────────────────────────────────────────────
        private void PumpUpdate()
        {
            foreach (var a in _actors)
                if (a.Unit.State == UnitState.Alive)
                    a.Unit.DummyUpdate();
        }

        private void ResolveCombat()
        {
            // 동시 턴: 이번 틱에 살아있던 유닛들이 각자 한 번 공격.
            var attackers = _actors.Where(a => a.Unit.State == UnitState.Alive).ToList();
            foreach (var attacker in attackers)
            {
                if (attacker.Unit.State != UnitState.Alive) continue; // 도중 사망 반영
                var target = ChooseTarget(attacker);
                attacker.Unit.Attack(target?.Unit); // IDamageable로만 가함 (타입 모름)
            }
        }

        /// <summary>대상 우선순위(결정론적): 종류별로 다른 어그로.</summary>
        private Actor ChooseTarget(Actor attacker)
        {
            var enemies = _actors.Where(a => a.Side != attacker.Side && a.Unit.State == UnitState.Alive).ToList();
            if (enemies.Count == 0) return null;

            string[] order = attacker.Kind switch
            {
                "Champion" => new[] { "Champion", "Minion", "Tower" },
                "Tower" => new[] { "Minion", "Champion" },
                _ => new[] { "Minion", "Champion", "Tower" }, // Minion
            };

            foreach (var kind in order)
            {
                var pick = enemies
                    .Where(e => e.Kind == kind)
                    .OrderBy(e => e.Unit.Health.Current) // 가장 약한 적부터 (포커싱)
                    .FirstOrDefault();
                if (pick != null) return pick;
            }
            return enemies[0];
        }

        // ── 검증 보조 ─────────────────────────────────────────────────────────────
        private void AssertHpBarsMatch()
        {
            foreach (var a in _actors)
            {
                // HPBar가 코어 HP를 정확히 반영하는지(=구독 동기화) 매 틱 확인.
                if (a.Bar.CurrentHP != a.Unit.Health.Current)
                    _hpBarAlwaysMatched = false;
            }
        }

        private void DetectAndLogTransitions(int t)
        {
            foreach (var a in _actors)
            {
                var now = a.Unit.State;
                if (now == a.PrevState) continue;

                switch (a.Kind)
                {
                    case "Champion":
                        if (now == UnitState.Reviving)
                        {
                            var champ = (Champion)a.Unit;
                            a.ReviveAtSecond = t + (int)champ.ReviveDelaySeconds;
                            LogEvent(t, $"[DEATH] {a.Unit.Name} 사망 → {champ.ReviveDelaySeconds:0}s 후 부활 예약 (예정 t={a.ReviveAtSecond}s)");
                        }
                        else if (a.PrevState == UnitState.Reviving && now == UnitState.Alive)
                        {
                            _championRevived = true;
                            LogEvent(t, $"[REVIVE] {a.Unit.Name} 부활! HP 만피 복귀 (총 {((Champion)a.Unit).ReviveCount}회)");
                            a.ReviveAtSecond = null;
                        }
                        break;

                    case "Minion":
                        if (now == UnitState.Destroyed)
                        {
                            _minionDespawned = true;
                            LogEvent(t, $"[DESPAWN] {a.Unit.Name} 소멸 (부활 없음)");
                        }
                        break;

                    case "Tower":
                        if (now == UnitState.Destroyed)
                        {
                            _towerPermanentlyDestroyed = true;
                            FirstTowerDeathSecond = Math.Min(FirstTowerDeathSecond, t);
                            LogEvent(t, $"[DESTROY] {a.Unit.Name} 영구 파괴! (IsPermanentlyDestroyed={((Tower)a.Unit).IsPermanentlyDestroyed})");
                        }
                        break;
                }
                a.PrevState = now;
            }
        }

        // ── 출력 ─────────────────────────────────────────────────────────────────
        private void Header()
        {
            Log("╔══════════════════════════════════════════════════════════════════════════╗");
            Log("║   MTM — 인게임 유닛 설계 검증 시뮬레이션  (Champion / Minion / Tower)      ║");
            Log("║   Blue  vs  Red   라인 교전 · 결정론적 · Unity 라이프사이클 더미 펌프      ║");
            Log("╚══════════════════════════════════════════════════════════════════════════╝");
        }

        private void RenderDashboard(string title)
        {
            Log("");
            Log($"┌─ {title} " + new string('─', Math.Max(0, 60 - title.Length)));
            foreach (var side in new[] { Side.Blue, Side.Red })
            {
                Log($"│  [{side}]");
                foreach (var a in _actors.Where(x => x.Side == side)
                                         .OrderBy(x => KindOrder(x.Kind)))
                {
                    if (a.Kind == "Minion" && a.Unit.State == UnitState.Destroyed) continue; // 소멸 미니언 숨김
                    Log("│    " + Row(a));
                }
            }
            Log("└" + new string('─', 66));
        }

        private static int KindOrder(string kind) => kind == "Tower" ? 0 : kind == "Champion" ? 1 : 2;

        private string Row(Actor a)
        {
            // 막대는 HPBar(CurrentHP/MaxHP)에서 그린다 → 바인딩된 표시 객체가 실제로 UI를 구동.
            string bar = AsciiBar(a.Bar.CurrentHP, a.Bar.MaxHP);
            string hp = $"{a.Bar.CurrentHP,4}/{a.Bar.MaxHP,-4}";
            string name = a.Unit.Name.PadRight(20);
            string state = StateLabel(a);
            string atk = a.Unit.State == UnitState.Destroyed ? "    " : $"ATK {a.Unit.AttackDamage,3}";
            return $"{name} {bar} {hp} {atk}  {state}";
        }

        private string StateLabel(Actor a)
        {
            switch (a.Unit.State)
            {
                case UnitState.Alive: return "ALIVE";
                case UnitState.Reviving:
                    int remain = (a.ReviveAtSecond ?? BattleManager.CurrentGameSeconds) - BattleManager.CurrentGameSeconds;
                    return $"REVIVING ({Math.Max(0, remain)}s)";
                case UnitState.Destroyed:
                    return a.Kind == "Tower" ? "DESTROYED (영구)" : "DESPAWNED";
                default: return a.Unit.State.ToString();
            }
        }

        private static string AsciiBar(int cur, int max)
        {
            if (max <= 0) max = 1;
            int filled = (int)Math.Round((double)cur / max * BarWidth);
            filled = Math.Max(0, Math.Min(BarWidth, filled));
            return "[" + new string('█', filled) + new string('░', BarWidth - filled) + "]";
        }

        private void LogEvent(int t, string msg) => Log($"  t={t,2}s  {msg}");

        // ── 검증 요약 ─────────────────────────────────────────────────────────────
        private void Verify()
        {
            bool minionScaled = _lastMinionAttack > _firstMinionAttack;

            Log("");
            Log("╔══════════════════════════════════ 자기 검증 ══════════════════════════════╗");
            Check("챔피언: 죽으면 일정 시간 후 '부활'", _championRevived,
                  "Reviving→Alive 전이 관측됨");
            Check("미니언: 죽으면 '소멸'(영구)", _minionDespawned,
                  "Destroyed 전이 후 재등록 없음");
            Check($"미니언: 생성 시간 비례 공격력 (첫 {_firstMinionAttack} → 마지막 {_lastMinionAttack})", minionScaled,
                  "후반 웨이브일수록 ATK 증가");
            Check("타워: 죽으면 '영구 파괴'", _towerPermanentlyDestroyed,
                  "IsPermanentlyDestroyed=true, 부활 경로 없음");
            Check("HPBar: 유닛 의존성 0으로 임의 대상 HP 실시간 반영", _hpBarAlwaysMatched,
                  "매 틱 HPBar.CurrentHP == Unit.Health.Current");
            Log("╚═══════════════════════════════════════════════════════════════════════════╝");

            bool all = _championRevived && _minionDespawned && minionScaled
                       && _towerPermanentlyDestroyed && _hpBarAlwaysMatched;
            Log("");
            Log(all ? "  ✅ ALL REQUIREMENTS DEMONSTRATED — 설계가 의도대로 동작합니다."
                    : "  ❌ 일부 요구사항이 시연되지 않았습니다 (튜닝 필요).");
        }

        private void Check(string name, bool ok, string detail)
        {
            Log($"  {(ok ? "✅" : "❌")}  {name}");
            Log($"        └ {detail}");
        }

        // ── 트랜스크립트 ───────────────────────────────────────────────────────────
        private void Log(string line)
        {
            Console.WriteLine(line);
            _transcript.Add(line);
        }

        private void Dump()
        {
            try
            {
                string root = FindRepoRoot();
                if (root == null) return;
                string path = Path.Combine(root, "docs", "sample-run.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, string.Join(Environment.NewLine, _transcript), new UTF8Encoding(false));
                Console.WriteLine($"\n(시뮬레이션 트랜스크립트 저장: {path})");
            }
            catch { /* 출력 저장 실패는 데모 본질과 무관하므로 무시 */ }
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "MTM.sln"))) return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }
    }
}
