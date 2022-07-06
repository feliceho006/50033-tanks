using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AI;


public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;            
    public float m_StartDelay = 3f;             
    public float m_EndDelay = 3f;               
    public CameraControl m_CameraControl;       
    public Text m_MessageText;                  
    public GameObject[] m_TankPrefabs;

    public GameObject[] m_PowerUpPrefabs;
    public TankManager[] m_Tanks;               
    public List<Transform> wayPointsForAI;

    public static int m_RoundNumber;                  
    private WaitForSeconds m_StartWait;         
    private WaitForSeconds m_EndWait;           
    private TankManager m_RoundWinner;          
    private TankManager m_GameWinner;           


    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SpawnAllPowerUps();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        m_Tanks[0].m_Instance =
            Instantiate(m_TankPrefabs[0], m_Tanks[0].m_SpawnPoint.position, m_Tanks[0].m_SpawnPoint.rotation) as GameObject;
        m_Tanks[0].m_PlayerNumber = 1;
        m_Tanks[0].SetupPlayerTank();

        for (int i = 1; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefabs[i], m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].SetupAI(wayPointsForAI);
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
            targets[i] = m_Tanks[i].m_Instance.transform;

        m_CameraControl.m_Targets = targets;
    }

    private void SpawnAllPowerUps()
    {
 
        // spawn all people
        for (int powerUpIndex = 0; powerUpIndex < m_PowerUpPrefabs.Length; powerUpIndex += 1)
        {
            // get a game location on the board
            Vector3 randomBoardLocation = GetRandomGameBoardLocation();
            
            Instantiate(m_PowerUpPrefabs[powerUpIndex], randomBoardLocation, Quaternion.Euler(-90, 0,0));

        }
    }

    private Vector3 GetRandomGameBoardLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
 
        int maxIndices = navMeshData.indices.Length - 3;
 
        // pick the first indice of a random triangle in the nav mesh
        int firstVertexSelected = UnityEngine.Random.Range(0, maxIndices);
        int secondVertexSelected = UnityEngine.Random.Range(0, maxIndices);
 
        // spawn on verticies
        Vector3 point = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
 
        Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
        Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];
 
        // eliminate points that share a similar X or Z position to stop spawining in square grid line formations
        if ((int)firstVertexPosition.x == (int)secondVertexPosition.x || (int)firstVertexPosition.z == (int)secondVertexPosition.z)
        {
            point = GetRandomGameBoardLocation(); // re-roll a position - I'm not happy with this recursion it could be better
        }
        else
        {
            // select a random point on it
            Vector3 secondpos = new Vector3( secondVertexPosition.x, secondVertexPosition.y+1.0f, secondVertexPosition.z);
            point = Vector3.Lerp(firstVertexPosition, secondpos, UnityEngine.Random.Range(0.05f, 0.95f));
        }
 
        return point;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null) SceneManager.LoadScene(0);
        else StartCoroutine(GameLoop());
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = $"ROUND {m_RoundNumber}";

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft()) yield return null;
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null) m_RoundWinner.m_Wins++;

        m_GameWinner = GetGameWinner();

        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf) numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        var sb = new StringBuilder();

        if (m_RoundWinner != null) sb.Append($"{m_RoundWinner.m_ColoredPlayerText} WINS THE ROUND!");
        else sb.Append("DRAW!");

        sb.Append("\n\n\n\n");

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            sb.AppendLine($"{m_Tanks[i].m_ColoredPlayerText}: {m_Tanks[i].m_Wins} WINS");
        }

        if (m_GameWinner != null)
            sb.Append($"{m_GameWinner.m_ColoredPlayerText} WINS THE GAME!");

        return sb.ToString();
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
    }
}