using UnityEngine;
using DiskCardGame;
using UnityEngine.SceneManagement;
using System.Collections;

namespace ThrDtoActTwo
{
    public class ThreeDEnvironmentManager : MonoBehaviour
    {
        private static ThreeDEnvironmentManager instance;
        public static ThreeDEnvironmentManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("3DEnvironmentManager");
                    instance = go.AddComponent<ThreeDEnvironmentManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private GameObject currentEnvironment;
        private Scene loadedCabinScene;
        private GameObject originalPlayer;
        private Camera mainCamera;

        public void CreateThreeDEnvironment(string locationName)
        {
            Debug.Log($"[ThrDtoActTwo] Creating 3D environment for: {locationName}");

            // Очищаем предыдущее окружение
            if (currentEnvironment != null)
            {
                Destroy(currentEnvironment);
            }

            // Создаем новое 3D окружение
            currentEnvironment = new GameObject("3DEnvironment_" + locationName);

            // Настраиваем камеру для 3D вида
            SetupThreeDCamera();

            // Создаем базовое окружение в зависимости от локации
            CreateLocationEnvironment(locationName);
        }

        private void SetupThreeDCamera()
        {
            // Ищем главную камеру или создаем новую
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                // Если нет Camera.main - создаём новую камеру
                GameObject cameraObj = new GameObject("3DMainCamera");
                cameraObj.transform.parent = currentEnvironment.transform;
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                Debug.Log("[ThrDtoActTwo] Created new main camera");
            }
            
            if (mainCamera != null)
            {
                // Настраиваем позицию камеры для хорошего обзора острова
                mainCamera.transform.position = new Vector3(0, 8, -12);
                mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
                mainCamera.fieldOfView = 60;
                
                // КРИТИЧНО: настраиваем clearFlags для видимого фона
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.5f, 0.7f, 1f); // Голубое небо
                
                // Настраиваем дальность отрисовки
                mainCamera.farClipPlane = 1000f;
                mainCamera.nearClipPlane = 0.1f;
                
                Debug.Log($"[ThrDtoActTwo] Camera configured at position: {mainCamera.transform.position}");
            }
            
            // Настраиваем глобальное освещение (Ambient Light)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.45f); // Серо-голубоватый
            RenderSettings.ambientIntensity = 1f;
            
            Debug.Log("[ThrDtoActTwo] 3D Camera and ambient lighting configured");
        }

        private void CreateLocationEnvironment(string locationName)
        {
            Debug.Log($"[ThrDtoActTwo] Creating Act1-style environment for: {locationName}");
            
            // Для ВСЕХ локаций используем универсальное окружение в стиле Act 1
            CreateAct1StyleEnvironment();
            
            Debug.Log($"[ThrDtoActTwo] 3D environment created for {locationName}");
        }

        private void CreateAct1StyleEnvironment()
        {
            Debug.Log("[ThrDtoActTwo] Creating Act1-style environment - Starting");
            
            // Запускаем корутину для асинхронной загрузки сцены Part1_Cabin
            StartCoroutine(LoadPart1CabinScene());
        }

        private IEnumerator LoadPart1CabinScene()
        {
            Debug.Log("[ThrDtoActTwo] Loading Part1_Cabin scene additively...");
            
            // Загружаем сцену Part1_Cabin аддитивно (не выгружая текущую сцену)
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Part1_Cabin", LoadSceneMode.Additive);
            
            if (asyncLoad == null)
            {
                Debug.LogError("[ThrDtoActTwo] Failed to start loading Part1_Cabin scene!");
                CreateFallbackEnvironment();
                yield break;
            }
            
            // Ждём завершения загрузки
            while (!asyncLoad.isDone)
            {
                Debug.Log($"[ThrDtoActTwo] Loading Part1_Cabin... Progress: {asyncLoad.progress * 100}%");
                yield return null;
            }
            
            Debug.Log("[ThrDtoActTwo] Part1_Cabin scene loaded successfully!");
            
            // Получаем загруженную сцену
            loadedCabinScene = SceneManager.GetSceneByName("Part1_Cabin");
            
            if (!loadedCabinScene.IsValid())
            {
                Debug.LogError("[ThrDtoActTwo] Loaded Part1_Cabin scene is not valid!");
                CreateFallbackEnvironment();
                yield break;
            }
            
            // Получаем все корневые объекты из загруженной сцены
            GameObject[] rootObjects = loadedCabinScene.GetRootGameObjects();
            Debug.Log($"[ThrDtoActTwo] Found {rootObjects.Length} root objects in Part1_Cabin scene");
            
            // Перемещаем все корневые объекты в наше окружение
            foreach (GameObject rootObj in rootObjects)
            {
                Debug.Log($"[ThrDtoActTwo] Processing root object: {rootObj.name}");
                
                // Ищем оригинального игрока из Part1_Cabin
                if (originalPlayer == null)
                {
                    // Проверяем по имени или по наличию компонентов управления
                    if (rootObj.name.ToLower().Contains("player") || 
                        rootObj.name == "FirstPersonController" ||
                        rootObj.GetComponent<CharacterController>() != null)
                    {
                        originalPlayer = rootObj;
                        Debug.Log($"[ThrDtoActTwo] ✅ FOUND ORIGINAL PLAYER: {rootObj.name}");
                    }
                }
                
                // Перемещаем объект в нашу иерархию
                rootObj.transform.SetParent(currentEnvironment.transform);
                
                // Сбрасываем позицию и поворот
                rootObj.transform.localPosition = Vector3.zero;
                rootObj.transform.localRotation = Quaternion.identity;
                
                Debug.Log($"[ThrDtoActTwo] Moved {rootObj.name} to 3D environment");
            }
            
            Debug.Log("[ThrDtoActTwo] All Part1_Cabin objects successfully integrated into 3D environment");
            
            // Добавляем маркер для отладки
            GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerMarker.transform.parent = currentEnvironment.transform;
            centerMarker.transform.position = new Vector3(0, 1, 0);
            centerMarker.transform.localScale = new Vector3(2, 2, 2);
            centerMarker.GetComponent<Renderer>().material.color = Color.red;
            Debug.Log("[ThrDtoActTwo] RED CUBE marker placed at center (0, 1, 0)");
        }

        private void CreateFallbackEnvironment()
        {
            Debug.Log("[ThrDtoActTwo] Creating fallback environment (primitives)");
            
            // Создаём минимальное окружение из примитивов
            CreateFloor(20, new Color(0.45f, 0.35f, 0.25f));
            CreateWalls(20, 5);
            CreateLighting(new Color(1f, 0.9f, 0.7f), 1.2f);
            
            // Маркер
            GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerMarker.transform.parent = currentEnvironment.transform;
            centerMarker.transform.position = new Vector3(0, 1, 0);
            centerMarker.transform.localScale = new Vector3(2, 2, 2);
            centerMarker.GetComponent<Renderer>().material.color = Color.yellow;
            Debug.Log("[ThrDtoActTwo] YELLOW CUBE marker (fallback mode)");
        }

        /// <summary>
        /// Возвращает оригинального игрока из Part1_Cabin сцены
        /// </summary>
        public GameObject GetOriginalPlayer()
        {
            return originalPlayer;
        }

        /// <summary>
        /// Настраивает оригинального игрока для использования во втором акте
        /// Снимает ограничения на движение по сетке
        /// </summary>
        public void SetupOriginalPlayer()
        {
            if (originalPlayer == null)
            {
                Debug.LogWarning("[ThrDtoActTwo] Original player not found!");
                return;
            }

            Debug.Log($"[ThrDtoActTwo] Setting up original player: {originalPlayer.name}");
            
            // Активируем игрока (на случай если он был отключен)
            originalPlayer.SetActive(true);
            
            // Устанавливаем начальную позицию
            originalPlayer.transform.position = new Vector3(0, 0, 0);
            originalPlayer.transform.rotation = Quaternion.identity;
            
            // Ищем все компоненты игрока
            Component[] allComponents = originalPlayer.GetComponents<Component>();
            Debug.Log($"[ThrDtoActTwo] Player has {allComponents.Length} components:");
            foreach (Component comp in allComponents)
            {
                Debug.Log($"[ThrDtoActTwo]   - {comp.GetType().Name}");
            }
            
            // Ищем компоненты, которые могут ограничивать движение
            // Отключаем компоненты с "Navigation" или "Grid" в названии
            MonoBehaviour[] behaviours = originalPlayer.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null) continue;
                
                string typeName = behaviour.GetType().Name;
                
                // Отключаем компоненты навигации и сетки
                if (typeName.Contains("Navigation") || 
                    typeName.Contains("Grid") || 
                    typeName.Contains("Obstacle") ||
                    typeName.Contains("NodeGrid") ||
                    typeName.Contains("PathFind"))
                {
                    behaviour.enabled = false;
                    Debug.Log($"[ThrDtoActTwo] ❌ DISABLED component: {typeName}");
                }
            }
            
            // Ищем дочерние объекты с ограничителями
            Transform[] children = originalPlayer.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name.Contains("Bound") || 
                    child.name.Contains("Limit") || 
                    child.name.Contains("Collision") ||
                    child.name.Contains("Grid"))
                {
                    Debug.Log($"[ThrDtoActTwo] Found child object: {child.name}");
                }
            }
            
            Debug.Log($"[ThrDtoActTwo] Original player setup complete");
        }

        private void CreateFloor(float size, Color color)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.parent = currentEnvironment.transform;
            floor.transform.localScale = new Vector3(size, 1, size);
            floor.transform.position = Vector3.zero;
            floor.GetComponent<Renderer>().material.color = color;
        }

        private void CreateWalls(float size, float height)
        {
            float halfSize = size * 5; // Plane scale * 10 / 2
            
            // Передняя стена
            CreateWall(new Vector3(0, height / 2, -halfSize), new Vector3(size * 10, height, 0.1f));
            // Задняя стена
            CreateWall(new Vector3(0, height / 2, halfSize), new Vector3(size * 10, height, 0.1f));
            // Левая стена
            CreateWall(new Vector3(-halfSize, height / 2, 0), new Vector3(0.1f, height, size * 10));
            // Правая стена
            CreateWall(new Vector3(halfSize, height / 2, 0), new Vector3(0.1f, height, size * 10));
        }

        private void CreateWall(Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.parent = currentEnvironment.transform;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().material.color = new Color(0.7f, 0.7f, 0.7f);
        }

        private void CreateColumn(Vector3 position, float height)
        {
            GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.transform.parent = currentEnvironment.transform;
            column.transform.position = position + new Vector3(0, height / 2, 0);
            column.transform.localScale = new Vector3(0.5f, height / 2, 0.5f);
            column.GetComponent<Renderer>().material.color = new Color(0.8f, 0.8f, 0.85f);
        }

        private void CreateTree(Vector3 position)
        {
            // Ствол
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.parent = currentEnvironment.transform;
            trunk.transform.position = position + new Vector3(0, 1.5f, 0);
            trunk.transform.localScale = new Vector3(0.3f, 1.5f, 0.3f);
            trunk.GetComponent<Renderer>().material.color = new Color(0.4f, 0.3f, 0.2f);
            
            // Крона
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            foliage.transform.parent = currentEnvironment.transform;
            foliage.transform.position = position + new Vector3(0, 4f, 0);
            foliage.transform.localScale = new Vector3(2f, 2f, 2f);
            foliage.GetComponent<Renderer>().material.color = new Color(0.2f, 0.6f, 0.2f);
        }

        private void CreateLighting(Color color, float intensity)
        {
            GameObject light = new GameObject("DirectionalLight");
            light.transform.parent = currentEnvironment.transform;
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.color = color;
            lightComponent.intensity = intensity;
            lightComponent.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private void CreatePointLight(Vector3 position, Color color, float range)
        {
            GameObject light = new GameObject("PointLight");
            light.transform.parent = currentEnvironment.transform;
            light.transform.position = position;
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Point;
            lightComponent.color = color;
            lightComponent.range = range;
            lightComponent.intensity = 1f;
        }

        public void DestroyEnvironment()
        {
            Debug.Log("[ThrDtoActTwo] Destroying 3D environment...");
            
            // Уничтожаем объекты окружения
            if (currentEnvironment != null)
            {
                Destroy(currentEnvironment);
                currentEnvironment = null;
                Debug.Log("[ThrDtoActTwo] Environment objects destroyed");
            }
            
            // Выгружаем загруженную сцену Part1_Cabin
            if (loadedCabinScene.IsValid() && loadedCabinScene.isLoaded)
            {
                Debug.Log("[ThrDtoActTwo] Unloading Part1_Cabin scene...");
                SceneManager.UnloadSceneAsync(loadedCabinScene);
                Debug.Log("[ThrDtoActTwo] Part1_Cabin scene unloaded");
            }
        }
    }
}
