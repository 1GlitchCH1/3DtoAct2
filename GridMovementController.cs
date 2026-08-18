using UnityEngine;
using System.Collections;

namespace ThrDtoActTwo
{
    /// <summary>
    /// Grid-based движение с WASD как в Act 1
    /// W/S - быстрое перемещение на 1 клетку сетки вперёд/назад
    /// A/D - поворот на 90 градусов
    /// </summary>
    public class GridMovementController : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float gridSize = 2f; // Размер одной клетки сетки (метры)
        public float movementSpeed = 8f; // Скорость перемещения (для анимации)
        public float rotationSpeed = 360f; // Градусов в секунду
        
        private bool isMoving = false;
        private bool isRotating = false;
        
        void Update()
        {
            // Не принимаем ввод если уже двигаемся или поворачиваемся
            if (isMoving || isRotating)
                return;
            
            HandleInput();
        }
        
        private void HandleInput()
        {
            // W - вперёд
            if (Input.GetKeyDown(KeyCode.W))
            {
                Vector3 targetPos = transform.position + transform.forward * gridSize;
                StartCoroutine(MoveToPosition(targetPos));
            }
            // S - назад
            else if (Input.GetKeyDown(KeyCode.S))
            {
                Vector3 targetPos = transform.position - transform.forward * gridSize;
                StartCoroutine(MoveToPosition(targetPos));
            }
            // A - поворот влево на 90°
            else if (Input.GetKeyDown(KeyCode.A))
            {
                StartCoroutine(RotateBy(-90f));
            }
            // D - поворот вправо на 90°
            else if (Input.GetKeyDown(KeyCode.D))
            {
                StartCoroutine(RotateBy(90f));
            }
        }
        
        private IEnumerator MoveToPosition(Vector3 targetPosition)
        {
            isMoving = true;
            
            Vector3 startPos = transform.position;
            float distance = Vector3.Distance(startPos, targetPosition);
            float duration = distance / movementSpeed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Используем SmoothStep для более плавной анимации
                float smoothT = t * t * (3f - 2f * t);
                
                transform.position = Vector3.Lerp(startPos, targetPosition, smoothT);
                yield return null;
            }
            
            transform.position = targetPosition;
            isMoving = false;
            
            Debug.Log($"[ThrDtoActTwo] Moved to position: {targetPosition}");
        }
        
        private IEnumerator RotateBy(float angle)
        {
            isRotating = true;
            
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = startRot * Quaternion.Euler(0, angle, 0);
            
            float duration = Mathf.Abs(angle) / rotationSpeed;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Используем SmoothStep для плавного поворота
                float smoothT = t * t * (3f - 2f * t);
                
                transform.rotation = Quaternion.Lerp(startRot, targetRot, smoothT);
                yield return null;
            }
            
            transform.rotation = targetRot;
            isRotating = false;
            
            Debug.Log($"[ThrDtoActTwo] Rotated by {angle} degrees");
        }
    }
}
