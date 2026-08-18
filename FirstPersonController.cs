using UnityEngine;

namespace ThrDtoActTwo
{
    /// <summary>
    /// First Person контроллер с WASD движением и мышью для поворота камеры
    /// Похож на контроллер из Act 1 игры
    /// </summary>
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 5f;
        public float runSpeed = 8f;
        public float gravity = 20f;
        
        
        private CharacterController characterController;
        private Camera playerCamera;
        private Vector3 moveDirection = Vector3.zero;
        
        void Start()
        {
            // Получаем или добавляем CharacterController
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.height = 2f;
                characterController.radius = 0.5f;
                characterController.center = new Vector3(0, 1, 0);
            }
            
            // Находим камеру
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
            
            
            Debug.Log("[ThrDtoActTwo] FirstPersonController initialized");
        }
        
        void Update()
        {
            HandleMovement();
        }
        
        private void HandleMovement()
        {
            // Получаем ввод WASD / Стрелки
            float horizontal = Input.GetAxis("Horizontal"); // A/D или стрелки влево/вправо
            float vertical = Input.GetAxis("Vertical");     // W/S или стрелки вверх/вниз
            
            // Проверяем бег (Shift)
            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            
            // A/D - ПОВОРАЧИВАЮТ персонажа (и камеру вместе с ним)
            float rotationSpeed = 100f; // градусов в секунду
            transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);
            
            // W/S - ДВИГАЕМ вперед/назад по направлению взгляда
            Vector3 forward = transform.forward;
            Vector3 desiredMove = forward * vertical * currentSpeed;
            
            if (characterController.isGrounded)
            {
                moveDirection = desiredMove;
            }
            else
            {
                // В воздухе применяем гравитацию
                moveDirection.x = desiredMove.x;
                moveDirection.z = desiredMove.z;
            }
            
            // Применяем гравитацию
            moveDirection.y -= gravity * Time.deltaTime;
            
            // Двигаем персонажа
            characterController.Move(moveDirection * Time.deltaTime);
        }
    }
}
