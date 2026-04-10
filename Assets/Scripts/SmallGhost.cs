using UnityEngine;

public class SmallGhost : MonoBehaviour
{
    public float speed;
    public LayerMask SolidLayer;
    int direction;
    public float RaycastLength;
    // wasd - 0123

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = Random.Range(0, 4);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector2 movementdir = SetMovementDirection(direction);

        if (Physics2D.Raycast(transform.position,movementdir, RaycastLength, SolidLayer))
        {
            direction = (4 + direction + (Random.Range(0,2)==0?1:-1)) % 4;   
        }
        print(direction);
        transform.position += (Vector3)movementdir * speed;
    }

    Vector2 SetMovementDirection(int direction)
    {

        Vector2 movementdir = Vector2.zero;
        switch (direction)
        {
            case 0: movementdir = Vector2.up; break;
            case 1: movementdir = Vector2.right; break;
            case 2: movementdir = Vector2.down; break;
            case 3: movementdir = Vector2.left; break;
        }
        return movementdir;
    }
}
