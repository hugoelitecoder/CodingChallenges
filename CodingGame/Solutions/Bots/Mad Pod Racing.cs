using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Player
{
    static class C
    {
        public const int SIMULATIONTURNS = 6;
        public const int SOLUTIONCOUNT = 8;
        public const int maxThrust = 200;
        public const int boostThrust = 650;
        public const int maxRotation = 18;
        public const int shieldCooldown = 4;
        public const float checkpointRadius = 600f;
        public const float checkpointRadiusSqr = checkpointRadius * checkpointRadius;
        public const float podRadius = 400f;
        public const float podRadiusSqr = podRadius * podRadius;
        public const float minImpulse = 120f;
        public const float frictionFactor = 0.85f;
        public const int timeoutFirstTurn = 950;
        public const int timeout = 70;
        public const float PI = 3.1415926535f;
    }

    public struct Vector2
    {
        public float x; public float y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public Vector2 Normalized()
        {
            float norm = (float)Math.Sqrt(x * x + y * y);
            if (norm < 1e-6f) return new Vector2(1, 0);
            return new Vector2(x / norm, y / norm);
        }
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator *(float k, Vector2 a) => new Vector2(k * a.x, k * a.y);
        public static float DistSqr(Vector2 a, Vector2 b) => (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
        public static float Dist(Vector2 a, Vector2 b) => (float)Math.Sqrt(DistSqr(a, b));
        public static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;
    }

    public struct Pod
    {
        public Vector2 position; public Vector2 speed; public int angle;
        public int checkpointId; public int checkpointsPassed;
        public bool canBoost; public int shieldCd; public int score;
        public int id;
    }

    public struct Move
    {
        public int rotation; public int thrust; public bool shield; public bool boost;
    }

    public struct Turn
    {
        public Move move0; public Move move1;
        public Move this[int m]
        {
            get => (m == 0) ? move0 : move1;
            set { if (m == 0) move0 = value; else move1 = value; }
        }
    }

    public class Solution
    {
        private readonly Turn[] turns = new Turn[C.SIMULATIONTURNS];
        public int score = int.MinValue;
        public void CopyFrom(Solution other)
        {
            this.score = other.score;
            for (int t = 0; t < C.SIMULATIONTURNS; t++) this.turns[t] = other.turns[t];
        }
        public Turn this[int t] { get => turns[t]; set => turns[t] = value; }
    }

    public class Simulation
    {
        private readonly List<Vector2> checkpoints;
        private readonly int checkpointCount;
        private readonly int maxCheckpoints;
        public static readonly float[] CosLUT = new float[360];
        public static readonly float[] SinLUT = new float[360];
        static Simulation()
        {
            for (int i = 0; i < 360; i++)
            {
                float angleRad = i * C.PI / 180f;
                CosLUT[i] = (float)Math.Cos(angleRad);
                SinLUT[i] = (float)Math.Sin(angleRad);
            }
        }
        public int MaxCheckpoints() => maxCheckpoints;
        public List<Vector2> Checkpoints() => checkpoints;
        public Simulation()
        {
            int laps = int.Parse(Console.ReadLine());
            checkpointCount = int.Parse(Console.ReadLine());
            maxCheckpoints = laps * checkpointCount;
            checkpoints = new List<Vector2>(checkpointCount);
            for (int i = 0; i < checkpointCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                checkpoints.Add(new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1])));
            }
        }
        public void PlaySolution(List<Pod> pods, Solution s)
        {
            for (int i = 0; i < C.SIMULATIONTURNS; i++) PlayOneTurn(pods, s[i]);
        }
        private void PlayOneTurn(List<Pod> pods, Turn turn)
        {
            Rotate(pods, turn); Accelerate(pods, turn); UpdatePositions(pods); Friction(pods); EndTurn(pods);
        }
        private void Rotate(List<Pod> pods, Turn turn)
        {
            for (int i = 0; i < 2; i++) { Pod p = pods[i]; p.angle += turn[i].rotation; pods[i] = p; }
        }
        private void Accelerate(List<Pod> pods, Turn turn)
        {
            for (int i = 0; i < 2; i++)
            {
                Pod p = pods[i]; Move m = turn[i]; ManageShield(m.shield, ref p);
                if (p.shieldCd <= 0)
                {
                    int angleNorm = p.angle % 360; if (angleNorm < 0) angleNorm += 360;
                    var direction = new Vector2(CosLUT[angleNorm], SinLUT[angleNorm]);
                    bool useBoost = p.canBoost && m.boost; int thrust = useBoost ? C.boostThrust : m.thrust;
                    if (useBoost) p.canBoost = false;
                    p.speed += thrust * direction;
                }
                pods[i] = p;
            }
        }
        private void UpdatePositions(List<Pod> pods)
        {
            float t = 0f;
            while (t < 1.0f)
            {
                int a_idx = -1; int b_idx = -1; float dt = 1.0f - t;
                for (int i = 0; i < 4; i++) for (int j = i + 1; j < 4; j++)
                    {
                        float collisionTime = CollisionTime(pods[i], pods[j]);
                        if ((collisionTime > 0f) && (t + collisionTime < 1.0f) && (collisionTime < dt)) { dt = collisionTime; a_idx = i; b_idx = j; }
                    }
                for (int i = 0; i < pods.Count; i++)
                {
                    Pod p = pods[i];
                    p.position += dt * p.speed;
                    while (Vector2.DistSqr(p.position, checkpoints[p.checkpointId]) < C.checkpointRadiusSqr)
                    {
                        p.checkpointsPassed++;
                        p.checkpointId = (p.checkpointId + 1) % checkpointCount;
                    }
                    pods[i] = p;
                }
                if (a_idx != -1) Rebound(pods, a_idx, b_idx);
                t += dt;
            }
        }
        private void Friction(List<Pod> pods)
        {
            for (int i = 0; i < pods.Count; i++) { Pod p = pods[i]; p.speed.x *= C.frictionFactor; p.speed.y *= C.frictionFactor; pods[i] = p; }
        }
        private void EndTurn(List<Pod> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                Pod p = pods[i]; p.speed.x = (int)p.speed.x; p.speed.y = (int)p.speed.y;
                p.position.x = (float)Math.Round(p.position.x);
                p.position.y = (float)Math.Round(p.position.y);
                pods[i] = p;
            }
        }
        private static void ManageShield(bool turnOn, ref Pod p) { if (turnOn) p.shieldCd = C.shieldCooldown; else if (p.shieldCd > 0) --p.shieldCd; }
        private static int Mass(in Pod p) => p.shieldCd == C.shieldCooldown ? 10 : 1;
        private static float CollisionTime(Pod p1, Pod p2)
        {
            var dP = p2.position - p1.position; var dS = p2.speed - p1.speed; float a = Vector2.Dot(dS, dS);
            if (a < 1e-6f) return float.PositiveInfinity; float b = 2f * Vector2.Dot(dP, dS); float c = Vector2.Dot(dP, dP) - 4f * C.podRadiusSqr;
            float delta = b * b - 4f * a * c; if (delta < 0f) return float.PositiveInfinity;
            float t = (-b - (float)Math.Sqrt(delta)) / (2f * a); return t <= 1e-6f ? float.PositiveInfinity : t;
        }
        private static void Rebound(List<Pod> pods, int idxA, int idxB)
        {
            Pod a = pods[idxA]; Pod b = pods[idxB]; float mA = Mass(a); float mB = Mass(b); var dP = b.position - a.position;
            float abDist = Vector2.Dist(a.position, b.position); if (abDist < 1.0f) abDist = 1.0f; var u = (1f / abDist) * dP;
            var dS = b.speed - a.speed; float m = (mA * mB) / (mA + mB); float k = Vector2.Dot(dS, u);
            if (k > 0) return;
            float impulse = -2f * m * k;
            if (impulse < C.minImpulse) impulse = C.minImpulse;
            var impulseVec = impulse * u;
            a.speed -= (1f / mA) * impulseVec; b.speed += (1f / mB) * impulseVec; pods[idxA] = a; pods[idxB] = b;
        }
    }

    class Solver
    {
        private readonly List<Solution> solutions;
        private readonly Simulation sim;
        private readonly List<Pod> simulationPods;
        private static uint g_seed = (uint)DateTime.Now.Ticks;
        public Solver(Simulation parSimulation)
        {
            sim = parSimulation; solutions = new List<Solution>(2 * C.SOLUTIONCOUNT);
            simulationPods = new List<Pod>(4); InitPopulation();
            FirstTurnBoostHack();
        }
        public Solution Solve(List<Pod> pods, int time)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < C.SOLUTIONCOUNT; ++i) { ShiftByOneTurn(solutions[i]); ComputeScore(solutions[i], pods); }
            int step = 0;
            while (stopwatch.ElapsedMilliseconds < time)
            {
                for (int i = 0; i < C.SOLUTIONCOUNT; ++i)
                {
                    solutions[C.SOLUTIONCOUNT + i].CopyFrom(solutions[i]);
                    Mutate(solutions[C.SOLUTIONCOUNT + i]);
                    ComputeScore(solutions[C.SOLUTIONCOUNT + i], pods);
                }
                solutions.Sort((a, b) => b.score.CompareTo(a.score));
                ++step;
            }
            return solutions[0];
        }
        private static int FastRand() { g_seed = (214013 * g_seed + 2531011); return (int)((g_seed >> 16) & 0x7FFF); }
        private static int Rnd(int a, int b) => (FastRand() % (b - a)) + a;
        private void InitPopulation()
        {
            for (int i = 0; i < 2 * C.SOLUTIONCOUNT; ++i) solutions.Add(new Solution());
            for (int s = 0; s < C.SOLUTIONCOUNT; s++) for (int t = 0; t < C.SIMULATIONTURNS; t++)
                {
                    var turn = solutions[s][t]; Randomize(ref turn.move0); Randomize(ref turn.move1); solutions[s][t] = turn;
                }
        }
        private void FirstTurnBoostHack()
        {
            float d = Vector2.DistSqr(sim.Checkpoints()[0], sim.Checkpoints()[1]);
            if (d < 9000000f) return;
            for (int s = 0; s < C.SOLUTIONCOUNT; ++s)
            {
                var turn = solutions[s][0]; turn.move0.boost = true; turn.move1.boost = false; solutions[s][0] = turn;
            }
        }
        private void Randomize(ref Move m, bool modifyAll = true)
        {
            const int pRot = 10, pThrust = pRot + 8, pShield = pThrust + 1;
            int choice = modifyAll ? -1 : Rnd(0, pShield);
            if (choice == -1 || choice < pRot) m.rotation = Rnd(-C.maxRotation, C.maxRotation + 1);
            if (choice == -1 || (choice >= pRot && choice < pThrust)) m.thrust = Rnd(0, C.maxThrust + 1);
            if (choice == -1 || choice >= pThrust) m.shield = Rnd(0, 10) > 8;
        }
        private void ShiftByOneTurn(Solution s)
        {
            for (int t = 0; t < C.SIMULATIONTURNS - 1; t++) s[t] = s[t + 1];
            var newTurn = new Turn(); Randomize(ref newTurn.move0); Randomize(ref newTurn.move1); s[C.SIMULATIONTURNS - 1] = newTurn;
        }
        private void Mutate(Solution s)
        {
            int t = Rnd(0, C.SIMULATIONTURNS);
            int i = Rnd(0, 2);
            Turn turn = s[t]; Move move = turn[i]; Randomize(ref move, false); turn[i] = move; s[t] = turn;
        }
        private int ComputeScore(Solution sol, List<Pod> pods)
        {
            simulationPods.Clear();
            for (int i = 0; i < pods.Count; i++) simulationPods.Add(pods[i]);
            sim.PlaySolution(simulationPods, sol);
            sol.score = RateSolution(simulationPods); return sol.score;
        }
        private int RateSolution(List<Pod> pods)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                Pod p = pods[i]; const int cpFactor = 50000;
                float distToCp = Vector2.Dist(p.position, sim.Checkpoints()[p.checkpointId]);
                p.score = cpFactor * p.checkpointsPassed - (int)distToCp; pods[i] = p;
            }
            Pod myPod0 = pods[0];
            Pod myPod1 = pods[1];
            Pod oppPod0 = pods[2];
            Pod oppPod1 = pods[3];

            Pod myRacer = (myPod0.score > myPod1.score) ? myPod0 : myPod1;
            Pod myBlocker = (myPod0.score > myPod1.score) ? myPod1 : myPod0;
            Pod opponentRacer = (oppPod0.score > oppPod1.score) ? oppPod0 : oppPod1;

            if (myRacer.checkpointsPassed >= sim.MaxCheckpoints()) return int.MaxValue;
            if (opponentRacer.checkpointsPassed >= sim.MaxCheckpoints()) return int.MinValue;

            int racerScore = myRacer.score * 10;
            Vector2 nextCpPos = sim.Checkpoints()[myRacer.checkpointId];
            Vector2 dirToNextCp = (nextCpPos - myRacer.position).Normalized();
            racerScore += (int)(Vector2.Dot(myRacer.speed, dirToNextCp) * 0.5f);

            int leadScore = (myRacer.score - opponentRacer.score) * 100;

            int blockerScore = 0;
            Vector2 opponentNextCpPos = sim.Checkpoints()[opponentRacer.checkpointId];
            Vector2 targetVector = (opponentNextCpPos - opponentRacer.position).Normalized();
            Vector2 interceptPoint = opponentRacer.position + 300f * targetVector;
            blockerScore -= (int)Vector2.Dist(myBlocker.position, interceptPoint);
            blockerScore -= (int)Vector2.Dist(myBlocker.position, opponentRacer.position);
            if (myBlocker.shieldCd > 0 && Vector2.DistSqr(myBlocker.position, opponentRacer.position) < (C.podRadius * 2.5f) * (C.podRadius * 2.5f))
            {
                blockerScore += 40000;
                blockerScore += (int)(Math.Sqrt(Vector2.Dot(opponentRacer.speed, opponentRacer.speed)) * 2);
            }

            int friendlyFirePenalty = 0;
            float distSqrBetweenFriends = Vector2.DistSqr(myRacer.position, myBlocker.position);
            if (distSqrBetweenFriends < (C.podRadius * 4.0f) * (C.podRadius * 4.0f))
            {
                Vector2 racerToCpVec = (nextCpPos - myRacer.position).Normalized();
                Vector2 racerToBlockerVec = myBlocker.position - myRacer.position;
                float dot = Vector2.Dot(racerToBlockerVec.Normalized(), racerToCpVec);

                if (dot > 0.5f)
                {
                    friendlyFirePenalty += (int)(200000 * dot / Math.Sqrt(distSqrBetweenFriends));
                }

                float speedDot = Vector2.Dot(myRacer.speed.Normalized(), racerToCpVec);
                if (speedDot < 0.2f)
                {
                    friendlyFirePenalty += 50000;
                }
            }
            return leadScore + racerScore + blockerScore - friendlyFirePenalty;
        }
    }

    static void ConvertSolutionToOutput(Solution sol, List<Pod> pods)
    {
        for (int i = 0; i < 2; i++)
        {
            var p = pods[i]; var m = sol[0][i]; int angle = p.angle + m.rotation; int angleNorm = angle % 360; if (angleNorm < 0) angleNorm += 360;
            var dir = new Vector2(Simulation.CosLUT[angleNorm], Simulation.SinLUT[angleNorm]); var target = p.position + 10000f * dir;
            Console.Write((int)Math.Round(target.x) + " " + (int)Math.Round(target.y) + " ");
            if (m.shield && p.shieldCd <= 0) Console.WriteLine("SHIELD");
            else
               if (m.boost && p.canBoost) Console.WriteLine("BOOST");
            else Console.WriteLine(m.thrust);
        }
    }
    static void UpdatePodsShieldBoostForNextTurn(Solution sol, List<Pod> pods)
    {
        for (int i = 0; i < 2; i++)
        {
            Pod p = pods[i]; Move m = sol[0][i];
            if (m.boost && p.canBoost && p.shieldCd <= 0) p.canBoost = false;
            if (m.shield && p.shieldCd <= 0) p.shieldCd = C.shieldCooldown;
            else if (p.shieldCd > 0) --p.shieldCd;
            pods[i] = p;
        }
    }
    static void OverrideAngle(ref Pod p, Vector2 target)
    {
        var dir = (target - p.position).Normalized(); float clampedX = Math.Clamp(dir.x, -1f, 1f);
        float a = (float)(Math.Acos(clampedX) * 180.0 / C.PI); if (dir.y < 0) a = 360f - a; p.angle = (int)a;
    }
    static Pod UpdatePodFromInput(Pod existingPod, int turn)
    {
        string[] inputs = Console.ReadLine().Split(' '); var newPod = existingPod;
        newPod.position = new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1])); newPod.speed = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3]));
        newPod.angle = int.Parse(inputs[4]); int nextCPId = int.Parse(inputs[5]);
        if (turn > 0 && existingPod.checkpointId != nextCPId)
        {
            newPod.checkpointsPassed++;
        }
        newPod.checkpointId = nextCPId;
        return newPod;
    }
    static string GetActionString(Move m, Pod p)
    {
        if (m.shield && p.shieldCd <= 0) return "SHIELD";
        if (m.boost && p.canBoost) return "BOOST";
        return m.thrust.ToString();
    }
    static void Main(string[] args)
    {
        var simulation = new Simulation(); var solver = new Solver(simulation);
        var pods = new List<Pod>(4) { new Pod { id = 0, canBoost = true }, new Pod { id = 1, canBoost = true }, new Pod { id = 2, canBoost = true }, new Pod { id = 3, canBoost = true } };
        int step = 0;
        while (true)
        {
            for (int i = 0; i < 4; i++) pods[i] = UpdatePodFromInput(pods[i], step);
            if (step == 0)
            {
                Vector2 firstCP = simulation.Checkpoints()[1];
                for (int i = 0; i < 2; i++) { Pod p = pods[i]; OverrideAngle(ref p, firstCP); pods[i] = p; }
            }
            int time = (step == 0) ? C.timeoutFirstTurn : C.timeout;
            Solution s = solver.Solve(pods, time);

            Console.Error.WriteLine($"[DEBUG] Best Score: {s.score}");
            Console.Error.WriteLine($"[DEBUG] Pod 0 Action: {GetActionString(s[0][0], pods[0])}");
            Console.Error.WriteLine($"[DEBUG] Pod 1 Action: {GetActionString(s[0][1], pods[1])}");

            ConvertSolutionToOutput(s, pods);
            UpdatePodsShieldBoostForNextTurn(s, pods);
            step++;
        }
    }
}