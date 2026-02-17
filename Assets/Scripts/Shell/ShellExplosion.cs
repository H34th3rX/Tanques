using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask; // Usado para filtrar a qué afecta la explosión de la bomba. Debería ajustarse a "Players"
    public ParticleSystem m_ExplosionParticles; // Referencia a las partículas que se reproducirán en la explosión
    public AudioSource m_ExplosionAudio; // Referencia al audio que se reproducirá en la explosión
    public float m_MaxDamage = 100f; // Cantidad de daño si la explosión está centrada en el tanque
    public float m_ExplosionForce = 1000f; // Fuerza añadida al tanque en el centro de la explosión
    public float m_MaxLifeTime = 2f; // Tiempo de vida en segundos de la bomba
    public float m_ExplosionRadius = 5f; // Radio máximo desde la explosión para calcular los tanques afectados

    private void Start()
    {
        // Si no se ha destruido aún, destruir la bomba después de su tiempo de vida
        Destroy(gameObject, m_MaxLifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Recoge los colliders en una esfera desde la posición de la bomba con el radio máximo
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        // Recorre los colliders
        for (int i = 0; i < colliders.Length; i++)
        {
            // Selecciona su Rigidbody
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

            // Si no tiene Rigidbody, pasa al siguiente
            if (!targetRigidbody)
                continue;

            // Añade la fuerza de la explosión
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            // Busca el script TankHealth asociado al Rigidbody
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

            // Si no hay script TankHealth, pasa al siguiente
            if (!targetHealth)
                continue;

            // Calcula el daño a aplicar en función de la distancia a la bomba
            float damage = CalculateDamage(targetRigidbody.position);

            // Aplica el daño al tanque
            targetHealth.TakeDamage(damage);
        }

        // Desacopla el sistema de partículas de la bomba
        m_ExplosionParticles.transform.parent = null;

        // Reproduce el sistema de partículas
        m_ExplosionParticles.Play();

        // Reproduce el audio
        m_ExplosionAudio.Play();

        // Cuando las partículas han terminado, destruye su objeto asociado
        Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);

        // Destruye la bomba
        Destroy(gameObject);
    }

    private float CalculateDamage(Vector3 targetPosition)
    {
        // Crea un vector desde la bomba al objetivo
        Vector3 explosionToTarget = targetPosition - transform.position;

        // Calcula la distancia desde la bomba al objetivo
        float explosionDistance = explosionToTarget.magnitude;

        // Calcula la proporción de máxima distancia desde la explosión al tanque
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        // Calcula el daño basado en esa proporción
        float damage = relativeDistance * m_MaxDamage;

        // Asegura que el mínimo daño siempre sea 0
        damage = Mathf.Max(0f, damage);

        // Devuelve el daño calculado
        return damage;
    }
}
