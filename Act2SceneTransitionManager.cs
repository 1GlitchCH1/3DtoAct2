using UnityEngine;
using DiskCardGame;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ThrDtoActTwo
{
    public class Act2SceneTransitionManager : MonoBehaviour
    {
        private static Act2SceneTransitionManager instance;
        public static Act2SceneTransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("Act2SceneTransitionManager");
                    instance = go.AddComponent<Act2SceneTransitionManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private bool isInThreeDMode = false;
        private string pendingSceneName = "";
        
        public bool IsInThreeDMode => isInThreeDMode;

        // Проверяем, является ли сцена сценой Act 2, которую нужно заменить
        public bool ShouldInterceptScene(string sceneName)
        {

            // Список сцен Act 2, которые мы заменяем на 3D
            // GBC_WorldMap - это карта мира, её НЕ трогаем (остаётся 2D)
            // Все остальные GBC_ сцены - это локации внутри островов, их заменяем на 3D
            return sceneName.StartsWith("GBC_") && sceneName != "GBC_WorldMap";
        }

        // Переход в 3D режим вместо загрузки 2D сцены
        public void TransitionToThreeD(string originalSceneName)
        {
            pendingSceneName = originalSceneName;
            isInThreeDMode = true;
            
            Debug.Log($"[ThrDtoActTwo] Transitioning to 3D mode for scene: {originalSceneName}");
            
            StartCoroutine(LoadThreeDEnvironment());
        }

        private IEnumerator LoadThreeDEnvironment()
        {
            // Даём время для завершения текущих процессов
            yield return new WaitForSeconds(0.1f);
            
            // Создаём 3D окружение
            ThreeDEnvironmentManager.Instance.CreateThreeDEnvironment(pendingSceneName);
            
            // Создаём контроллер игрока для 3D
            CreateThreeDPlayer();
            
            Debug.Log($"[ThrDtoActTwo] 3D environment loaded successfully");
        }

        private void CreateThreeDPlayer()
        {
            Debug.Log("[ThrDtoActTwo] Setting up player...");
            
            // Получаем оригинального игрока из Part1_Cabin
            GameObject player = ThreeDEnvironmentManager.Instance.GetOriginalPlayer();
            
            if (player != null)
            {
                Debug.Log($"[ThrDtoActTwo] ✅ Using ORIGINAL player from Part1_Cabin: {player.name}");
                
                // Настраиваем оригинального игрока
                ThreeDEnvironmentManager.Instance.SetupOriginalPlayer();
                
                // Настраиваем камеру
                Camera cam = Camera.main;
                if (cam != null)
                {
                    // КРИТИЧНО: Устанавливаем cullingMask чтобы камера видела ВСЕ слои
                    cam.cullingMask = -1; // -1 = все слои (включая Default layer 0)
                    Debug.Log($"[ThrDtoActTwo] Camera cullingMask set to see all layers");
                }
                
                Debug.Log($"[ThrDtoActTwo] Original player setup complete at position: {player.transform.position}");
            }
            else
            {
                Debug.LogError("[ThrDtoActTwo] ❌ Original player NOT FOUND! Creating fallback player...");
                CreateFallbackPlayer();
            }
        }
        
        private void CreateFallbackPlayer()
        {
            Debug.LogWarning("[ThrDtoActTwo] Creating fallback 3D player (original not found)");
            
            GameObject player = new GameObject("3DPlayer_Fallback");
            player.transform.position = new Vector3(0, 0, 0);
            
            // Добавляем камеру к игроку
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.parent = player.transform;
                cam.transform.localPosition = new Vector3(0, 3f, 0);
                cam.transform.localRotation = Quaternion.Euler(-15, 0, 0);
                cam.cullingMask = -1;
                
                Debug.Log($"[ThrDtoActTwo] Fallback camera attached to player");
            }
            
            // Добавляем GridMovementController
            GridMovementController controller = player.AddComponent<GridMovementController>();
            controller.gridSize = 2f;
            controller.movementSpeed = 8f;
            controller.rotationSpeed = 360f;
            
            Debug.Log($"[ThrDtoActTwo] Fallback player with GridMovementController created");
        }

        // Выход из 3D режима
        public void ExitThreeDMode()
        {
            if (isInThreeDMode)
            {
                Debug.Log($"[ThrDtoActTwo] Exiting 3D mode");
                ThreeDEnvironmentManager.Instance.DestroyEnvironment();
                isInThreeDMode = false;
            }
        }
    }
}
