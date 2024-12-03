using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;
using System.Reflection;
using UnityEngine;

namespace Pathfixer
{
    static class CharacterFootPositionFix
    {
        static bool _tempDisablePatches;

        public static void Init()
        {
            MethodInfo CharacterBody_get_corePosition = AccessTools.DeclaredPropertyGetter(typeof(CharacterBody), nameof(CharacterBody.corePosition));
            if (CharacterBody_get_corePosition != null)
            {
                new ILHook(CharacterBody_get_corePosition, CharacterBody_CorePositionFix);
            }
            else
            {
                Log.Error("Failed to find CharacterBody.get_corePosition method");
            }

            MethodInfo CharacterBody_get_footPosition = AccessTools.DeclaredPropertyGetter(typeof(CharacterBody), nameof(CharacterBody.footPosition));
            if (CharacterBody_get_footPosition != null)
            {
                new ILHook(CharacterBody_get_footPosition, CharacterBody_CorePositionFix);
            }
            else
            {
                Log.Error("Failed to find CharacterBody.get_footPosition method");
            }
        }

        public static Vector3 GetUnmodifiedFootPosition(CharacterBody body)
        {
            _tempDisablePatches = true;
            try
            {
                return body.footPosition;
            }
            finally
            {
                _tempDisablePatches = false;
            }
        }

        public static Vector3 GetUnmodifiedCorePosition(CharacterBody body)
        {
            _tempDisablePatches = true;
            try
            {
                return body.corePosition;
            }
            finally
            {
                _tempDisablePatches = false;
            }
        }

        static void CharacterBody_CorePositionFix(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryFindNext(out ILCursor[] cursors,
                              x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.transform)),
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(Transform), nameof(Transform.position)))))
            {
                ILCursor cursor = cursors[cursors.Length - 1];

                cursor.Index++;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(getColliderCenterPosition);
                static Vector3 getColliderCenterPosition(Vector3 origPosition, CharacterBody body)
                {
                    if (!_tempDisablePatches)
                    {
                        CharacterMotor characterMotor = body.characterMotor;
                        if (characterMotor && characterMotor.capsuleCollider)
                        {
                            return characterMotor.capsuleCollider.transform.TransformPoint(characterMotor.capsuleCollider.center);
                        }
                    }

                    return origPosition;
                }
            }
            else
            {
                Log.Error("Failed to find patch location");
            }
        }
    }
}
