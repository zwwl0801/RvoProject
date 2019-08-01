using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FrameWorkCmdDefine
{

    SceneLeave =-100,

    FSMCallEnter = -10001,
    FSMCallExit = -10002,

    UICallEnter = -20001,
    UICallExit = -20002,
    UICallRelease = -20003,

    //rvo
    GET_KDTREE = -30001 ,
    SET_KDTREE_TREENODE =-30002,
    SET_LEFT_LEAF =-30003,
    SET_RIGHT_LEAF = -30004,
    GET_OBSTACLES = -30005,

    DO_MESH_2_OBS =-30006,
  
    DO_ADD_2_RVO =-30007,

    End =-1,

}
