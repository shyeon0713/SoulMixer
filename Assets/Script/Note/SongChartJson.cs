using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SongChartJson
{
    public string title; // æ«∞Ó¡¶∏Ò
    public float totalsec; //¿¸√º Ω√∞£ 
    public float totalLongSec;
    public List<NoteJson> notes;
}
