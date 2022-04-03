using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHNode : Node
{
    public List<Coord> Tiles;
    public Node left;
    public Node right;

    int Box_Compare(Node a, Node b, int axis)
    {
        return a.pos[axis].CompareTo(b.pos[axis]);
    }

    int box_x_compare(Node a, Node b) {
	    return Box_Compare(a, b, 0);
    }

    int box_y_compare(Node a, Node b)
    {
        return Box_Compare(a, b, 1);
    }

    int box_z_compare(Node a, Node b)
    {
        return Box_Compare(a, b, 2);
    }

    BVHNode(List<Node> src_objects, int start, int end)
    {
        var objects = src_objects;

        int axis = UnityEngine.Random.Range(0, 3);

        Comparison<Node> comparator = box_x_compare;

        switch (axis)
        {
            case 0:
                comparator = box_x_compare;
                break;
            case 1:
                comparator = box_y_compare;
                break;
            case 2:
                comparator = box_z_compare;
                break;
            default:
                break;
        }

        int object_span = end - start;

        if(object_span == 1)
        {
            left = right = objects[start];
        }
        else if(object_span == 2)
        {
            if(objects[start].pos[axis] < objects[start + 1].pos[axis])
            {
                left = objects[start];
                right = objects[start + 1];
            }
            else
            {
                left = objects[start + 1];
                right = objects[start];
            }
        }
        else
        {
            objects.Sort(comparator);

            var mid = start + object_span / 2;

            left = new BVHNode(objects, start, mid);
            right = new BVHNode(objects, mid, end);
        }

        int x = Mathf.Abs(objects[start].pos.x - objects[end].pos.x);
        int y = Mathf.Abs(objects[start].pos.y - objects[end].pos.y);
        int z = Mathf.Abs(objects[start].pos.z - objects[end].pos.z);

        pos = new Vector3Int(x, y, z);
    }
}
