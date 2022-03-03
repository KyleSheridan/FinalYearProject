using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle
{
    public List<Coord> fBMMovement;

    public Particle(List<Coord> _path)
    {
        fBMMovement = _path;
    }

    public List<Coord> GeneratePath(int width, int height, int depth, int stepLength, int edgeSize = 5)
    {
        Coord startPoint = new Coord(Random.Range(edgeSize, width - edgeSize),
                                     Random.Range(edgeSize, height - edgeSize),
                                     Random.Range(edgeSize, depth - edgeSize)
                                     );

        List<Coord> path = new List<Coord>();

        path.Add(startPoint);

        Coord currentPoint = startPoint;

        for (int i = 0; i < fBMMovement.Count; i++)
        {
            currentPoint += (fBMMovement[i] * stepLength);
            path.Add(currentPoint);
        }

        return path;
    }
}

public class BrownianMotion : MonoBehaviour
{
    public Particle GenerateParticle(int length, float H)
    {
        List<Coord> path = new List<Coord>();

        while (path.Count < length)
        {
            int remainingLength = length - path.Count;

            List<int> xAxis = FBMAxis(BrownianMotionAxis(remainingLength), H);
            List<int> yAxis = FBMAxis(BrownianMotionAxis(remainingLength), H);
            List<int> zAxis = FBMAxis(BrownianMotionAxis(remainingLength), H);


            for (int i = 0; i < remainingLength; i++)
            {
                bool conditions = ((xAxis[i] == 0 && yAxis[i] == 0 && zAxis[i] == 0)
                                  || (xAxis[i] == 0 && yAxis[i] == 1 && zAxis[i] == 0)
                                  || (xAxis[i] == 0 && yAxis[i] == -1 && zAxis[i] == 0)
                                  );

                if (conditions)
                {
                    continue;
                }
                path.Add(new Coord(xAxis[i], yAxis[i], zAxis[i]));
            }
        }

        return new Particle(path);
    }

    public List<int> FBMAxis(List<float> BMAxis, float H)
    {
        List<int> FBMList = new List<int>();

        FBMList.Add((int)BMAxis[0]);

        for (int i = 1; i < BMAxis.Count; i++)
        {
            FBMList.Add(FBMStep(BMAxis[i], BMAxis[i - 1], H));
        }

        return FBMList;
    }

    int FBMStep(float t, float s, float H)
    {
        if(H == 0)
        {
            Debug.LogError("H cannot be equal to 0");
            return 0;
        }

        // add 1 to values as cannot sqrt -1
        float FBM = 0.5f * (Mathf.Pow((t + 1), 2 * H) + Mathf.Pow((s + 1), 2 * H) - Mathf.Pow(Mathf.Abs((t + 1) - (s + 1)), 2 * H));

        int val = Mathf.RoundToInt(FBM) - 1;

        return Mathf.Clamp(val, -1, 1);
    }

    public List<float> BrownianMotionAxis(int length)
    {
        List<float> BMList = new List<float>();

        for(int i = 0; i < length; i++)
        {
            BMList.Add(RandomAxisStep());
        }

        return BMList;
    }

    float RandomAxisStep()
    {
        return Random.Range(-1, 2);
    }
}
