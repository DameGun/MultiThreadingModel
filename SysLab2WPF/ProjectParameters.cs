using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SysLab2WPF
{
    public class ProjectParameters
    {
        private int _arrayN;
        public int ArrayN
        {
            get => _arrayN;
            set => _arrayN = value; 
        }

        private int _k;
        public int K
        {
            get => _k;
            set => _k = value;
        }

        private int _threadsN;
        public int ThreadsN
        {
            get => _threadsN;
            set => _threadsN = value;
        }

        private int _deltaThreads;
        public int DeltaThreads
        {
            get => _deltaThreads;
            set => _deltaThreads = value;
        }

        private int _deltaK;
        public int DeltaK
        {
            get => _deltaK;
            set => _deltaK = value;
        }

        private string _projectMode;

        public string ProjectMode
        {
            get => _projectMode;
            set => _projectMode = value;
        }

        private int runtimeStep;

        public double[] a;
        public double[] b;

        public List<(int, int)> intervals = new List<(int, int)>();

        public Dictionary<double, double> plotData = new Dictionary<double, double>();

        private Stopwatch sw = new Stopwatch();

        public ProjectParameters(string userArrayN, string userK, string userThreadsN, 
            string userDeltaK, string userDeltaThreads)
        {
            ArrayN = int.Parse(userArrayN);
            K = int.Parse(userK);
            ThreadsN = int.Parse(userThreadsN);

            if(!string.IsNullOrEmpty(userDeltaK))
            {
                ProjectMode = "DeltaK";
                DeltaK = int.Parse(userDeltaK);                
            }
            else if(!string.IsNullOrEmpty(userDeltaThreads))
            {
                ProjectMode = "DeltaThreads";
                DeltaThreads = int.Parse(userDeltaThreads);
            }

            a = new double[ArrayN];
            b = new double[ArrayN];

            SetArrayRandomValues();
        }

        public void SetArrayRandomValues()
        {
            Random rnd = new Random();
            for (int i = 0; i < a.Length; i++) a[i] = rnd.NextDouble();
        }

        public void ResetForIteration()
        {
            sw.Reset();
            a = new double[ArrayN];
            b = new double[ArrayN];
            SetArrayRandomValues();
            intervals.Clear();
        }

        public void SetThreadsIntervals(int runtimeThreads)
        {
            int delta_thread = (int)Math.Ceiling((double)ArrayN / runtimeThreads);
            int leftVal = 0;
            int rightVal = delta_thread;
            for (int i = 0; i < runtimeThreads; i++)
            {
                if (leftVal >= ArrayN)
                {
                    leftVal = ArrayN - delta_thread;
                    rightVal = ArrayN;
                }
                if (rightVal >= ArrayN) rightVal = ArrayN;
                intervals.Add((leftVal, rightVal));
                leftVal += delta_thread;
                rightVal += delta_thread;
            }
        }

        public void SetKIntervals(int runtimeK)
        {
            int delta_k = (int)Math.Round((double)K / runtimeK);
            for (int i = 0, rangeVal = delta_k; i < runtimeK; i++, rangeVal += delta_k)
            {
                if (K - rangeVal < delta_k) delta_k = K - rangeVal;
            }
        }

        public async Task<Dictionary<double,double>> ProgramStart(string projectMode)
        {
            await Task.Run(() =>
            {
                switch (projectMode)
                {
                    case "DeltaK":
                        {
                            runtimeStep = DeltaK;
                            while (runtimeStep <= K)
                            {
                                SetThreadsIntervals(ThreadsN);
                                SetKIntervals(ThreadsN);
                                ParallelModel(runtimeStep);
                                runtimeStep += DeltaK;
                                ResetForIteration();
                            }
                            break;
                        }
                    case "DeltaThreads":
                        {
                            runtimeStep = DeltaThreads;
                            DeltaK = K;
                            while (runtimeStep <= ThreadsN)
                            {
                                SetThreadsIntervals(runtimeStep);
                                ParallelModel(runtimeStep);
                                runtimeStep += DeltaThreads;
                                ResetForIteration();
                            }
                            break;
                        }
                    default:
                        throw new ArgumentException("ProjectMode variable is null");
                }
            });
            return plotData;
        }

        private void ParallelModel(int stepValue)
        {
            int threadsNumber = ProjectMode == "DeltaK" ? ThreadsN : stepValue;

            Thread[] threads = new Thread[intervals.Count];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(Algorithm);
            }
            sw.Start();
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start(intervals[i]);
            }
            while (threads.Any(t => t.IsAlive)) ;
            LogResult(stepValue);
        }

        private void LogResult(int value)
        {
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            plotData.Add(value, ts.TotalMilliseconds);
        }

        private void Algorithm(object o)
        {
            int leftVal = (((int, int))o).Item1;
            int rightVal = (((int, int))o).Item2;
            for (int i = leftVal; i < rightVal; i++)
            {
                for (int j = 0; j < DeltaK; j++)
                {
                    b[i] += Math.Pow(a[i], 1.789);
                }
            }
        }

        public double TestAlgo()
        {
            sw.Start();
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < K; j++)
                {
                    b[i] += Math.Pow(a[i], 1.789);
                }
            }
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            sw.Reset();
            return ts.TotalMilliseconds;
        }
    }
}
