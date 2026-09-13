import sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from guid_fixer import build_guid_map, process_project_files

BASE = "/Users/juelz/Documents/GitHub/Redmatch Source Code"
OLD = f"{BASE}/Assets/Scripts/UnityPackages"
CACHE = f"{BASE}/Library/PackageCache"

pairs = [
    ("Probuilder", "com.unity.probuilder@8be9724b2335"),
    ("Unity.Postprocessing.Runtime", "com.unity.postprocessing@141166b789e3"),
    ("Unity.Burst", "com.unity.burst@2f3d916873ea"),
    ("Unity.Burst.Unsafe", "com.unity.burst@2f3d916873ea"),
    ("Unity.Collections", "com.unity.collections@84e8eb3fc12e"),
    ("Unity.Collections.LowLevel.ILSupport", "com.unity.collections@84e8eb3fc12e"),
    ("Unity.InputSystem", "com.unity.inputsystem@7a4e1a2a8194"),
    ("Unity.InputSystem.ForUI", "com.unity.inputsystem@7a4e1a2a8194"),
    ("Unity.Mathematics", "com.unity.mathematics@19a9377c4ffa"),
    ("Unity.RenderPipeline.Universal.ShaderLibrary", "com.unity.render-pipelines.universal@7bda08004641"),
    ("Unity.RenderPipelines.Core.Runtime", "com.unity.render-pipelines.core@36dd213002aa"),
    ("Unity.RenderPipelines.Core.Runtime.Shared", "com.unity.render-pipelines.core@36dd213002aa"),
    ("Unity.RenderPipelines.GPUDriven.Runtime", "com.unity.render-pipelines.core@36dd213002aa"),
    ("Unity.RenderPipelines.Universal.Runtime", "com.unity.render-pipelines.universal@7bda08004641"),
    ("Unity.Splines", "com.unity.splines@e6e1d5900acc"),
    ("Unity.TextMeshPro", "com.unity.textmeshpro@53f43edfd2e0"),
    ("Unity.TextMeshPro", "com.unity.ugui@4433950e33ba"),
    ("Unity.VisualScripting.Core", "com.unity.visualscripting@4edfcc09512f"),
    ("Unity.XR.Management", "com.unity.xr.management@2ad85c7dd480"),
    ("Unity.XR.Oculus", "com.unity.xr.oculus@bd9286b5058f"),
    ("UnityEngine.SpatialTracking", "com.unity.xr.legacyinputhelpers@3f62d634f63b"),
    ("UnityEngine.UI", "com.unity.ugui@4433950e33ba"),
]

combined = {}
for old_name, cache_name in pairs:
    incorrect = f"{OLD}/{old_name}"
    correct = f"{CACHE}/{cache_name}"
    m = build_guid_map(incorrect, correct)
    print(f"{old_name} -> {cache_name}: {len(m)} mapped")
    combined.update(m)

extra_pairs = [
    (f"{BASE}/Assets/Scripts/ProBuilderCore", f"{CACHE}/com.unity.probuilder@8be9724b2335"),
    (f"{BASE}/Assets/Scripts/PhotonNetworking", f"{BASE}/Assets/Photon"),
]
for incorrect, correct in extra_pairs:
    m = build_guid_map(incorrect, correct)
    print(f"{incorrect} -> {correct}: {len(m)} mapped")
    combined.update(m)

import re
from pathlib import Path

def read_guid(meta_path):
    with open(meta_path, "r", encoding="utf-8", errors="ignore") as f:
        for line in f:
            if "guid:" in line:
                return line[line.find("guid:") + 6:].strip()[:32]
    return ""

compute_builtins = Path(f"{CACHE}/com.unity.postprocessing@141166b789e3/PostProcessing/Shaders/Builtins")
old_compute_dir = Path(f"{BASE}/Assets/ComputeShader")
name_pairs = [
    ("MultiScaleVODownsample1", "MultiScaleVODownsample1"),
    ("MultiScaleVODownsample2", "MultiScaleVODownsample2"),
    ("MultiScaleVORender", "MultiScaleVORender"),
    ("MultiScaleVOUpsample", "MultiScaleVOUpsample"),
]
compute_map = {}
for old_stem, new_stem in name_pairs:
    old_meta = old_compute_dir / f"{old_stem}.asset.meta"
    new_meta = compute_builtins / f"{new_stem}.compute.meta"
    if old_meta.exists() and new_meta.exists():
        old_guid = read_guid(old_meta)
        new_guid = read_guid(new_meta)
        if old_guid and new_guid:
            compute_map[old_guid] = new_guid
            print(f"Compute mapped: {old_guid} -> {new_guid} ({old_stem})")
combined.update(compute_map)

pp_root = Path(f"{CACHE}/com.unity.postprocessing@141166b789e3")
old_shader_dir = Path(f"{BASE}/Assets/Shader")
shader_name_pairs = [
    ("Hidden_PostProcessing_Bloom", "Bloom"),
    ("Hidden_PostProcessing_Copy", "Copy"),
    ("Hidden_PostProcessing_CopyStd", "CopyStd"),
    ("Hidden_PostProcessing_CopyStdFromTexArray", "CopyStdFromTexArray"),
    ("Hidden_PostProcessing_CopyStdFromDoubleWide", "CopyStdFromDoubleWide"),
    ("Hidden_PostProcessing_DiscardAlpha", "DiscardAlpha"),
    ("Hidden_PostProcessing_DepthOfField", "DepthOfField"),
    ("Hidden_PostProcessing_FinalPass", "FinalPass"),
    ("Hidden_PostProcessing_GrainBaker", "GrainBaker"),
    ("Hidden_PostProcessing_MotionBlur", "MotionBlur"),
    ("Hidden_PostProcessing_TemporalAntialiasing", "TemporalAntialiasing"),
    ("Hidden_PostProcessing_SubpixelMorphologicalAntialiasing", "SubpixelMorphologicalAntialiasing"),
    ("Hidden_PostProcessing_Texture2DLerp", "Texture2DLerp"),
    ("Hidden_PostProcessing_Uber", "Uber"),
    ("Hidden_PostProcessing_Lut2DBaker", "Lut2DBaker"),
    ("Hidden_PostProcessing_Debug_LightMeter", "LightMeter"),
    ("Hidden_PostProcessing_Debug_Histogram", "Histogram"),
    ("Hidden_PostProcessing_Debug_Waveform", "Waveform"),
    ("Hidden_PostProcessing_Debug_Vectorscope", "Vectorscope"),
    ("Hidden_PostProcessing_Debug_Overlays", "Overlays"),
    ("Hidden_PostProcessing_DeferredFog", "DeferredFog"),
    ("Hidden_PostProcessing_ScalableAO", "ScalableAO"),
    ("Hidden_PostProcessing_MultiScaleVO", "MultiScaleVO"),
    ("Hidden_PostProcessing_ScreenSpaceReflections", "ScreenSpaceReflections"),
]
shader_map = {}
new_meta_index = {p.name: p for p in pp_root.rglob("*.shader.meta")}
for old_stem, new_stem in shader_name_pairs:
    old_meta = old_shader_dir / f"{old_stem}.shader.meta"
    new_meta = new_meta_index.get(f"{new_stem}.shader.meta")
    if old_meta.exists() and new_meta and new_meta.exists():
        old_guid = read_guid(old_meta)
        new_guid = read_guid(new_meta)
        if old_guid and new_guid:
            shader_map[old_guid] = new_guid
            print(f"Shader mapped: {old_guid} -> {new_guid} ({old_stem} -> {new_stem})")
    else:
        print(f"Shader NOT found: {old_stem} -> {new_stem}")
combined.update(shader_map)

print(f"\nTotal combined mappings: {len(combined)}")

modified = process_project_files(f"{BASE}/Assets", combined)
print(f"\nTotal files modified: {modified}")
