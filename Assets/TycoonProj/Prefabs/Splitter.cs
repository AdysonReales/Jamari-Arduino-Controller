using UnityEngine;
using System.Collections.Generic;

public class Splitter : MonoBehaviour
{
    public enum SplitterType
    {
        ThreeWay,
        Intersection,
        TwoWay
    }

    public SplitterType splitterType = SplitterType.ThreeWay;
    public float moveSpeed = 2f;
    public Transform centerPoint;

    private Queue<GameObject> itemsAtCenter = new Queue<GameObject>();
    private Dictionary<GameObject, Vector3> lastPositions = new Dictionary<GameObject, Vector3>();
    private int outputIndex = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Bit")) return;

        if (!lastPositions.ContainsKey(other.gameObject))
            lastPositions.Add(other.gameObject, other.transform.position);

        StartCoroutine(MoveToTarget(other.gameObject, centerPoint.position, () =>
        {
            itemsAtCenter.Enqueue(other.gameObject);
            TryDispatchNext();
        }));
    }

    private void TryDispatchNext()
    {
        if (itemsAtCenter.Count == 0) return;

        GameObject item = itemsAtCenter.Dequeue();
        List<Vector3> outputs = GetConnectedOutputs(item);

        if (outputs.Count == 0) return;

        // Round-robin dispatch if multiple outputs
        Vector3 nextPos = outputs[outputIndex % outputs.Count];
        outputIndex++;

        if (lastPositions.ContainsKey(item)) lastPositions.Remove(item);

        StartCoroutine(MoveToTarget(item, nextPos, null));
    }

    private List<Vector3> GetConnectedOutputs(GameObject item)
    {
        List<Vector3> outputs = new List<Vector3>();
        Collider2D hit;

        Vector3 lastPos = lastPositions.ContainsKey(item) ? lastPositions[item] : item.transform.position;
        Vector3 localIncoming = transform.InverseTransformPoint(centerPoint.position) - transform.InverseTransformPoint(lastPos);

        switch (splitterType)
        {
            case SplitterType.Intersection:
                Vector3 rightWorld = transform.right;
                Vector3 leftWorld = -transform.right;
                Vector3 upWorld = transform.up;
                Vector3 downWorld = -transform.up;

                List<Vector3> possibleOutputs = new List<Vector3>();

                // All directions
                //Collider2D hit;
                hit = Physics2D.OverlapCircle((Vector2)(transform.position + rightWorld), 0.1f, LayerMask.GetMask("Conveyor"));
                if (hit != null) possibleOutputs.Add(hit.transform.position);

                hit = Physics2D.OverlapCircle((Vector2)(transform.position + leftWorld), 0.1f, LayerMask.GetMask("Conveyor"));
                if (hit != null) possibleOutputs.Add(hit.transform.position);

                hit = Physics2D.OverlapCircle((Vector2)(transform.position + upWorld), 0.1f, LayerMask.GetMask("Conveyor"));
                if (hit != null) possibleOutputs.Add(hit.transform.position);

                hit = Physics2D.OverlapCircle((Vector2)(transform.position + downWorld), 0.1f, LayerMask.GetMask("Conveyor"));
                if (hit != null) possibleOutputs.Add(hit.transform.position);

                // Remove the output that goes back to where it came from
                Vector3 incomingDir = (centerPoint.position - lastPos).normalized;
                possibleOutputs.RemoveAll(pos => Vector3.Dot((pos - centerPoint.position).normalized, incomingDir) < -0.5f);

                // Prefer straight along the same axis as incoming
                Vector3 straight = Vector3.zero;
                if (Mathf.Abs(incomingDir.x) > Mathf.Abs(incomingDir.y))
                    straight = new Vector3(Mathf.Sign(incomingDir.x), 0, 0);
                else
                    straight = new Vector3(0, Mathf.Sign(incomingDir.y), 0);

                Vector3? preferred = null;
                foreach (var pos in possibleOutputs)
                {
                    Vector3 dir = (pos - centerPoint.position).normalized;
                    if (Vector3.Dot(dir, straight) > 0.9f) preferred = pos;
                }

                if (preferred.HasValue)
                    outputs.Add(preferred.Value);
                else
                    outputs.AddRange(possibleOutputs);

                break;
        
        

            case SplitterType.TwoWay:
                if (Mathf.Abs(localIncoming.x) > Mathf.Abs(localIncoming.y))
                {
                    Vector3 horizontalDir = localIncoming.x > 0 ? transform.right : -transform.right;
                    hit = Physics2D.OverlapCircle((Vector2)(transform.position + horizontalDir), 0.1f, LayerMask.GetMask("Conveyor"));
                    if (hit != null) outputs.Add(hit.transform.position);

                    Vector3 downDir = -transform.up;
                    hit = Physics2D.OverlapCircle((Vector2)(transform.position + downDir), 0.1f, LayerMask.GetMask("Conveyor"));
                    if (hit != null) outputs.Add(hit.transform.position);
                }
                break;
        

            case SplitterType.ThreeWay:
                Vector2[] dirs = { Vector2.right, Vector2.up, Vector2.down };
                foreach (var dir in dirs)
                {
                    hit = Physics2D.OverlapCircle((Vector2)transform.position + dir, 0.1f, LayerMask.GetMask("Conveyor"));
                    if (hit != null) outputs.Add(hit.transform.position);
                }
                break;
        }

        return outputs;
    }

    private System.Collections.IEnumerator MoveToTarget(GameObject item, Vector3 target, System.Action onArrive)
    {
        if (item == null) yield break;

        while (item != null && Vector3.Distance(item.transform.position, target) > 0.01f)
        {
            item.transform.position = Vector3.MoveTowards(item.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (item != null)
        {
            item.transform.position = target;
            onArrive?.Invoke();
        }
    }
}
