using UnityEngine;
using DiskCardGame;

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
            
            // Шаг 1: Попытка найти и клонировать объекты из Act 1 (Cabin)
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            GameObject act1Root = null;
            
            // Ищем корневые объекты сцены Act 1
            foreach (GameObject obj in allObjects)
            {
                // Ищем характерные объекты из Act 1
                if (obj.name.Contains("Cabin") || obj.name.Contains("GBC_Room") || 
                    obj.name.Contains("FirstPerson") || obj.name.Contains("Farmhouse"))
                {
                    Debug.Log($"[ThrDtoActTwo] Found potential Act1 object: {obj.name} at {obj.transform.position}");
                    if (act1Root == null && obj.transform.parent == null)
                    {
                        act1Root = obj;
                    }
                }
            }
            
            // Шаг 2: Если нашли Act 1 объекты - клонируем их
            if (act1Root != null)
            {
                Debug.Log($"[ThrDtoActTwo] Cloning Act1 environment from: {act1Root.name}");
                GameObject clonedEnvironment = GameObject.Instantiate(act1Root);
                clonedEnvironment.transform.parent = currentEnvironment.transform;
                clonedEnvironment.transform.position = Vector3.zero;
                Debug.Log("[ThrDtoActTwo] Act1 environment cloned successfully");
            }
            else
            {
                Debug.Log("[ThrDtoActTwo] No Act1 environment found, creating minimal grid-based environment");
                
                // Создаём минимальное окружение в стиле Act 1 с сеткой
                // Пол с текстурой как в Cabin
                CreateFloor(20, new Color(0.45f, 0.35f, 0.25f)); // Коричневый деревянный пол
                
                // Создаём стены вокруг как в Cabin
                CreateWalls(20, 5);
                
                // Базовое освещение как в Cabin (тёплое внутреннее освещение)
                CreateLighting(new Color(1f, 0.9f, 0.7f), 1.2f);
            }
            
            // Шаг 3: ОБЯЗАТЕЛЬНО добавляем красный куб в центре как маркер
            GameObject centerMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerMarker.transform.parent = currentEnvironment.transform;
            centerMarker.transform.position = new Vector3(0, 1, 0);
            centerMarker.transform.localScale = new Vector3(2, 2, 2);
            centerMarker.GetComponent<Renderer>().material.color = Color.red; // ЯРКО-КРАСНЫЙ
            Debug.Log("[ThrDtoActTwo] RED CUBE marker placed at center (0, 1, 0)");
            
            Debug.Log("[ThrDtoActTwo] Act1-style environment creation completed");
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
            if (currentEnvironment != null)
            {
                Destroy(currentEnvironment);
                currentEnvironment = null;
            }
        }
    }
}
