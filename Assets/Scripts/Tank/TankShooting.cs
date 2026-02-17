using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
// TankShooting.cs  (versión con cooldown anti-spam)
//
// Cambio respecto al original:
//   • Se añade m_FireCooldown (por defecto 1.5 segundos).
//     Mientras el cooldown no termine, el jugador no puede volver a disparar
//     aunque mantenga o pulse el botón repetidamente.
//   • El cooldown es ajustable desde el Inspector.
// ─────────────────────────────────────────────────────────────────────────────

public class TankShooting : MonoBehaviour
{
    [Header("Configuración base")]
    public int       m_PlayerNumber    = 1;
    public Rigidbody m_Shell;
    public Transform m_FireTransform;
    public Slider    m_AimSlider;
    public AudioSource m_ShootingAudio;
    public AudioClip m_ChargingClip;
    public AudioClip m_FireClip;
    public float     m_MinLaunchForce  = 15f;
    public float     m_MaxLaunchForce  = 30f;
    public float     m_MaxChargeTime   = 0.75f;

    [Header("Anti-spam")]
    [Tooltip("Tiempo mínimo en segundos entre un disparo y el siguiente.")]
    public float     m_FireCooldown    = 1.5f;   // ← nuevo: cooldown entre disparos

    // ── privados ──────────────────────────────────────────────────────────────
    private string m_FireButton;
    private float  m_CurrentLaunchForce;
    private float  m_ChargeSpeed;
    private bool   m_Fired;
    private float  m_LastFireTime = -999f;       // ← nuevo: marca del último disparo

    // ─────────────────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value    = m_MinLaunchForce;
    }

    private void Start()
    {
        m_FireButton  = "Fire" + m_PlayerNumber;
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }

    private void Update()
    {
        m_AimSlider.value = m_MinLaunchForce;

        // ── Verificar si el cooldown ya pasó ─────────────────────────────────
        bool cooldownReady = (Time.time - m_LastFireTime) >= m_FireCooldown;

        // Si llegó a fuerza máxima, disparar (solo si cooldown listo)
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce;
            if (cooldownReady) Fire();
        }
        // Botón recién pulsado → empezar carga (solo si cooldown listo)
        else if (Input.GetButtonDown(m_FireButton) && cooldownReady)
        {
            m_Fired              = false;
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }
        // Botón mantenido → incrementar fuerza
        else if (Input.GetButton(m_FireButton) && !m_Fired)
        {
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            m_AimSlider.value     = m_CurrentLaunchForce;
        }
        // Botón soltado → disparar (solo si cooldown listo)
        else if (Input.GetButtonUp(m_FireButton) && !m_Fired && cooldownReady)
        {
            Fire();
        }
    }

    private void Fire()
    {
        m_Fired        = true;
        m_LastFireTime = Time.time;   // ← registrar momento del disparo

        Rigidbody shellInstance =
            Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        m_CurrentLaunchForce = m_MinLaunchForce;
    }
}