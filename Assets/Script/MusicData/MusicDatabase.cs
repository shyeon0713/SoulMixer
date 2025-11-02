using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicDatabase", menuName = "Scriptable Objects/MusicDatabase")]
public class MusicDatabase : ScriptableObject
{
    public List<SongEntry> songs = new();
}
