using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MazeHell
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("MazeHell/Structure Lighting Controller")]
    public sealed class StructureLightingController : MonoBehaviour
    {
        [Serializable]
        public sealed class LightSettings
        {
            public Color color = Color.white;
            [Min(0)] public float intensity = 45;
            [Min(0.01f)] public float range = 19;
            public LightShadows shadows = LightShadows.Soft;
            [Range(0, 1)] public float shadowStrength = 1;

            public LightSettings() { }
            public LightSettings(float intensity, float range)
            {
                this.intensity = intensity;
                this.range = range;
            }
        }

        [Header("All room lights")]
        public LightSettings rooms = new LightSettings(45, 19);
        [Header("All corridor lights")]
        public LightSettings corridors = new LightSettings(18, 17);

        [Tooltip("Only tagged lights below this object are controlled. Black passages have no lights.")]
        [SerializeField, HideInInspector] int controlledLights;

        void OnEnable() => ScheduleApply();
        void OnValidate() => ScheduleApply();

        void ScheduleApply()
        {
#if UNITY_EDITOR
            // OnValidate may run during deserialization. Apply to Unity objects on the next editor tick.
            EditorApplication.update -= ApplyDelayed;
            EditorApplication.update += ApplyDelayed;
#else
            ApplySettings();
#endif
        }

#if UNITY_EDITOR
        void ApplyDelayed()
        {
            EditorApplication.update -= ApplyDelayed;
            if (this && isActiveAndEnabled) ApplySettings();
        }
#endif

        void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= ApplyDelayed;
#endif
        }

        [ContextMenu("Apply lighting to all structures")]
        public void ApplySettings()
        {
            if (rooms == null || corridors == null) return;
            var targets = GetComponentsInChildren<StructureLight>(true);
            controlledLights = targets.Length;
            foreach (var target in targets)
            {
                var light = target.GetComponent<Light>();
                if (!light) continue;
                var settings = target.role == StructureLight.Role.Corridor ? corridors :
                    rooms;
                light.color = settings.color;
                light.useColorTemperature = false;
                light.intensity = Mathf.Max(0, settings.intensity);
                light.range = Mathf.Max(.01f, settings.range);
                light.shadows = settings.shadows;
                light.shadowStrength = Mathf.Clamp01(settings.shadowStrength);
                // This controller adjusts light properties only; enabled states remain manual.
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(light);
                    if (PrefabUtility.IsPartOfPrefabInstance(light))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(light);
                }
#endif
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
                SceneView.RepaintAll();
            }
#endif
        }
    }
}
