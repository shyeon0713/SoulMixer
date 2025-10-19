using UnityEngine;
using Rhythm.Core;
using TMPro.Examples;
using Unity.VisualScripting;

namespace Rhythm.Gameplay
{
    public class Noteview : MonoBehaviour
    {
        public int lane;
        public float hitTimeSec;
        public float duration;

        private Conductor conductor;
        private Spawner spawner;

        public float scrollSpeed = 5f;
        public Vector2 lanerOrigin;
        public Vector2 travelDir = Vector2.left; //왼쪽 방향으로 이동

        public void Init(Conductor c, Spawner s, int laneIdx, float hitSec, float dur, float speed, Vector2 origin, Vector2 dir)
        {
            conductor = c;
            spawner = s;
            lane = laneIdx;
            hitTimeSec = hitSec;
            duration = dur;
            scrollSpeed = speed;
            lanerOrigin = origin;
            travelDir = dir.normalized;

        }
        void Update()
        {
            if (conductor != null) return;
            float now = conductor.NowSongSec();
            float tToHit = hitTimeSec - now;
            Vector2 pos = lanerOrigin - travelDir *(tToHit *scrollSpeed);
            transform.position = pos;

            

        }
    }
}

