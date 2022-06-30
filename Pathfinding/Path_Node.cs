using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path_Node<T> 
{
    public T data;
    public float mCost;//movement cost

    public Path_Edge<T>[] edges; //edges leading from this node
}
