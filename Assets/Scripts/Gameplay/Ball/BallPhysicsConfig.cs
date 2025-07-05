using UnityEngine;

namespace PlushLeague.Gameplay.Ball
{
    /// <summary>
    /// Scriptable Object for configuring ball physics and behavior
    /// Allows designers to easily tweak ball feel without code changes
    /// </summary>
    [CreateAssetMenu(fileName = "BallPhysicsConfig", menuName = "Plush League/Ball Physics Config")]
    public class BallPhysicsConfig : ScriptableObject
    {
        [Header("Possession")]
        [Tooltip("Local offset from player center where ball follows during dribble")]
        public Vector2 dribbleOffset = new Vector2(0, -0.5f);
        
        [Tooltip("How quickly ball follows player during dribble (higher = tighter)")]
        [Range(1f, 50f)]
        public float followLerpSpeed = 25f;
        
        [Tooltip("Maximum distance for ball possession/stealing")]
        [Range(0.5f, 3f)]
        public float possessionRadius = 1.0f;
        
        [Header("Free Ball Physics")]
        [Tooltip("Drag when ball is free (higher = stops faster)")]
        [Range(0f, 10f)]
        public float freeBallDrag = 2f;
        
        [Tooltip("Drag when ball is being dribbled")]
        [Range(0f, 5f)]
        public float dribbleDrag = 0f;
        
        [Tooltip("Bounciness of ball off walls and objects")]
        [Range(0f, 1f)]
        public float bounciness = 0.7f;
        
        [Tooltip("Friction with ground/surfaces")]
        [Range(0f, 1f)]
        public float friction = 0.3f;
        
        [Header("Kick Prevention")]
        [Tooltip("Time after kick before ball can be possessed again")]
        [Range(0f, 1f)]
        public float kickCooldown = 0.1f;
        
        [Header("Sound Thresholds")]
        [Tooltip("Minimum velocity for bounce sound")]
        [Range(0f, 5f)]
        public float bounceVelocityThreshold = 2f;
        
        [Tooltip("Volume multiplier for ball sounds")]
        [Range(0f, 2f)]
        public float soundVolume = 1f;
        
        [Header("Visual Effects")]
        [Tooltip("Enable ball trail during high-speed movement")]
        public bool enableTrail = true;
        
        [Tooltip("Minimum speed for trail effect")]
        [Range(0f, 10f)]
        public float trailSpeedThreshold = 5f;
        
        [Tooltip("Duration of possession effect")]
        [Range(0f, 1f)]
        public float possessionEffectDuration = 0.2f;
    }
}
