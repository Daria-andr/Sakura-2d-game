using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sakura.Trial
{
    public static class TrialArt
    {
        private const string Root = "Assets/ThreeEnemyTrial/";

        // The sheets, clips and controllers are authored assets. Resolve their references
        // before assembling a scene so a broken import is reported at the source.
        public static void Build()
        {
            foreach (string kind in new[] { "Patrol", "Thrower", "Charger" })
            {
                RuntimeAnimatorController controller = GetController(kind);
                if (controller.animationClips.Length != 6)
                    throw new InvalidOperationException(kind + " needs six animation clips.");
                foreach (AnimationClip clip in controller.animationClips)
                {
                    var curves = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                    if (curves.Length != 1 || curves[0].propertyName != "m_Sprite")
                        throw new InvalidOperationException("Missing sprite track in " + clip.name);
                    var keys = AnimationUtility.GetObjectReferenceCurve(clip, curves[0]);
                    if (keys.Length == 0 || keys.Any(key => !(key.value is Sprite)))
                        throw new InvalidOperationException("Missing sprite frame in " + clip.name);
                }
                GetSprite(kind);
            }
            GetSprite("Crate");
        }

        public static RuntimeAnimatorController GetController(string kind)
        {
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                Root + "Animation/" + kind + "/" + kind + ".controller");
            if (controller == null)
                throw new InvalidOperationException("Missing animation controller for " + kind);
            return controller;
        }

        public static Sprite GetSprite(string name)
        {
            bool crate = name == "Crate";
            string sheet = crate ? "Props/Crate" : name + "/Idle";
            string frame = crate ? "Crate_00" : "Idle_00";
            var sprite = AssetDatabase.LoadAllAssetsAtPath(Root + "Art/" + sheet + ".png")
                .OfType<Sprite>().FirstOrDefault(candidate => candidate.name == frame);
            if (sprite == null)
                throw new InvalidOperationException("Missing " + frame + " in " + sheet);
            return sprite;
        }
    }
}
