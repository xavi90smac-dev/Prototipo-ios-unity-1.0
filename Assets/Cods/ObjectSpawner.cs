using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn; 
    public int numberOfObjects = 10; 
    public float spacing = 3.5f;     // Aumenta este valor para separarlas más
    
    public enum Direction { Forward, Right, Left, Back }
    public Direction spawnDirection = Direction.Right;

    [ContextMenu("Spawn Objects")]
    void Spawn()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("¡Falta asignar el prefab de la barrera!");
            return;
        }

        // Crea un objeto vacío contenedor en la jerarquía para agruparlas todas
        GameObject parentGroup = new GameObject("BarrierRow_" + System.DateTime.Now.Ticks);
        parentGroup.transform.position = transform.position;
        parentGroup.transform.rotation = transform.rotation;

        Vector3 spawnPosition = transform.position;
        Vector3 directionVector = Vector3.forward;

        switch (spawnDirection)
        {
            case Direction.Forward: directionVector = transform.forward; break;
            case Direction.Back: directionVector = -transform.forward; break;
            case Direction.Right: directionVector = transform.right; break;
            case Direction.Left: directionVector = -transform.right; break;
        }

        for (int i = 0; i < numberOfObjects; i++)
        {
            // Instancia la barrera y ponla como hija del contenedor
            GameObject newObj = Instantiate(prefabToSpawn, spawnPosition, transform.rotation, parentGroup.transform);
            spawnPosition += directionVector * spacing; 
        }
    }
}