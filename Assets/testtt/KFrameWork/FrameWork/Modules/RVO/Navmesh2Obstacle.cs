using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;

namespace KFrameWork
{
    public class Navmesh2Obstacle : UnityMonoBehaviour
    {
        private class intersectCls
        {
            public List<KInt2> points = new List<KInt2>();

            public Segment intersectseg;
        }

        public enum ExactType
        {
            /// <summary>
            /// normal polygons
            /// </summary>
            ZERO,
            /// <summary>
            /// it will remove some segment that cant combine a polygon
            /// </summary>
            ONE,
            /// <summary>
            /// it will remove some segment that cant combine a polygon and combine some segments
            /// </summary>
            TWO,
        }

        public enum DebugMode
        {
            None,
            ShowTips,
            ShowAll,
            ShowAndLog,
        }

        [Tooltip ("auto release some data")]
        public bool AutoRelease = true;
        [Tooltip ("draw gizmos")]
        public bool DrawGizmos = true;
        [Tooltip ("draw obstacle lines")]
        public bool Drawsegments = true;
        [Tooltip ("draw remove lines")]
        public bool DrawRmovelist = true;

        [Tooltip ("draw nearst triangles")]
        public bool DrawNearstTriangles = true;

        public bool processObstacles = true;

        public ExactType ExactMode = ExactType.ZERO;

        public DebugMode ObstacleDebug = DebugMode.None;

        public float Seekrange = 10;

        public int ErrorRadius = 10000;

        public float IntersectBias =0.1f;

        private Matrix4x4 MeshMatrix;

        private bool Inited = false;

        #region need to release

        public Mesh mesh;
        private TriangleTree tree;
        private List<Segment> segments = new List<Segment> ();
        private Dictionary<KInt2, List<NavmeshTriangle>> point2triangle = new Dictionary<KInt2, List<NavmeshTriangle>> ();
        private NavmeshTriangle[] ObsTriangles;


        #endregion

        #if UNITY_EDITOR
        public Vector2 targetPoint;
        private Vector2 lastpoint;

        private KInt3[] ObsVertices;
        private List<Segment> willremoveList = new List<Segment> ();
        private IList<NavmeshTriangle> SeekedTriangleList;
        #endif

        [FrameWorkStartAttribute]
        private static void Start (int p)
        {
            Navmesh2Obstacle[] obstacles = FindObjectsOfType<Navmesh2Obstacle> ();
            for (int i = 0; i < obstacles.Length; ++i) {
                if (obstacles [i].Inited == false && obstacles [i].enabled)
                    obstacles [i].UpdateObstacle ();
            }
        }

        void Update ()//OnEnable()
        {
            if (!Inited) {
               
                UpdateObstacle();
            }
        }

        [FrameWorkDeviceQuitAttribute]
        private static void Clear (int p)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearProgressBar ();
            #endif
        }

        [Script_SharpLogicAttribute((int)FrameWorkCmdDefine.DO_MESH_2_OBS)]
        static void DoMesh2Obs(AbstractParams p)
        {
            Navmesh2Obstacle script = p.ReadObject() as Navmesh2Obstacle;
            script.Mesh2Obstacle();
        }

        [Script_SharpLogicAttribute((int)FrameWorkCmdDefine.DO_ADD_2_RVO)]
        static void DoAddRvo(AbstractParams p)
        {
            Navmesh2Obstacle script = p.ReadObject() as Navmesh2Obstacle;
            script.Add2Rvo();
        }

        void BeginSample (string Key)
        {
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.BeginSample (Key);
#endif
        }

        void EndSample ()
        {
#if UNITY_EDITOR
            UnityEngine.Profiling.Profiler.EndSample ();
#endif
        }

        public void UpdateObstacle ()
        {
            if (MainLoop.getInstance () == null) {
                LogMgr.LogError ("please create kframework first");
                return;
            }

            if (mesh != null) {
                MeshMatrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, 1));
                try {
                    
                    Mesh2Obstacle ();
                    Add2Rvo ();

                    if (AutoRelease) {
                        this.mesh = null;
                    }
                    this.Inited = true;
                } catch (System.Exception ex) {
                    LogMgr.LogError (ex);
                    this.Inited = true;
                    ClearProgressBar ();
                }

            }
        }

        void Add2Rvo ()
        {
            BeginSample ("Add2Rvo");
            float firsttime = Time.realtimeSinceStartup;
            if (ExactMode == ExactType.ZERO) {
                BeginSample ("ZERO COST");
                int[] uses = new int[20];

                for (int i = 0; i < ObsTriangles.Length; i++) {
                    NavmeshTriangle node = ObsTriangles [i];

                    uses [0] = uses [1] = uses [2] = 0;

                    if (node != null) {
                        for (int j = 0; j < node.connections.Count; j++) {
                            NavmeshTriangle other = node.connections [j];
                            if (other != null) {
                                int a = node.SharedEdge (other);
                                if (a != -1)
                                    uses [a] = 1;
                            }
                        }

                        for (int j = 0; j < 3; j++) {
                            if (uses [j] == 0) {
                                var v1 = node.GetVertex (j);
                                var v2 = node.GetVertex ((j + 1) % 3);

                                Simulator.Instance.addObstacle (v1, v2);
                            }
                        }
                    }

                    ShowProgress ("插入障碍点", (float)i / (ObsTriangles.Length));
                }
                EndSample ();
            }


            if (ExactMode >= ExactType.ONE) {
                BeginSample ("Buildsegments");
                Buildsegments ();
                EndSample ();

                BeginSample ("RemoveUnused");
                RemoveUnused ();
                EndSample ();

                BeginSample ("PushSegmentstoObstacles");
                PushSegmentstoObstacles ();
                EndSample ();
            }

            ShowProgress ("准备构建ObstacleTree", 0);
            float time = Time.realtimeSinceStartup;
            BeginSample ("processObstacles");
            if(processObstacles)
                Simulator.Instance.processObstacles ();
            EndSample ();
            LogMgr.LogFormat ("Add2Rvo cost :{0}  processObstacles cost :{1} ", Time.realtimeSinceStartup - firsttime, Time.realtimeSinceStartup - time);
            ShowProgress ("构建ObstacleTree 完成", 1);
            this.ClearProgressBar ();
            EndSample ();

#if !UNITY_EDITOR

            ObsTriangles = null;
            segments = null;
            tree = null;
            point2triangle = null;
#else
            if (ObstacleDebug >= DebugMode.ShowAll && UnityEditor.EditorUtility.DisplayDialog ("tips", "export kdtree asset or not ?", "OK", "NO")) {
                string assetpath = UnityEditor.EditorUtility.SaveFilePanelInProject ("save", "kdtree", "asset", "please enter a filename");
                if (!string.IsNullOrEmpty (assetpath)) {
                    KdtreeAsset node = ScriptableObject.CreateInstance<KdtreeAsset> ();
                    ScriptCommand cmd = ScriptCommand.Create ((int)FrameWorkCmdDefine.GET_KDTREE);
                    cmd.Excute ();

                    ScriptCommand obscmd = ScriptCommand.Create ((int)FrameWorkCmdDefine.GET_OBSTACLES);
                    obscmd.Excute ();

                    KdTree tree = cmd.ReturnParams.ReadObject () as KdTree;
                    IList<Obstacle> obstacles = obscmd.ReturnParams.ReadObject () as IList<Obstacle>;
                    node.CreateKdtree (tree, obstacles);

                    cmd.Release (true);
                    obscmd.Release (true);

                    UnityEditor.AssetDatabase.CreateAsset (node, assetpath);
                }
            }
#endif
        }


        void SortAndAdd2Segments (Dictionary<Segment, intersectCls> intersectDic)
        {
            var en = intersectDic.GetEnumerator ();
            while (en.MoveNext ()) {
                Segment seg = en.Current.Key;
                List<KInt2> points = en.Current.Value.points;

                if (points.Count > 0) {
                    float distance = (seg.end - seg.start).sqrMagnitude;
                    points.Sort ((p1, p2) => {
                        float d1 = (p1 - seg.start).sqrMagnitude;
                        float d2 = (p2 - seg.start).sqrMagnitude;
                        if (d1 > d2) {
                            return 1;
                        } else if (d1 < d2) {
                            return -1;
                        }
                        return 0;
                    });

                    for (int i = 0; i < points.Count; ++i) {
                        if (i == 0) {
                            segments.Add (new Segment (seg.start, points [i]+(points[i] -seg.start).normalized * IntersectBias));

                        } else {
                            Segment newseg = new Segment (points [i - 1], points [i]+(points[i] -points[i-1]).normalized * IntersectBias);
                            segments.Add (newseg);

                        }
                    }

                    Segment endseg = new Segment (points [points.Count - 1], seg.end +(seg.end  -points [points.Count - 1]).normalized * IntersectBias);

                    segments.Add (endseg);

                }

            }
        }

        void PolygonIntersect ()
        {
            //相交检测
            BeginSample ("PolygonIntersect - SegmentIntersect");

            this.SortByXcoordinate(segments);

            List<Segment> removeList = new List<Segment> (segments.Count / 2);
            Dictionary<Segment, intersectCls> SegPointsdict = new Dictionary<Segment, intersectCls> (segments.Count / 2);
            Dictionary<KInt2,HashSet<Segment>> dic = new Dictionary<KInt2, HashSet<Segment>>();
            for (int i = 0; i < segments.Count; ++i) {
                Segment firstline = segments [i];
                for (int j = i + 1; j < segments.Count; ++j) {
                    Segment secondline = segments [j];

                    if(firstline.MinX > secondline.MaxX)
                        break;

                    long maxminy = KMath.Max(segments[i].MinY, segments[j].MinY);
                    long minmaxy = KMath.Min(segments[i].MaxY, segments[j].MaxY);
                    if (maxminy > minmaxy)
                        continue;

                    KInt2 result;
                    if (!firstline.isConnect (secondline) && Segment.Intersect (firstline.start, firstline.end, secondline.start, secondline.end, out result)) {

                        if(dic.ContainsKey(result))
                        {
                            dic[result].Add(firstline);
                            dic[result].Add(secondline);
                        }else
                        {
                            HashSet<Segment> hashdata = new HashSet<Segment>();
                            hashdata.Add(firstline);
                            hashdata.Add(secondline);
                            dic[result] =hashdata;
                        }

                        if (firstline.start != result && firstline.end != result) {
                            if (SegPointsdict.ContainsKey (firstline)) {
                                SegPointsdict [firstline].points.TryAdd (result);
                            } else {
                                intersectCls data = new intersectCls();
                                data.points.Add(result);
                                data.intersectseg = secondline;
                                SegPointsdict [firstline] = data;
        
                            }

                            removeList.Add (firstline);

                        }

                        if (secondline.start != result && secondline.end != result) {
                            if (SegPointsdict.ContainsKey (secondline)) {
                                SegPointsdict [secondline].points.TryAdd (result);
                            } else {
                                intersectCls data = new intersectCls();
                                data.points.Add(result);
                                data.intersectseg = firstline;
                                SegPointsdict [secondline] = data;
                            }
                            removeList.Add (secondline);
                        }

                        // LogMgr.LogError("first =>" + firstline.uid + " => " + secondline.uid);
                    }
                }

                ShowProgress ("相交检测", (float)i / segments.Count);
            }

            EndSample ();

            //remove old segments  即使有重复也无所谓，减少多次检查的开销
            for (int i = 0; i < removeList.Count; ++i) {
                segments.Remove (removeList [i]);
            }

            BeginSample ("PolygonIntersect - SortAndAdd2Segments");
            SortAndAdd2Segments (SegPointsdict);
            EndSample ();


            DefineIDforSegments (segments);
            //
            BeginSample ("Build - TriangleTree");
            tree = new TriangleTree ();
            tree.Build (ObsTriangles, 2);
            EndSample ();
            //
            List<NavmeshTriangle> testtriangles = new List<NavmeshTriangle> (10);
            List<KInt2> nearestidxs = new List<KInt2>(10);
            removeList.Clear ();

            BeginSample ("Build - Test Triangle");
            for (int i = 0; i < segments.Count; ++i) {
                Segment seg = segments [i];
                testtriangles.Clear ();
                nearestidxs.Clear();

                tree.ball_queryPoints ((seg.start + seg.end) / 2, Seekrange, nearestidxs);
                for (int k = 0; k < nearestidxs.Count; ++k) {
                    List<NavmeshTriangle> list = point2triangle [nearestidxs [k]];
                    for (int v = 0; v < list.Count; ++v) {
                        testtriangles.Add (list [v]);
                    }
                }

                ShowProgress ("获取邻近三角形", (float)i / segments.Count);

                TriangleCheck (testtriangles, dic, removeList, seg);
            }

            EndSample ();
            //
#if UNITY_EDITOR
            willremoveList.AddRange (removeList);
#endif
            for (int i = 0; i < removeList.Count; ++i) {
                segments.Remove (removeList [i]);
            }

        }

        void TriangleCheck (List<NavmeshTriangle> testtriangles, Dictionary<KInt2,HashSet<Segment>> IntersectCache, List<Segment> removeList, Segment TestSegment)
        {
            HashSet<Segment> hitCache = null;
            if (IntersectCache.ContainsKey (TestSegment.start)) {
                hitCache = IntersectCache [TestSegment.start];
            }

            if (IntersectCache.ContainsKey (TestSegment.end)) {
                HashSet<Segment> cache = IntersectCache [TestSegment.end];
                if(cache != null)
                {
                    if(hitCache == null)
                    {
                        hitCache = cache;
                    }
                    else
                    {
                        var en = cache.GetEnumerator();
                        while(en.MoveNext())
                        {
                            hitCache.Add(en.Current);
                        }
                    }
                }
            }
                
            for (int PassedTricnt = 0; PassedTricnt < testtriangles.Count; ++PassedTricnt) {
                NavmeshTriangle targettriangle = testtriangles [PassedTricnt];

                int insidecnt = 0;
                int hitcnt = 0;
                int concnt = 0;

                KInt2 result;

                for (int j = 0; j < 3; ++j) {
                    Segment side = targettriangle.GetSegment(j);
                    bool h1 =hitCache != null && hitCache.Contains (side);
                    bool h2 = Segment.Intersect (TestSegment.start, TestSegment.end, side.start, side.end, out result);
                    if (h1 || h2) {
                        hitcnt++;
                    }

                    if (side.ContainsSegment (TestSegment)) {
                        insidecnt++;
                    }

                    if (TestSegment.isConnect (side)) {
                        concnt++;
                    }
                }

                if (insidecnt == 0 && concnt == 0) {
                    int b1 = isInside (targettriangle.v03xz, targettriangle.v13xz, targettriangle.v23xz, TestSegment.start, ErrorRadius);
                    int b2 = isInside (targettriangle.v03xz, targettriangle.v13xz, targettriangle.v23xz, TestSegment.end, ErrorRadius);
                    if (b1 == 2 && b2 == 2) {
                        removeList.Add (TestSegment);
                        break;
                    } else if ( (b1 == 1 && b2 == 2) || (b1 == 2 && b2 == 1)) {
                        removeList.Add (TestSegment);
                        break;
                    }  else if ((b1 <=1 &&  b2 <= 1) && hitcnt == 2) {
                        removeList.Add (TestSegment);
                        break;
                    }
                    else if ((b1 == 2 || b2 == 2) && hitcnt > 0) {
                        removeList.Add (TestSegment);
                        break;
                    }
                }

                ShowProgress ("三角相交检测 移除相交线段", (float)PassedTricnt / testtriangles.Count);
            }
        }

        long doublearea (KInt2 p1, KInt2 p2, KInt2 p3)
        {
            return KMath.Abs ((p1.IntX * (p2.IntY - p3.IntY) + p2.IntX * (p3.IntY - p1.IntY) + p3.IntX * (p1.IntY - p2.IntY)));
        }
            
        int isInside (KInt2 p1, KInt2 p2, KInt2 p3, KInt2 target, int radius)
        {
            long A = doublearea (p1, p2, p3);
            long A1 = doublearea (target, p2, p3);
            long A2 = doublearea (p1, target, p3);
            long A3 = doublearea (p1, p2, target);
            long delta = A - (A1 + A2 + A3);
            if(delta == 0)
            {
                return 1;
            }

            if( delta < radius && delta > -radius)
            {
                return 2;
            }
            return -1;
        }

        void DefineIDforSegments (List<Segment> targetList)
        {
            #if UNITY_EDITOR
            for (int i = 0; i < targetList.Count; ++i) {
                Segment seg = targetList [i];
                seg.uid = i;
                targetList [i] = seg;
            }
            #endif
        }

        void TriangleTreeBuildProgress (float value)
        {
            this.ShowProgress ("构建三角树进度", value);
        }

        void Buildsegments ()
        {
            segments.Clear ();

            for (int i = 0; i < ObsTriangles.Length; i++) {
                NavmeshTriangle node = ObsTriangles [i];
                for (int j = 0; j < node.shared.Count; ++j) 
                {
                    if(node.shared[j] == false)
                    {
                        segments.Add(node.GetSegment(j));
                    }
                }

                ShowProgress ("构建单边列表", (float)i / (ObsTriangles.Length));
            }

            DefineIDforSegments (segments);
        }

        void PushSegmentstoObstacles ()
        {
            for (int i = 0; i < segments.Count; ++i) {
                Simulator.Instance.addObstacle (segments [i].start, segments [i].end);
            }

        }

        void SortByXcoordinate (List<Segment> container)
        {
            container.Sort ((a, b) => {
                return (int)(b.MaxX -a.MaxX);
            });
        }

        void RemoveUnused ()
        {
            //sort
            BeginSample ("SortByXcoordinate");
            SortByXcoordinate(segments);
            EndSample();
            BeginSample ("connectlines check");
            HashSet<KInt2> allpoints = new HashSet<KInt2> ();
            Dictionary<KInt2, HashSet<KInt2>> connectlines = new Dictionary<KInt2, HashSet<KInt2>>(segments.Count);
            for (int i = 0; i < segments.Count; ++i) {
                Segment seg = segments [i];
                if (allpoints.Add (seg.start)) {
                    connectlines [seg.start] = new HashSet<KInt2> ();
                }

                if (allpoints.Add (seg.end)) {
                    connectlines [seg.end] = new HashSet<KInt2> ();
                }
            }


            KInt2 p1, p2, p3;
            for (int i = 0; i < segments.Count; ++i) {
                Segment firstline = segments [i];
                for (int j = i + 1; j < segments.Count; ++j) {

                    Segment secondline = segments [j];
                    if(firstline.MinX > secondline.MaxX)
                        break;

                    long maxminy = KMath.Max(firstline.MinY, secondline.MinY);
                    long minmaxy = KMath.Min(firstline.MaxY, secondline.MaxY);
                    if (maxminy > minmaxy)
                        continue;

                    if (Segment.isConnectRetPoints (firstline, secondline, out p1, out p2, out p3)) {
                        //only three point

                        connectlines [p1].Add (p2);
                        connectlines [p1].Add (p3);
                    }
                }

                ShowProgress ("创建链接点", (float)i / segments.Count);
            }

            EndSample ();
            this.segments.Clear ();

            HashSet<Segment> realList = new HashSet<Segment> ();
            List<KInt2> linequeue = new List<KInt2> (segments.Count);
            HashSet<KInt2> openlist = new HashSet<KInt2> ();
            HashSet<KInt2> enabledlist = new HashSet<KInt2> ();

            BeginSample ("combine polygon check");
            int progress = 0;
            var pointen = allpoints.GetEnumerator ();
            while (pointen.MoveNext ()) {
                KInt2 pt = pointen.Current;

                if (enabledlist.Contains (pt) || connectlines [pt].Count < 2) {
                    continue;
                }
                openlist.Clear ();
                linequeue.Clear ();

                linequeue.Add (pt);
                if (TryConnectUntilFindTarget (pt, pt, linequeue, openlist, connectlines)) {
                    if (!isAllLine (linequeue)) {
                        AddtoList (realList, linequeue);
                        for (int k = 0; k < linequeue.Count; ++k) {
                            enabledlist.Add (linequeue [k]);
                        }
                    } else {
#if UNITY_EDITOR
                        if (ObstacleDebug >= DebugMode.ShowAndLog) {
                            LogMgr.LogFormat ("准备剔除 原始目标点 为 :{0}", pt);
                            for (int k = 0; k < linequeue.Count; ++k) {
                                LogMgr.LogFormat ("准备剔除 :{0}", linequeue [k]);
                            }
                            LogMgr.Log ("剔除结束");
                        }

#endif
                    }
                }
                progress++;
                ShowProgress ("检测非多边形点", (float)progress / allpoints.Count);
            }
            EndSample ();

            segments.Capacity = realList.Count;

            var realen = realList.GetEnumerator ();
            while (realen.MoveNext ()) {
                segments.Add (realen.Current);
            }

            if (ExactMode >= ExactType.TWO) {
                BeginSample ("PolygonIntersect");
                PolygonIntersect ();
                EndSample ();
            }

        }

        void AddtoList (HashSet<Segment> container, List<KInt2> linequeue)
        {
            if (linequeue.Count > 2) {
                container.Add (new Segment (linequeue [0], linequeue [1]));
                for (int i = 1; i < linequeue.Count - 1; ++i) {
                    KInt2 current = linequeue [i];
                    KInt2 next = linequeue [i + 1];
                    container.Add (new Segment (current, next));
                }
            }
        }

        bool isAllLine (List<KInt2> linequeue)
        {
            if (linequeue.Count <= 2) {
                return true;
            }

            KInt2 firstdir = linequeue [1] - linequeue [0];

            for (int i = 1; i < linequeue.Count - 1; ++i) {
                KInt2 current = linequeue [i];
                KInt2 next = linequeue [i + 1];
                KInt det = RVOMath.det (next - current, firstdir);
                if (det != 0) {
                    return false;
                }
            }
            return true;
        }

        bool TryConnectUntilFindTarget (KInt2 current, KInt2 target, List<KInt2> linequeue, HashSet<KInt2> openlist, Dictionary<KInt2, HashSet<KInt2>> connectlines)
        {
            openlist.Add (current);

            HashSet<KInt2> conncects = connectlines [current];
            if (conncects.Count > 0) {
                var en = conncects.GetEnumerator ();
                while (en.MoveNext ()) {
                    KInt2 pt = en.Current;
                    if (linequeue.Count > 1) {
                        if (pt.Equals (target)) {
                            linequeue.Add (target);
                            return true;
                        }

                        if (openlist.Contains (pt)) {
                            continue;
                        }
                    }

                    linequeue.Add (pt);
                    if (TryConnectUntilFindTarget (pt, target, linequeue, openlist, connectlines)) {
                        return true;
                    } else {
                        linequeue.Remove (pt);
                    }
                }

            }
            return false;
        }

        bool Mesh2Obstacle ()
        {
            
            if (mesh.vertexCount == 0 || mesh.triangles.Length == 0) {
                LogMgr.LogError ("Convert Error");
                return false;
            }
            BeginSample ("Mesh2Obstacle");
            //old
            Vector3[] vectorVertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            //create new
            KInt3[] vertices = new KInt3[mesh.vertexCount];

            Dictionary<KInt3, int> hashedVerts = new Dictionary<KInt3, int> (mesh.vertexCount);
            int[] newVertices = new int[vertices.Length];

            //从模型坐标系转换到世界坐标系
            for (int i = 0; i < vertices.Length; i++) {
                vertices [i] = (KInt3)MeshMatrix.MultiplyPoint3x4 (vectorVertices [i]);
                ShowProgress ("从模型坐标系转换到世界坐标系", (float)i / vertices.Length);
            }
            //旧顶点在新顶点中的索引
            int newVerIdx = 0;

            for (int i = 0; i < vertices.Length; i++) {
                //当hash 顶点集合不包含这个顶点的时候，把这个点加入进去,去除共点
                if (!hashedVerts.ContainsKey (vertices [i])) {
                    newVertices [newVerIdx] = i;
                    hashedVerts.Add (vertices [i], newVerIdx);
                    newVerIdx++;
                }
                ShowProgress ("构建映射", (float)i / vertices.Length);
            }

            for (int x = 0; x < triangles.Length; x++) {
                //把老的顶点索引映射到新的顶点索引上
                KInt3 vertex = vertices [triangles [x]];

                triangles [x] = hashedVerts [vertex];
                ShowProgress ("三角映射", (float)x / triangles.Length);
            }

            KInt3[] totalIntVertices = vertices;
            vertices = new KInt3[newVerIdx];
            for (int i = 0; i < newVerIdx; i++) {
                vertices [i] = totalIntVertices [newVertices [i]];
                ShowProgress ("顶点索引转换", (float)i / newVerIdx);
            }
            //顶点创建结束
#if UNITY_EDITOR
            this.ObsVertices = vertices;
#endif
            if (triangles.Length % 3 != 0) {
                LogMgr.LogErrorFormat ("triangle lenth error :{0}", triangles.Length);
            }

            //创建三角
            ObsTriangles = new NavmeshTriangle[triangles.Length / 3];

            for (int i = 0; i < ObsTriangles.Length; i++) {
                NavmeshTriangle tri = new NavmeshTriangle ();
                ObsTriangles [i] = tri;
#if UNITY_EDITOR
                //init
                tri.uid = i;
#endif
                //
                tri.v0 = triangles [i * 3];
                tri.v1 = triangles [i * 3 + 1];
                tri.v2 = triangles [i * 3 + 2];

                if (RVOMath.IsClockwiseXZ (vertices [tri.v0], vertices [tri.v1], vertices [tri.v2]) == false) {
                    int tmp = tri.v0;
                    tri.v0 = tri.v2;
                    tri.v2 = tmp;
                }

                //finish position
                tri.v03 = vertices [tri.v0];
                tri.v13 = vertices [tri.v1];
                tri.v23 = vertices [tri.v2];

                tri.v03xz = new KInt2 (tri.v03.x, tri.v03.z);
                tri.v13xz = new KInt2 (tri.v13.x, tri.v13.z);
                tri.v23xz = new KInt2 (tri.v23.x, tri.v23.z);

                tri.averagePos = (tri.v03 + tri.v13 + tri.v23) / 3;

                if (this.ExactMode >= ExactType.TWO) {
                    tri.xzPos = new KInt2 (tri.averagePos.x, tri.averagePos.z);

                    if (point2triangle.ContainsKey (tri.xzPos)) {
                        point2triangle [tri.xzPos].Add (tri);
                    } else {
                        point2triangle [tri.xzPos] = new List<NavmeshTriangle> ();
                        point2triangle [tri.xzPos].Add (tri);
                    }
                }

                ShowProgress ("构建三角形", (float)i / ObsTriangles.Length);
            }

            Dictionary<KInt2, NavmeshTriangle> sides = new Dictionary<KInt2, NavmeshTriangle> (triangles.Length * 3);

            //创建三角形的边
            for (int i = 0, j = 0; i < triangles.Length; j += 1, i += 3) {
                sides [KInt2.ToInt2 (triangles [i + 0], triangles [i + 1])] = ObsTriangles [j];
                sides [KInt2.ToInt2 (triangles [i + 1], triangles [i + 2])] = ObsTriangles [j];
                sides [KInt2.ToInt2 (triangles [i + 2], triangles [i + 0])] = ObsTriangles [j];
            }

            HashSet<NavmeshTriangle> connections = new HashSet<NavmeshTriangle> ();

            for (int i = 0, j = 0; i < triangles.Length; j += 1, i += 3) {
                connections.Clear ();

                NavmeshTriangle node = ObsTriangles [j];

                for (int q = 0; q < 3; q++) {
                    NavmeshTriangle other;
                    //如果是mesh中的边，则加入
                    if (sides.TryGetValue (KInt2.ToInt2 (triangles [i + ((q + 1) % 3)], triangles [i + q]), out other)) {
                        connections.Add (other);
                    }
                }
                //拷贝当前的一份连接点，而不是赋值引用过去
                node.connections = new List<NavmeshTriangle> ();
                node.connections .AddRange(connections);
  
                node.CreateSharedInfo();
                ShowProgress ("构建连接点", (float)i / (triangles.Length / 3));
            }
            ClearProgressBar();
            EndSample ();

            return true;
        }

        void ShowProgress (string label, float value)
        {
#if UNITY_EDITOR
            if (ObstacleDebug >= DebugMode.ShowTips)
                UnityEditor.EditorUtility.DisplayProgressBar ("提示", label, value);
#endif
        }

        void ClearProgressBar ()
        {
#if UNITY_EDITOR
            if (ObstacleDebug >= DebugMode.ShowTips)
                UnityEditor.EditorUtility.ClearProgressBar ();
#endif
        }

        #if UNITY_EDITOR

        void OnDrawGizmos ()
        {
            if (ObsTriangles == null || !DrawGizmos)
                return;

            bool show = !Drawsegments && !DrawRmovelist && !DrawNearstTriangles && DrawGizmos;

            if (DrawRmovelist) {
                for (int i = 0; i < willremoveList.Count; ++i) {
                    Gizmos.color = Color.red;
                    Segment seg = willremoveList [i];
                    KInt3 start = new KInt3 (seg.start.x, 0, seg.start.y);
                    KInt3 end = new KInt3 (seg.end.x, 0, seg.end.y);
                    Gizmos.DrawLine (start, end);
                    UnityEditor.Handles.Label ((start + end) / 2, "序号 [" + seg.uid + "]");
                }
            }

            if (Drawsegments) {
                for (int i = 0; i < segments.Count; ++i) {
                    Gizmos.color = (i % 2 == 0) ? Color.green : Color.blue;
                    Segment seg = segments [i];
                    KInt3 start = new KInt3 (seg.start.x, 0, seg.start.y);
                    KInt3 end = new KInt3 (seg.end.x, 0, seg.end.y);
                    Gizmos.DrawLine (start, end);
                    UnityEditor.Handles.Label ((start + end) / 2, "序号 [" + seg.uid + "]");
                }
            }

            if (DrawNearstTriangles) {
                if (tree != null && lastpoint != targetPoint) {
                    if (SeekedTriangleList == null) {
                        SeekedTriangleList = new List<NavmeshTriangle> ();
                    }

                    SeekedTriangleList.Clear ();

                    List<KInt2> startidxs = ListPool.TrySpawn<List<KInt2>> ();
                    tree.ball_queryPoints ((KInt2)targetPoint, Seekrange, startidxs);
                    for (int k = 0; k < startidxs.Count; ++k) {
                        var trianglelist = point2triangle [startidxs [k]];
                        for (int v = 0; v < trianglelist.Count; ++v) {
                            SeekedTriangleList.Add (trianglelist [v]);
                        }
                    }
                    ListPool.TryDespawn (startidxs);
                    lastpoint = targetPoint;
                }

                Gizmos.DrawWireCube (new Vector3 (targetPoint.x, 0, targetPoint.y), Vector3.one);

                if (SeekedTriangleList != null) {
                    for (int i = 0; i < SeekedTriangleList.Count; i++) {
                        NavmeshTriangle node = SeekedTriangleList [i];
                        Gizmos.color = Color.cyan;
                        for (int q = 0; q < node.connections.Count; q++) {
                            Gizmos.DrawLine (node.averagePos, Vector3.Lerp (node.averagePos, node.connections [q].averagePos, 0.45f));
                        }

                        Gizmos.color = Color.black;
                        Gizmos.DrawLine (ObsVertices [node.v0], ObsVertices [node.v1]);
                        Gizmos.DrawLine (ObsVertices [node.v1], ObsVertices [node.v2]);
                        Gizmos.DrawLine (ObsVertices [node.v2], ObsVertices [node.v0]);

                        UnityEditor.Handles.Label (node.averagePos, "序号 [" + node.uid + "]");
                    }
                }
            }

            if (show) {
                for (int i = 0; i < ObsTriangles.Length; i++) {
                    NavmeshTriangle node = ObsTriangles [i];
                    Gizmos.color = Color.cyan;
                    for (int q = 0; q < node.connections.Count; q++) {
                        Gizmos.DrawLine (node.averagePos, Vector3.Lerp (node.averagePos, node.connections [q].averagePos, 0.45f));
                    }

                    Gizmos.color = Color.black;
                    Gizmos.DrawLine (ObsVertices [node.v0], ObsVertices [node.v1]);
                    Gizmos.DrawLine (ObsVertices [node.v1], ObsVertices [node.v2]);
                    Gizmos.DrawLine (ObsVertices [node.v2], ObsVertices [node.v0]);

                }
            }
        }
        #endif
    }
}


