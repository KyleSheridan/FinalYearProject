using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BVHNode : Node
{
    public Node left;
    public Node right;

    int BoxCompare(Node a, Node b, int axis)
    {
        return a.pos[axis].CompareTo(b.pos[axis]);
    }

    int BoxXCompare(Node a, Node b) {
	    return BoxCompare(a, b, 0);
    }

    int BoxYCompare(Node a, Node b)
    {
        return BoxCompare(a, b, 1);
    }

    int BoxZCompare(Node a, Node b)
    {
        return BoxCompare(a, b, 2);
    }

    public BVHNode(List<Coord> srcObjects)
    {
        List<Coord> objects = srcObjects;

        int axis = UnityEngine.Random.Range(0, 3);

        Comparison<Node> comparator = BoxXCompare;

        switch (axis)
        {
            case 0:
                comparator = BoxXCompare;
                break;
            case 1:
                comparator = BoxYCompare;
                break;
            case 2:
                comparator = BoxZCompare;
                break;
            default:
                break;
        }

        int objectSpan = objects.Count;

        if(objectSpan == 1)
        {
            left = right = objects[0];
        }
        else if(objectSpan == 2)
        {
            if(objects[0].pos[axis] < objects[1].pos[axis])
            {
                left = objects[0];
                right = objects[1];
            }
            else
            {
                left = objects[1];
                right = objects[0];
            }
        }
        else
        {
            objects.Sort(comparator);

            int mid = objectSpan / 2;

            left = new BVHNode(objects.GetRange(0, mid));
            right = new BVHNode(objects.GetRange(mid, objects.Count - mid));
        }

        int x = Mathf.Abs(objects[0].pos.x - objects[objects.Count - 1].pos.x);
        int y = Mathf.Abs(objects[0].pos.y - objects[objects.Count - 1].pos.y);
        int z = Mathf.Abs(objects[0].pos.z - objects[objects.Count - 1].pos.z);

        pos = new Vector3Int(x, y, z);
    }
    public override Node FindClosest(Vector3Int target)
    {
        float leftDist = Vector3Int.Distance(left.pos, target);
        float rightDist = Vector3Int.Distance(right.pos, target);

        return leftDist < rightDist ? left.FindClosest(target) : right.FindClosest(target);
    }
}
