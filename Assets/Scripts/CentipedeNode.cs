using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class CentipedeNode : MonoBehaviour
{
    public CentipedeNode nextNode;
    public bool head;
    public bool tail;
    public bool thisistheonethatisgoingtobeseparatedwhenyoupressp;
    Queue<Vector3> Path;
    int direction;
    public float speed;
    public float RaycastLength;
    public LayerMask SolidLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Path = new Queue<Vector3>();
        direction = Random.Range(0, 4);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tail) return;

        if (Path.Count > 10)
        {
            nextNode.transform.position = Path.Dequeue();
        }
        print(Path.Count);
        Path.Enqueue(transform.position);

        if (head)
        {
            Vector2 movementdir = SetMovementDirection(direction);

            if (Physics2D.Raycast(transform.position, movementdir, RaycastLength, SolidLayer))
            {
                direction = (4 + direction + (Random.Range(0, 2) == 0 ? 1 : -1)) % 4;
            }
            print(direction);
            transform.position += (Vector3)movementdir * speed;

        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && thisistheonethatisgoingtobeseparatedwhenyoupressp)
        {
            split();
        } 
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

    void split()
    {
        nextNode.head = true;
        Destroy(this.gameObject);
    }

}
