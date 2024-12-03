using RoR2;
using UnityEngine;

namespace Pathfixer
{
#if DEBUG
    static class CharacterOriginVisualizer
    {
        [ConCommand(commandName = "toggle_character_origin_visualization")]
        static void CCToggleVisualization(ConCommandArgs args)
        {
            setVisualizationActive(!_active);
        }

        static bool _active;

        static void setVisualizationActive(bool active)
        {
            if (_active == active)
                return;

            _active = active;

            foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
            {
                setVisualizationActive(body, _active);
            }

            if (_active)
            {
                CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            }
            else
            {
                CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
            }
        }

        static void CharacterBody_onBodyStartGlobal(CharacterBody obj)
        {
            setVisualizationActive(obj, _active);
        }

        static void setVisualizationActive(CharacterBody body, bool active)
        {
            if (body.TryGetComponent(out OriginVisualizerController visualizerController))
            {
                visualizerController.enabled = active;
            }
            else
            {
                if (active)
                {
                    body.gameObject.AddComponent<OriginVisualizerController>();
                }
            }
        }

        class OriginVisualizerController : MonoBehaviour
        {
            static Mesh _corePositionMesh;
            static Mesh _unmodifiedCorePositionMesh;

            static Mesh _footPositionMesh;
            static Mesh _unmodifiedFootPositionMesh;

            static void initMeshesIfNeeded()
            {
                if (!_corePositionMesh)
                {
                    using WireMeshBuilder builder = new WireMeshBuilder();
                    builder.AddLine(Vector3.zero, Color.magenta, Vector3.forward, Color.magenta);

                    _corePositionMesh = builder.GenerateMesh();
                }

                if (!_unmodifiedCorePositionMesh)
                {
                    using WireMeshBuilder builder = new WireMeshBuilder();
                    builder.AddLine(Vector3.zero, Color.yellow, Vector3.forward, Color.yellow);

                    _unmodifiedCorePositionMesh = builder.GenerateMesh();
                }

                if (!_footPositionMesh)
                {
                    using WireMeshBuilder builder = new WireMeshBuilder();
                    builder.AddLine(Vector3.zero, Color.green, Vector3.forward * 0.5f, Color.green);

                    _footPositionMesh = builder.GenerateMesh();
                }

                if (!_unmodifiedFootPositionMesh)
                {
                    using WireMeshBuilder builder = new WireMeshBuilder();
                    builder.AddLine(Vector3.zero, Color.red, Vector3.forward * 0.5f, Color.red);

                    _unmodifiedFootPositionMesh = builder.GenerateMesh();
                }
            }

            CharacterBody _body;

            DebugOverlay.MeshDrawer _corePositionDrawer;
            DebugOverlay.MeshDrawer _unmodifiedCorePositionDrawer;

            DebugOverlay.MeshDrawer _footPositionDrawer;
            DebugOverlay.MeshDrawer _unmodifiedFootPositionDrawer;

            void Awake()
            {
                _body = GetComponent<CharacterBody>();
            }

            void OnEnable()
            {
                initMeshesIfNeeded();

                _corePositionDrawer = DebugOverlay.GetMeshDrawer();
                _corePositionDrawer.mesh = _corePositionMesh;

                _unmodifiedCorePositionDrawer = DebugOverlay.GetMeshDrawer();
                _unmodifiedCorePositionDrawer.mesh = _unmodifiedCorePositionMesh;

                _footPositionDrawer = DebugOverlay.GetMeshDrawer();
                _footPositionDrawer.mesh = _footPositionMesh;

                _unmodifiedFootPositionDrawer = DebugOverlay.GetMeshDrawer();
                _unmodifiedFootPositionDrawer.mesh = _unmodifiedFootPositionMesh;
            }

            void OnDisable()
            {
                _corePositionDrawer?.Dispose();
                _unmodifiedCorePositionDrawer?.Dispose();

                _footPositionDrawer?.Dispose();
                _unmodifiedFootPositionDrawer?.Dispose();
            }

            void Update()
            {
                if (!_body)
                    return;

                Vector3 characterForward = _body.transform.forward;
                if (_body.characterDirection)
                {
                    characterForward = _body.characterDirection.forward;
                }

                if (_corePositionDrawer != null)
                {
                    _corePositionDrawer.transform.position = _body.corePosition;
                    _corePositionDrawer.transform.forward = characterForward;
                }

                if (_unmodifiedCorePositionDrawer != null)
                {
                    Vector3 unmodifiedCorePosition = CharacterFootPositionFix.GetUnmodifiedCorePosition(_body);
                    bool shouldShowUnmodifiedCorePosition = unmodifiedCorePosition != _body.corePosition;
                    if (shouldShowUnmodifiedCorePosition)
                    {
                        _unmodifiedCorePositionDrawer.transform.position = unmodifiedCorePosition;
                        _unmodifiedCorePositionDrawer.transform.forward = characterForward;
                    }

                    _unmodifiedCorePositionDrawer.enabled = shouldShowUnmodifiedCorePosition;
                }

                if (_footPositionDrawer != null)
                {
                    _footPositionDrawer.transform.position = _body.footPosition;
                    _footPositionDrawer.transform.forward = characterForward;
                }

                if (_unmodifiedFootPositionDrawer != null)
                {
                    Vector3 unmodifiedFootPosition = CharacterFootPositionFix.GetUnmodifiedFootPosition(_body);
                    bool shouldShowUnmodifiedFootPosition = unmodifiedFootPosition != _body.footPosition;
                    if (shouldShowUnmodifiedFootPosition)
                    {
                        _unmodifiedFootPositionDrawer.transform.position = unmodifiedFootPosition;
                        _unmodifiedFootPositionDrawer.transform.forward = characterForward;
                    }

                    _unmodifiedFootPositionDrawer.enabled = shouldShowUnmodifiedFootPosition;
                }
            }
        }
    }
#endif
}
