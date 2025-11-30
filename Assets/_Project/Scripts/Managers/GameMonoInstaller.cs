using System.Collections.Generic;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.Player;
using Scripts.PointSystem;
using Scripts.Services.Localization;
using UnityEngine;
using Zenject;

namespace Scripts.Managers
{
    public class GameMonoInstaller : MonoInstaller
    {
        public PlayerMovement _playerMovement;
        public LevelGenerator _levelGenerator;
        public LevelManager _levelManager;
        public PointReceiver _pointReceiver;


        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerMovement>().FromInstance(_playerMovement).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelGenerator>().FromInstance(_levelGenerator).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelManager>().FromInstance(_levelManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PointReceiver>().FromInstance(_pointReceiver).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelObjectsFactory>().AsSingle().NonLazy();
        }
    }
}