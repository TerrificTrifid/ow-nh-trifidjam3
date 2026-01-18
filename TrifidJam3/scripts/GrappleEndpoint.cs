using NewHorizons;
using UnityEngine;

namespace TrifidJam3
{
    public class GrappleEndpoint : MonoBehaviour
    {
        public GrappleController Grapple;

        public LineRenderer Line;
        public static int Segments = 25;

        private void Awake()
        {
            
        }

        private void Start()
        {
            Grapple = GetComponentInParent<GrappleController>() ?? GrappleController.Instance;
            Line.positionCount = Segments;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            UpdateLine();
        }

        public void UpdateLine()
        {
            var separation = transform.InverseTransformPoint(Grapple.transform.position);
            var distance = separation.magnitude;
            //Line.positionCount = (int)Mathf.Lerp(MinSegments, MaxSegments, distance / GrappleController.MaxLength);

            var tension = Mathf.Clamp01(Grapple.GetTension() * .5f + .5f);

            var p0 = transform.position;
            var p1 = transform.TransformPoint(Vector3.forward * distance * tension);
            var p2 = Grapple.transform.TransformPoint(Vector3.forward * distance * tension);
            var p3 = Grapple.AttachPoint.position;
            for (int i = 0; i < Line.positionCount; i++)
            {
                var t = (float)i / (Line.positionCount - 1);
                Line.SetPosition(i, CubicBezierCurve(p0, p1, p2, p3, t));
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
