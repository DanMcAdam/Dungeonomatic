using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNodeAlignmentTestScript : MonoBehaviour
{
    public bool align = false;

    public GameObject parent;

    public GameObject targetNode;

    public GameObject alignNode;

    public List<GameObject> doorList = new List<GameObject>();
    public List<GameObject> roomList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (align)
        {
            Vector3 alignOffset;
            GameObject targetRoom = roomList[0];
            ChildNodeAlignmentTestScript targetComponent = targetRoom.GetComponent<ChildNodeAlignmentTestScript>();
            targetNode = targetComponent.doorList[Random.Range(0, targetComponent.doorList.Count)];
            alignNode = doorList[Random.Range(0, doorList.Count)];

            Quaternion rot = Quaternion.FromToRotation(alignNode.transform.forward, -targetNode.transform.forward);
            rot.x = 0;
            rot.z = 0;
            parent.transform.rotation = rot * parent.transform.rotation;

            //alignTargetAngle = targetNode.transform.forward;
            //parent.transform.rotation = Quaternion.LookRotation(alignTargetAngle);
            alignOffset = parent.transform.position - alignNode.transform.position;
            parent.transform.position = targetNode.transform.position + alignOffset;
            align = false;
        }
    }
}
