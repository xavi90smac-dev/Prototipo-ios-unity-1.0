using UnityEngine;

public class RandomTreeSpawner : MonoBehaviour
{
    [Header("Prefabs de Árboles (Coloca 3)")]
    public GameObject[] treePrefabs = new GameObject[3];

    [Header("Configuración de Posición")]
    [Tooltip("Radio del área circular donde se generarán los árboles.")]
    public float spawnRadius = 20f;
    [Tooltip("Cantidad total de árboles a generar en cada tanda.")]
    public int spawnCount = 30;

    [Header("Configuración de Tamaño")]
    public float minScale = 0.8f;
    public float maxScale = 1.8f;

    [Header("Semilla de Randomización")]
    [Range(0, 1000)]
    public int randomSeed = 0;

    [ContextMenu("Generar Árboles")]
    public void SpawnTrees()
    {
        if (treePrefabs == null || treePrefabs.Length < 3 || treePrefabs[0] == null || treePrefabs[1] == null || treePrefabs[2] == null)
        {
            Debug.LogWarning("Por favor, asigna los 3 prefabs de árboles en el inspector.");
            return;
        }

        // Limpiar los árboles anteriores automáticamente antes de crear los nuevos
        ClearTrees();

        // Establecer la semilla para controlar la randomización mediante la barra
        Random.InitState(randomSeed);

        Terrain activeTerrain = Terrain.activeTerrain;

        for (int i = 0; i < spawnCount; i++)
        {
            // Seleccionar uno de los 3 prefabs al azar
            GameObject selectedPrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Calcular posición aleatoria dentro del radio circular
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Obtener la altura exacta del Terrain en esa posición X, Z
            if (activeTerrain != null)
            {
                spawnPosition.y = activeTerrain.SampleHeight(spawnPosition) + activeTerrain.transform.position.y;
            }
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(spawnPosition + Vector3.up * 500f, Vector3.down, out hit, 1000f))
                {
                    spawnPosition.y = hit.point.y;
                }
            }

            // Instanciar el árbol
            GameObject newTree = Instantiate(selectedPrefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);

            // Aplicar escala aleatoria entre el mínimo y máximo configurado
            float randomScale = Random.Range(minScale, maxScale);
            newTree.transform.localScale = Vector3.one * randomScale;
        }

        Debug.Log("¡Árboles generados, pegados al terreno y sincronizados con éxito!");
    }

    [ContextMenu("Eliminar Árboles")]
    public void ClearTrees()
    {
        // Borra todos los árboles hijos que se hayan creado
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        Debug.Log("¡Vegetación eliminada por completo!");
    }
}