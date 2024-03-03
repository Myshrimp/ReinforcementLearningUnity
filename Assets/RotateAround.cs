using UnityEngine;

public class RotateAround : MonoBehaviour
{
    [SerializeField] private float spin_speed = 100f;
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if(Input.GetKey(KeyCode.E)) {
            rb.angularVelocity = Vector3.up *  spin_speed * Time.deltaTime;
        }
        else { rb.angularVelocity = Vector3.zero; }

        if (Input.GetKey(KeyCode.Q))
        {
            rb.angularVelocity = -Vector3.up * spin_speed * Time.deltaTime;
        }
        else { rb.angularVelocity = Vector3.zero; }
    }
}
