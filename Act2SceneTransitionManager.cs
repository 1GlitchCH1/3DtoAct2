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
            GameObject player = new GameObject("3DPlayer");
            player.transform.position = new Vector3(0, 0, 0); // На уровне земли острова
            
            // Добавляем камеру к игроку
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.parent = player.transform;
                cam.transform.localPosition = new Vector3(0, 3f, 0); // Поднимаем выше для лучшего обзора пола и острова
                cam.transform.localRotation = Quaternion.Euler(-15, 0, 0); // Больше наклон вниз для видимости пола
                
                // КРИТИЧНО: Устанавливаем cullingMask чтобы камера видела ВСЕ слои
                cam.cullingMask = -1; // -1 = все слои (включая Default layer 0)
                
                Debug.Log($"[ThrDtoActTwo] Camera attached to player");
                Debug.Log($"[ThrDtoActTwo] Camera localPos: {cam.transform.localPosition}, WORLD pos: {cam.transform.position}");
            }
            
            // Добавляем GridMovementController для управления как в Act 1:
            // W/S - быстрое перемещение по сетке вперёд/назад
            // A/D - поворот на 90 градусов
            GridMovementController controller = player.AddComponent<GridMovementController>();
            controller.gridSize = 2f; // Размер клетки 2 метра
            controller.movementSpeed = 8f; // Скорость перемещения
            controller.rotationSpeed = 360f; // Быстрый поворот
            
            Debug.Log($"[ThrDtoActTwo] 3D player with GridMovementController created at {player.transform.position}");
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
