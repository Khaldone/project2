using UnityEngine;
using BilliardGame.UI.Notifications;
using ibc.game;
using System;

namespace BilliardGame.Core
{
    public class BilliardNotificationIntegration : MonoBehaviour
    {
        [Header("Messages")]
        [SerializeField] private string solidsAssignedMessage = "You're Solids!";
        [SerializeField] private string stripesAssignedMessage = "You're Stripes!";
        [SerializeField] private string player1WinMessage = "Player 1 Wins!";
        [SerializeField] private string player2WinMessage = "Player 2 Wins!";
        [SerializeField] private string foulMessage = "Foul! Ball in hand.";
        [SerializeField] private string timeWarningMessage = "5 seconds remaining!";
        [SerializeField] private string eightBallOnBreakMessage = "8-Ball on break! Restarting...";

        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        private NotificationManager _notificationManager;
        private EightBallRules _rules;

        private void Start()
        {
            _notificationManager = NotificationManager.Instance;

            if (_gameManager == null)
                _gameManager = FindObjectOfType<GameManager>();

            SubscribeToGameEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }

        private void SubscribeToGameEvents()
        {
            if (_gameManager == null)
            {
                Debug.LogWarning("BilliardNotificationIntegration: GameManager not found.");
                return;
            }

            _rules = _gameManager.Rules as EightBallRules;
            if (_rules == null)
            {
                Debug.LogWarning("BilliardNotificationIntegration: Rules not available.");
                return;
            }

            _rules.Context.OnBallTypeAssigned += HandleBallTypeAssigned;
            _rules.OnGameWon += HandleGameWon;
            _rules.OnFoul += HandleFoul;
            _rules.OnTimeWarning += HandleTimeWarning;
            _rules.Context.OnEightBallOnBreak += HandleEightBallOnBreak;
            _rules.OnBallPocketed += HandleBallPocketed;
            _rules.OnTurnChanged += ShowTurnIndicator;

            ShowTurnIndicator(_rules.Context.CurrentPlayer);
        }

        private void UnsubscribeFromGameEvents()
        {
            if (_rules == null) return;

            _rules.Context.OnBallTypeAssigned -= HandleBallTypeAssigned;
            _rules.OnGameWon -= HandleGameWon;
            _rules.OnFoul -= HandleFoul;
            _rules.OnTimeWarning -= HandleTimeWarning;
            _rules.Context.OnEightBallOnBreak -= HandleEightBallOnBreak;
            _rules.OnBallPocketed -= HandleBallPocketed;
            _rules.OnTurnChanged -= ShowTurnIndicator;
        }

        private void HandleBallTypeAssigned(int playerId, PlayerSide ballType)
        {
            string message = ballType == PlayerSide.Solid ? solidsAssignedMessage : stripesAssignedMessage;
            _notificationManager?.ShowByType(NotificationType.Center, message);
        }

        private void HandleGameWon(int winnerId)
        {
            string message = winnerId == 1 ? player1WinMessage : player2WinMessage;
            _notificationManager?.ShowByType(NotificationType.Center, message);
        }

        private void HandleFoul(FoulType foulType)
        {
            string message = GetFoulMessage(foulType);
            _notificationManager?.ShowByType(NotificationType.Corner, message);
        }

        private void HandleTimeWarning()
        {
            _notificationManager?.ShowByType(NotificationType.Corner, timeWarningMessage, 5f);
        }

        private void HandleEightBallOnBreak()
        {
            _notificationManager?.ShowByType(NotificationType.Center, eightBallOnBreakMessage, 2.5f);
        }

        private void ShowTurnIndicator(int playerId)
        {
            string message = $"Player {playerId}'s Turn";
            _notificationManager?.ShowByType(NotificationType.Corner, message, 2f);
        }

        private void HandleBallPocketed(int ballNumber, BallType ballType)
        {
            string ballTypeText = GetBallTypeText(ballType);
            string message = $"{ballTypeText} {ballNumber} pocketed!";
            _notificationManager?.ShowByType(NotificationType.Corner, message, 1.5f);
        }

        private string GetBallTypeText(BallType ballType)
        {
            switch (ballType)
            {
                case BallType.Solid: return "Solid";
                case BallType.Stripe: return "Stripe";
                case BallType.Eight: return "8-Ball";
                case BallType.Cue: return "Cue";
                default: return "Ball";
            }
        }

        private string GetFoulMessage(FoulType foulType)
        {
            switch (foulType)
            {
                case FoulType.CueBallPocketed: return "Foul: Cue ball pocketed!";
                case FoulType.NoBallHit: return "Foul: No ball hit!";
                case FoulType.WrongBallHitFirst: return "Foul: Wrong ball hit first!";
                case FoulType.Timeout: return "Foul: Time expired!";
                default: return foulMessage;
            }
        }
    }
}