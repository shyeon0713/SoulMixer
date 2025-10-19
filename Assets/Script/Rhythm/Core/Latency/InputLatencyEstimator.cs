using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Rhythm.Core
{
    public class InputLatencyEstimator : MonoBehaviour
    {
        public Conductor conductor;
        public int window = 50;
        private readonly Queue<double> _samples = new Queue<double> ();
        private double _sum;

        public void AddSample(double inputEventTime, double dspTimeAtEvent)
        {
            double diff = dspTimeAtEvent - inputEventTime;  // Â÷ÀÌ Æò±Õ
            _samples.Enqueue (diff);
            _sum += diff;
            while (_samples.Count > window) _sum -= _samples.Dequeue ();
            double avg = _sum / _samples.Count;
            conductor.inputOffsetMs = (float)(avg * 1000.0);

        }
    }
}
