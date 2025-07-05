using UnityEngine;

namespace PlushLeague.Gameplay.Abilities
{
    /// <summary>
    /// Scriptable Object for creating and configuring SlideTackle ability instances
    /// This allows designers to create different configurations of slide tackle
    /// </summary>
    [CreateAssetMenu(fileName = "SlideTackleAbility", menuName = "Plush League/Abilities/Slide Tackle", order = 2)]
    public class SlideTackleAbilityAsset : SlideTackle
    {
        // This class inherits all functionality from SlideTackle
        // and is used solely for creating ScriptableObject assets in the Unity Editor
        
        // Additional configuration can be added here if needed for specific variants
        // For example: different tackle types, different effects, etc.
        
        [Header("Asset Info")]
        [SerializeField, TextArea(2, 4)] private new string description = "Standard slide tackle - lunges forward to steal ball or contest possession";
        
        public new string Description => description;
    }
}
