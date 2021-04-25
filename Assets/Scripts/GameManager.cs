using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{    
    [Header("Prefabs")]
    [SerializeField] private GameObject rowObj;
    [SerializeField] private GameObject imgObj;
    
    [Header("Panels")]
    [SerializeField] private Transform gameOverPanel;
    [SerializeField] private Transform rowsPanel;
    [SerializeField] private Transform mainMenuPanel;
    
    [Header("UI")]
    [SerializeField] private Text movesText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Text gameOverBestMovesText;
    [SerializeField] private Text gameOverBestTimeText;

    [Header("Buttons")]
    [SerializeField] private Text easyText;
    [SerializeField] private Text mediumText;
    [SerializeField] private Text hardText;

    [Header("Animation speed")]
    [SerializeField] int randomIterations = 100;
    [SerializeField] private float animationRandomSpeed = 100f;
    [SerializeField] private float animationSpeed = 10f;
    private float currentAnimationSpeed = 0f;
    private int currentRandomIterations = 0;
    private PuzzleTile tile1ForAnimation;
    private PuzzleTile tile2ForAnimation;
    private Vector3 tile1PositionForAnimation;
    private Vector3 tile2PositionForAnimation;

    // Game status
    private enum gameStatus {Gaming, Moving, Over};
    private gameStatus currentGameStatus;
    private bool isMixing = false;

    // Game level
    private enum gameLevel {easy, medium, hard};
    private gameLevel currentGameLevel;

    // Game variables
    private int rows = 4;
    private int cols = 4;
    private List<PuzzleTile> tilesList;
    private int moves = 0;
    private float gamingTime = 0;

    public void Start()
    {
        WriteButtonTexts();

        // Manage panels
        gameOverPanel.gameObject.SetActive(false);
        mainMenuPanel.gameObject.SetActive(true);
    }

    private void WriteButtonTexts()
    {
        int easyScore = PlayerPrefs.GetInt("easyScore", 0);
        int mediumScore = PlayerPrefs.GetInt("mediumScore", 0);
        int hardScore = PlayerPrefs.GetInt("hardScore", 0);

        TimeSpan easyTimeSpan = TimeSpan.FromSeconds(PlayerPrefs.GetFloat("easyTime", 0));
        TimeSpan mediumTimeSpan = TimeSpan.FromSeconds(PlayerPrefs.GetFloat("mediumTime", 0));
        TimeSpan hardTimeSpan = TimeSpan.FromSeconds(PlayerPrefs.GetFloat("hardTime", 0));

        easyText.text = "Easy (3x3)\nBest moves: " + easyScore.ToString() + 
        "\nBest time:" + string.Format("{0:D2}:{1:D2}:{2:D2}", easyTimeSpan.Hours, easyTimeSpan.Minutes, easyTimeSpan.Seconds);

        mediumText.text = "Medium (4x4)\nBest moves: " + mediumScore.ToString() + 
        "\nBest time:" + string.Format("{0:D2}:{1:D2}:{2:D2}", mediumTimeSpan.Hours, mediumTimeSpan.Minutes, mediumTimeSpan.Seconds);

        hardText.text = "Hard (5x5)\nBest moves: " + hardScore.ToString() + 
        "\nBest time:" + string.Format("{0:D2}:{1:D2}:{2:D2}", hardTimeSpan.Hours, hardTimeSpan.Minutes, hardTimeSpan.Seconds);
    }

    public void StartLevel(int _rowsAndCols)
    {
        // Set difficolty
        if(_rowsAndCols == 3) { currentGameLevel = gameLevel.easy; }
        else if(_rowsAndCols == 4) { currentGameLevel = gameLevel.medium; }
        else { currentGameLevel = gameLevel.hard; }

        gamingTime = 0;
        rows = _rowsAndCols;
        cols = _rowsAndCols;
        GenerateMatrix();
        mainMenuPanel.gameObject.SetActive(false);

        currentRandomIterations = 0;
        isMixing = true;
    }

    public bool IsMoving()
    {
        return currentGameStatus == gameStatus.Moving;
    }

    public void GenerateMatrix()
    {
        // Destroy all old childrens
        foreach(Transform child in rowsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new matrix
        int nTile = 0;
        tilesList = new List<PuzzleTile>();
        for (int i = 0; i < rows; i++)
        {
            // Create row
            GameObject _row = Instantiate(rowObj);
            _row.transform.SetParent(rowsPanel);
            _row.transform.localScale = Vector3.one;

            for (int y = 0; y < cols; y++)
            {
                // Create img
                nTile++;
                GameObject _img = Instantiate(imgObj);
                _img.transform.SetParent(_row.transform);
                _img.transform.localScale = Vector3.one;
                _img.GetComponentInChildren<Text>().text = (nTile).ToString();
                PuzzleTile tile = _img.GetComponent<PuzzleTile>();
                tile.SetStartIndex(nTile);
                tilesList.Add(tile);
            }
        }

        // Set last tile
        tilesList[tilesList.Count - 1].SetLast();
    }

    private PuzzleTile GetRandomTile()
    {
        return tilesList[UnityEngine.Random.Range(0, tilesList.Count)];
    }

    private Vector2 GetRandomDirection()
    {
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
    }

    private PuzzleTile GetTileFromDirection(PuzzleTile _tile1, Vector2 _direction)
    {
        if(_tile1 == null) {
            return null;
        }

        int tile1Index = _tile1.transform.GetSiblingIndex();
        int parent1Index = _tile1.transform.parent.GetSiblingIndex();

        if(Mathf.Abs(_direction.x) > Mathf.Abs(_direction.y)) {
            // Move horizontally
            if(_direction.x > 0) {
                if(tile1Index >= cols -1) { return null; } 
                // (Move right) Get the PuzzleTile of the next sibbiling object
                return _tile1.transform.parent.GetChild(tile1Index + 1).GetComponent<PuzzleTile>();
            } else {
                if(tile1Index <= 0) { return null; }
                // (Move left) Get the PuzzleTile of the previous sibbiling object
                return _tile1.transform.parent.GetChild(tile1Index - 1).GetComponent<PuzzleTile>();
            }
        }
        else {
            // Move vertically
            if(_direction.y > 0) {
                if(parent1Index <= 0) { return null; } 
                // (Move up) Get the PuzzleTile of the previous parent with the same sibbiling object
                return _tile1.transform.parent.parent.GetChild(parent1Index - 1).GetChild(tile1Index).GetComponent<PuzzleTile>();
            } else {
                if(parent1Index >= rows -1) { return null; }
                // (Move down) Get the PuzzleTile of the next parent with the same sibbiling object
                return _tile1.transform.parent.parent.GetChild(parent1Index + 1).GetChild(tile1Index).GetComponent<PuzzleTile>();
            }
        }           
    }

    public bool Move(PuzzleTile _tile1, Vector2 _direction)
    {
        // If the two tiles exist
        PuzzleTile _tile2 = GetTileFromDirection(_tile1, _direction);
        if(_tile1 == null || _tile2 == null) {
            return false;
        }

        // One of the two tiles need to be the empty one
        if(!_tile1.IsLast() && !_tile2.IsLast()) {
            return false;
        }

        // invert parent and Sibling Index
        Transform tile1Transform = _tile1.transform.parent;
        Transform tile2Transform = _tile2.transform.parent;
        int tile1SiblingIndex = _tile1.transform.GetSiblingIndex();
        int tile2SiblingIndex = _tile2.transform.GetSiblingIndex();
        _tile1.transform.SetParent(tile2Transform);
        _tile2.transform.SetParent(tile1Transform);
        _tile1.transform.SetSiblingIndex(tile2SiblingIndex);
        _tile2.transform.SetSiblingIndex(tile1SiblingIndex);

        // invert index
        int tile1Index = _tile1.GetIndex();
        int tile2Index = _tile2.GetIndex();
        _tile1.SetCurrentIndex(tile2Index);
        _tile2.SetCurrentIndex(tile1Index);

        // Setup animation
        tile1ForAnimation = _tile1;
        tile2ForAnimation = _tile2;
        tile1PositionForAnimation = _tile1.transform.position;
        tile2PositionForAnimation = _tile2.transform.position;
        //tile1ForAnimation.transform.position = tile1PositionForAnimation;
        //tile2ForAnimation.transform.position = tile2PositionForAnimation;
        currentAnimationSpeed = 0;
        currentGameStatus = gameStatus.Moving;

        return true;

    }

    void TryGameOver()
    {
        // Verify all the tiles are in position
        foreach (PuzzleTile tile in tilesList)
        {
            if( !tile.IsInPosition() ) {
                return;
            }
        }

        // Set game over
        currentGameStatus = gameStatus.Over;

        // Print game over score
        int movesScore = SaveMovesScore();
        TimeSpan timeScore = SaveTimeScore();
        TimeSpan gamingTimeSpan = TimeSpan.FromSeconds(gamingTime);

        gameOverScoreText.text = "Your score:\n" + moves.ToString() + " moves\n" + 
        string.Format("{0:D2}:{1:D2}:{2:D2}", gamingTimeSpan.Hours, gamingTimeSpan.Minutes, gamingTimeSpan.Seconds);

        gameOverBestMovesText.text = "Best moves: " + movesScore.ToString();
        gameOverBestTimeText.text = "Best time:" + string.Format("{0:D2}:{1:D2}:{2:D2}", timeScore.Hours, timeScore.Minutes, timeScore.Seconds);

        gameOverPanel.gameObject.SetActive(true);
    }

    int SaveMovesScore()
    {
        int savedScore = 0;
        if(currentGameLevel == gameLevel.easy) {
            savedScore = PlayerPrefs.GetInt("easyScore", 0);
            if(savedScore == 0 || moves < savedScore) {
                savedScore = moves;
                PlayerPrefs.SetInt("easyScore", moves);
            }
        }
        else if(currentGameLevel == gameLevel.medium) {
            savedScore = PlayerPrefs.GetInt("mediumScore", 0);
            if(savedScore == 0 || moves < savedScore) {
                savedScore = moves;
                PlayerPrefs.SetInt("mediumScore", moves);
            }
        }
        else {
            savedScore = PlayerPrefs.GetInt("hardScore", 0);
            if(savedScore == 0 || moves < savedScore) {
                savedScore = moves;
                PlayerPrefs.SetInt("hardScore", moves);
            }
        }
        return savedScore;
    }

    TimeSpan SaveTimeScore()
    {
        float scoreTime = 0f;
        if(currentGameLevel == gameLevel.easy) {
            scoreTime = PlayerPrefs.GetFloat("easyTime", 0f);
            if(scoreTime == 0f || gamingTime < scoreTime) {
                scoreTime = gamingTime;
                PlayerPrefs.SetFloat("easyTime", gamingTime);
            }
        }
        else if(currentGameLevel == gameLevel.medium) {
            scoreTime = PlayerPrefs.GetFloat("mediumTime", 0f);
            if(scoreTime == 0f || gamingTime < scoreTime) {
                scoreTime = gamingTime;
                PlayerPrefs.SetFloat("mediumTime", gamingTime);
            }
        }
        else {
            scoreTime = PlayerPrefs.GetFloat("hardTime", 0f);
            if(scoreTime == 0f || gamingTime < scoreTime) {
                scoreTime = gamingTime;
                PlayerPrefs.SetFloat("hardTime", gamingTime);
            }
        }
        return TimeSpan.FromSeconds(scoreTime);
    }

    void Update()
    {
        if(currentGameStatus == gameStatus.Moving)
        {
            //Reset position with lerp
            if(currentAnimationSpeed < 1f)
            {
                // lerp towards our target
                tile1ForAnimation.transform.position = Vector3.Lerp(tile1PositionForAnimation, tile2PositionForAnimation, currentAnimationSpeed);
                tile2ForAnimation.transform.position = Vector3.Lerp(tile2PositionForAnimation, tile1PositionForAnimation, currentAnimationSpeed);
                currentAnimationSpeed += Time.deltaTime * (isMixing ? animationRandomSpeed : animationSpeed);
            }
            else
            {
                tile1ForAnimation.transform.position = tile2PositionForAnimation;
                tile2ForAnimation.transform.position = tile1PositionForAnimation;
                currentGameStatus = gameStatus.Gaming;
            
                if(!isMixing) {
                    // Update moves
                    moves++;
                    movesText.text = moves.ToString() + " moves";
                    TryGameOver();
                }
            }
        }
        else if(isMixing)
        {
            //If mixing matrix
            if (currentRandomIterations >= randomIterations)
            {
                currentGameStatus = gameStatus.Gaming;
                isMixing = false;
            }
            else if( Move( GetRandomTile(), GetRandomDirection() ) )
            {
                currentRandomIterations++;
            }
        }
        else if(currentGameStatus == gameStatus.Gaming)
        {
            // Add time if is gaming
            gamingTime += Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        // Show time left
        TimeSpan timeSpan = TimeSpan.FromSeconds(gamingTime);
        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    /*public void Realod()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }*/

    public void Exit() {
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}