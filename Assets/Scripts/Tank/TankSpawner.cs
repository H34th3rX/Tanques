using System.Collections.Generic;
using UnityEngine;
public class TankSpawner : MonoBehaviour
{
    public static TankSpawner Instance { get; private set; }

    public GameObject tankPrefab;
    public int        numberOfTanks = 10;
    public Vector3    mapSize       = new Vector3(100f, 0f, 100f);

    // Guardamos referencia a todos los bots para destruirlos antes de recrearlos
    private List<GameObject> m_SpawnedBots = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // El primer spawn lo hace el GameManager junto al resto de la ronda 1.
        // Si prefieres que aparezcan inmediatamente al cargar, descomenta la línea:
        // RespawnBots();
    }

    // Llamado por el GameManager al inicio de cada ronda
    public void RespawnBots()
    {
        // 1. Destruir los bots anteriores que aún estén en escena
        foreach (GameObject bot in m_SpawnedBots)
        {
            if (bot != null)
                Destroy(bot);
        }
        m_SpawnedBots.Clear();

        // 2. Crear los nuevos bots en posiciones aleatorias
        for (int i = 0; i < numberOfTanks; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-mapSize.x / 2, mapSize.x / 2),
                0f,
                Random.Range(-mapSize.z / 2, mapSize.z / 2)
            );

            GameObject bot = Instantiate(tankPrefab, randomPosition, Quaternion.identity);
            m_SpawnedBots.Add(bot);
        }
    }
}