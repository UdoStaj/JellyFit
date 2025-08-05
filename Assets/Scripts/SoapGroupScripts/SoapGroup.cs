using System.Collections.Generic;
using UnityEngine;

public class SoapGroup : MonoBehaviour
{
    [Header("Group Settings")]
    public Color groupColor = Color.white;

    private List<SoapCube> members = new List<SoapCube>();
    private bool isMoving = false;

    public List<SoapCube> Members => members;
    public int MemberCount => members.Count;
    public bool IsMoving => isMoving;

    private void Awake()
    {
        // Random renk ata (debug için)
        groupColor = new Color(
            Random.Range(0.3f, 1f),
            Random.Range(0.3f, 1f),
            Random.Range(0.3f, 1f),
            0.3f
        );
    }

    public void AddMember(SoapCube cube)
    {
        if (cube != null && !members.Contains(cube))
        {
            members.Add(cube);
            cube.SetGroup(this);

            // Küpün rengini grup rengine ayarla (debug için)
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = groupColor;
            }
        }
    }

    public void RemoveMember(SoapCube cube)
    {
        if (cube != null && members.Contains(cube))
        {
            members.Remove(cube);
            cube.SetGroup(null);

            // Grup boþaldýysa kendini yok et
            if (members.Count == 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void MoveGroup(Vector3 offset)
    {
        if (isMoving) return;

        isMoving = true;

        foreach (var member in members)
        {
            if (member != null)
            {
                member.transform.position += offset;
                member.UpdateGridPosition();
            }
        }

        isMoving = false;
    }

    public void MoveGroupTo(Vector3 targetPosition)
    {
        if (members.Count == 0) return;

        Vector3 currentCenter = GetGroupCenter();
        Vector3 offset = targetPosition - currentCenter;
        MoveGroup(offset);
    }

    public Vector3 GetGroupCenter()
    {
        if (members.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (var member in members)
        {
            if (member != null)
            {
                center += member.transform.position;
            }
        }
        return center / members.Count;
    }

    public SoapCube GetClosestMemberToPoint(Vector3 point)
    {
        SoapCube closest = null;
        float minDistance = float.MaxValue;

        foreach (var member in members)
        {
            if (member != null)
            {
                float distance = Vector3.Distance(member.transform.position, point);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = member;
                }
            }
        }

        return closest;
    }

    public bool ContainsCube(SoapCube cube)
    {
        return members.Contains(cube);
    }

    // Grubun sýnýrlarýný göster (debug için)
    private void OnDrawGizmos()
    {
        if (members.Count == 0) return;

        Gizmos.color = groupColor;
        Vector3 center = GetGroupCenter();
        Gizmos.DrawWireSphere(center, 0.5f);

        // Grup üyelerini vurgula
        foreach (var member in members)
        {
            if (member != null)
            {
                Gizmos.DrawWireCube(member.transform.position, Vector3.one * 1.1f);
            }
        }
    }
}