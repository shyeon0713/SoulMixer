using Rhythm.Charting;
using Rhythm.Core;
using Rhythm.Pooling;
using System.Collections.Generic;
using UnityEngine;
using static TreeEditor.TreeEditorHelper;

namespace Rhythm.Gameplay
{
    public class Spawner : MonoBehaviour
    {
        [Header("Refs")]
        public Conductor conductor;
        public Chart chart;
        public Noteview notePrefab;

        [Header("Spawn Params")]
        public float spawnLeanTime = 2.0f;
        public float scrollSpeed = 6.0f;
        public float despawnAfterSec = 1.0f;

        [Header("Lanes Layout")]
        public int laneCount = 3; // 라인은 총 3개
        public Vector2 judgeLinPos = Vector2.zero; //판정 라인 위치
        public Vector2 travelDir = Vector2.left;
        public float laneSpacing = 1.25f;

        private SimplePool<Noteview> pool;
        public Transform poolRoot;
        private int nextindex;
        private readonly List<Noteview> active = new List<Noteview>();

         void Awake()
         {
            pool = new SimplePool<Noteview>(notePrefab,initial:64, parent: poolRoot);
            nextindex = 0;
         }

        void Update()
        {
            if(chart == null || conductor == null) return;
            float now = conductor.NowSongSec();
            while (nextindex < chart.notes.Count)
            {
                var note = chart.notes[nextindex];
                if (note.timesec - now <= spawnLeanTime)
                {
                    Spawn(note);
                    nextindex++;
                }
                else break;
            }

        }

        private void Spawn(notedata note)
        {
            var v = pool.Get();
            Vector2 origin = judgeLinPos + new Vector2((note.lane - (laneCount - 1) * 0.5f)*laneSpacing,0); 
               
            v.Init(conductor, this, note.lane, note.timesec, note.type == notetype.hold ? note.duration : 0f,
            scrollSpeed, origin, travelDir);
            active.Add(v);
        }


        public void Despawn(Noteview v)
        {
            active.Remove(v);
            pool.Release(v);
        }


        public bool TryPeekNextNote(int lane, int fromIndex, out notedata note, out int index)
        {
            index = -1; note = default;
            for (int i = fromIndex; i < chart.notes.Count; i++)
            {
                if (chart.notes[i].lane == lane)
                { note = chart.notes[i]; index = i; return true; }
            }
            return false;
        }
    }
}