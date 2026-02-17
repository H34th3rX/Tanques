using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public float m_MaxGameTime = 240f;
    public Text m_TimerText;
    public CameraControl m_CameraControl;
    public Text m_MessageText;
    public GameObject m_TankPrefab;
    public TankManager[] m_Tanks;

    private int m_RoundNumber = 0;
    private WaitForSeconds m_StartWait;
    private WaitForSeconds m_EndWait;
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;

    private float m_GameStartTime;
    private float m_GameEndTime;

    // Duración del fade-out del overlay (debe coincidir con RoundOverlay.m_FadeOutDuration)
    private const float k_OverlayFadeOut = 0.7f;

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait   = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        m_GameStartTime = Time.time;

        StartCoroutine(TitleThenGameLoop());
    }

    // Muestra el título y luego arranca el juego
    private IEnumerator TitleThenGameLoop()
    {
        // Esperar un frame para que Awake/Start del RoundOverlay termine
        yield return null;

        // Mostrar "¡TANQUES!" con overlay durante 2 segundos
        if (RoundOverlay.Instance != null)
        {
            RoundOverlay.Instance.ShowTitle("¡TANQUES!", 2f);
            yield return new WaitForSeconds(2f + 0.5f); // holdSeconds + fade-in
        }

        // Ahora sí arrancar el juego normal
        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position,
                            m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];
        for (int i = 0; i < targets.Length; i++)
            targets[i] = m_Tanks[i].m_Instance.transform;
        m_CameraControl.m_Targets = targets;
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner == null)
            StartCoroutine(GameLoop());
    }

    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();
        m_CameraControl.SetStartPositionAndSize();

        // ── Respawnear bots al inicio de cada ronda ────────────────────────
        TankSpawner.Instance?.RespawnBots();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        // ── Overlay: mostrar fondo oscuro con fade-in ──────────────────────
        RoundOverlay.Instance?.ShowOverlay();

        yield return m_StartWait;   // esperar el delay de inicio

        // ── Overlay: ocultar con fade-out antes de que empiece a jugarse ──
        RoundOverlay.Instance?.HideOverlay();
        yield return new WaitForSeconds(k_OverlayFadeOut); // esperar que termine el fade
    }

    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            float elapsed   = Time.time - m_GameStartTime;
            float remaining = m_MaxGameTime - elapsed;

            int minutes = (int)(remaining / 60);
            int seconds = (int)(remaining % 60);
            m_TimerText.text = "TIEMPO: " + minutes.ToString("00") + ":" + seconds.ToString("00");

            if (elapsed >= m_MaxGameTime)
                break;

            yield return null;
        }
    }

    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        // ── Overlay: mostrar fondo oscuro al final de la ronda ─────────────
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        m_GameWinner = GetGameWinner();

        string message     = EndMessage();
        m_MessageText.text = message;
        m_TimerText.text   = "";

        RoundOverlay.Instance?.ShowOverlay();

        // ── Tiempo agotado ─────────────────────────────────────────────────
        if (Time.time - m_GameStartTime >= m_MaxGameTime)
        {
            m_MessageText.text = "¡TIEMPO AGOTADO!\nAMBOS JUGADORES PIERDEN";
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene(0);
            yield break;
        }

        // ── Ganador del juego ──────────────────────────────────────────────
        if (m_GameWinner != null)
        {
            m_GameEndTime      = Time.time;
            m_MessageText.text = EndMessage() + "\n\n" + FinalGameMessage();
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene(0);
            yield break;
        }

        yield return m_EndWait;
    }

    private string FinalGameMessage()
    {
        string result = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n";
        for (int i = 0; i < m_Tanks.Length; i++)
            result += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " VICTORIAS\n";
        return result;
    }

    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
            if (m_Tanks[i].m_Instance.activeSelf) numTanksLeft++;
        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
            if (m_Tanks[i].m_Instance.activeSelf) return m_Tanks[i];
        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin) return m_Tanks[i];
        return null;
    }

    private string EndMessage()
    {
        string message = "EMPATE!";
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n\n";
        for (int i = 0; i < m_Tanks.Length; i++)
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";

        if (m_GameWinner != null)
        {
            float totalTime = m_GameEndTime - m_GameStartTime;
            int minutes     = (int)(totalTime / 60);
            int seconds     = (int)(totalTime % 60);
            message         = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!\n";
            message        += "TIEMPO TOTAL: " + minutes + "m " + seconds + "s\n";
        }

        return message;
    }

    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }

    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
        SetBotsCanShoot(true);   // ← bots empiezan a disparar solo al jugar
    }

    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
        SetBotsCanShoot(false);  // ← bots dejan de disparar en tiempo muerto
    }

    // Habilita o deshabilita el disparo de todos los bots en escena
    private void SetBotsCanShoot(bool canShoot)
    {
        TankAutoShooting[] bots = FindObjectsOfType<TankAutoShooting>();
        foreach (TankAutoShooting bot in bots)
        {
            if (canShoot) bot.EnableShooting();
            else          bot.DisableShooting();
        }
    }
}