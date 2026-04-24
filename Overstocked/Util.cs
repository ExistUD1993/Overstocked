using GorillaNetworking.Store;
using UnityEngine;

namespace Overstocked;

public class Util
{
    private readonly GameObject _buttonToClone;

    public Util(GameObject buttonToClone)
    {
        _buttonToClone = buttonToClone;
    }

    public GameObject CreateButton(Transform parent, Vector3 localPosition, Vector3 localScale, string text, Action onPressed)
    {
        GameObject button = UnityEngine.Object.Instantiate(_buttonToClone, parent, false);
        UnityEngine.Object.Destroy(button.GetComponentInChildren<DynamicCosmeticStand_Link>());
        UnityEngine.Object.Destroy(button.transform.GetChild(2).gameObject);

        Transform child = button.transform.GetChild(1);
        child.localPosition = localPosition;
        child.localScale = localScale;

        GorillaPressableButton pressableButton = button.GetComponentInChildren<GorillaPressableButton>();
        pressableButton.offText = text;
        pressableButton.UpdateColor();
        pressableButton.onPressButton = null;
        pressableButton.onPressed += (_, _) => onPressed();
        return button;
    }
}