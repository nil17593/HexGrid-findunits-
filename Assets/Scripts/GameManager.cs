using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

namespace ImmersiveStudio.Hex
{
    /// <summary>
    /// This class handles the Grid generation, adding units to the hexagons, click mechanism 
    /// also finding the neighbour hexagons by taking reference of Hexagon
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region HEx Grid Generation
        [Header("Grid Generation Settings")]
        [SerializeField] private Hexagon hexPrefab;
        //[SerializeField] private GameObject unitPrefab;
        [SerializeField] private Transform hexGridParent;
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        float hexWidth = 1.732f;
        float hexHeight = 2.0f;
        #endregion     

        #region Counts
        [Header("Counts")]
        [SerializeField] private int UNIT_COUNT = 10;
        [SerializeField] private int MAX_CLICK = 15;
        [SerializeField] private int RANGE = 1;
        [SerializeField] private int revealedCount = 0;
        [SerializeField] private int clickCount = 0;
        #endregion

        #region UI
        [SerializeField] private GameObject gameOverUI;
        [SerializeField] private TextMeshProUGUI txtRemainingClicks;
        [SerializeField] private TextMeshProUGUI txtRemainingUnits;
        [SerializeField] private TextMeshProUGUI textNoUnitsInRange;
        #endregion

        #region lists
        [Header("Lists")]
        [SerializeField] private List<Hexagon> hexes = new List<Hexagon>();
        [SerializeField] private List<GameObject> randomUnit = new List<GameObject>();
        #endregion

        void Start()
        {
            txtRemainingClicks.text = "Clicks left= " + MAX_CLICK.ToString();
            txtRemainingUnits.text = "Units Left= " + UNIT_COUNT.ToString();
            CreateGrid();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Hexagon hex = hit.collider.GetComponent<Hexagon>();
                    if (hex != null && hex.hasUnit && hex.isRevealed)
                    {
                        return;
                    }
                    else
                    {
                        if (hex != null && hex.hasUnit && !hex.isRevealed)
                        {
                            //GameObject unitInstance = Instantiate(unitPrefab, hex.transform.position, unitPrefab.transform.rotation);
                            //unitInstance.transform.SetParent(hex.transform);
                            hex.RevealUnit();
                            revealedCount++;
                            int remainUnits = UNIT_COUNT - revealedCount;
                            if (remainUnits <= 0)
                            {
                                remainUnits = 0;
                            }
                            txtRemainingUnits.text = "Units Left= " + remainUnits.ToString();
                            clickCount++;
                            int remainClicks = MAX_CLICK - clickCount;
                            if (remainClicks <= 0)
                            {
                                remainClicks = 0;
                            }
                            txtRemainingClicks.text = "Clicks left= "+ remainClicks.ToString();
                            return;
                        }

                        else if (!hex.isRevealed)
                        {
                            if (clickCount < MAX_CLICK)
                            {
                                List<Hexagon> neighbors = GetNeighbors(hex, RANGE);
                                if (neighbors.Count >= 1)
                                {
                                    hex.HighLight();
                                }
                                else
                                {
                                    StartCoroutine(ActivateNoUnitText());
                                }
                                clickCount++;
                                int remainClicks = MAX_CLICK - clickCount;
                                txtRemainingClicks.text = "click left= " + remainClicks.ToString();
                            }
                        }

                        if (revealedCount == UNIT_COUNT || clickCount == MAX_CLICK)
                        {
                            GameOver();
                        }
                    }
                }
            }
        }
        #region Grid Generation

        //calculate positions for the grid generation
        Vector3 CalculatePosition(int x, int y)
        {
            float xPos = hexWidth * x;
            float zPos = hexHeight * (y + x * 0.5f);
            return new Vector3(xPos, 0, zPos);
        }

        //grid creation
        void CreateGrid()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Vector3 position = CalculatePosition(x, y);
                    Hexagon hex = Instantiate(hexPrefab, position, Quaternion.identity);
                    hex.gameObject.transform.SetParent(hexGridParent);
                    hexes.Add(hex);
                    Hexagon coordinates = hex.GetComponent<Hexagon>();
                    if (coordinates != null)
                    {
                        coordinates.SetCoordinates(x, y);
                    }
                    else
                    {
                        Debug.LogWarning("HexCoordinates component not found on hexagon.");
                    }
                }
            }
            PlaceUnitsRandomly();
        }

        /// <summary>
        /// placing units randomly on hexagons with given number of UNIT_COUNT
        /// the boolean "hasUnit" set to true 
        /// the unit will be added and deactivated at the same time
        /// </summary>
        void PlaceUnitsRandomly()
        {
            for (int i = 0; i < UNIT_COUNT; i++)
            {
                Hexagon hexTile = GetRandomHextTile();
                hexTile.AddUnit();
                randomUnit.Add(hexTile.gameObject);
            }
        }

        //returns random hext tiles to add units
        public Hexagon GetRandomHextTile()
        {
            List<Hexagon> hasUnitHex = new List<Hexagon>();
            hasUnitHex.AddRange(hexes.FindAll(o => !o.hasUnit));
            ShuffleList(hasUnitHex);
            return hasUnitHex[0];
        }

        //shuffle the lists
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #endregion


        #region Find Units in Given RANGE

        //returns the neigbour hexagons from clicked hexagon in Given RANGE
        List<Hexagon> GetNeighbors(Hexagon hexagon, int range)
        {
            List<Hexagon> neighborHexes = new List<Hexagon>();


            if (hexagon == null)
            {
                Debug.LogWarning("HexCoordinates component not found on hexagon.");
                return neighborHexes;
            }

            int centerX = hexagon.X;
            int centerY = hexagon.Y;

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (IsValidCoordinate(x, y))
                    {
                        Hexagon neighbor = GetHexagonAtCoordinate(x, y);
                        if (neighbor != null && neighbor != hexagon && neighbor.hasUnit && !neighbor.isRevealed)
                        {
                            neighborHexes.Add(neighbor);
                        }
                    }
                }
            }

            return neighborHexes;
        }

        //check for corner hexes if there are no neighbour
        bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        //Get the exact hexagon with given x and y
        Hexagon GetHexagonAtCoordinate(int x, int y)
        {
            foreach (Hexagon hexagon in hexes)
            {
                Hexagon hexCoordinate = hexagon.GetComponent<Hexagon>();
                if (hexCoordinate != null && hexCoordinate.X == x && hexCoordinate.Y == y)
                {
                    return hexagon;
                }
            }
            return null;
        }
        #endregion


        #region GameOver and Retry
        void GameOver()
        {
            gameOverUI.SetActive(true);
        }

        public void OnRetryButtonClick()
        {
            foreach (Hexagon hex in hexes)
            {
                hex.Clear();
            }
            hexes.Clear();

            foreach (GameObject unit in randomUnit)
            {
                Destroy(unit);
            }
            randomUnit.Clear();
            revealedCount = 0;
            clickCount = 0;
            gameOverUI.SetActive(false);
            CreateGrid();
            txtRemainingClicks.text = "Click left= " + MAX_CLICK.ToString();
            txtRemainingUnits.text = "Units Left= " + UNIT_COUNT.ToString();
        }
        #endregion

        IEnumerator ActivateNoUnitText()
        {
            textNoUnitsInRange.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            textNoUnitsInRange.gameObject.SetActive(false);
        }
    }
}
