using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractableParams : MonoBehaviour
{
    public Toggle onlyOneRoom;
    public Toggle connectRooms;
    public Slider passageRadius;
    public Toggle randomSeed;
    public TMP_InputField seed;

    public Slider NumParticles;

    public Slider[] ParticleParams;

    // Start is called before the first frame update
    void Start()
    {
        onlyOneRoom.onValueChanged.AddListener(delegate { OneRoomChange(); });
        connectRooms.onValueChanged.AddListener(delegate { ConnectRoomsChange(); });
        randomSeed.onValueChanged.AddListener(delegate { RandomSeedChange(); });
        NumParticles.onValueChanged.AddListener(delegate { ParticleChange(); });

        OneRoomChange();
        ConnectRoomsChange();
        RandomSeedChange();
        ParticleChange();
    }

    void OneRoomChange()
    {
        if(onlyOneRoom.isOn)
        {
            connectRooms.isOn = false;
            connectRooms.interactable = false;
        }
        else
        {
            connectRooms.interactable = true;
        }
    }

    void ConnectRoomsChange()
    {
        if (connectRooms.isOn)
        {
            passageRadius.interactable = true;
        }
        else
        {
            passageRadius.interactable = false;
        }
    }

    void RandomSeedChange()
    {
        if (randomSeed.isOn)
        {
            seed.interactable = false;
        }
        else
        {
            seed.interactable = true;
        }
    }

    void ParticleChange()
    {
        if(NumParticles.value == 0)
        {
            foreach(Slider param in ParticleParams)
            {
                param.interactable = false;
            }
        }
        else
        {
            foreach (Slider param in ParticleParams)
            {
                param.interactable = true;
            }
        }
    }
}
