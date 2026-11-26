#if UNITY_EDITOR
using System.Collections.Generic;

namespace Scripts.LevelLab
{
    public static class LevelLabCatalog
    {
        private const string Root = "Assets/_Project/Resources/Prefabs/";

        private static readonly Dictionary<string, string> _paths = new Dictionary<string, string>
        {
            { "SmallFly", "Enemies/StartFloor/SmallFlyEnemy.prefab" },
            { "BigFly", "Enemies/StartFloor/BigFlyEnemy Variant.prefab" },
            { "Shooter", "Enemies/StartFloor/DefaultShooterEnemy Variant.prefab" },
            { "LaserShooter", "Enemies/StartFloor/LaserShootingEnemy Variant.prefab" },
            { "SpreadShooter", "Enemies/StartFloor/SpreadShooterEnemy Variant.prefab" },
            { "Follower", "Enemies/StartFloor/FollowerEnemy Variant.prefab" },
            { "Walker", "Enemies/StartFloor/WalkerSpot.prefab" },
            { "WalkerSpot", "Enemies/StartFloor/WalkerSpot.prefab" },
            { "Bouncer", "LevelObjects/Bouncers/Bouncer.prefab" },
            { "Bouncer1", "LevelObjects/Bouncers/Bouncer 1.prefab" },
            { "Bouncer2", "LevelObjects/Bouncers/Bouncer 2.prefab" },
            { "Bouncer3", "LevelObjects/Bouncers/Bouncer 3.prefab" },
            { "Bouncer4", "LevelObjects/Bouncers/Bouncer 4.prefab" },
            { "BouncerV", "LevelObjects/Bouncers/Bouncer Variant.prefab" },
            { "BouncerV1", "LevelObjects/Bouncers/Bouncer Variant 1.prefab" },
            { "SphereBouncer", "LevelObjects/Bouncers/SphereBouncer.prefab" },
            { "Arrow", "LevelObjects/Arrow.prefab" },
            { "Cannon", "LevelObjects/Cannon.prefab" },
            { "Hole", "LevelObjects/Hole.prefab" },
            { "PointObject", "LevelObjects/PointObject.prefab" },
            { "MoveRoad", "LevelObjects/MoveRoads/MoveRoad.prefab" },
            { "InnerWall", "LevelObjects/InnerWallObject.prefab" },
        };

        public static IEnumerable<string> AllKeys => _paths.Keys;

        public static bool TryGetPrefabPath(string key, out string path)
        {
            if (key != null && _paths.TryGetValue(key, out var relative))
            {
                path = Root + relative;
                return true;
            }

            path = null;
            return false;
        }

        public static bool TryGetKeyForPath(string assetPath, out string key)
        {
            foreach (var pair in _paths)
            {
                if (Root + pair.Value != assetPath) continue;
                key = pair.Key;
                return true;
            }

            key = null;
            return false;
        }
    }
}
#endif
