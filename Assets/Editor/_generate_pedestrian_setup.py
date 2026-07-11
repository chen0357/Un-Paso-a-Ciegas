#!/usr/bin/env python3
"""Generate pedestrian capsule prefabs and inject PedestrianSystem into Level_Street.unity."""

from __future__ import annotations

import os

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PREFAB_DIR = os.path.join(ROOT, "Assets", "Prefab", "Pedestrians")
SCENE_PATH = os.path.join(ROOT, "Assets", "Localizacion", "Level_Street.unity")

WALKER_GUID = "e7c8d9f0a1b2345678901234abcdef01"
DAMAGE_GUID = "69b0d7d0d0d71f947b729747878c21b9"
SPAWNER_GUID = "f8d9e0a1b2c3456789012345abcdef01"
HIT_CLIP_GUID = "9e7e527deea5fe84a854328f3b262128"

PREFABS = [
    ("Pedestrian_Capsule_Blue", "b1c2d3e4f5a6789012345678abcdef01", (0.25, 0.55, 0.95, 1.0)),
    ("Pedestrian_Capsule_Green", "c2d3e4f5a6b6789012345678abcdef02", (0.35, 0.85, 0.45, 1.0)),
    ("Pedestrian_Capsule_Red", "d3e4f5a6b7c6789012345678abcdef03", (0.95, 0.45, 0.35, 1.0)),
]


def write_meta(path: str, guid: str, folder: bool = False) -> None:
    os.makedirs(os.path.dirname(path), exist_ok=True)
    if folder:
        content = (
            "fileFormatVersion: 2\n"
            f"guid: {guid}\n"
            "folderAsset: yes\n"
            "DefaultImporter:\n"
            "  externalObjects: {}\n"
            "  userData:\n"
            "  assetBundleName:\n"
            "  assetBundleVariant:\n"
        )
    else:
        content = (
            "fileFormatVersion: 2\n"
            f"guid: {guid}\n"
            "PrefabImporter:\n"
            "  externalObjects: {}\n"
            "  userData:\n"
            "  assetBundleName:\n"
            "  assetBundleVariant:\n"
        )
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(content)


def color_block(r: float, g: float, b: float, a: float, mat_id: int) -> str:
    return f"""--- !u!21 &{mat_id}
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: CapsuleMaterial
  m_Shader: {{fileID: 4800000, guid: 933532a4fcc9baf4fa049bdeb0d8756b, type: 3}}
  m_Parent: {{fileID: 0}}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap:
    RenderType: Opaque
  disabledShaderPasses: []
  m_LockedProperties:
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - unity_Lightmaps:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - unity_LightmapsInd:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    - unity_ShadowMasks:
        m_Texture: {{fileID: 0}}
        m_Scale: {{x: 1, y: 1}}
        m_Offset: {{x: 0, y: 0}}
    m_Ints: []
    m_Floats:
    - _Smoothness: 0.35
    m_Colors:
    - _BaseColor: {{r: {r}, g: {g}, b: {b}, a: {a}}}
    - _Color: {{r: {r}, g: {g}, b: {b}, a: {a}}}
  m_BuildTextureStacks: []
  m_AllowLocking: 1
"""


def build_prefab(name: str, color: tuple[float, float, float, float], ids: dict[str, int]) -> str:
    r, g, b, a = color
    return f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &{ids['root_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ids['root_tr']}}}
  - component: {{fileID: {ids['mesh_filter']}}}
  - component: {{fileID: {ids['mesh_renderer']}}}
  - component: {{fileID: {ids['body_col']}}}
  - component: {{fileID: {ids['walker']}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{ids['root_tr']}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['root_go']}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 0.55, y: 1, z: 0.55}}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {{fileID: {ids['child_tr']}}}
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!33 &{ids['mesh_filter']}
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['root_go']}}}
  m_Mesh: {{fileID: 10208, guid: 0000000000000000e000000000000000, type: 0}}
--- !u!23 &{ids['mesh_renderer']}
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['root_go']}}}
  m_Enabled: 1
  m_CastShadows: 1
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RayTraceProcedural: 0
  m_RayTracingAccelStructBuildFlagsOverride: 0
  m_RayTracingAccelStructBuildFlags: 1
  m_SmallMeshCulling: 1
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: {ids['material']}}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 3
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_AdditionalVertexStreams: {{fileID: 0}}
--- !u!136 &{ids['body_col']}
CapsuleCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['root_go']}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 0
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 2
  m_Radius: 0.28
  m_Height: 2
  m_Direction: 1
  m_Center: {{x: 0, y: 0, z: 0}}
--- !u!114 &{ids['walker']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['root_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {WALKER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  speed: 1.8
  followDistance: 1.5
  slowdownDistance: 4
  minFollowSpeed: 0.4
  spawnOffset: {{x: 0, y: 0, z: 0}}
  despawnPoint: {{fileID: 0}}
--- !u!1 &{ids['child_go']}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {ids['child_tr']}}}
  - component: {{fileID: {ids['trigger_col']}}}
  - component: {{fileID: {ids['damage']}}}
  m_Layer: 0
  m_Name: Damage Trigger
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{ids['child_tr']}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['child_go']}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {ids['root_tr']}}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!136 &{ids['trigger_col']}
CapsuleCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['child_go']}}}
  m_Material: {{fileID: 0}}
  m_IncludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_ExcludeLayers:
    serializedVersion: 2
    m_Bits: 0
  m_LayerOverridePriority: 0
  m_IsTrigger: 1
  m_ProvidesContacts: 0
  m_Enabled: 1
  serializedVersion: 2
  m_Radius: 0.35
  m_Height: 2.1
  m_Direction: 1
  m_Center: {{x: 0, y: 0, z: 0}}
--- !u!114 &{ids['damage']}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {ids['child_go']}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {DAMAGE_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  damageAmount: 5
  damageCooldown: 1
  hitHintMessage: Peatón
  hitClip: {{fileID: 8300000, guid: {HIT_CLIP_GUID}, type: 3}}
  hitVolume: 1
{color_block(r, g, b, a, ids['material'])}"""


def make_ids(base: int) -> dict[str, int]:
    keys = [
        "root_go", "root_tr", "mesh_filter", "mesh_renderer", "body_col", "walker",
        "child_go", "child_tr", "trigger_col", "damage", "material",
    ]
    return {key: base + i for i, key in enumerate(keys)}


def create_prefabs() -> list[tuple[str, str, int]]:
    os.makedirs(PREFAB_DIR, exist_ok=True)
    write_meta(os.path.join(PREFAB_DIR, "Pedestrians.meta"), "e1f2a3b4c5d6789012345678abcdef99", folder=True)

    created: list[tuple[str, str, int]] = []
    for index, (name, guid, color) in enumerate(PREFABS):
        ids = make_ids(920200000 + index * 100)
        prefab_path = os.path.join(PREFAB_DIR, f"{name}.prefab")
        with open(prefab_path, "w", encoding="utf-8", newline="\n") as f:
            f.write(build_prefab(name, color, ids))
        write_meta(prefab_path + ".meta", guid)
        created.append((name, guid, ids["root_go"]))
        print(f"Created prefab: {prefab_path}")
    return created


def prefab_ref(guid: str, file_id: int) -> str:
    return f"{{fileID: {file_id}, guid: {guid}, type: 3}}"


def spawner_block(name: str, go_id: int, tr_id: int, mb_id: int, prefabs: list[tuple[str, str, int]],
                  lanes: list[tuple[str, int, int]], spawn_interval: float,
                  min_speed: float, max_speed: float, max_active: int, flow_axis: int) -> str:
    prefab_lines = "\n".join(f"  - {prefab_ref(guid, file_id)}" for _, guid, file_id in prefabs)
    lane_lines = "\n".join(
        f"  - laneName: {lane_name}\n"
        f"    spawnPoint: {{fileID: {spawn_id}}}\n"
        f"    despawnPoint: {{fileID: {despawn_id}}}\n"
        f"    spawnInterval: 0\n"
        f"    initialDelay: 0\n"
        f"    speed: 0\n"
        f"    maxActiveCars: 0"
        for lane_name, spawn_id, despawn_id in lanes
    )
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  - component: {{fileID: {mb_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 920000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!114 &{mb_id}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {SPAWNER_GUID}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
  pedestrianPrefabs:
{prefab_lines}
  pedestrianPrefab: {prefab_ref(prefabs[0][1], prefabs[0][2])}
  lanes:
{lane_lines}
  spawnInterval: {spawn_interval}
  initialDelay: 2
  minSpeed: {min_speed}
  maxSpeed: {max_speed}
  maxActivePedestrians: {max_active}
  spawnPoint: {{fileID: 0}}
  despawnPoint: {{fileID: 0}}
  flowAxis: {flow_axis}
  releaseDelay: 0.5
  lateralOffsetRange: 0.75
"""


def spawn_point(name: str, go_id: int, tr_id: int, parent_id: int, pos: tuple[float, float, float],
                rot: tuple[float, float, float, float], euler: tuple[float, float, float]) -> str:
    x, y, z = pos
    qx, qy, qz, qw = rot
    ex, ey, ez = euler
    return f"""--- !u!1 &{go_id}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr_id}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr_id}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go_id}}}
  serializedVersion: 2
  m_LocalRotation: {{x: {qx}, y: {qy}, z: {qz}, w: {qw}}}
  m_LocalPosition: {{x: {x}, y: {y}, z: {z}}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: {parent_id}}}
  m_LocalEulerAnglesHint: {{x: {ex}, y: {ey}, z: {ez}}}
"""


def build_scene_block(prefabs: list[tuple[str, str, int]]) -> str:
    parts = [
        """--- !u!1 &920000001
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 920000002}
  m_Layer: 0
  m_Name: PedestrianSystem
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &920000002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 920000001}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 920000004}
  - {fileID: 920000021}
  - {fileID: 920000010}
  - {fileID: 920000027}
  m_Father: {fileID: 0}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
--- !u!1 &920000003
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 920000004}
  m_Layer: 0
  m_Name: Lane_NS
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &920000004
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 920000003}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 920000006}
  - {fileID: 920000008}
  - {fileID: 920000031}
  - {fileID: 920000033}
  - {fileID: 920000041}
  - {fileID: 920000043}
  - {fileID: 920000045}
  - {fileID: 920000047}
  m_Father: {fileID: 920000002}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
""",
        spawn_point("PedSpawn_NS_West_N", 920000005, 920000006, 920000004, (-8, 1, -149.5), (0, 0, 0, 1), (0, 0, 0)),
        spawn_point("PedDespawn_NS_West_N", 920000007, 920000008, 920000004, (-8, 1, 144.2), (0, 0, 0, 1), (0, 0, 0)),
        spawn_point("PedSpawn_NS_West_S", 920000040, 920000041, 920000004, (-8, 1, 144.2), (0, 1, 0, 0), (0, 180, 0)),
        spawn_point("PedDespawn_NS_West_S", 920000042, 920000043, 920000004, (-8, 1, -149.5), (0, 1, 0, 0), (0, 180, 0)),
        spawn_point("PedSpawn_NS_East_N", 920000044, 920000045, 920000004, (8, 1, -149.5), (0, 0, 0, 1), (0, 0, 0)),
        spawn_point("PedDespawn_NS_East_N", 920000046, 920000047, 920000004, (8, 1, 144.2), (0, 0, 0, 1), (0, 0, 0)),
        spawn_point("PedSpawn_NS_East_S", 920000030, 920000031, 920000004, (8, 1, 144.2), (0, 1, 0, 0), (0, 180, 0)),
        spawn_point("PedDespawn_NS_East_S", 920000032, 920000033, 920000004, (8, 1, -149.5), (0, 1, 0, 0), (0, 180, 0)),
        spawner_block("Spawner_NS", 920000009, 920000010, 920000011, prefabs, [
            ("West Sidewalk Northbound", 920000006, 920000008),
            ("West Sidewalk Southbound", 920000041, 920000043),
            ("East Sidewalk Northbound", 920000045, 920000047),
            ("East Sidewalk Southbound", 920000031, 920000033),
        ], 7, 1, 3.2, 5, 0),
        """--- !u!1 &920000020
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 920000021}
  m_Layer: 0
  m_Name: Lane_EW
  m_TagString: Untagged
  m_Icon: {fileID: 0}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &920000021
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 920000020}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_ConstrainProportionsScale: 0
  m_Children:
  - {fileID: 920000023}
  - {fileID: 920000025}
  - {fileID: 920000035}
  - {fileID: 920000037}
  - {fileID: 920000049}
  - {fileID: 920000051}
  - {fileID: 920000053}
  - {fileID: 920000055}
  m_Father: {fileID: 920000002}
  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}
""",
        spawn_point("PedSpawn_EW_North_E", 920000022, 920000023, 920000021, (-142.7, 1, 8), (0, 0.7071068, 0, 0.7071068), (0, 90, 0)),
        spawn_point("PedDespawn_EW_North_E", 920000024, 920000025, 920000021, (149.9, 1, 8), (0, 0.7071068, 0, 0.7071068), (0, 90, 0)),
        spawn_point("PedSpawn_EW_North_W", 920000048, 920000049, 920000021, (149.9, 1, 8), (0, -0.7071068, 0, 0.7071068), (0, 270, 0)),
        spawn_point("PedDespawn_EW_North_W", 920000050, 920000051, 920000021, (-142.7, 1, 8), (0, -0.7071068, 0, 0.7071068), (0, 270, 0)),
        spawn_point("PedSpawn_EW_South_W", 920000034, 920000035, 920000021, (149.9, 1, -8), (0, -0.7071068, 0, 0.7071068), (0, 270, 0)),
        spawn_point("PedDespawn_EW_South_W", 920000036, 920000037, 920000021, (-142.7, 1, -8), (0, -0.7071068, 0, 0.7071068), (0, 270, 0)),
        spawn_point("PedSpawn_EW_South_E", 920000052, 920000053, 920000021, (-142.7, 1, -8), (0, 0.7071068, 0, 0.7071068), (0, 90, 0)),
        spawn_point("PedDespawn_EW_South_E", 920000054, 920000055, 920000021, (149.9, 1, -8), (0, 0.7071068, 0, 0.7071068), (0, 90, 0)),
        spawner_block("Spawner_EW", 920000026, 920000027, 920000028, prefabs, [
            ("North Sidewalk Eastbound", 920000023, 920000025),
            ("North Sidewalk Westbound", 920000049, 920000051),
            ("South Sidewalk Westbound", 920000035, 920000037),
            ("South Sidewalk Eastbound", 920000053, 920000055),
        ], 6, 1, 3.2, 5, 1),
    ]
    return "\n".join(parts)


def patch_scene(prefabs: list[tuple[str, str, int]]) -> None:
    with open(SCENE_PATH, "r", encoding="utf-8") as f:
        scene = f.read()

    if "PedestrianSystem" in scene:
        start = scene.index("--- !u!1 &920000001")
        end = scene.index("--- !u!1660057539 &9223372036854775807")
        scene = scene[:start] + scene[end:]
        scene = scene.replace("  - {fileID: 920000002}\n", "", 1)

    marker = "--- !u!1660057539 &9223372036854775807"
    block = build_scene_block(prefabs)
    scene = scene.replace(marker, block + marker, 1)

    roots_marker = "  - {fileID: 910000002}\n"
    scene = scene.replace(roots_marker, roots_marker + "  - {fileID: 920000002}\n", 1)

    with open(SCENE_PATH, "w", encoding="utf-8", newline="\n") as f:
        f.write(scene)
    print(f"Patched scene: {SCENE_PATH}")


def main() -> None:
    prefabs = create_prefabs()
    patch_scene(prefabs)
    print("Pedestrian setup complete.")


if __name__ == "__main__":
    main()
