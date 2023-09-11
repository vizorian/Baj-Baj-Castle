namespace Game_Logic;

public class CameraMovement : MonoBehaviour
{
    public float BoundX = 0.1f;
    public float BoundY = 0.5f;
    public Transform Target;

    [UsedImplicitly]
    private void LateUpdate()
    {
        if (Target == null) return;

        var delta = Vector3.zero;

        // Check if in bounds on X axis
        var deltaX = Target.position.x - transform.position.x;
        if (deltaX > BoundX || deltaX < -BoundX)
        {
            if (transform.position.x < Target.position.x)
                delta.x = deltaX - BoundX;
            else
                delta.x = deltaX + BoundX;
        }

        // Check if in bounds on Y axis
        var deltaY = Target.position.y - transform.position.y;
        if (deltaY > BoundY || deltaY < -BoundY)
        {
            if (transform.position.y < Target.position.y)
                delta.y = deltaY - BoundY;
            else
                delta.y = deltaY + BoundY;
        }

        transform.position += delta;
    }

    // Find a player target
    [UsedImplicitly]
    private void FindPlayer()
    {
        Target = GameObject.Find("Player").transform;
    }
}