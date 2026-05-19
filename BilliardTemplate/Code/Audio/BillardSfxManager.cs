using ibc.solvers;
using Unity.Mathematics;
using UnityEngine;

namespace ibc.audio
{
    /// <summary>
    /// Manages billiard sound effects using the AudioManager system.
    /// Adds realistic randomness to pitch and volume for natural feel.
    /// </summary>
    public class BillardSfxManager : MonoBehaviour
    {
        [Header("Audio Layers")]
        [SerializeField] private string _ballCollisionLayer = "BallCollision";
        [SerializeField] private string _strikeLayer = "Strike";
        [SerializeField] private string _pocketLayer = "Pocket";
        [SerializeField] private string _cushionLayer = "Cushion";

        [Header("Ball Collision Settings")]
        [SerializeField] private float _minBallCollisionVelocity = 0.1f;
        [SerializeField] private float _maxBallCollisionVelocity = 3f;
        [SerializeField] private float _ballCollisionCooldown = 0.05f;

        [Header("Stick Impact Settings")]
        [SerializeField] private float _minStrikeVelocity = 0.1f;
        [SerializeField] private float _maxStrikeVelocity = 3f;

        [Header("Pocket Settings")]
        [SerializeField] private float _minPocketVelocity = 0.1f;
        [SerializeField] private float _maxPocketVelocity = 3f;

        [Header("Cushion Settings")]
        [SerializeField] private float _minCushionVelocity = 0.05f;
        [SerializeField] private float _maxCushionVelocity = 3f;
        [SerializeField] private float _cushionCollisionCooldown = 0.05f;

        [Header("Volume & Pitch Modulation")]
        [Tooltip("Volume curve based on intensity (0-1). Allows fine-tuning volume response.")]
        [SerializeField] private AnimationCurve _volumeCurve = AnimationCurve.Linear(0, 0.3f, 1, 1f);

        [Tooltip("Random pitch variation applied to each sound (-value to +value).")]
        [SerializeField] private float _pitchRandomRange = 0.05f;

        [Tooltip("Random volume variation applied to each sound (-value to +value).")]
        [SerializeField] private float _volumeRandomRange = 0.05f;

        [Header("Debug")]
        [SerializeField] private bool _logEvents = false;

        private float _lastBallCollisionTime = -999f;
        private float _lastCushionCollisionTime = -999f;

        private void Start()
        {
            if (AudioManager.Instance == null)
                Debug.LogError("BillardSfxManager: AudioManager not found! Make sure AudioManager exists in the scene.");
        }

        public void OnStrike(float velocity)
        {
            float speed = math.abs(velocity);
            float intensity = CalculateIntensity(speed, _minStrikeVelocity, _maxStrikeVelocity);
            float baseVolume = _volumeCurve.Evaluate(intensity);
            float finalVolume = ApplyRandomVolume(baseVolume);
            float pitchOffset = GetRandomPitch();

            if (_logEvents)
                Debug.Log($"Strike - Vel:{speed:F2} Int:{intensity:F2} Vol:{finalVolume:F2} Pitch:{pitchOffset:F2}");

            PlaySound(_strikeLayer, intensity, finalVolume, pitchOffset, "Strike");
        }

        public void OnPhysicsEvent(PhysicsSolver.Event ev)
        {
            switch (ev.Type)
            {
                case PhysicsSolver.EventType.PocketCollision: HandlePocketCollision(ev); break;
                case PhysicsSolver.EventType.BallCollision: HandleBallCollision(ev); break;
                case PhysicsSolver.EventType.CushionCollision: HandleCushionCollision(ev); break;
            }
        }

        private void HandlePocketCollision(PhysicsSolver.Event ev)
        {
            float speed = math.length(ev.Ball1Velocity);
            float intensity = CalculateIntensity(speed, _minPocketVelocity, _maxPocketVelocity);
            float baseVolume = _volumeCurve.Evaluate(intensity);
            float finalVolume = ApplyRandomVolume(baseVolume);
            float pitchOffset = GetRandomPitch();

            if (_logEvents)
                Debug.Log($"Pocket - Vel:{speed:F2} Int:{intensity:F2} Vol:{finalVolume:F2} Pitch:{pitchOffset:F2}");

            PlaySound(_pocketLayer, intensity, finalVolume, pitchOffset, "Pocket");
        }

        private void HandleBallCollision(PhysicsSolver.Event ev)
        {
            if (Time.time - _lastBallCollisionTime < _ballCollisionCooldown)
                return;

            float3 relativeVelocity = ev.Ball1Velocity - ev.Ball2Velocity;
            float speed = math.length(relativeVelocity);
            float intensity = CalculateIntensity(speed, _minBallCollisionVelocity, _maxBallCollisionVelocity);

            if (intensity < 0.01f)
                return;

            float baseVolume = _volumeCurve.Evaluate(intensity);
            float finalVolume = ApplyRandomVolume(baseVolume);
            float pitchOffset = GetRandomPitch();

            if (_logEvents)
                Debug.Log($"BallCollision - Vel:{speed:F2} Int:{intensity:F2} Vol:{finalVolume:F2} Pitch:{pitchOffset:F2}");

            PlaySound(_ballCollisionLayer, intensity, finalVolume, pitchOffset, "BallCollision");
            _lastBallCollisionTime = Time.time;
        }

        private void HandleCushionCollision(PhysicsSolver.Event ev)
        {
            if (Time.time - _lastCushionCollisionTime < _cushionCollisionCooldown)
                return;

            float speed = math.length(ev.Ball1Velocity);
            float intensity = CalculateIntensity(speed, _minCushionVelocity, _maxCushionVelocity);

            if (intensity < 0.01f)
                return;

            float baseVolume = _volumeCurve.Evaluate(intensity);
            float finalVolume = ApplyRandomVolume(baseVolume);
            float pitchOffset = GetRandomPitch();

            if (_logEvents)
                Debug.Log($"Cushion - Vel:{speed:F2} Int:{intensity:F2} Vol:{finalVolume:F2} Pitch:{pitchOffset:F2}");

            PlaySound(_cushionLayer, intensity, finalVolume, pitchOffset, "Cushion");
            _lastCushionCollisionTime = Time.time;
        }

        private float CalculateIntensity(float velocity, float minVelocity, float maxVelocity)
        {
            return math.clamp((velocity - minVelocity) / (maxVelocity - minVelocity), 0f, 1f);
        }

        private void PlaySound(string layerName, float intensity, float volume, float pitchOffset, string debugName)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogWarning($"BillardSfxManager: Cannot play {debugName} sound - AudioManager not available");
                return;
            }

            long soundId = AudioManager.Instance.PlaySFXByIntensity(layerName, intensity, volume, pitchOffset);

            if (soundId == -1 && _logEvents)
                Debug.LogWarning($"BillardSfxManager: Failed to play {debugName} sound from layer '{layerName}'");
        }

        private float GetRandomPitch()
        {
            return UnityEngine.Random.Range(-_pitchRandomRange, _pitchRandomRange);
        }

        private float ApplyRandomVolume(float baseVolume)
        {
            float offset = UnityEngine.Random.Range(-_volumeRandomRange, _volumeRandomRange);
            return math.clamp(baseVolume + offset, 0f, 1f);
        }

#if UNITY_EDITOR
        [ContextMenu("Test Strike (Medium)")]
        private void TestStrikeMedium() => OnStrike((_minStrikeVelocity + _maxStrikeVelocity) * 0.5f);
#endif
    }
}
