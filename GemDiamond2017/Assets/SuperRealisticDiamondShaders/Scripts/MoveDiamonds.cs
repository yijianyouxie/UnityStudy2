
using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class MoveDiamonds : MonoBehaviour

{


    public float MaxDistance = 8;


    private Vector3 mOffset;



    private float mZCoord;

    public float SpeedRotation = 8;


    private void Update()
    {
        


      //  GetComponent<Rigidbody>().AddTorque.transform.Rotate(Vector3(0, speed, 0));

        if (transform.position.y < -MaxDistance * 0.3)
        {
            Vector3 poss = transform.position;
            poss.y = 3;
            transform.position = poss;
            
        }
        if (transform.position.x < -MaxDistance || transform.position.x > MaxDistance)
        {
            Vector3 poss = transform.position;
            poss.x = 0;
            transform.position = poss;

        }
        if (transform.position.z < -MaxDistance || transform.position.z > MaxDistance)
        {
            Vector3 poss = transform.position;
            poss.z = 0;
            transform.position = poss;

        }
    }
    void OnMouseDown()

    {

        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        GetComponent<Rigidbody>().AddTorque(new Vector3(1,1,1) * SpeedRotation);

        // Store offset = gameobject world pos - mouse world pos

        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();

    }



    private Vector3 GetMouseAsWorldPoint()

    {

        // Pixel coordinates of mouse (x,y)

        Vector3 mousePoint = Input.mousePosition;



        // z coordinate of game object on screen

        mousePoint.z = mZCoord;



        // Convert it to world points

        return Camera.main.ScreenToWorldPoint(mousePoint);

    }



    void OnMouseDrag()

    {

        transform.position = GetMouseAsWorldPoint() + mOffset;

    }

}