using System.Collections.Generic;
using UnityEngine;
using RVO;
using System;
using KUtils;

namespace KFrameWork
{

    public class MinHeap<Tkey> where Tkey : IComparable<Tkey>
    {

        /// root is assumed to be at end of the vector
        List<ClsTuple<Tkey, int>> heap = new List<ClsTuple<Tkey, int>>() ;
        /**
         *  maintain a list of back indexes.
         *  * -1           not in heap
         *  * other 	   index that point to cell in vector heap
         */
        List<int> backIdx = new List<int>();
        /**
         * If useBackIdx==false it means that the current structure
         * is not making use of a backindexed heap. Thus, no update 
         * is available
         */
        bool useBackIdx;

        /// back indexes constructor used for cross updates
        public MinHeap(int Nindex)
        {
            backIdx.Capacity = Nindex;
            // initialize the back indexes with pseudo-null pointers
            for (int i = 0; i < Nindex; ++i)
            {
                backIdx.Add(-1);
            }
            useBackIdx = true;
            heap.Capacity = Nindex;
        }
        /// Simple constructor with NO cross updates
        public MinHeap()
        {
            useBackIdx = false;
        }

        int PARENT(int pos)
        {
            return ((pos - 1) >> 1);
        }

        int LEFT(int pos)
        {
            return ((pos << 1) + 1);
        }

        int RIGHT(int pos)
        {
            return ((pos << 1) + 2);
        }

        /// pushes a new value in the heap
        public void push(Tkey key, int index)
        {
            //cout << "pushing " << index << endl;
            if (useBackIdx && index >= (int)backIdx.Count)
                throw new InvalidCastException("the index in the push must be smaller than the maximal allowed index (specified in constructor)");

            // If key is not in backindexes or there is no backindexes AT ALL.... complete push (no update)
            if (!useBackIdx)
            {
                // add to the back of the vector
                heap.Add(new ClsTuple<Tkey, int>(key, index));
                // recursive call to increase key
                heapDecreaseKey(heap.Count - 1, key);
            }
            else
            {
                if (useBackIdx || backIdx[index] == -1)
                {
                    // add to the back of the vector
                    heap.Add(new ClsTuple<Tkey, int>(key, index));
                    //initially point to back
                    backIdx[index] = heap.Count - 1;
                    // recursive call to increase key
                    heapDecreaseKey(heap.Count - 1, key);
                    // USE STL STUFF
                    //push_heap(heap.begin(),heap.end());
                }
                // update push (a key exists)
                else
                {
                    heapDecreaseKey(backIdx[index], key);
                }
            }
        }

        /// return a constant reference to the MINIMAL KEY element stored in the head of the heap
        public ClsTuple<Tkey, int> top()
        {
            if (heap.Count == 0)
                throw new Exception("Impossible to get top element, empty heap");
            else
                return heap[0];
        }

        /// removes the top element of the queue (minimal)
        public void pop()
        {
            if (heap.Count < 1) //a.k.a. heap.empty()
                throw new Exception("heap underflow");

            // overwrite top with tail element
            heap[0] = heap[heap.Count - 1];

            // USE STL FUNCTIONALITIES (NOT ALLOW BACKINDEXs)
            //pop_heap(heap.begin(), heap.end());

            // shorten the vector
            heap.RemoveAt(heap.Count - 1);

            // start heapify from root
            minHeapify(0);
        }

        /// returns the size of the heap
        public int size()
        {
            return heap.Count;
        }

        /// check for emptyness
        public bool empty()
        {
            return heap.Count == 0;
        }

        // this does not work, how do you provide a new ordering function to is_heap??
        /// check recursively if the substructures is correct using STL provided algorithm
        //bool verifyHeap( ){
        //	return std::__is_heap(heap.begin(), heap.end() );
        //}

        /// computes full heap sort and returns the corresponding indexing structure
        /// Requires the indexes to be allocated already.
        public void heapsort(List<int> indexes)
        {
            // until empty... keep popping
            int i = 0;
            while (empty() == false)
            {
                ClsTuple<Tkey, int> t = top();
                pop();
                indexes[i++] = t.Value;
            }
        }


        /// check and applies MaxHeap Correctness down the subtree with index "currIdx"
        void minHeapify(int currIdx)
        {
            int leftIdx = LEFT(currIdx);
            int rightIdx = RIGHT(currIdx);

            // decide if and where ta swap, left or right, then swap
            // current is the best choice (defalut)
            int smallerIdx;

            // is left a better choice? (exists an invalid placed smaller value on the left side)
            if (leftIdx < heap.Count && heap[leftIdx].Key.CompareTo(heap[currIdx].Key) < 0)
                smallerIdx = leftIdx;
            else
                smallerIdx = currIdx;

            // is right a better choice? (exists an invalid placed smaller value on the right side)
            if (rightIdx < heap.Count && heap[rightIdx].Key.CompareTo(heap[smallerIdx].Key) < 0)
                smallerIdx = rightIdx;

            // a better choice exists?
            if (smallerIdx != currIdx)
            {
                // swap elements
                swap(currIdx, smallerIdx);

                // recursively call this function on alterated subtree
                minHeapify(smallerIdx);
            }
        }

        /// swap the content of two elements in position pos1 and pos2
        void swap(int pos1, int pos2)
        {
            Debug.Assert(heap.Count != 0);
            Debug.Assert(pos1 >= 0 && pos1 < heap.Count);
            Debug.Assert(pos2 >= 0 && pos2 < heap.Count);

            // update backindexes
            if (useBackIdx)
            {
                backIdx[heap[pos1].Value] = pos2;
                backIdx[heap[pos2].Value] = pos1;
            }

            // update heap
            ClsTuple<Tkey, int> temp = heap[pos1];
            heap[pos1] = heap[pos2];
            heap[pos2] = temp;
        }

        /// propagates the correctness (in heap sense) down from a vertex currIdx
        void heapDecreaseKey(int currIdx, Tkey key)
        {
            // check if given key update is actually an increase
            if (key.CompareTo(heap[currIdx].Key) > 0)
                throw new Exception("In MinHeaps only decrease in key updates are legal");

            // update value with current key
            heap[currIdx].Key = key;

            // traverse the tree up making necessary swaps
            int parentIdx = PARENT(currIdx);
            while (currIdx > 0)
            {
                if (heap[parentIdx].Key.CompareTo(heap[currIdx].Key) > 0)
                {
                    // make swap
                    swap(currIdx, parentIdx);
                    // move up
                    currIdx = parentIdx;
                    parentIdx = PARENT(currIdx);
                }
                else
                {
                    break;
                }
            }
        }
    }
}