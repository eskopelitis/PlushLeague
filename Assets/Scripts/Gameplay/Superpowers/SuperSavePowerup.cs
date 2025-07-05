using UnityEngine;
using System.Collections;

namespace PlushLeague.Gameplay.Superpowers
{
    /// <summary>
    /// Super Save: Goalie instantly saves any shot within a large radius for 5 seconds
    /// Only usable by goalies
    /// </summary>
    [CreateAssetMenu(fileName = "SuperSave", menuName = "Plush League/Superpowers/Super Save")]
    public class SuperSavePowerup : BasePowerup
    {
        [Header("Super Save Settings")]
        [SerializeField] private float saveRadius = 15f;
        [SerializeField] private float saveDuration = 5f;
        [SerializeField] private Color saveAuraColor = Color.blue;
        [SerializeField] private GameObject saveFieldEffect;
        [SerializeField] private GameObject instantSaveEffect;
        
        public override bool CanActivate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!base.CanActivate(user)) return false;
            
            // Only goalies can use Super Save
            // Check if player is in goalie position or has goalie role
            return IsGoalie(user);
        }
        
        public override bool Activate(PlushLeague.Gameplay.Player.PlayerController user)
        {
            if (!CanActivate(user)) return false;
            
            // Apply super save effect
            var saveComponent = user.GetComponent<SuperSaveEffect>();
            if (saveComponent == null)
            {
                saveComponent = user.gameObject.AddComponent<SuperSaveEffect>();
            }
            
            saveComponent.Initialize(this, saveDuration, saveRadius);
            
            return true;
        }
        
        public override void OnEffectEnd(PlushLeague.Gameplay.Player.PlayerController user)
        {
            var saveComponent = user.GetComponent<SuperSaveEffect>();
            if (saveComponent != null)
            {
                saveComponent.RemoveEffect();
            }
        }
        
        private bool IsGoalie(PlushLeague.Gameplay.Player.PlayerController player)
        {
            // Check if player has GoalieSave ability (indicates they are a goalie)
            var abilityManager = player.GetComponent<PlushLeague.Gameplay.Abilities.AbilityManager>();
            if (abilityManager != null)
            {
                // Check if player has goalie save ability
                var goalieSave = abilityManager.GetComponent<PlushLeague.Gameplay.Abilities.GoalieSave>();
                return goalieSave != null;
            }
            
            // Fallback: check position near goal
            var goals = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            foreach (var goal in goals)
            {
                if (goal.CompareTag("Goal"))
                {
                    float distanceToGoal = Vector3.Distance(player.transform.position, goal.transform.position);
                    if (distanceToGoal < 20f) // Within 20 units of goal
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Perform instant save on a ball
        /// </summary>
        public void PerformInstantSave(PlushLeague.Gameplay.Ball.BallController ball, PlushLeague.Gameplay.Player.PlayerController goalie)
        {
            var ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb == null) return;
            
            // Stop the ball
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            
            // Move ball to goalie's hands
            ball.transform.position = goalie.transform.position + Vector3.up * 1.5f;
            
            // Give ball to goalie
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager != null)
            {
                ballManager.SetBallCarrier(goalie);
            }
            
            // Play save effect
            if (instantSaveEffect != null)
            {
                GameObject effect = Instantiate(instantSaveEffect, ball.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        public Color GetSaveAuraColor() => saveAuraColor;
        public GameObject GetSaveFieldEffect() => saveFieldEffect;
    }
    
    /// <summary>
    /// Component that handles the super save field effect
    /// </summary>
    public class SuperSaveEffect : MonoBehaviour
    {
        private SuperSavePowerup powerup;
        private float remainingTime;
        private float saveRadius;
        private GameObject saveFieldObject;
        private bool isActive = true;
        
        // Visual effects
        private Renderer[] renderers;
        private Color[] originalColors;
        
        public void Initialize(SuperSavePowerup savePowerup, float duration, float radius)
        {
            powerup = savePowerup;
            remainingTime = duration;
            saveRadius = radius;
            
            ApplyVisualEffects();
            CreateSaveField();
        }
        
        private void Update()
        {
            if (!isActive) return;
            
            // Check if effect expired
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                RemoveEffect();
                return;
            }
            
            // Check for balls within save radius
            CheckForBallsToSave();
        }
        
        private void CheckForBallsToSave()
        {
            var ballManager = FindFirstObjectByType<PlushLeague.Gameplay.Ball.BallManager>();
            if (ballManager == null) return;
            
            var ballController = ballManager.GetBallController();
            if (ballController == null) return;
            
            // Check if ball is within save radius and moving toward goal
            float distanceToBall = Vector3.Distance(transform.position, ballController.transform.position);
            
            if (distanceToBall <= saveRadius)
            {
                var ballRb = ballController.GetComponent<Rigidbody>();
                if (ballRb != null && ballRb.linearVelocity.magnitude > 5f) // Ball is moving fast (shot)
                {
                    // Check if ball is moving toward this goalie's goal
                    if (IsBallMovingTowardGoal(ballController, ballRb))
                    {
                        // Perform instant save
                        powerup.PerformInstantSave(ballController, GetComponent<PlushLeague.Gameplay.Player.PlayerController>());
                    }
                }
            }
        }
        
        private bool IsBallMovingTowardGoal(PlushLeague.Gameplay.Ball.BallController ball, Rigidbody ballRb)
        {
            // Find the goal closest to this goalie
            var goals = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            Collider closestGoal = null;
            float closestDistance = float.MaxValue;
            
            foreach (var goal in goals)
            {
                if (goal.CompareTag("Goal"))
                {
                    float distance = Vector3.Distance(transform.position, goal.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestGoal = goal;
                    }
                }
            }
            
            if (closestGoal == null) return false;
            
            // Check if ball is moving toward the goal
            Vector3 ballToGoal = (closestGoal.transform.position - ball.transform.position).normalized;
            Vector3 ballVelocity = ballRb.linearVelocity.normalized;
            
            float dot = Vector3.Dot(ballVelocity, ballToGoal);
            return dot > 0.5f; // Ball is moving toward goal
        }
        
        private void ApplyVisualEffects()
        {
            // Apply aura effect to goalie
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material != null)
                {
                    originalColors[i] = renderers[i].material.color;
                    
                    // Apply save aura color
                    Color saveColor = Color.Lerp(originalColors[i], powerup.GetSaveAuraColor(), 0.4f);
                    renderers[i].material.color = saveColor;
                }
            }
        }
        
        private void CreateSaveField()
        {
            if (powerup.GetSaveFieldEffect() != null)
            {
                saveFieldObject = Instantiate(powerup.GetSaveFieldEffect(), transform.position, transform.rotation, transform);
                
                // Scale the effect to match save radius
                saveFieldObject.transform.localScale = Vector3.one * (saveRadius / 5f); // Assuming base effect is 5 units
            }
        }
        
        public void RemoveEffect()
        {
            if (!isActive) return;
            
            isActive = false;
            
            // Restore original colors
            if (renderers != null && originalColors != null)
            {
                for (int i = 0; i < renderers.Length && i < originalColors.Length; i++)
                {
                    if (renderers[i] != null && renderers[i].material != null)
                    {
                        renderers[i].material.color = originalColors[i];
                    }
                }
            }
            
            // Remove save field effect
            if (saveFieldObject != null)
            {
                Destroy(saveFieldObject);
            }
            
            // Remove this component
            Destroy(this);
        }
        
        private void OnDestroy()
        {
            RemoveEffect();
        }
        
        // Visualize save radius in editor
        private void OnDrawGizmosSelected()
        {
            if (isActive)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, saveRadius);
            }
        }
    }
}
