// SafeZone.cs
using UnityEngine;

public class SafeZone : MonoBehaviour
{
    [Header("Размер зоны")]
    [SerializeField] private Vector3 zoneSize = new Vector3(10f, 5f, 10f);
    
    [Header("Смещение зоны")]
    [SerializeField] private Vector3 zoneOffset = Vector3.zero;
    
    // Возвращает границы безопасной зоны
    public Bounds GetZoneBounds()
    {
        return new Bounds(transform.position + zoneOffset, zoneSize);
    }
    
    // Проверяет, находится ли точка внутри зоны
    public bool IsPointInZone(Vector3 point)
    {
        Bounds bounds = GetZoneBounds();
        return bounds.Contains(point);
    }
    
    // Визуализация зоны в редакторе (только для отладки)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Полупрозрачный зеленый
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + zoneOffset, 
            transform.rotation, 
            transform.lossyScale
        );
        
        // Рисуем куб
        Gizmos.DrawCube(Vector3.zero, zoneSize);
        
        // Рисуем контур
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, zoneSize);
        
        // Сбрасываем матрицу
        Gizmos.matrix = Matrix4x4.identity;
    }
    
    // Для отображения в режиме игры (опционально)
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.matrix = Matrix4x4.TRS(
            transform.position + zoneOffset, 
            transform.rotation, 
            transform.lossyScale
        );
        Gizmos.DrawCube(Vector3.zero, zoneSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}