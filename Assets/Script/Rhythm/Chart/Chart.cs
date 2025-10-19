using System.Collections.Generic;
using UnityEngine;

namespace Rhythm.Charting
{
    [CreateAssetMenu(fileName = "Chart", menuName = "Script/Rhythm")]
    public class Chart : ScriptableObject
    {
        public chartmeta meta = new chartmeta();
        public List<notedata> notes = new List<notedata>();

    }
}



