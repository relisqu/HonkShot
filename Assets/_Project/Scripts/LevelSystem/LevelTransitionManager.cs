using System.Collections;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.LevelSystem.TechnicalScripts;
using Scripts.Player;
using Scripts.UI;
using UnityEngine;
using Zenject;

namespace Scripts.LevelSystem
{
    public class LevelTransitionManager : MonoBehaviour
    {
        private PlayerMovement _playerMovement;
        private LevelObjectsFactory _levelObjectsFactory;

        [Space] [Header(("Item generation"))] [Range(0f, 1f)] [SerializeField]
        private float _itemScreenSpawnChance;

        [Min(1)]
        [Tooltip("Once per how many rooms item room will try spawn - if select 1, item will try to spawn every room")]
        [SerializeField]
        private int _itemScreenPerRoomRate = 2;

        [SerializeField] private BackgroundPropSpawner _backgroundPropSpawner;
        [SerializeField] private float _timeTransitionDuration = 1.2f;
        [SerializeField] private ItemSelectionUI _itemSelectionUI;


        [Inject]
        private void Construct(PlayerMovement playerMovement, LevelObjectsFactory levelObjectsFactory)
        {
            _playerMovement = playerMovement;
            _levelObjectsFactory = levelObjectsFactory;
        }

        private bool ShouldGiveItems(int completedRooms)
        {
            var shouldTryGenerateRoom = completedRooms % _itemScreenPerRoomRate == 0;
            if (shouldTryGenerateRoom)
            {
                return Random.value <= _itemScreenSpawnChance;
            }

            return false;
        }

        public void ShowItemSelectionUI(int completedRooms)
        {
            if (ShouldGiveItems(completedRooms))
            {
                bool itemSelected = false;
                int selectedItem = -1;
                _itemSelectionUI.ShowItems(null);
            }
        }

        public IEnumerator ShowEndLevelTransition(int completedRooms)
        {
            ShowItemSelectionUI(completedRooms);

            while (_itemSelectionUI.IsActive)
            {
                yield return null;
            }
        }

        public void PrepareRoom(Room room)
        {
            _backgroundPropSpawner.SetLevel(room.SpriteShapeController);
        }

        public IEnumerator SpawnEnterPortal(Room room)
        {
            var portal = _levelObjectsFactory.SpawnLevelEnterPortal(null);
            portal.transform.position = room.SpawnPointTransform.position;
            portal.PlayJumpOut(_playerMovement.GetTransform());

            yield return new WaitUntil(portal.FinishedAnimation);
            float t = 0f;
            while (t < _timeTransitionDuration)
            {
                TimeManager.Instance.SlowGame(Mathf.Lerp(0f, 1f, t / _timeTransitionDuration));
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public LevelExitPortal SpawnExitPortal(Room room)
        {
            var portal = _levelObjectsFactory.SpawnLevelExitPortal(null);
            portal.transform.position = room.ExitPortalSpawnPoint.position;
            return portal;
        }
    }
}