using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public float health = 100f;
    public int reward = 10;

    private Transform[] waypoints;
    private int waypointIndex = 0;

    public void SetWaypoints(Transform[] points)
    {
        waypoints = points;
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints == null || waypointIndex >= waypoints.Length) return;

        Transform target = waypoints[waypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            waypointIndex++;

        if (waypointIndex >= waypoints.Length)
        {
            GameManager.Instance.LoseLife();
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            GameManager.Instance.AddMoney(reward);
            Destroy(gameObject);
        }
    }
}
