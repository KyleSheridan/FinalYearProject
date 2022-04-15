using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    public MapGenerator generator;

    public Slider width;
    public Slider height;
    public Slider depth;
    public Slider iterations;
    public Slider fillPercent;

    public Toggle onlyOneRoom;
    public Toggle connectRooms;
    public Slider passageRadius;

    public Slider numParticles;
    public Slider P_Length;
    public Slider P_StepLength;
    public Slider P_Radius;
    public Slider P_FillPercent;
    public Slider P_H;

    public Toggle useRandomSeed;
    public TMP_InputField seed;

    public TMP_Text seedText;

    public void QuitProgram()
    {
        Application.Quit();
    }

    public void GenerateCave()
    {
        generator.width = (int)width.value;
        generator.height = (int)height.value;
        generator.depth = (int)depth.value;

        generator.iterations = (int)iterations.value;
        generator.randomFillPercent = (int)fillPercent.value;

        generator.onlyOneRoom = onlyOneRoom.isOn;
        generator.connectRooms = connectRooms.isOn;
        generator.passageRadius = (int)passageRadius.value;

        if((int)numParticles.value > 0)
        {
            generator.patricles = new MapGenerator.ParticleParams[(int)numParticles.value];

            for (int i = 0; i < (int)numParticles.value; i++)
            {
                generator.patricles[i].length = (int)P_Length.value;
                generator.patricles[i].stepLength = (int)P_StepLength.value;
                generator.patricles[i].radius = (int)P_Radius.value;
                generator.patricles[i].fillPercent = (int)P_FillPercent.value;
                generator.patricles[i].H = P_H.value;

                generator.patricles[i].edgeSize = 10;
            }
        }

        generator.edgeSize = 4;

        generator.useRandomSeed = useRandomSeed.isOn;
        generator.seed = seed.text;

        generator.GenerateMap();

        seedText.text = "Seed: " + generator.seed;
    }
}
