using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinesGame : MonoBehaviour
{

    public GameObject dotPrefab;
    public Dropdown mineDropDown;
    public GameObject tilePrefab;
    public Transform gridParent;
    public int gridSize = 5;
    public int numberOfMines = 1;
    public Text cashoutText;
    public Text nextRewardText;
    public Text balanceText;
    public Text betAmountText;
    public Button cashoutButton, betButton;
    public GameObject gameOverPanel;
    public Button playAgainButton, minusBetButton, addBetButton;
    public Sprite mineSprite;
    public Sprite safeSprite;
    public Sprite coverTailSprite;
    public float betAmount = 0.1f;
    public float balance = 3000f;

    public GameObject[,] tiles;
    private bool[,] mines;
    private float currentCashout = 0.0f;
    private float nextReward = 1f;
    int temptail = 25;
    IEnumerator Start()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        GameObject loading = GameObject.Find("loading");
        loading.LeanScale(Vector3.zero, 1f).setEaseSpring();
        betAmount = betOption[0];
        nextReward = 1f + ((float)numberOfMines / (25 - numberOfMines));
        for (int i = 0; i < 20; i++)
        {

            Dropdown.OptionData optionData = new Dropdown.OptionData();
            optionData.text = (i + 1).ToString();
            mineDropDown.options.Add(optionData);
        }
        mineDropDown.value = 0;

        mineDropDown.onValueChanged.AddListener((value) =>
        {
            numberOfMines = value + 1;
            nextReward = 1f + ((float)numberOfMines / (25 - numberOfMines));
            nextRewardText.text = "Next: " + nextReward.ToString("F2") + "X";
        });
        temptail -= numberOfMines;
        cashoutButton.onClick.AddListener(OnCashoutButtonClicked);
        playAgainButton.onClick.AddListener(ResetGame);
        betButton.onClick.AddListener(() =>
        {
            minusBetButton.interactable = addBetButton.interactable = false;
            temptail -= numberOfMines;
            betButton.gameObject.SetActive(false);
            mineDropDown.interactable = false;
            InitializeGrid();
            PlaceMines();
        });

        UpdateUI();
    }

    void InitializeGrid()
    {
        tiles = new GameObject[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                GameObject tile = Instantiate(tilePrefab, gridParent);
                tile.GetComponent<Button>().onClick.AddListener(() => OnTileClicked(tile));
                tile.GetComponent<Image>().sprite = coverTailSprite;
                GameObject bonus = tile.gameObject.transform.GetChild(0).gameObject;
                bonus.GetComponent<Image>().sprite = safeSprite;
                tiles[i, j] = tile;
            }
        }
    }

    void PlaceMines()
    {
        mines = new bool[gridSize, gridSize];
        int placedMines = 0;
        while (placedMines < numberOfMines)
        {
            int x = Random.Range(0, gridSize);
            int y = Random.Range(0, gridSize);
            if (!mines[x, y])
            {
                mines[x, y] = true;
                tiles[x, y].GetComponent<Image>().sprite = coverTailSprite;
                tiles[x, y].transform.GetChild(0).gameObject.GetComponent<Image>().sprite = mineSprite;
                placedMines++;
            }
        }
    }

    void OnTileClicked(GameObject tile)
    {
        tile.GetComponent<Button>().interactable = false;

        int x = (int)tile.transform.GetSiblingIndex() / gridSize;
        int y = (int)tile.transform.GetSiblingIndex() % gridSize;

        if (mines[x, y])
        {
            cashoutButton.interactable = false;
            Debug.Log("Game Over!");
            RevealTile(tile);
            balance -= betAmount;
            UpdateUI();
            StartCoroutine(GameOverShow());
            return;
        }
        temptail -= 1;
        if (temptail == numberOfMines)
        {

        }

        GameObject count = GameObject.Find("count");
        GameObject dot = Instantiate(dotPrefab, count.transform);

        RevealTile(tile);
        currentCashout = betAmount * nextReward;
        nextReward = nextReward + (float)numberOfMines / (float)(temptail - numberOfMines);

        UpdateUI();
    }

    IEnumerator GameOverShow()
    {

        for (int i = 0; i < gridParent.transform.childCount; i++)
        {
            gridParent.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(true);
        }
        minusBetButton.interactable = addBetButton.interactable = true;
        yield return new WaitForSeconds(4f);
        ResetGame();
        cashoutButton.interactable = true;
        betButton.gameObject.SetActive(true);
    }

    void RevealTile(GameObject tile)
    {
        LeanTween.scale(tile, Vector3.one * 1.2f, 0.2f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            LeanTween.scale(tile, Vector3.one, 0.2f).setEase(LeanTweenType.easeInBounce);

            tile.transform.GetChild(0).gameObject.SetActive(true); // Assuming the star is the first child
        });
    }

    void UpdateUI()
    {
        balanceText.text = $"balance: ${balance.ToString("0.00")}";
        betAmountText.text = $"bet: ${betAmount.ToString("0.00")}";
        cashoutText.text = currentCashout.ToString("F2") + " USD";
        nextRewardText.text = "Next: " + nextReward.ToString("F2") + "X";
    }

    public void OnCashoutButtonClicked()
    {
        cashoutButton.interactable = false;
        balance += currentCashout;
        UpdateUI();
        StartCoroutine(GameOverShow());
    }

    void ResetGame()
    {

        nextReward = 1f + ((float)numberOfMines / (25 - numberOfMines));
        currentCashout = 0.0f;
        ClearGrid();
        mineDropDown.interactable = true;
        temptail = 25 - numberOfMines;
        UpdateUI();
        // gameOverPanel.SetActive(false);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ClearGrid()
    {
        GameObject count = GameObject.Find("count");
        List<GameObject> gameObjects = new List<GameObject>();
        List<GameObject> dots = new List<GameObject>();
        for (int j = 0; j < count.transform.childCount; j++)
        {
            dots.Add(count.transform.GetChild(j).gameObject);
        }
        for (int i = 0; i < gridParent.transform.childCount; i++)
        {
            gameObjects.Add(gridParent.transform.GetChild(i).gameObject);
        }
        dots.ForEach(g => Destroy(g));
        gameObjects.ForEach(g => Destroy(g));
    }


    void DisableAllTailButton()
    {
        for (int i = 0; i < gridParent.transform.childCount; i++)
        {
            gridParent.transform.GetChild(i).gameObject.GetComponent<Button>().interactable = false;
        }
    }

    float[] betOption = { 0.2f, 0.5f, 1f, 1.2f, 1.5f, 2f, 4f, 5f, 6f, 7f, 8f, 10f };

    int currentBetOption = 0;
    public void ChangeBetOption(bool isAdd)
    {
        if (isAdd)
        {
            currentBetOption += 1;
            if (currentBetOption >= betOption.Length) currentBetOption = 0;
        }
        else
        {
            currentBetOption -= 1;
            if (currentBetOption < 0) currentBetOption = betOption.Length - 1;
        }

        betAmount = betOption[currentBetOption];
        betAmountText.text = $"bet: ${betAmount.ToString("0.00")}";
    }

}
