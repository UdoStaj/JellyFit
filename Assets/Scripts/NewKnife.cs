using UnityEngine;

public class NewKnife : MonoBehaviour
{
    public GameObject gridSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gridSystem = FindAnyObjectByType<GridSystem>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static void CutBetweenCells(Vector2Int from, GridSystem grid)
    {
        GameObject fromObj = grid.GetCellObject(from);

        CubeGroupControl ctrl = fromObj.GetComponent<CubeGroupControl>();
        ctrl?.CutBetween(Vector3Int.left);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<CubeGroupControl>() != null)
        {
            Vector2Int vector2Int = new Vector2Int
                (
                gridSystem.GetComponent<GridSystem>().GetGridIndexFromWorld(this.transform.position).x, 
                gridSystem.GetComponent<GridSystem>().GetGridIndexFromWorld(this.transform.position).y
                );
            CutBetweenCells(vector2Int, gridSystem.GetComponent<GridSystem>());
            Debug.Log("tamam");
        }
    }

}
