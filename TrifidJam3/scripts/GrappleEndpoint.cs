using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleEndpoint : MonoBehaviour
    {
        public GrappleController Grapple;

        public LineRenderer Line;

        public static int MinSegments = 5;
        public static int MaxSegments = 25;

        private void Awake()
        {
            
        }

        private void Start()
        {
            Grapple = GetComponentInParent<GrappleController>() ?? GrappleController.Instance;
        }

        private void Update()
        {
            UpdateLine();
        }

        public void UpdateLine()
        {
            Line.positionCount = (int)Mathf.Lerp(MinSegments, MaxSegments, transform.InverseTransformPoint(Grapple.transform.position).magnitude / GrappleController.MaxLength);

            for (int i = 0; i < Line.positionCount; i++)
            {
                var t = (float)i / (Line.positionCount - 1);
                Line.SetPosition(i, Vector3.Lerp(transform.position, Grapple.AttachPoint.position, t));
            }
        }

        private Vector3 CubicBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            var a = Vector3.Lerp(p0, p1, t);
            var b = Vector3.Lerp(p1, p2, t);
            var c = Vector3.Lerp(p2, p3, t);
            var d = Vector3.Lerp(a, b, t);
            var e = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(d, e, t);
        }
    }
}
