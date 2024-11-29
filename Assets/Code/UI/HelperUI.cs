using UnityEngine;
using UnityEngine.UIElements;

public class HelperUI : MonoBehaviour
{
    private UIDocument hudDocument;
    private VisualElement baseContainer;

    void Awake()
    {
        hudDocument = gameObject.GetComponent<UIDocument>();
    }

    void Update()
    {

    }
}
