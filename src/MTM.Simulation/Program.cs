using System;
using System.Text;

namespace MTM.Simulation
{
    /// <summary>
    /// 실행 진입점. 설계 라이브러리(MTM.Units)를 실제로 구동해
    /// 요구사항이 동작함을 콘솔에서 시각적으로 검증한다.
    /// </summary>
    internal static class Program
    {
        private static void Main()
        {
            try { Console.OutputEncoding = Encoding.UTF8; } catch { /* 일부 콘솔 미지원 */ }
            new BattleSimulation().Run();
        }
    }
}
