using System.Collections;
using UnityEngine;

public class TankAutoShooting : MonoBehaviour
{
    [Header("Proyectil")]
    public Rigidbody m_Shell;               // Prefab Shell (arrastrar desde /Prefabs)

    [Header("Punto de disparo")]
    public Transform m_FireTransform;       // Opcional; si vacío se crea automático

    [Header("Fuerza de lanzamiento")]
    public float m_MinLaunchForce = 15f;    // Fuerza mínima
    public float m_MaxLaunchForce = 25f;    // Fuerza máxima

    [Header("Cadencia")]
    public float m_MinFireInterval = 2f;    // Segundos mínimos entre disparos
    public float m_MaxFireInterval = 5f;    // Segundos máximos entre disparos

    [Header("Audio (opcional)")]
    public AudioClip m_FireClip;            // Clip de sonido al disparar (puede estar vacío)
    [Range(0f, 1f)]
    public float m_FireVolume = 0.6f;

    // ── privados ──────────────────────────────────────────────────────────────
    private AudioSource m_ShootingAudio;
    private bool        m_CanShoot = false;  // Controlado por el GameManager

    // ── API pública ───────────────────────────────────────────────────────────
    public void EnableShooting()  { m_CanShoot = true;  }
    public void DisableShooting() { m_CanShoot = false; }

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Crear FireTransform automático si no se asignó ninguno
        if (m_FireTransform == null)
        {
            GameObject ft       = new GameObject("AutoFireTransform");
            ft.transform.SetParent(transform);
            ft.transform.localPosition = new Vector3(0f, 1.7f, 1.35f);
            ft.transform.localRotation = Quaternion.Euler(350f, 0f, 0f);
            m_FireTransform            = ft.transform;
        }

        // Buscar o crear AudioSource para el disparo
        m_ShootingAudio = GetComponent<AudioSource>();
        if (m_ShootingAudio == null)
        {
            m_ShootingAudio             = gameObject.AddComponent<AudioSource>();
            m_ShootingAudio.playOnAwake = false;
            m_ShootingAudio.spatialBlend = 1f;  // 3D
        }

        StartCoroutine(ShootingRoutine());
    }

    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator ShootingRoutine()
    {
        // Delay inicial aleatorio para que los bots no disparen todos al mismo tiempo
        yield return new WaitForSeconds(Random.Range(1f, 3f));

        while (true)
        {
            // Solo disparar si el bot está vivo Y el juego lo permite
            if (gameObject.activeSelf && m_Shell != null && m_CanShoot)
            {
                Fire();
            }

            // Esperar tiempo aleatorio hasta el próximo disparo
            float waitTime = Random.Range(m_MinFireInterval, m_MaxFireInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    private void Fire()
    {
        // Fuerza aleatoria para dar variedad a los disparos
        float launchForce = Random.Range(m_MinLaunchForce, m_MaxLaunchForce);

        // Crear la bomba en la posición y rotación del FireTransform
        Rigidbody shellInstance =
            Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // Asignar velocidad en la dirección que apunta el cañón
        shellInstance.velocity = launchForce * m_FireTransform.forward;

        // Reproducir sonido si hay clip asignado
        if (m_FireClip != null)
        {
            m_ShootingAudio.clip   = m_FireClip;
            m_ShootingAudio.volume = m_FireVolume;
            m_ShootingAudio.Play();
        }
    }
}