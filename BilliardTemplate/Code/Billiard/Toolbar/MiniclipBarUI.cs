using System.Collections;
using System.Collections.Generic;
using ibc.trajectory;
using UnityEngine;

namespace ibc.game
{
    public class MiniclipBarUI : MonoBehaviour
    {
        [Header("Game Refs")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private BallColorList _ballColors;
        [Header("UI Refs")] 
        [SerializeField] private PlayerPanelUI _leftPanel;
        [SerializeField] private PlayerPanelUI _rightPanel;
        [SerializeField] private Sprite _p1Avatar;
        [SerializeField] private Sprite _p2Avatar;

        private int _activePlayer = 1;

        private void Start()
        {
            if (_gameManager == null)
            {
                Debug.LogError("GameManager not assigned to MiniclipBarUI!");
                this.enabled = false;
                return;
            }

            _leftPanel.Setup(_ballColors, _p1Avatar);
            _rightPanel.Setup(_ballColors, _p2Avatar);
            
            SubscribeToEvents();
            InitializeUIState();
        }

        private void SubscribeToEvents()
        {
            _gameManager.OnTurnStarted += OnTurnChanged;
            _gameManager.OnTimerUpdated += OnTimerUpdated;
            _gameManager.OnBallPocketedUpdate += RefreshPocketedStrips;
            _gameManager.OnSidesAssigned += RefreshPocketedStrips;
            _gameManager.OnGameWonEvent += OnGameWon;
            _gameManager.OnGameResetEvent += OnGameReset;
        }

        private void OnDisable()
        {
            if (_gameManager == null) return;

            _gameManager.OnTurnStarted -= OnTurnChanged;
            _gameManager.OnTimerUpdated -= OnTimerUpdated;
            _gameManager.OnBallPocketedUpdate -= RefreshPocketedStrips;
            _gameManager.OnSidesAssigned -= RefreshPocketedStrips;
            _gameManager.OnGameWonEvent -= OnGameWon;
            _gameManager.OnGameResetEvent -= OnGameReset;
        }

        private void InitializeUIState()
        {
            _activePlayer = _gameManager.Context.CurrentPlayer;
            RefreshPocketedStrips();
            UpdateHighlight();
            ResetTimerVisuals();
        }

        private void OnTurnChanged(int currentPlayer)
        {
            _activePlayer = currentPlayer;
            UpdateHighlight();
            ResetTimerVisuals();
        }

        private void OnTimerUpdated(float t)
        {
            if (_activePlayer == 1)
            {
                _leftPanel.SetTimerFill(t);
                _rightPanel.SetTimerFill(0f);
            }
            else
            {
                _rightPanel.SetTimerFill(t);
                _leftPanel.SetTimerFill(0f);
            }
        }

        private void UpdateHighlight()
        {
            _leftPanel.SetActiveHighlight(_activePlayer == 1);
            _rightPanel.SetActiveHighlight(_activePlayer == 2);
        }

        private void OnGameWon(int winnerId)
        {
            _leftPanel.SetTimerFill(0f);
            _rightPanel.SetTimerFill(0f);
        }

        private void OnGameReset()
        {
            InitializeUIState();
        }

        private void ResetTimerVisuals()
        {
            if (_activePlayer == 1)
            {
                _leftPanel.SetTimerFill(1f);
                _rightPanel.SetTimerFill(0f);
            }
            else
            {
                _rightPanel.SetTimerFill(1f);
                _leftPanel.SetTimerFill(0f);
            }
        }

        private void RefreshPocketedStrips()
        {
            Debug.Log("Refreshing Pocketed Strips");
            var ctx = _gameManager.Context;
            var rules = _gameManager.Rules;
            var p1 = new List<(int, BallType)>();
            var p2 = new List<(int, BallType)>();

            if (ctx.SidesAssigned)
            {
                foreach (var id in ctx.PocketedBalls)
                {
                    var type = rules.GetBallType(id);
                    if (type == BallType.Solid && ctx.Player1Side == PlayerSide.Solid) p1.Add((id, type));
                    if (type == BallType.Solid && ctx.Player2Side == PlayerSide.Solid) p2.Add((id, type));
                    if (type == BallType.Stripe && ctx.Player1Side == PlayerSide.Stripe) p1.Add((id, type));
                    if (type == BallType.Stripe && ctx.Player2Side == PlayerSide.Stripe) p2.Add((id, type));
                }
            }

            _leftPanel.RenderPocketedBalls(p1);
            _rightPanel.RenderPocketedBalls(p2);
        }
    }
}