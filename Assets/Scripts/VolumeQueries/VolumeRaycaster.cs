using UnityEngine;
using System.Collections.Generic;

namespace UnityVolumeRendering
{
    public struct RaycastHit
    {
        public VolumeRenderedObject volumeObject;
        public Vector3 point;
        public float distance;
    }

    /// <summary>
    /// Volume raycaster, for raycasting datasets.
    /// </summary>
    public class VolumeRaycaster
    {
        /// <summary>
        /// Raycast all datasets in the scene.
        /// </summary>
        /// <param name="ray">World space ray.</param>
        /// <param name="hit">First hit, if any.</param>
        /// <returns></returns>
        public bool RaycastScene(Ray ray, out RaycastHit hit)
        {
            VolumeRenderedObject[] objects = GameObject.FindObjectsOfType<VolumeRenderedObject>();
            List<RaycastHit> hits = new List<RaycastHit>();
            foreach (VolumeRenderedObject obj in objects)
            {
                RaycastHit currentHit;
                if (RaycastObject(ray, obj, out currentHit))
                {
                    hits.Add(currentHit);
                }
            }
            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));
                hit = hits[0];
                return true;
            }
            else
            {
                hit = new RaycastHit();
                return false;
            }
        }

        private bool RaycastObject(Ray worldSpaceRay, VolumeRenderedObject volumeObject, out RaycastHit hit)
        {
            Transform volumeTransform = volumeObject.volumeContainerObject.transform;

            hit = new RaycastHit();
            CrossSectionManager crossSectionManager = volumeObject.GetCrossSectionManager();
            CrossSectionData[] crossSectionDatas = crossSectionManager.GetCrossSectionData();

            VolumeDataset dataset = volumeObject.dataset;
            Vector2 visibilityWindow = volumeObject.GetVisibilityWindow();
            float minValue = Mathf.Lerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), visibilityWindow.x);
            float maxValue = Mathf.Lerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), visibilityWindow.y);
            Ray localRay = worldSpaceRay;
            localRay.origin = volumeTransform.InverseTransformPoint(worldSpaceRay.origin);
            localRay.direction = volumeTransform.InverseTransformVector(worldSpaceRay.direction);
            Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);
            if (localBounds.IntersectRay(localRay, out float tStart))
            {
                Vector3 start = localRay.origin + localRay.direction * tStart;
                Vector3 direction = localRay.direction.normalized;
                for (float t = 0.0f; ; t += 0.01f)
                {
                    Vector3 position = start + direction * t;
                    if (!localBounds.Contains(position))
                        break;
                    if (IsCutout(position, crossSectionDatas))
                        continue;
                    Vector3 uvw = position + Vector3.one * 0.5f;
                    Vector3Int index = new Vector3Int((int)(uvw.x * dataset.dimX), (int)(uvw.y * dataset.dimY), (int)(uvw.z * dataset.dimZ));
                    float value = dataset.GetData(index.x, index.y, index.z);
                    float normalisedValue = Mathf.InverseLerp(dataset.GetMinDataValue(), dataset.GetMaxDataValue(), value);
                    if (value >= minValue && value <= maxValue && volumeObject.transferFunction.GetColour(normalisedValue).a > 0.0f)
                    {
                        hit.point = volumeTransform.TransformPoint(position);
                        hit.distance = (worldSpaceRay.origin - volumeTransform.TransformPoint(position)).magnitude;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsCutout(Vector3 pos, CrossSectionData[] crossSections)
        {
            bool clipped = false;
            for (int i = 0; i < crossSections.Length && !clipped; ++i)
            {
                CrossSectionType type = crossSections[i].type;
                Matrix4x4 mat = crossSections[i].matrix;

                Vector3 planeSpacePos = mat.MultiplyPoint(pos);
                if (type == CrossSectionType.Plane)
                    clipped = planeSpacePos.z > 0.0f;
                else if (type == CrossSectionType.BoxInclusive)
                    clipped = !(planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f &&
                                planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f);
                else if (type == CrossSectionType.BoxExclusive)
                    clipped = planeSpacePos.x >= -0.5f && planeSpacePos.x <= 0.5f && planeSpacePos.y >= -0.5f &&
                              planeSpacePos.y <= 0.5f && planeSpacePos.z >= -0.5f && planeSpacePos.z <= 0.5f;
            }

            return clipped;
        }
    }
}
