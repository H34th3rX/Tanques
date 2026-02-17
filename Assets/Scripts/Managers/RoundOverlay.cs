using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoundOverlay : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static RoundOverlay Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("Referencias (arrastra desde la Hierarchy)")]
    public Text m_MessageText;      // El Text "Titulo" del MessageCanvas
    public Text m_NameText;         // El Text "nombre" (puede dejarse vacío)

    [Header("Overlay")]
    [Range(0f, 1f)]
    public float m_OverlayAlpha     = 0.65f;    // Oscuridad máxima del fondo (0=transparente, 1=negro total)
    public Color m_OverlayColor     = Color.black;

    [Header("Animación")]
    public float m_FadeInDuration   = 0.4f;     // Segundos que tarda en aparecer el overlay
    public float m_FadeOutDuration  = 0.6f;     // Segundos que tarda en desaparecer

    // ── privados ──────────────────────────────────────────────────────────────
    private Image   m_OverlayImage;             // Panel oscuro creado por código
    private Canvas  m_Canvas;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        m_Canvas = GetComponent<Canvas>();

        CreateOverlayImage();
    }

    // Crea el panel oscuro por código, debajo de los textos existentes
    private void CreateOverlayImage()
    {
        // Crear un GameObject hijo que irá DETRÁS de los textos
        GameObject overlayGO        = new GameObject("_DarkOverlay");
        overlayGO.transform.SetParent(transform, false);

        // Colocarlo como primer hijo para que quede detrás de los textos
        overlayGO.transform.SetAsFirstSibling();

        // RectTransform: cubrir todo el canvas
        RectTransform rt            = overlayGO.AddComponent<RectTransform>();
        rt.anchorMin                = Vector2.zero;
        rt.anchorMax                = Vector2.one;
        rt.offsetMin                = Vector2.zero;
        rt.offsetMax                = Vector2.zero;

        // Image negra semitransparente
        m_OverlayImage              = overlayGO.AddComponent<Image>();
        Color startColor            = m_OverlayColor;
        startColor.a                = 0f;           // Empieza invisible
        m_OverlayImage.color        = startColor;

        // Bloquear raycast para que el jugador no pueda interactuar mientras está el overlay
        m_OverlayImage.raycastTarget = true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // API pública — llamar desde GameManager
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra el título original ("¡TANQUES!") con overlay antes del primer ROUND.
    /// Llama esto UNA VEZ al arrancar el juego.
    /// </summary>
    public void ShowTitle(string titleText, float holdSeconds)
    {
        StopAllCoroutines();
        StartCoroutine(TitleSequence(titleText, holdSeconds));
    }

    private IEnumerator TitleSequence(string titleText, float holdSeconds)
    {
        // Poner el texto del título
        if (m_MessageText != null) m_MessageText.text = titleText;

        // Fade-in del overlay + título
        yield return StartCoroutine(FadeOverlay(0f, m_OverlayAlpha, m_FadeInDuration, showTexts: true));

        // Mantener visible el tiempo indicado
        yield return new WaitForSeconds(holdSeconds);

        // Fade-out limpio antes de que el GameLoop cambie el texto
        yield return StartCoroutine(FadeOverlay(m_OverlayAlpha, 0f, m_FadeOutDuration, showTexts: false));
    }

    /// <summary>Muestra el overlay oscuro y los textos con fade-in.</summary>
    public void ShowOverlay()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOverlay(0f, m_OverlayAlpha, m_FadeInDuration, showTexts: true));
    }

    /// <summary>Oculta el overlay y los textos con fade-out.</summary>
    public void HideOverlay()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOverlay(m_OverlayAlpha, 0f, m_FadeOutDuration, showTexts: false));
    }

    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator FadeOverlay(float fromAlpha, float toAlpha, float duration, bool showTexts)
    {

        // Asegurarse de que los textos estén visibles durante el fade-in
        if (showTexts) SetTextsAlpha(0f);
        SetTextsVisible(true);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = Mathf.Clamp01(elapsed / duration);

            // Overlay
            float alpha          = Mathf.Lerp(fromAlpha, toAlpha, t);
            Color c              = m_OverlayColor;
            c.a                  = alpha;
            m_OverlayImage.color = c;

            // Textos: mismo alpha que el overlay (aparecen y desaparecen juntos)
            SetTextsAlpha(alpha / m_OverlayAlpha); // normalizado 0-1

            yield return null;
        }

        // Asegurar estado final exacto
        Color final  = m_OverlayColor;
        final.a      = toAlpha;
        m_OverlayImage.color = final;

        if (!showTexts)
        {
            // Al terminar el fade-out: textos ocultos, overlay transparente
            SetTextsAlpha(0f);
            SetTextsVisible(false);
        }
        else
        {
            SetTextsAlpha(1f);
        }

    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers para los textos
    // ─────────────────────────────────────────────────────────────────────────
    private void SetTextsAlpha(float alpha)
    {
        if (m_MessageText != null)
        {
            Color c = m_MessageText.color; c.a = alpha; m_MessageText.color = c;
        }
        if (m_NameText != null)
        {
            Color c = m_NameText.color; c.a = alpha; m_NameText.color = c;
        }
    }

    private void SetTextsVisible(bool visible)
    {
        if (m_MessageText != null) m_MessageText.gameObject.SetActive(visible);
        if (m_NameText    != null) m_NameText.gameObject.SetActive(visible);
    }
}