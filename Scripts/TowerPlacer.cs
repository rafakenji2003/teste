using UnityEngine;

public class TowerPlacer : MonoBehaviour
{
    public GameObject towerPrefab;
    public int towerCost = 50;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPlaceTower();
    }

    void TryPlaceTower()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (hit.collider.CompareTag("PlacementArea"))
        {
            if (GameManager.Instance.SpendMoney(towerCost))
            {
                Vector3 pos = hit.point;
                pos.y = 0;
                Instantiate(towerPrefab, pos, Quaternion.identity);
            }
            else
            {
                Debug.Log("Dinheiro insuficiente!");
            }
        }
    }
}
