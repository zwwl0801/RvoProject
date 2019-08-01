using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO;
using System;
using KUtils;

namespace KFrameWork
{
    public class GameKdTreeNode
    {
        public float key = -1;     // the key (value along k-th dimension) of the split
        public int LIdx = -1;       // the index to the left cell  (-1 if none)
        public int RIdx = -1;       // the index to the right cell (-1 if none)

        /**
         * A poiter back to the structure data of input points, 
         * but ONLY if the node is a LEAF, otherwise value is (-1)
         */
        public int pIdx = -1;

        public bool isLeaf()
        {
            return pIdx >= 0;
        }

    }


    internal class TriangleTree
    {
         List<KInt2> points;    // Points data
         List<GameKdTreeNode> nodesPtrs = new List<GameKdTreeNode>(); // Memory to keep nodes
         int ndim;                // Data dimensionality
         int npoints;             // Number of points
         List<int> workarray = new List<int>(); // Used in tree construction

        // Data used ONLY for "kNN" search (k-nearest neighbors)
         KInt2 Bmin;              // bounding box lower bound
         KInt2 Bmax;              // bounding box upper bound
         MaxHeap<float> pq;      // <key,idx> = <distance, node idx>
         int k;                   // number of records to search for

        /// Heapsort algorithm used by fast KDtree construction
        // Note: this is copied almost verbatim from the heapsort 
        // wiki page: http://en.wikipedia.org/wiki/Heapsort 
        // 11/9/05
        // there was a bug for len==2 though!
        void heapsort(int dim, List<int> idx, int len)
        {
            int n = len;
            int i = len / 2;
            int parent, child;
            int t;

            for (;;)
            {
                if (i > 0)
                {
                    i--;
                    t = idx[i];
                }
                else
                {
                    n--;
                    if (n == 0)
                        break;
                    t = idx[n];
                    idx[n] = idx[0];
                }

                parent = i;
                child = i * 2 + 1;

                while (child < n)
                {
                    if ((child + 1 < n) && (points[idx[child + 1]][dim] > points[idx[child]][dim]))
                    {
                        child++;
                    }
                    if (points[idx[child]][dim] > points[t][dim])
                    {
                        idx[parent] = idx[child];
                        parent = child;
                        child = parent * 2 + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                idx[parent] = t;
            } // end of for loop

        } // end of heapsort

        public KInt2 Mapidx2Point(int idx)
        {
            if (idx < 0 || idx >= points.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return points[idx];
        }

        void Reset(List<int> list, int cnt, int val)
        {
            list.Clear();
            list.Capacity = cnt;
            for (int i = 0; i < cnt; ++i)
            {
                list.Add(val);
            }
        }

        public void Build(IList<NavmeshTriangle> triangles,int dimdata)
        {
            // initialize data
            npoints = triangles.Count;
            ndim = dimdata;

            if(points == null)
            {
                points = new List<KInt2>(triangles.Count);
            }
            else
                points.Clear();
            
            for(int i =0; i < triangles.Count;++i)
            {
                points.Add(triangles[i].xzPos);
            }

            nodesPtrs.Capacity = npoints;
            Reset(workarray, npoints, -1);

            // create the heap structure to support the tree creation
            List<MinHeap<float>> heaps = new List<MinHeap<float>>();
            for (int i = 0; i < ndim; i++)
            {
                heaps.Add(new MinHeap<float>(npoints));
            }
            for (int dIdx = 0; dIdx < ndim; dIdx++)
                for (int pIdx = 0; pIdx < npoints; pIdx++)
                    heaps[dIdx].push(points[pIdx][dIdx], pIdx);

            // Invoke heap sort generating indexing vectors
            // indexes[dim][i]: in dimension dim, which is the index of the i-th smallest element?
            List<List<int>> indexes = new List<List<int>>();//(ndim, vector<int>(npoints,0) );
            for (int i = 0; i < ndim; i++)
            {
                List<int> list = new List<int>(npoints);
                Reset(list, npoints, 0);
                indexes.Add(list);
            }
            for (int dIdx = 0; dIdx < ndim; dIdx++)
                heaps[dIdx].heapsort(indexes[dIdx]);

            // Invert the indexing structure!! 
            // srtidx[dim][j]: in dimension dim, which is ordering number of point j-th?
            // the j-th smallest entry in dim dimension
            List<List<int>> srtidx = new List<List<int>>();
            for (int i = 0; i < ndim; i++)
            {
                List<int> list = new List<int>(npoints);
                Reset(list, npoints, 0);
                srtidx.Add(list);
            }

            for (int dim = 0; dim < indexes.Count; dim++)
                for (int i = 0; i < indexes[dim].Count; i++)
                    srtidx[dim][indexes[dim][i]] = i;

            // First partition is on every single point ([1:npoints])
            List<int> pidx = ListPool.TrySpawn<List<int>>();
            pidx.Capacity = npoints;

            for (int i = 0; i < npoints; i++) pidx.Add(i);

            build_recursively(srtidx, pidx, 0);

        }

        public void Build(List<KInt2> newpoints,int dimdata)
        {
            // initialize data
            npoints = newpoints.Count;
            ndim = dimdata;
            points = newpoints;
            nodesPtrs.Capacity = npoints;
            Reset(workarray, npoints, -1);

            // create the heap structure to support the tree creation
            List<MinHeap<float>> heaps = new List<MinHeap<float>>();
            for (int i = 0; i < ndim; i++)
            {
                heaps.Add(new MinHeap<float>(npoints));
            }
            for (int dIdx = 0; dIdx < ndim; dIdx++)
                for (int pIdx = 0; pIdx < npoints; pIdx++)
                    heaps[dIdx].push(points[pIdx][dIdx], pIdx);

            // Invoke heap sort generating indexing vectors
            // indexes[dim][i]: in dimension dim, which is the index of the i-th smallest element?
            List<List<int>> indexes = new List<List<int>>();//(ndim, vector<int>(npoints,0) );
            for (int i = 0; i < ndim; i++)
            {
                List<int> list = new List<int>(npoints);
                Reset(list, npoints, 0);
                indexes.Add(list);
            }
            for (int dIdx = 0; dIdx < ndim; dIdx++)
                heaps[dIdx].heapsort(indexes[dIdx]);

            // Invert the indexing structure!! 
            // srtidx[dim][j]: in dimension dim, which is ordering number of point j-th?
            // the j-th smallest entry in dim dimension
            List<List<int>> srtidx = new List<List<int>>();
            for (int i = 0; i < ndim; i++)
            {
                List<int> list = new List<int>(npoints);
                Reset(list, npoints, 0);
                srtidx.Add(list);
            }

            for (int dim = 0; dim < indexes.Count; dim++)
                for (int i = 0; i < indexes[dim].Count; i++)
                    srtidx[dim][indexes[dim][i]] = i;



            // First partition is on every single point ([1:npoints])
            List<int> pidx = ListPool.TrySpawn<List<int>>();
            pidx.Capacity = npoints;

            for (int i = 0; i < npoints; i++) pidx.Add(i);

            build_recursively(srtidx, pidx, 0);

        }

        void Copy(List<int> self, List<int> other, int selfstart, int selfend, int otheridx)
        {
            int tempidx = otheridx;
            for (int i = selfstart; i < selfend; ++i)
            {
                if (tempidx >= other.Count)
                {
                    LogMgr.LogError(tempidx + " " + other.Count);
                }
                other[tempidx] = self[i];
                tempidx++;
            }
           
        }

        int build_recursively(List<List<int>> sortidx, List<int> pidx, int dim)
        {
            GameKdTreeNode node = null;
            // Stop condition
            if (pidx.Count == 1)
            {
                node = new GameKdTreeNode();        // create a new node
                int gamenodeidx = nodesPtrs.Count; // its address is
                nodesPtrs.Add(node);  // important to push back here
                node.LIdx = -1;                // no child
                node.RIdx = -1;                // no child
                node.pIdx = pidx[0];           // the only index available
                node.key = 0;                  // key is useless here
                return gamenodeidx;
            }

            // allocate the vectors
            List<int> Larray = ListPool.TrySpawn<List<int>>();
            List<int> Rarray = ListPool.TrySpawn<List<int>>();

            // initialize the "partition" array
            // Setting parray to -1 indicates we are not using the point
            for (int i = 0; i < npoints; i++)
                workarray[i] = -1;
            for (int i = 0; i < pidx.Count; i++)
                workarray[sortidx[dim][pidx[i]]] = pidx[i];


            int pivot = -1; //index of the median element
            if (pidx.Count * Math.Log(pidx.Count) < npoints)
            {
                Larray.Resize(pidx.Count / 2 + (pidx.Count % 2 == 0 ? 0 : 1), -1);
                Rarray.Resize(pidx.Count / 2, -1);
                heapsort(dim, pidx, pidx.Count);
                Copy(pidx, Larray, 0, Larray.Count, 0);
                Copy(pidx, Rarray, Larray.Count, pidx.Count, 0);
                pivot = pidx[(pidx.Count - 1) / 2];
            }
            else
            {
                // The middle valid value of parray is the pivot,
                // the left go to a node on the left, the right
                // and the pivot go to a node on the right.
                int TH = (pidx.Count - 1) / 2; //defines median offset
                int cnt = 0; //number of points found
                for (int i = 0; i < npoints; i++)
                {
                    // Is the current point not in the current selection? skip
                    if (workarray[i] == -1)
                        continue;

                    // len/2 is on the "right" of pivot. 
                    // Pivot is still put on the left side
                    if (cnt == TH)
                    {
                        pivot = workarray[i];
                        Larray.Add(workarray[i]);
                    }
                    else if (cnt > TH)
                        Rarray.Add(workarray[i]);
                    else
                        Larray.Add(workarray[i]);

                    // Don't overwork, if we already read all the necessary just stop.
                    cnt++;
                    if (cnt > pidx.Count)
                        break;
                }
            }
            // CREATE THE NODE
            node = new GameKdTreeNode();
            int nodeIdx = nodesPtrs.Count; //why it's not +1? should not happend after push back? -> no since size() is the index of last element+1!!
            nodesPtrs.Add(node); //important to push back here
            node.pIdx = -1; //not a leaf
            node.key = points[pivot][dim];

            node.LIdx = build_recursively(sortidx, Larray, (dim + 1) % ndim);
            ListPool.TryDespawn(Larray);

            node.RIdx = build_recursively(sortidx, Rarray, (dim + 1) % ndim);
            ListPool.TryDespawn(Rarray);
            return nodeIdx;
        }

        public void ball_queryPoints(KInt2 point, float radius, List<KInt2> idxsInRange)
        {
            if (nodesPtrs.Count > 0)
            {
                // create pmin pmax that bound the sphere
                KInt2 pmin = new KInt2(point.x - radius, point.y - radius);
                KInt2 pmax = new KInt2(point.x + radius, point.y + radius);

                List<float> distances = ListPool.TrySpawn<List<float>>();
                // start from root at zero-th dimension
                ball_bbox_query(0, pmin, pmax, idxsInRange,distances, point, radius * radius, 0);

                ListPool.TryDespawn(distances);
            }

        }

        public void ball_query(KInt2 point, float radius, List<int> idxsInRange, List<float> distances)
        {
            if (nodesPtrs.Count > 0)
            {
                // create pmin pmax that bound the sphere
                KInt2 pmin = new KInt2(point.x - radius, point.y - radius);
                KInt2 pmax = new KInt2(point.x + radius, point.y + radius);
                // start from root at zero-th dimension
                ball_bbox_query(0, pmin, pmax, idxsInRange, distances, point, radius * radius, 0);
            }

        }

        void ball_bbox_query(int nodeIdx, KInt2 pmin, KInt2 pmax, List<KInt2> inrange_idxs,List<float> distances,  KInt2 point, float radiusSquared, int dim = 0)
        {
            GameKdTreeNode node = nodesPtrs[nodeIdx];

            // if it's a leaf and it lies in R
            if (node.isLeaf())
            {
                float distance = (points[node.pIdx] - point).sqrMagnitude;
                if (distance <= radiusSquared)
                {
                    bool insert = false;
                    for(int i=0; i < distances.Count;++i)
                    {
                        if(distance < distances[i])
                        {
                            insert = true;
                            distances.Insert(i,distance);
                            inrange_idxs.Insert(i,this.points[node.pIdx]);
                            break;
                        }
                    }

                    if(!insert)
                    {
                        distances.Add(distance);
                        inrange_idxs.Add(this.points[node.pIdx]);
                    }
                    return;
                }
            }
            else
            {
                if (node.key >= pmin[dim] && node.LIdx != -1)
                    ball_bbox_query(node.LIdx, pmin, pmax, inrange_idxs,distances, point, radiusSquared, (dim + 1) % ndim);
                if (node.key <= pmax[dim] && node.RIdx != -1)
                    ball_bbox_query(node.RIdx, pmin, pmax, inrange_idxs,distances, point, radiusSquared, (dim + 1) % ndim);
            }
        }

        /** @see ball_query, range_query
        *
        * Returns all the points withing the ball bounding box and their distances
        *
        * @note this is similar to "range_query" i just replaced "lies_in_range" with "euclidean_distance"
        */
        void ball_bbox_query(int nodeIdx, KInt2 pmin, KInt2 pmax, List<int> inrange_idxs, List<float> distances, KInt2 point, float radiusSquared, int dim = 0)
        {
            GameKdTreeNode node = nodesPtrs[nodeIdx];

            // if it's a leaf and it lies in R
            if (node.isLeaf())
            {
                float distance = (points[node.pIdx] - point).sqrMagnitude;
                if (distance <= radiusSquared)
                {
                    inrange_idxs.Add(node.pIdx);
                    if(distances != null)
                        distances.Add(distance);
                    return;
                }
            }
            else
            {
                if (node.key >= pmin[dim] && node.LIdx != -1)
                    ball_bbox_query(node.LIdx, pmin, pmax, inrange_idxs, distances, point, radiusSquared, (dim + 1) % ndim);
                if (node.key <= pmax[dim] && node.RIdx != -1)
                    ball_bbox_query(node.RIdx, pmin, pmax, inrange_idxs, distances, point, radiusSquared, (dim + 1) % ndim);
            }
        }

        void k_closest_points(KInt2 Xq, int cnt, List<int> idxs)
        {
            // initialize search data
            Bmin = KInt2.min;
            Bmax = KInt2.max;

            this.k = cnt;

            // call search on the root [0] fill the queue
            // with elements from the search
            knn_search(Xq);

            // scan the created pq and extract the first "k" elements
            // pop the remaining
            int N = pq.size();
            for (int i = 0; i < N; i++)
            {
                ClsTuple<float, int> topel = pq.top();
                pq.pop();
                if (i >= N - k)
                    idxs.Add(topel.Value);
            }

            // invert the vector, passing first closest results
            idxs.Reverse();
        }

        void knn_search(KInt2 Xq, int nodeIdx = 0, int dim = 0)
        {
            // cout << "at node: " << nodeIdx << endl;
            GameKdTreeNode node = nodesPtrs[nodeIdx];
            float temp;

            // We are in LEAF
            if (node.isLeaf())
            {
                float distance = (Xq - points[node.pIdx]).sqrMagnitude;

                // pqsize is at maximum size k, if overflow and current record is closer
                // pop further and insert the new one
                if (pq.size() == k && pq.top().Key > distance)
                {
                    pq.pop(); // remove farther record
                    pq.push(distance, node.pIdx); //push new one
                }
                else if (pq.size() < k)
                    pq.push(distance, node.pIdx);

                return;
            }

            ////// Explore the sons //////
            // recurse on closer son
            if (Xq[dim] <= node.key)
            {
                temp = Bmax[dim]; Bmax[dim] = node.key;

                knn_search(Xq, node.LIdx, (dim + 1) % ndim);
                Bmax[dim] = temp;
            }
            else
            {
                temp = Bmin[dim]; Bmin[dim] = node.key;

                knn_search(Xq, node.RIdx, (dim + 1) % ndim);
                Bmin[dim] = temp;
            }
            // recurse on farther son
            if (Xq[dim] <= node.key)
            {
                temp = Bmin[dim]; Bmin[dim] = node.key;
                if (bounds_overlap_ball(Xq))

                    knn_search(Xq, node.RIdx, (dim + 1) % ndim);
                Bmin[dim] = temp;
            }
            else
            {
                temp = Bmax[dim]; Bmax[dim] = node.key;
                if (bounds_overlap_ball(Xq))

                    knn_search(Xq, node.LIdx, (dim + 1) % ndim);
                Bmax[dim] = temp;
            }
        }

        bool bounds_overlap_ball(KInt2 Xq)
        {
            // k-closest still not found. termination test unavailable
            if (pq.size() < k)
                return true;

            float sum = 0;
            //extract best distance from queue top
            float best_dist_sq = pq.top().Key;
            // cout << "current best dist: " << best_dist_sq << endl;
            for (int d = 0; d < ndim; d++)
            {
                // lower than low boundary
                if (Xq[d] < Bmin[d])
                {
                    sum += (Xq[d] - Bmin[d]) * (Xq[d] - Bmin[d]);
                    if (sum > best_dist_sq)
                        return false;
                }
                else if (Xq[d] > Bmax[d])
                {
                    sum += (Xq[d] - Bmax[d]) * (Xq[d] - Bmax[d]);
                    if (sum > best_dist_sq)
                        return false;
                }
                // else it's in range, thus distance 0
            }

            return true;
        }
    }
}


