using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMove : MonoBehaviour
{
    // Присваиваем переменные
    public float mouseSensitivity = 3f;
    public float speed = 5f;
    private Vector3 transfer;
    float rotationX = 0f;
    float rotationY = 0f;
    Quaternion originalRotation;

    void Awake()
    {
       
    }

    void Start()
    {
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            // Движения мыши -> Вращение камеры
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
            rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;
            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, Vector3.left);
            transform.rotation = originalRotation * xQuaternion * yQuaternion;

            // Ускорение при нажатии клавиши Shift
            if (Input.GetKeyDown(KeyCode.LeftShift))
                speed *= 5;
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                speed /= 5;

            // Поднятие и опускание камеры
            Vector3 newPos = new Vector3(0, 1, 0);
            if (Input.GetKey(KeyCode.E))
                transform.position += newPos * speed * Time.deltaTime;
            else if (Input.GetKey(KeyCode.Q))
                transform.position -= newPos * speed * Time.deltaTime;

            // перемещение камеры
            transfer = transform.forward * Input.GetAxis("Vertical");
            transfer += transform.right * Input.GetAxis("Horizontal");
            transform.position += transfer * speed * Time.deltaTime;
        } 
    }
        

    public static float ClampAngle (float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp (angle, min, max);
    }
       
}