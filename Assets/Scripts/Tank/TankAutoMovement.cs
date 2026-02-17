using System.Collections;
using UnityEngine;

public class TankAutoMovement : MonoBehaviour
{
    public float speed = 5f;           // Velocidad de movimiento
    public float turnSpeed = 90f;      // Velocidad de giro
    public float minChangeTime = 1f;   // Tiempo mínimo entre cambios de dirección
    public float maxChangeTime = 3f;   // Tiempo máximo entre cambios de dirección

    private float moveDirection;       // -1 para atrás, 0 para parar, 1 para adelante
    private float turnDirection;       // -1 para izquierda, 0 para no girar, 1 para derecha

    void Start()
    {
        StartCoroutine(ChangeDirectionRoutine());
    }

    void Update()
    {
        // Movimiento adelante o atrás
        transform.Translate(Vector3.forward * moveDirection * speed * Time.deltaTime);

        // Rotación
        transform.Rotate(Vector3.up, turnDirection * turnSpeed * Time.deltaTime);
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // Movimiento aleatorio: -1, 0 o 1
            moveDirection = Random.Range(-1, 2);  

            // Giro aleatorio: -1, 0 o 1
            turnDirection = Random.Range(-1, 2);

            // Espera un tiempo aleatorio antes de cambiar de dirección
            float waitTime = Random.Range(minChangeTime, maxChangeTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}

