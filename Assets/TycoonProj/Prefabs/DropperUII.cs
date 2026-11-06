using UnityEngine;
using UnityEngine.UI;

public class DropperUII : MonoBehaviour
{
    [SerializeField] private Dropper dropper;  // keep private
    public GameObject panel;          
    public Slider speedSlider;
    public Toggle bit0Toggle;
    public Toggle bit1Toggle;
    public Toggle bothToggle;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false); // hide by default
    }

    // Call this from BuildManager after placement
    public void Setup(Dropper parentDropper)
    {
        dropper = parentDropper;

        // Initialize UI based on current spawnMode
        UpdateTogglesFromDropper();
    }

    private void UpdateTogglesFromDropper()
    {
        if (dropper == null) return;

        switch (dropper.spawnMode)
        {
            case Dropper.BitSpawnMode.Bit0:
                bit0Toggle.isOn = true;
                bit1Toggle.isOn = false;
                bothToggle.isOn = false;
                break;
            case Dropper.BitSpawnMode.Bit1:
                bit0Toggle.isOn = false;
                bit1Toggle.isOn = true;
                bothToggle.isOn = false;
                break;
            case Dropper.BitSpawnMode.Both:
            default:
                bit0Toggle.isOn = true;
                bit1Toggle.isOn = true;
                bothToggle.isOn = true;
                break;
        }
    }

    public void OnBitSelectionChanged()
    {
        if (dropper == null) return;

        if (bit0Toggle.isOn && bit1Toggle.isOn)
            dropper.spawnMode = Dropper.BitSpawnMode.Both;
        else if (bit0Toggle.isOn)
            dropper.spawnMode = Dropper.BitSpawnMode.Bit0;
        else if (bit1Toggle.isOn)
            dropper.spawnMode = Dropper.BitSpawnMode.Bit1;
        else
            dropper.spawnMode = Dropper.BitSpawnMode.Both;
    }

    public void TogglePanel()
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }
}
