using UnityEngine;

namespace BomberTeen
{
    /// <summary>
    /// Global repository for string constants to prevent typos and ease refactoring.
    /// </summary>
    public static class Constants
    {
        public struct Menus
        {
            public const string Login = "Login";
            public const string MainMenu = "MainMenu";
            public const string Lobby = "Lobby";
            public const string Options = "Options";
            public const string GameOver = "GameOver";
            public const string Win = "Win";
            
            // For SceneManager
            public const string MainMenuScene = "MenuInicial";
        }

        public struct Audio
        {
            public const string Theme = "Theme";
            public const string Click = "Click";
            public const string Drop = "Drop";
            public const string Explosion = "Explosion";
            public const string GetItem = "GetItem";
        }

        public struct Tags
        {
            public const string Player = "Player";
        }

        public struct Layers
        {
            public const string Explosion = "Explosion";
        }

        public struct RPC
        {
            public const string AddPlayer = "AddPlayer";
            public const string StartGame = "StartGame";
            public const string InitializePlayer = "Initialize";
            public const string ChangeSprite = "ChangeSprite";
            
            public const string RequestPlaceBomb = "RequestPlaceBomb";
            public const string RefundBomb = "RefundBomb";
            public const string TriggerExplosion = "TriggerExplosion";
            
            public const string SetActiveRenderer = "SetActiveRenderer";
            public const string SetDirection = "SetDirection";
            public const string DestroyAfter = "DestroyAfter";
            
            public const string Destructible = "Destructible";
            public const string ApplyItem = "RPC_ApplyItem";
            public const string DeathSequence = "RPC_DeathSequence";
        }
        
        public struct Animations
        {
            public const string ExplosionStart = "start";
            public const string ExplosionMiddle = "middle";
            public const string ExplosionEnd = "end";
        }
    }
}
