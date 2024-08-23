using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCamera : MonoBehaviour
{

    public Transform targetObj;
    // Update is called once per frame
    public float speed = 0.5f;
    private float X;
    private float Y;

    float r = 0;
    public float SpeedAutoRotation = 10;

    Quaternion OldPos;
    Vector2 OldAxis;
    Vector2 Axis;
    Quaternion rot;
    float mp;
    float mpY;
    Vector2 oldPosDown;
    /*  Vector3 rotiks(Vector3 eulerAngles)
      {
          Quaternion r = transform.rotation;
          r.x += eulerAngles.x;
              return r;
      } */

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            oldPosDown.x = Input.mousePosition.x;
            oldPosDown.y = Input.mousePosition.y;

        }

        if (Input.GetMouseButton(0))
        {
            mp = (Input.mousePosition.x - oldPosDown.x) * speed * (1 * Time.deltaTime);
            mpY = ((Input.mousePosition.y - oldPosDown.y) * speed) * (-0.3f * Time.deltaTime);

            OldPos = transform.rotation;



            Axis.x = Input.GetAxis("Mouse Y") * speed;
            Axis.y = Input.GetAxis("Mouse X") * speed;


            //   
            transform.Rotate(new Vector3(mpY, mp, 0));

            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;


            oldPosDown.x = Input.mousePosition.x;
            oldPosDown.y = Input.mousePosition.y;
            OldAxis.x = Input.GetAxis("Mouse X") * speed;
            OldAxis.y = Input.GetAxis("Mouse Y") * speed;


        }
        r += SpeedAutoRotation * Time.deltaTime + mp;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, r, 0);

    }
}
