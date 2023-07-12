using UnityEngine;

namespace ImmersiveStudio.Hex
{
    /// <summary>
    /// this class is attached on hexagon prefab
    /// </summary>
    public class Hexagon : MonoBehaviour
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        #region booleans
        public bool hasUnit = false;
        public bool isRevealed = false;
        #endregion

        private Material mat;

        #region Public references of Gameobjects
        public GameObject unitPrefab;
        [Tooltip("(This field Generates Automatically at Runtime) No need to add a Gameobject here it is the reference for the unitPrefab at runtime")]
        public GameObject instantiatedUnit;
        #endregion

        private void Awake()
        {
            mat = GetComponent<Renderer>().material;
        }

        //set the x ,y coordinates of hexagon on grid
        public void SetCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        //highlight the hexagon if there is unit near the hexagon
        public void HighLight()
        {
            mat.color = Color.blue;
        }

        //clear all previous references when retry the game
        public void Clear()
        {
            isRevealed = false;
            hasUnit = false;
            if (instantiatedUnit != null)
            {
                Destroy(instantiatedUnit);
            }
            Destroy(gameObject);
        }

        #region Unit Related
        //unit will get add on this hexagon
        public void AddUnit()
        {
            instantiatedUnit = Instantiate(unitPrefab, transform.position, unitPrefab.transform.rotation, transform);
            instantiatedUnit.SetActive(false);
            hasUnit = true;
        }

        //added unit will reveal here
        public void RevealUnit()
        {
            if (instantiatedUnit != null)
            {
                instantiatedUnit.SetActive(true);
                isRevealed = true;
            }
        }
        #endregion
    }
}