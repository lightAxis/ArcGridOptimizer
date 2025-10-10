// dotnet add package Google.OrTools

using ArcGridOptimizer.Cvts;
using ArcGridOptimizer.Enums;
using ArcGridOptimizer.Models;
using ArcGridOptimizer.ViewModels.Items;
using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using System.Windows.Input;

namespace ArcGridOptimizer.Models.Opt
{
    #region Inputs (records)

    public record CoreParam(
        string Id,
        string Grade,                 // 
        int WillCapacity,             // 코어 의지력(용량)
        Dictionary<int, double> StageIncrement // {10:Δ,14:Δ,17:Δ,18:Δ,19:Δ,20:Δ} (증분 DPS%)
    );

    public record GemParam(
        string Id,
        int WillCost,   // 젬이 소모하는 의지력
        int Points,     // 젬이 제공하는 코어 포인트
        double Value,    // 젬의 (ATK/ADD/BOSS 합산) DPS 점수 (예: 0.344 → +0.344% DPS)
        int AtkLv = 0,
        int ExtraLv = 0,
        int BossLv = 0
    );

    public record HardStageReq(string CoreId, int MinStage); // 예: ("core2", 17)

    public record SolveOptions(
        int MaxGemsPerCore = 4,
        bool InventoryShared = true,
        int TimeLimitSec = 5,     // CP-SAT 시간 제한(초)
        int Workers = 0,          // 0=자동(보통 물리 코어 수)
        int Scale = 1000,         // 목적계수 스케일(예: ×1000 → 0.001정밀도)
        bool LogProgress = true   // CP-SAT 진행 로그 on/off
    );

    #endregion

    #region Progress callback (time, LB, UB, gap)

    public class ProgressPoint
    {
        public double TimeSec;
        public double LB;
        public double UB;
        public double Gap; // (UB-LB)/max(1e-9, |LB|)
    }

    public class ProgressCb : CpSolverSolutionCallback
    {
        private readonly int _scale;
        public readonly List<ProgressPoint> Points = new();

        public ProgressCb(int scale) => _scale = scale;

        public override void OnSolutionCallback()
        {
            // Maximization: ObjectiveValue() = incumbent(LB), BestObjectiveBound() = UB
            double lb = ObjectiveValue() / _scale;
            double ub = BestObjectiveBound() / _scale;
            double t = WallTime();
            double gap = (ub - lb) / Math.Max(1e-9, Math.Abs(lb));
            Points.Add(new ProgressPoint { TimeSec = t, LB = lb, UB = ub, Gap = gap });
        }
    }

    #endregion

    public class ArcgridCpSat
    {
        static readonly int[] CanonicalStages = new[] { 10, 14, 17, 18, 19, 20 };
        public record GemPickResult(
            string Id,
            int WillCost,
            int Points,
            int Atk,
            int Extra,
            int Boss,
            double Value
        );

        public class Solution
        {
            public double Objective;                  // (descaled)
            public double LB;                         // incumbent (descaled)
            public double UB;                         // best bound (descaled)
            public double Gap;

            public Dictionary<string, List<string>> CoreToGemIds = new();
            public Dictionary<string, List<GemPickResult>> CoreToGemDetails = new();
            public Dictionary<string, List<int>> CoreStagesOn = new();
            public Dictionary<string, double> CoreValueSum = new();
            public Dictionary<string, (string Grade, int SumPoints, int UsedWill)> CoreStats = new();
            public List<ProgressPoint> Timeline = new();
        }

        public static Models.OptResultDTO Solve(CoreListDTO coreDTO,
            GemListDTO gemDTO)
        {
            var core_params = new List<CoreParam>();
            var gem_params = new List<GemParam>();
            var cvt = new Cvts.Enum2DescCvt();


            CoreParam coreCvt(CoreDTO dto)
            {
                int maxstage = Params.CoreParam.CoreGradeMaxEffectLevel[dto.Grade];
                maxstage = Params.CoreParam.CoreEffectLevels[maxstage];
                Dictionary<int, double> stageInc = new();
                int maxwill = Params.CoreParam.MaxSupplyWill[dto.Grade];
                for (int i = 0; i < dto.CoreValues.Count; i++)
                {
                    if (dto.CoreValues[i].Level > maxstage)
                        dto.CoreValues[i].Value = 0.0;
                    stageInc[dto.CoreValues[i].Level] = dto.CoreValues[i].Value;
                }
                return new CoreParam(dto.Name,
                    (string)cvt.Convert2(dto.Grade, typeof(string)),
                    maxwill,
                    stageInc
                    );
            }

            double optionValue(Enums.eGemProperty gemType, int value)
            {
                switch (gemType)
                {
                    case Enums.eGemProperty.None:
                        return 0.0;
                    case Enums.eGemProperty.Attack:
                        return Params.GemParam.AtkValues[value];
                    case Enums.eGemProperty.ExtraDamage:
                        return Params.GemParam.ExtraDmgValues[value];
                    case Enums.eGemProperty.BossDamage:
                        return Params.GemParam.BossDmgValues[value];
                }
                return 0.0;
            }

            (int,int,int) optionLevel(GemPropertyDTO gemprop1, GemPropertyDTO gemprop2)
            {
                int atk = 0, add = 0, boss = 0;
                if (gemprop1.Type == Enums.eGemProperty.Attack)
                    atk = gemprop1.Value;
                else if (gemprop1.Type == Enums.eGemProperty.ExtraDamage)
                    add = gemprop1.Value;
                else if (gemprop1.Type == Enums.eGemProperty.BossDamage)
                    boss = gemprop1.Value;
                if (gemprop2.Type == Enums.eGemProperty.Attack)
                    atk = gemprop2.Value;
                else if (gemprop2.Type == Enums.eGemProperty.ExtraDamage)
                    add = gemprop2.Value;
                else if (gemprop2.Type == Enums.eGemProperty.BossDamage)
                    boss = gemprop2.Value;
                return (atk, add, boss);
            }

            int a = 1;
            GemParam gemCvt(GemDTO dto)
            {
                double value = optionValue(dto.Property1.Type, dto.Property1.Value) +
                    optionValue(dto.Property2.Type, dto.Property2.Value);
                string name = "젬" + a.ToString();
                a++;
                var (atk, add, boss) = optionLevel(dto.Property1, dto.Property2);
                return new GemParam(dto.Name, dto.RequiredWill, dto.RewardPoint, value, atk, add, boss);
            }

            core_params = coreDTO.Cores.Select(x => coreCvt(x)).ToList();
            gem_params = gemDTO.Gems.Select(x => gemCvt(x)).ToList();

            var solve_res = Solve(core_params, gem_params);
            return ConvertToOptResultDTO(solve_res);
        }

        public static Models.OptResultDTO ConvertToOptResultDTO(Solution sol)
        {
            List<string> coreIds = sol.CoreStats.Keys.ToList();
            Enum2DescCvt enumCvt = new Enum2DescCvt();

            var res = new Models.OptResultDTO();

            static (GemPropertyDTO prop1, GemPropertyDTO prop2) getgemProps(GemPickResult gemRes)
            {
                GemPropertyDTO? prop1 = null;
                GemPropertyDTO? prop2 = null;
                if (gemRes.Atk > 0)
                {
                    prop1 = new GemPropertyDTO() { Type = eGemProperty.Attack, Value = gemRes.Atk };
                }
                if(gemRes.Extra >0)
                {
                    if(prop1 == null)
                        prop1 = new GemPropertyDTO() { Type = eGemProperty.ExtraDamage, Value = gemRes.Extra };
                    else
                        prop2 = new GemPropertyDTO() { Type = eGemProperty.ExtraDamage, Value = gemRes.Extra };
                }
                if(gemRes.Boss >0)
                {
                    if(prop1 == null)
                        prop1 = new GemPropertyDTO() { Type = eGemProperty.BossDamage, Value = gemRes.Boss };
                    else
                        prop2 = new GemPropertyDTO() { Type = eGemProperty.BossDamage, Value = gemRes.Boss };
                }
                return (prop1 ?? new GemPropertyDTO() { Type = eGemProperty.None, Value = 0 },
                    prop2 ?? new GemPropertyDTO() { Type = eGemProperty.None, Value = 0 });
            }
            
            foreach(string coreName in coreIds)
            {
                CoreDTO coreDTO = new CoreDTO();
                coreDTO.Name = coreName;
                coreDTO.Grade = (eCoreGrade)enumCvt.ConvertBack2(sol.CoreStats[coreName].Grade, typeof(eCoreGrade));

                List<GemOptResultDTO> gemResults = new List<GemOptResultDTO>();
                foreach (var gem in sol.CoreToGemDetails[coreName])
                {
                    var gemDTO = new GemOptResultDTO();
                    gemDTO.Name = gem.Id;
                    gemDTO.RequiredWill = gem.WillCost;
                    gemDTO.RewardPoint = gem.Points;
                    gemDTO.Score = gem.Value;
                    (gemDTO.Property1, gemDTO.Property2) = getgemProps(gem);
                    gemResults.Add(gemDTO);
                }

                var coreOptDTO = new CoreOptResultDTO();
                coreOptDTO.GemOptResults = gemResults;
                coreOptDTO.Core = coreDTO;
                res.CoreResults.Add(coreOptDTO);
            }

            return res;
        }

        private static Solution Solve(
            IList<CoreParam> cores,
            IList<GemParam> gems,
            IList<HardStageReq> hardStageReqs = null,
            SolveOptions opt = null)
        {
            opt ??= new SolveOptions();

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new DefaultTraceListener());
            Trace.AutoFlush = true;

            var K = CanonicalStages;
            int I = cores.Count;     // 3이하
            int J = gems.Count;

            var model = new CpModel();

            // ---------- 변수 ----------
            // x[i,j] ∈ {0,1}
            var x = new BoolVar[I, J];
            for (int i = 0; i < I; i++)
                for (int j = 0; j < J; j++)
                    x[i, j] = model.NewBoolVar($"x_{i}_{j}");

            // u[i,k] ∈ {0,1}
            var u = new Dictionary<(int i, int k), BoolVar>();
            foreach (var k in K)
                for (int i = 0; i < I; i++)
                    u[(i, k)] = model.NewBoolVar($"u_{i}_{k}");

            // S_i ∈ [0, Smax_i] (정수)
            var S = new IntVar[I];
            for (int i = 0; i < I; i++)
            {
                //int sMax = ComputeSMaxUpperBound(cores[i], gems, opt.MaxGemsPerCore);
                S[i] = model.NewIntVar(0, 20, $"S_{i}");
            }

            // ---------- 제약 ----------
            // (1) S_i = Σ p_j x[i,j]
            for (int i = 0; i < I; i++)
            {
                var terms = new List<LinearExpr>();
                for (int j = 0; j < J; j++) terms.Add(gems[j].Points * x[i, j]);
                model.Add(S[i] == LinearExpr.Sum(terms));
            }

            // (2) 코어 슬롯 ≤ MaxGemsPerCore
            for (int i = 0; i < I; i++)
            {
                var terms = new List<LinearExpr>();
                for (int j = 0; j < J; j++) terms.Add(x[i, j]);
                model.Add(LinearExpr.Sum(terms) <= opt.MaxGemsPerCore);
            }

            // (3) 코어 의지력 ≤ WillCapacity
            for (int i = 0; i < I; i++)
            {
                var terms = new List<LinearExpr>();
                for (int j = 0; j < J; j++) terms.Add(gems[j].WillCost * x[i, j]);
                model.Add(LinearExpr.Sum(terms) <= cores[i].WillCapacity);
            }

            // (4) 인벤 공유: 각 젬은 최대 1회 사용
            if (opt.InventoryShared)
            {
                for (int j = 0; j < J; j++)
                {
                    var terms = new List<LinearExpr>();
                    for (int i = 0; i < I; i++) terms.Add(x[i, j]);
                    model.Add(LinearExpr.Sum(terms) <= 1);
                }
            }

            // (5) 단계 연계 (양방향, Big-M 없이)
            foreach (var k in K)
                for (int i = 0; i < I; i++)
                {
                    model.Add(S[i] >= k).OnlyEnforceIf(u[(i, k)]);
                    model.Add(S[i] <= k - 1).OnlyEnforceIf(u[(i, k)].Not());
                }

            // (6) 단계 누적성: u[i,kt] ≤ u[i,kt-1]
            for (int i = 0; i < I; i++)
                for (int t = 1; t < K.Length; t++)
                    model.Add(u[(i, K[t])] <= u[(i, K[t - 1])]);

            // (7) 하드 최소 단계(옵션): S_i ≥ T*
            if (hardStageReqs != null && hardStageReqs.Count > 0)
            {
                var idx = new Dictionary<string, int>();
                for (int i = 0; i < I; i++) idx[cores[i].Id] = i;
                foreach (var req in hardStageReqs)
                {
                    if (!idx.TryGetValue(req.CoreId, out var i)) continue;
                    model.Add(S[i] >= req.MinStage);
                }
            }

            // ---------- 목적함수 ----------
            // 정수 스케일(×opt.Scale)로 변환
            long Scale(double v) => (long)Math.Round(v * opt.Scale);

            LinearExpr obj = LinearExpr.Sum(Enumerable.Empty<LinearExpr>());

            // (A) 코어 단계 증분 보너스
            for (int i = 0; i < I; i++)
            {
                var inc = cores[i].StageIncrement ?? new Dictionary<int, double>();
                foreach (var k in K)
                {
                    double delta = 0.0;
                    if (inc.TryGetValue(k, out var val) && val > 0) delta = val;
                    long w = Scale(delta);
                    if (w != 0) obj += w * u[(i, k)];
                }
            }

            // (B) 젬 옵션 가치
            for (int i = 0; i < I; i++)
                for (int j = 0; j < J; j++)
                {
                    long w = Scale(gems[j].Value);
                    if (w != 0) obj += w * x[i, j];
                }

            model.Maximize(obj);

            // ---------- 솔버 실행 ----------
            var solver = new CpSolver();
            var paramList = new List<string>();
            if (opt.Workers > 0) paramList.Add($"num_search_workers:{opt.Workers}");
            if (opt.TimeLimitSec > 0) paramList.Add($"max_time_in_seconds:{opt.TimeLimitSec}");
            if (opt.LogProgress) paramList.Add("log_search_progress:true");
            solver.StringParameters = string.Join(" ", paramList);

            var cb = new ProgressCb(opt.Scale);
            CpSolverStatus status = solver.Solve(model, cb);

            // ---------- 해 추출 ----------
            var sol = new Solution();

            double lb = solver.ObjectiveValue / opt.Scale;
            double ub = solver.BestObjectiveBound / opt.Scale;
            double gap = (ub - lb) / Math.Max(1e-9, Math.Abs(lb));
            sol.LB = lb; sol.UB = ub; sol.Gap = gap; sol.Objective = lb;
            sol.Timeline = cb.Points;

            for (int i = 0; i < I; i++)
            {
                var gemIds = new List<string>();
                var picked = new List<GemPickResult>(); // ✅ 상세 리스트
                int sumP = 0;
                int usedWill = 0;
                double sumV = 0.0;

                for (int j = 0; j < J; j++)
                {
                    if (solver.BooleanValue(x[i, j]))
                    {
                        var g = gems[j];
                        gemIds.Add(g.Id);

                        // ✅ 상세 객체 생성
                        picked.Add(new GemPickResult(
                            Id: g.Id,
                            WillCost: g.WillCost,
                            Points: g.Points,
                            Value: g.Value,
                            Atk: g.AtkLv,
                            Extra: g.ExtraLv,
                            Boss: g.BossLv
                        ));

                        sumP += g.Points;
                        usedWill += g.WillCost;
                        sumV += g.Value;
                    }
                }

                sol.CoreToGemIds[cores[i].Id] = gemIds;
                sol.CoreToGemDetails[cores[i].Id] = picked;      // ✅ 상세 저장
                sol.CoreStats[cores[i].Id] = (cores[i].Grade, sumP, usedWill);
                sol.CoreValueSum[cores[i].Id] = sumV;        // (선택) 합계 가치

                var stagesOn = new List<int>();
                foreach (var k in K)
                    if (solver.BooleanValue(u[(i, k)])) stagesOn.Add(k);
                sol.CoreStagesOn[cores[i].Id] = stagesOn;

                Trace.WriteLine($"\n[{cores[i].Id}] Grade={cores[i].Grade}  S={sumP}  Will={usedWill}/{cores[i].WillCapacity}  vSum={sumV:F3}");
                Trace.WriteLine($"  Stages ON: {(stagesOn.Count == 0 ? "(none)" : string.Join(", ", stagesOn))}");

                // ── 각 젬의 구체 정보 표시 ──
                foreach (var g in picked)
                {
                    Trace.WriteLine($"    + {g.Id} | Will={g.WillCost}, Pts={g.Points}, Atk={g.Atk}, Extra={g.Extra}, Boss={g.Boss}, Value={g.Value:F3}");
                    // Trace.WriteLine($"    + {g.Id} | Will={g.WillCost}, Pts={g.Points}, Value={g.Value:F3}, ATK={g.AtkLv}, BOSS={g.BossLv}, ADD={g.AddLv}");
                }
            }
            // 상태/목적값 요약은 루프 밖에서 한 번만
            Trace.WriteLine($"\nCP-SAT Status: {status}");
            Trace.WriteLine($"Objective(LB)={sol.LB:F6}, UB={sol.UB:F6}, gap={sol.Gap:P3}");
            Trace.WriteLine($"Timeline points = {sol.Timeline.Count}");

            return sol;

        }
    }
}