using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.XR;

public class BoxSelect : MonoBehaviour
{
    public Transform start_pointer, end_pointer;
    public GameObject Blocks_Parent, SelectArea;
    public LayerMask table;
    public GameObject start_sphere, end_sphere;
    private List<Vector3> snap_points = new List<Vector3>();
    private InputDevice targetDevice;
    private bool selecting = false;
    private Vector3 start, end;
    private List<GameObject> selected = new List<GameObject>();
    private bool isGrabbed;
    private GameObject objGrabbed;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 start_point = new Vector3(-0.3f, 0.7f, 1f);
        for (int i = 0; i < 4; i ++)
        {
            for (int j = 0; j < 4; j++)
            {
                Vector3 new_point = new Vector3(start_point.x + i * 0.2f, start_point.y, start_point.z + j * 0.2f);
                snap_points.Add(new_point);
            }
        }

        List<InputDevice> devices = new List<InputDevice>();
        InputDeviceCharacteristics rightControllerChars = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerChars, devices);
        foreach (var item in devices)
        {
            Debug.Log(item.name + item.characteristics);
        }
        if (devices.Count > 0)
        {
            targetDevice = devices[0];
        }

    }

    // Update is called once per frame
    void Update()
    {
        targetDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonValue);
        targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue);
        targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue);
        if (primaryButtonValue && !gripValue && triggerValue > 0.1f)
        {
            if (!selecting)
            {
                Vector3 fwd = (end_pointer.position - start_pointer.position).normalized;
                Vector3 pos = start_pointer.position;
                RaycastHit hit;
                selecting = true;
                if (Physics.Raycast(pos, fwd, out hit, Mathf.Infinity, 1 << 13))
                {
                    float minDist = Mathf.Infinity;
                    Vector3 snap = new Vector3(-0.3f, 0.7f, 1f);
                    foreach (Vector3 poss_point in snap_points)
                    {
                        if (Vector3.Distance(poss_point, hit.point) < minDist)
                        {
                            minDist = Vector3.Distance(poss_point, hit.point);
                            snap = poss_point;
                        }
                    }
                    start = snap;
                    end = snap;
                } else
                {
                    selecting = false;
                }
            } else
            {
                Vector3 fwd = (end_pointer.position - start_pointer.position).normalized;
                Vector3 pos = start_pointer.position;
                RaycastHit hit;
                if (Physics.Raycast(pos, fwd, out hit, Mathf.Infinity, 1 << 13))
                {
                    float minDist = Mathf.Infinity;
                    Vector3 snap = new Vector3(-0.3f, 0.7f, 1f);
                    foreach (Vector3 poss_point in snap_points)
                    {
                        if (Vector3.Distance(poss_point, hit.point) < minDist)
                        {
                            minDist = Vector3.Distance(poss_point, hit.point);
                            snap = poss_point;
                        }
                    }
                    end = snap;
                }
            }
        } else if (selecting && start != end)
        {
            Debug.Log("Selected Start: " + start + ", End: " + end);
            selecting = false;
            Vector3 minPoint = new Vector3(Mathf.Min(start.x, end.x), Mathf.Min(start.y, end.y), Mathf.Min(start.z, end.z));
            Vector3 maxPoint = new Vector3(Mathf.Max(start.x, end.x), Mathf.Max(start.y, end.y), Mathf.Max(start.z, end.z));
            selected.Clear();
            foreach (Transform child in Blocks_Parent.transform)
            {
                if (minPoint.x <= child.position.x && child.position.x <= maxPoint.x && minPoint.z <= child.position.z && child.position.z <= maxPoint.z)
                {
                    selected.Add(child.gameObject);
                }
            }
        }
        if (selecting)
        {
            start_sphere.transform.position = start;
            end_sphere.transform.position = end;
            SelectArea.transform.position = (start + end) / 2;
            SelectArea.transform.localScale = new Vector3(Mathf.Abs(end.x - start.x), 0.001f, Mathf.Abs(end.z - start.z));
        }
        foreach (GameObject block in selected)
        {
            if (block.transform.GetComponent<OVRGrabbable>().isGrabbed)
            {
                Debug.Log(block.name + "is Grabbed");
                isGrabbed = true;
                objGrabbed = block;
                SelectArea.transform.position = Vector3.zero;
                start_sphere.transform.position = Vector3.zero;
                end_sphere.transform.position = Vector3.zero;
            }
        }
        if (isGrabbed)
        {
            if (!objGrabbed.transform.GetComponent<OVRGrabbable>().isGrabbed)
            {
                isGrabbed = false;
                objGrabbed = null;
                selected.Clear();
            }
            else
            {
                Rigidbody rb = objGrabbed.GetComponent<Rigidbody>();
                Vector3 new_vel = rb.velocity;
                Vector3 new_avel = rb.angularVelocity;
                foreach (GameObject block in selected)
                {
                    Rigidbody other_rb = block.GetComponent<Rigidbody>();
                    other_rb.velocity = new_vel;
                    other_rb.angularVelocity = new_avel;
                }
            }
        }
        
    }
    public bool isSelected(GameObject obj)
    {
        return selected.Contains(obj);
    }
}
