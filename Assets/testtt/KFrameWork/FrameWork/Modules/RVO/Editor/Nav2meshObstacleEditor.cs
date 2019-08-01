using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace KFrameWork
{
    [CustomEditor(typeof(Navmesh2Obstacle))]
    public class Nav2meshObstacleEditor : Editor {

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();
            Navmesh2Obstacle script = target as Navmesh2Obstacle;
            if(GUILayout.Button("Do Mesh 2 Obstacle"))
            {
                ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.DO_MESH_2_OBS);
                cmd.CallParams.WriteObject(script);
                cmd.ExcuteAndRelease();
            }

            if(GUILayout.Button("Add to rvo"))
            {
                ScriptCommand cmd = ScriptCommand.Create((int)FrameWorkCmdDefine.DO_ADD_2_RVO);
                cmd.CallParams.WriteObject(script);
                cmd.ExcuteAndRelease();
            }
        }
    }

}

