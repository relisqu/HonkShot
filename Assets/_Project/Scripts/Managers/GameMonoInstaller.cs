using System.Collections.Generic;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.LevelSystem.LevelGeneration.Factories;
using Scripts.Managers.Tutorial;
using Scripts.Player;
using Scripts.Player.InputHandling;
using Scripts.PointSystem;
using Scripts.Progress;
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
        public InputHandler _inputHandler;
        public TutorialController _tutorialController;
        public LevelTransitionManager _levelTransitionManager;
        public PlayerBallMovement _playerBallMovement;
        

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputHandler>().FromInstance(_inputHandler).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerMovement>().FromInstance(_playerMovement).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelGenerator>().FromInstance(_levelGenerator).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelManager>().FromInstance(_levelManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PointReceiver>().FromInstance(_pointReceiver).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelTransitionManager>().FromInstance(_levelTransitionManager).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<LevelObjectsFactory>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ProgressSaver>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TutorialController>().FromInstance(_tutorialController).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerBallMovement>().FromInstance(_playerBallMovement).AsSingle().NonLazy();
        }

        public override void Start()
        {
            base.Start();
            Container.Resolve<ProgressSaver>().Start();
        }
    }
}