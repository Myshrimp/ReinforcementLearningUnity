
using DroneController.Physics;
using UnityEngine;
namespace DroneController {

    public class drone : MonoBehaviour
    {
        public DroneMovementScript move;
        public DronePropelers propelers;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            move.W = true;
        }
    }


}

