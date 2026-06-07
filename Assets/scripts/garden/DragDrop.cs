
using UnityEngine;
using UnityEngine.EventSystems; 
public class DragDrop: MonoBehaviour
{
    private BoxCollider2D _collider;
    private Vector3 _startDragPosition;
    private bool isFollowingMouse = false;
    public Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main; // Получаем ссылку на основную камеру
    }

    // Метод для включения слежения за мышью
    public void on_mouse_follow()
    {
        isFollowingMouse = true;
    }

    private void Update()
    {
        if (isFollowingMouse)
        {
            // Получаем позицию мыши в мировых координатах
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Устанавливаем z-координату, чтобы объект не уходил вглубь/вперёд

            // Обновляем позицию объекта
            transform.position = mousePosition;

            // Проверяем клик мыши
            if (Input.GetMouseButtonDown(0)) // Левая кнопка мыши
            {
                // Проверяем, кликнули ли по объекту с тегом "Pot"
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null && hit.collider.CompareTag("Pot"))
                {
                    isFollowingMouse = false; // Отключаем слежение
                    Debug.Log("Seed placed in a pot!");
                }
            }
        }
    }
    
    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
    }
    

    private void OnMouseDown()
    {
        Debug.Log("мы нажали на объект ");
        // Сохраняем начальную позицию
        _startDragPosition = transform.position;
        // Устанавливаем позицию по мыши
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    private void OnMouseDrag()
    {
        Debug.Log("передвигаем ");
        // Обновляем позицию при перетаскивании
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    private void OnMouseUp()
    {
        Debug.Log("Отпускаем мышку");
        // Проверяем, куда упала карта
        _collider.enabled = false;
        Collider2D dropArea = Physics2D.OverlapPoint(transform.position);
        _collider.enabled = true;
        Debug.Log("Seed needs watering");
        if (dropArea != null && dropArea.GetComponent<iPotDropArea>() != null)
        {
            Debug.Log("Мы нашли куда поставить");
            dropArea.GetComponent<iPotDropArea>().OnPotDrop(this.gameObject);
        }
        else
        {
            Debug.Log("мы не нашли куда поместить ");
            // Возвращаем в исходное положение
            transform.position = _startDragPosition;
        }
    }
}
