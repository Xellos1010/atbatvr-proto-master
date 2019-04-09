using UnityEngine;
using CurvedUI;

public class CUI_gvrController_Movement : MonoBehaviour {

        [SerializeField]
        CurvedUISettings mySettings;
        [SerializeField]
        Transform pivot;
        [SerializeField]
        float sensitivity = 0.1f;
        Vector3 lastMouse;

        // Use this for initialization
        void Start()
        {
            lastMouse = Input.mousePosition;
        }

        void Update()
        {
            UpdatePointer();
            //UpdateStatusMessage();
        }

        // Update is called once per frame
        void UpdatePointer()
        {

            /*Vector3 mouseDelta = Input.mousePosition - lastMouse;
            lastMouse = Input.mousePosition;
            pivot.localEulerAngles += new Vector3(-mouseDelta.y, mouseDelta.x, 0) * sensitivity;
            */
            if (GvrController.State != GvrConnectionState.Connected)
            {
                pivot.gameObject.SetActive(false);
            }
            pivot.gameObject.SetActive(true);
            pivot.transform.rotation = GvrController.Orientation;
            //pass ray to canvas
            Ray myRay = new Ray(this.transform.position, this.transform.forward);

            mySettings.CustomControllerRay = myRay;

        }
    
}
