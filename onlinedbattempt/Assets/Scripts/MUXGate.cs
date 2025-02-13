﻿// written by: Khalid Akash
// tested by: Khalid Akash
// debugged by: Khalid Akash
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// The chips are all movable devices that contain 14 Logic Nodes.
/// These Logic Nodes are stored in Hash Tables, but the implementation
/// can easily be changed to Lists as well. To function correctly,
/// they must be ‘snapped’ to 14 other nodes, typically this means 
/// a collision between all of the chip’s Logic Nodes and the Protoboard’s
/// Logic Nodes are detected simultaneously. Once the chip detect that all
/// 14 nodes are collided with, and the user lifts the mouse, the OnMouseUp()
/// callback is recorded, and the position of the chip is snapped to the
/// top left Logic Node’s collided Logic Node’s position (arbitrarily chosen),
/// a green indicator is shown to show potential snappings.  Once the device
/// is snapped, before any logic calculation is done, the chip must detect
/// a collided node on both the 7th pin, and the 14th pin, with a logic low
/// and a logic high going to the respective nodes. After that, based
/// on the datasheet, the collided input Logic Node’s states are taken, and the 
/// output is set.
/// </summary>
public class MUXGate : MonoBehaviour, LogicInterface
{


    private Dictionary<string, GameObject> logic_dictionary = new Dictionary<string, GameObject>(); //Contains all the gameobject nodes for the 74LS400 chip.+
    private GameObject DeviceGameObject;
    private GameObject snapIndicatorGameObj;
    public const string LOGIC_DEVICE_ID = "74LS151A_MUX_NODE_";
    private Vector3 screenPoint;
    private Vector3 offset;
    private bool SNAPPED = false; //Set to true if all Logic Nodes of this device is in collision with an external node


    public void SetSnapped(bool snap)
    {
        this.SNAPPED = snap;
    }

    public Dictionary<string, GameObject> GetLogicDictionary()
    {
        return logic_dictionary;
    }

    /// <summary>
    /// Sets all 14 nodes in the specified position in the Logic Chips
    /// </summary>
    void Start()
    {
        DeviceGameObject = this.gameObject;
        //Loop that places Logic Nodes on the 74LS400 chip
        float horizontal_pos = -.205f; //set up for left side of the chip
        float vertical_pos = .58f; //top of the chip
        float vertical_direct = -.208f;
        for (int i = 0; i < 14; i++)
        {
            GameObject logicNode = new GameObject(LOGIC_DEVICE_ID + i); //logic node with the name leftlogicnode_{i}_0
            logicNode.transform.parent = DeviceGameObject.transform; //sets the Protoboard game object as logicNode_0's parent
            logicNode.transform.localPosition = new Vector3(horizontal_pos, vertical_pos + i * (vertical_direct), 0); //'localPosition' sets the position of this node RELATIVE to the protoboard
            logicNode.transform.localScale = new Vector3(.10F, .10F, 0);
            logicNode.AddComponent<LogicNode>();
            logic_dictionary.Add(LOGIC_DEVICE_ID + i, logicNode);
            if (i == 6) //when the left side is complete
            {
                vertical_pos = vertical_pos + (13 * vertical_direct);
                vertical_direct = .208f;
                horizontal_pos = horizontal_pos + .532f; //change the horizontal position to the right side

            }
        }

        //add SNAP indicator object to the chip
        snapIndicatorGameObj = new GameObject(LOGIC_DEVICE_ID + "_SNAP_INDICATOR_");
        snapIndicatorGameObj.transform.parent = DeviceGameObject.transform; //sets the Protoboard game object as logicNode_0's parent
        snapIndicatorGameObj.transform.localPosition = new Vector3(-.0775f, .575f, 0); //'localPosition' sets the position of this node RELATIVE to the protoboard
        snapIndicatorGameObj.transform.localScale = new Vector3(.10F, .10F, 0);
        SpriteRenderer sprite_renderer = snapIndicatorGameObj.AddComponent<SpriteRenderer>(); //adds a test "circle" graphic
        sprite_renderer.sprite = Resources.Load<Sprite>("Sprites/logicCircle");
        sprite_renderer.sortingLayerName = "FrontLayer";
        sprite_renderer.material.color = new Color(1, 1, 1);
    }

    /// <summary>
    /// Mouse down on GameObject activates the movement of the object
    /// to follow the mouse position
    /// </summary>
    void OnMouseDown()
    {
        Debug.Log("74LS151A Mouse Down");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

    }
    /// <summary>
    /// Callback that notifies the object that the mouse is being dragged
    /// on it. This is used to help 'move' the GameObject by calculating the offset
    /// from the previous mouse position. It also checks if the chip is in the
    /// 'Snapped' position if the Mouse click is let go.
    /// </summary>
    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        transform.position = curPosition;

        //Check for snapping when chip is removed from set place.
        //Check if all nodes with the chip is colliding with another logic node;
        SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
        foreach (KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logic_node = entry.Value;
            LogicNode logic_behavior = logic_node.GetComponent<LogicNode>();
            if (logic_behavior.GetCollidingNode() == null)
            {
                //indicator
                spr_ren.material.color = new Color(1, 1, 1); //neutral
                SNAPPED = false;
                Debug.Log("Snap not set.");
                return;
            }
        }
        //if execution reached here, it means all colliding nodes are valid nodes
        //indicate device can be active
        spr_ren.material.color = new Color(0, 1, 0); //green

    }

    /// <summary>
    /// Checks if pin 7 and pin 14 is connected to ground and logic high respectively.
    /// This is checked whenever a new state change is requested to be reacted to.
    /// </summary>
    /// <returns>True or False boolean value on whether the Device is considered on</returns>
    public bool IsDeviceOn()
    {
        if (!SNAPPED)
        {
            return false;
        }
        GameObject logic_gnd;
        GameObject logic_vcc;
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 6, out logic_gnd)
            && logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 13, out logic_vcc))
        {
            LogicNode logic_behavior_gnd = logic_gnd.GetComponent<LogicNode>();
            LogicNode logic_behavior_vcc = logic_vcc.GetComponent<LogicNode>();
            LogicNode gndCollision = logic_behavior_gnd.GetCollidingNode().GetComponent<LogicNode>();
            LogicNode vccCollision = logic_behavior_vcc.GetCollidingNode().GetComponent<LogicNode>();
            Debug.Log("GND Set to: " + gndCollision.GetLogicState() + " for Device " + this.gameObject.name);
            Debug.Log("VCC Set to: " + vccCollision.GetLogicState() + " for Device " + this.gameObject.name);
            if (gndCollision.GetLogicState() == (int)LOGIC.LOW
                && vccCollision.GetLogicState() == (int)LOGIC.HIGH)
            {
                Debug.Log(this.DeviceGameObject.name + " is ON.");
                return true;
            }
        }
        Debug.Log(this.DeviceGameObject.name + " is OFF.");
        return false;
    }


    /// <summary>
    /// Checks if the Device is On, and then continues to check all the inputs colliding
    /// node and sets the output nodes to the correct state.
    /// </summary>
    private void ChipIO()
    {
        GameObject select0, select1, select2, enable, z, zinv, input_0, input_1, input_2, 
            input_3, input_4, input_5, input_6, input_7, logic_6, logic_13;

        //GND
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 6, out logic_6))
        {
            LogicNode logic_behavior = logic_6.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            //GND pin collision node is not GND
            if (collided_state != (int)LOGIC.LOW)
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A Ground Input not set to LOW");
            }

        }
        //VCC
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 13, out logic_13))
        {
            LogicNode logic_behavior = logic_13.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state != (int)LOGIC.HIGH)
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A VCC Input not set to HIGH");
            }

        }
        /**
         * INPUTs find the collided nodes of the input pins and sets the input's
         * pin state to the collided node's state.
         * 
         */
        //MUX Selector 0, Node 11
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 10, out select0))
        {
            LogicNode logic_behavior = select0.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Selector 1, Node 10
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 9, out select1))
        {
            LogicNode logic_behavior = select1.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Selector 2, Node 9
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 8, out select2))
        {
            LogicNode logic_behavior = select2.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }

        //MUX Input 0, Node 4
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 3, out input_0))
        {
            LogicNode logic_behavior = input_0.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 1, Node 3
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 2, out input_1))
        {
            LogicNode logic_behavior = input_1.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 2, Node 2
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 1, out input_2))
        {
            LogicNode logic_behavior = input_2.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 3, Node 1
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 0, out input_3))
        {
            LogicNode logic_behavior = input_3.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 4, Node 15
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 14, out input_4))
        {
            LogicNode logic_behavior = input_4.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 5, Node 14
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 13, out input_5))
        {
            LogicNode logic_behavior = input_5.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 6, Node 13
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 12, out input_6))
        {
            LogicNode logic_behavior = input_6.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }
        //MUX Input 7, Node 12
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 11, out input_7))
        {
            LogicNode logic_behavior = input_7.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }

        //MUX Enable, Node 7
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 6, out enable))
        {
            LogicNode logic_behavior = enable.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            if (collided_state == (int)LOGIC.INVALID || !IsDeviceOn())
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
                Debug.Log("MUX 74LS151A input 0 is invalid.");
            }

        }


        //MUX Output Z
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 2, out z))
        {
            LogicNode logic_behavior = z.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            LogicNode sel0, sel1, sel2, i0, i1, i2, i3, i4, i5, i6, i7, e; //LogicNode references
            sel0 = select0.GetComponent<LogicNode>(); sel1 = select1.GetComponent<LogicNode>(); sel2 = select2.GetComponent<LogicNode>();
            i0 = input_0.GetComponent<LogicNode>(); i1 = input_1.GetComponent<LogicNode>(); i2 = input_2.GetComponent<LogicNode>();
            i3 = input_3.GetComponent<LogicNode>(); i4 = input_4.GetComponent<LogicNode>(); i5 = input_5.GetComponent<LogicNode>();
            i6 = input_6.GetComponent<LogicNode>(); i7 = input_7.GetComponent<LogicNode>(); e = enable.GetComponent<LogicNode>();
            int low = (int)LOGIC.LOW;
            int invalid = (int)LOGIC.INVALID;
            if (IsDeviceOn())
            {
                if(sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW)
                {
                    logic_behavior.SetLogicState(i0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH)
                {
                    logic_behavior.SetLogicState(i1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                    sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW)
                {
                    logic_behavior.SetLogicState(i2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                    sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH)
                {
                    logic_behavior.SetLogicState(i3.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                    sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                    sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW)
                {
                    logic_behavior.SetLogicState(i4.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                     sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW &&
                     sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH)
                {
                    logic_behavior.SetLogicState(i5.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                     sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                     sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.LOW)
                {
                    logic_behavior.SetLogicState(i6.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else if (sel0.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                     sel1.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH &&
                     sel2.GetCollidingNode().GetComponent<LogicNode>().GetLogicState() == (int)LOGIC.HIGH)
                {
                    logic_behavior.SetLogicState(i7.GetCollidingNode().GetComponent<LogicNode>().GetLogicState());
                }
                else
                {
                    logic_behavior.SetLogicState((int)LOGIC.INVALID);
                }
            }
            else
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
            }
            collided_behavior.RequestStateChange(logic_behavior.GetLogicState());
        }

        //MUX Output Z
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 2, out zinv))
        {
            LogicNode logic_behavior = zinv.GetComponent<LogicNode>();
            GameObject collided_node = logic_behavior.GetCollidingNode();
            LogicNode collided_behavior = collided_node.GetComponent<LogicNode>();
            int collided_state = collided_behavior.GetLogicState();
            LogicNode zNode; //LogicNode references
            zNode = z.GetComponent<LogicNode>();
            int low = (int)LOGIC.LOW;
            int invalid = (int)LOGIC.INVALID;
            if (IsDeviceOn())
            {
                if(zNode.GetLogicState() == (int)LOGIC.HIGH)
                {
                    logic_behavior.SetLogicState((int)LOGIC.LOW);
                }
                else if (zNode.GetLogicState() == (int)LOGIC.LOW)
                {
                    logic_behavior.SetLogicState((int)LOGIC.HIGH);
                }
                else if (zNode.GetLogicState() == (int)LOGIC.INVALID)
                {
                    logic_behavior.SetLogicState((int)LOGIC.INVALID);
                }
            }
            else
            {
                logic_behavior.SetLogicState((int)LOGIC.INVALID);
            }
            collided_behavior.RequestStateChange(logic_behavior.GetLogicState());
        }




    }
    /// <summary>
    /// Check if the device has all it's nodes is colliding with another set of Logic Nodes
    /// </summary>
    private void CheckIfSnapped()
    {
        Debug.Log("XOR 74LS86A Mouse Up");

        //Check if all nodes with the chip is colliding with another logic node;
        foreach (KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logic_node = entry.Value;
            LogicNode logic_behavior = logic_node.GetComponent<LogicNode>();
            if (logic_behavior.GetCollidingNode() == null)
            {
                //indicator
                SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
                spr_ren.material.color = new Color(1, 1, 1); //neutral
                SNAPPED = false;
                Debug.Log("Snap not set.");
                return;
            }
        }
        //On release of mouse, SNAP the chip to the position
        GameObject node_left;
        //get both top left and top right logic nodes on the chip to check if they collided with any other logic nodes
        if (logic_dictionary.TryGetValue(LOGIC_DEVICE_ID + 0, out node_left))
        {
            LogicNode logicNodeScript_l = node_left.GetComponent<LogicNode>();
            GameObject collidingNodeLeft = logicNodeScript_l.GetCollidingNode();
            Debug.Log("XOR 74LS86A SNAPPED!");
            Vector3 collidingNodePos = collidingNodeLeft.transform.position;
            Vector3 offsetPosition = new Vector3(collidingNodePos.x + .245f, collidingNodePos.y - .58f, collidingNodePos.z);
            DeviceGameObject.transform.position = offsetPosition;
            //indicator
            SpriteRenderer spr_ren = snapIndicatorGameObj.GetComponent<SpriteRenderer>();
            spr_ren.material.color = new Color(0, 1, 0); //green
            SNAPPED = true;
        }
    }
    /// <summary>
    /// Checks if the chip is snapped when the Mouse click is released to snap it
    /// into position.
    /// </summary>
    public void OnMouseUp()
    {
        CheckIfSnapped();
    }


    /// <summary>
    /// Sets all the nodes of the chip to a logic of invalid/neutral.
    /// </summary>
    private void ClearChip()
    {
        foreach (KeyValuePair<string, GameObject> entry in logic_dictionary)
        {
            GameObject logicNodeGameObj = entry.Value;
            LogicNode logic_node = logicNodeGameObj.GetComponent<LogicNode>();
            logic_node.SetLogicState((int)LOGIC.INVALID);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// If the chip is snapped, react to the input logics and set the outputs
    /// to the correct states. Otherwise, clear the chips.
    /// </summary>
    /// <param name="logicNode"></param>
    /// <param name="requestedState"></param>
    public void ReactToLogic(GameObject logicNode, int requestedState)
    {
        if (requestedState == (int)LOGIC.INVALID && !SNAPPED)
        {
            ClearChip();
        }
        //Check if chip is snapped to protoboard, and then updates logic
        else if (SNAPPED)
        {
            ChipIO();
        }

    }


    public bool isSnapped()
    {
        return SNAPPED;
    }


    public void ReactToLogic(GameObject LogicNode)
    {

    }


}