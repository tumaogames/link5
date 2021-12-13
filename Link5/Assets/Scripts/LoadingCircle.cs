using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    private RectTransform rectComponent;
    [SerializeField]private float rotateSpeed = 200f;
    [SerializeField] private int direction = 1;

    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectComponent.Rotate(0f, 0f, rotateSpeed * direction * Time.deltaTime);
    }
}
