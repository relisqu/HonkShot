#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.LevelLab
{
    // All coordinates are room-local (the room root sits at the origin at runtime).
    [Serializable]
    public class RoomRecipe
    {
        public string id;
        public string name;
        public string idea;
        public int difficulty = 1;
        public bool chill;
        public float[] outline;
        public float[] spawn;
        public float[] exit;
        public float[] door;
        public RecipeObject[] objects;

        public List<Vector2> OutlinePoints()
        {
            var points = new List<Vector2>();
            if (outline == null) return points;
            for (int i = 0; i + 1 < outline.Length; i += 2)
                points.Add(new Vector2(outline[i], outline[i + 1]));
            return points;
        }

        public static Vector2 ToVector(float[] values, Vector2 fallback)
        {
            return values != null && values.Length >= 2 ? new Vector2(values[0], values[1]) : fallback;
        }

        public string FileName => $"{id}_{name}";
    }

    [Serializable]
    public class RecipeObject
    {
        public string key;
        public float x;
        public float y;
        public float rot;
        public float sx;
        public float sy;
        public float[] path;

        public Vector2 Position => new Vector2(x, y);

        public List<Vector2> PathPoints()
        {
            var points = new List<Vector2>();
            if (path == null) return points;
            for (int i = 0; i + 1 < path.Length; i += 2)
                points.Add(new Vector2(path[i], path[i + 1]));
            return points;
        }
        public Vector3 Scale => new Vector3(sx == 0 ? 1 : sx, sy == 0 ? (sx == 0 ? 1 : sx) : sy, 1);
    }
}
#endif
