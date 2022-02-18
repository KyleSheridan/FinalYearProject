using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    public CubeGrid cubeGrid;

    public void GenerateMesh(int[,,] map, float cubeSize)
    {
        cubeGrid = new CubeGrid(map, cubeSize);
    }

    public void ClearMesh()
    {
        cubeGrid = null;
    }

    private void OnDrawGizmos()
    {
        if(cubeGrid != null)
        {
            for (int x = 0; x < cubeGrid.cubes.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < cubeGrid.cubes.GetLength(1) - 1; y++)
                {
                    for (int z = 0; z < cubeGrid.cubes.GetLength(1) - 1; z++)
                    {
                        Gizmos.color = cubeGrid.cubes[x, y, z].A.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].A.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].B.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].B.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].C.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].C.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].D.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].D.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].E.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].E.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].F.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].F.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].G.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].G.position, Vector3.one * .4f);

                        Gizmos.color = cubeGrid.cubes[x, y, z].H.active ? Color.red : Color.white;
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].H.position, Vector3.one * .4f);

                        Gizmos.color = Color.gray;

                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].ab.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].bd.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].cd.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].ac.position, Vector3.one * .1f);

                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].ae.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].bf.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].cg.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].dh.position, Vector3.one * .1f);

                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].ef.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].fh.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].gh.position, Vector3.one * .1f);
                        Gizmos.DrawCube(cubeGrid.cubes[x, y, z].eg.position, Vector3.one * .1f);
                    }
                }
            }
        }
    }

    public class CubeGrid
    {
        public Cube[,,] cubes;

        public CubeGrid(int[,,] map, float cubeSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            int nodeCountZ = map.GetLength(2);

            float mapWidth = nodeCountX * cubeSize;
            float mapHeight = nodeCountY * cubeSize;
            float mapDepth = nodeCountZ * cubeSize;

            ControlNode[,,] controlNodes = new ControlNode[nodeCountX, nodeCountY, nodeCountZ];

            for(int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    for (int z = 0; z < nodeCountZ; z++)
                    {
                        Vector3 pos = new Vector3(-mapWidth / 2 + x * cubeSize,
                                                -mapHeight / 2 + y * cubeSize,
                                                -mapDepth / 2 + z * cubeSize
                                                );

                        controlNodes[x, y, z] = new ControlNode(pos, map[x, y, z] == 1, cubeSize);
                    }
                }
            }

            cubes = new Cube[nodeCountX - 1, nodeCountY - 1, nodeCountZ - 1];

            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    for (int z = 0; z < nodeCountZ - 1; z++)
                    {
                        //---------- COULD HAVE ERROR --------------
                        cubes[x, y, z] = new Cube(controlNodes[x, y + 1, z],
                                                controlNodes[x + 1, y + 1, z],
                                                controlNodes[x, y, z],
                                                controlNodes[x + 1, y, z],
                                                controlNodes[x, y + 1, z + 1],
                                                controlNodes[x + 1, y + 1, z + 1],
                                                controlNodes[x, y, z + 1],
                                                controlNodes[x + 1, y, z + 1]
                                                );
                    }
                }
            }
        }
    }

    public class Cube
    {
        /*
         *      E ------ F
         *      |        |
         *      | A ------- B
         *      | |      |  |
         *      G | ---- H  |
         *        |         |
         *        C ------- D
         */

        public ControlNode A, B, C, D, E, F, G, H;

        // always alphabetical
        public Node ab, bd, cd, ac,
                    ae, bf, cg, dh,
                    ef, fh, gh, eg;

        public Cube(ControlNode _A, ControlNode _B, ControlNode _C, ControlNode _D, ControlNode _E, ControlNode _F, ControlNode _G, ControlNode _H)
        {
            A = _A;
            B = _B;
            C = _C;
            D = _D;
            E = _E;
            F = _F;
            G = _G;
            H = _H;

            ab = A.right;
            bd = D.up;
            cd = C.right;
            ac = C.up;

            ae = A.forward;
            bf = B.forward;
            cg = C.forward;
            dh = D.forward;

            ef = E.right;
            fh = H.up;
            gh = G.right;
            eg = G.up;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node up;
        public Node right;
        public Node forward;

        public ControlNode(Vector3 _pos, bool _active, float cubeSize) : base(_pos)
        {
            active = _active;

            up = new Node(position + Vector3.up * cubeSize / 2);
            right = new Node(position + Vector3.right * cubeSize / 2);
            forward = new Node(position + Vector3.forward * cubeSize / 2);
        }
    }
}
