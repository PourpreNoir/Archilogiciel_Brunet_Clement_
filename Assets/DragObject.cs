using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Vector3 mOffset;
    private float mZCoord;
    private DistortGO distortGO;

    public void SetDistortGO(DistortGO distortGO)
    {
        this.distortGO = distortGO;
    }

    private void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mOffset = gameObject.transform.position - GetMouseWorldPos();
    }

    private void OnMouseDrag()
    {
        Vector3 newPosition = GetMouseWorldPos() + mOffset;
        transform.position = newPosition;


        // Informer l'instance de DistortGO du déplacement du PickingObject
        if (distortGO != null)
        {
            distortGO.OnPickingObjectMoved(GetInstanceID(), transform);
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mZCoord;

        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}