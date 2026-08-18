using HarmonyLib;
using DiskCardGame;
using UnityEngine;

namespace ThrDtoActTwo
{
    [HarmonyPatch]
    public class Act2LocationPatch
    {

        // Патч для перехвата загрузки сцены боя в Act 2
        [HarmonyPatch(typeof(SceneLoader), "Load")]
        [HarmonyPrefix]
        public static bool LoadScenePrefix(string sceneName)
        {
            Debug.Log($"[ThrDtoActTwo] Loading scene: {sceneName}");
            
            // Проверяем, нужно ли перехватывать эту сцену
            if (Act2SceneTransitionManager.Instance.ShouldInterceptScene(sceneName))
            {
                Debug.Log($"[ThrDtoActTwo] BLOCKING 2D scene load: {sceneName}");
                Debug.Log($"[ThrDtoActTwo] Switching to 3D mode instead!");
                
                // Блокируем загрузку 2D сцены и переходим в 3D режим
                Act2SceneTransitionManager.Instance.TransitionToThreeD(sceneName);
                
                // Возвращаем false чтобы ЗАБЛОКИРОВАТЬ оригинальную загрузку сцены
                return false;
            }
            
            // Для всех остальных сцен - продолжаем стандартную загрузку
            return true;
        }

        // Патч для обработки возврата на карту мира
        [HarmonyPatch(typeof(GameFlowManager), "TransitionToGameState")]
        [HarmonyPrefix]
        public static void OnGameStateTransition(GameState gameState)
        {
            Debug.Log($"[ThrDtoActTwo] Game state transitioning to: {gameState}");
            
            // Если возвращаемся на карту мира - выходим из 3D режима
            if (gameState == GameState.Map && Act2SceneTransitionManager.Instance.IsInThreeDMode)
            {
                Debug.Log($"[ThrDtoActTwo] Returning to world map - exiting 3D mode");
                Act2SceneTransitionManager.Instance.ExitThreeDMode();
            }
        }
    }
}
