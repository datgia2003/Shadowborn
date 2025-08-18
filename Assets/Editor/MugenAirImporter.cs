// // ================================
// // FILE: Assets/Editor/MugenAirImporter.cs
// // DESCRIPTION: One-click importer for M.U.G.E.N .air + .sff (v2) → Unity AnimationClip
// // REQUIREMENTS: Unity 2020+; Place this file and the parser files in an Editor folder.
// // Supports: SFF v2 (PNG/JPG packed). For SFF v1, see notes at the end.
// // ================================
// using UnityEngine;
// using UnityEditor;
// using System.Collections.Generic;
// using System.IO;

// public class MugenAirImporter : EditorWindow
// {
//     [Header("Inputs")]
//     public Object airFile;               // .air
//     public Object sffFile;               // .sff (v2)
//     public string outputFolder = "Assets/MugenImported";
//     public float tickDuration = 1f / 60f;   // 1 tick ~ 1/60s
//     public int defaultPPU = 100;            // Pixel Per Unit for created Sprites

//     [Header("Action ID to import (optional)")]
//     public string actionIdFilter = "";  // e.g., "1100"; empty = import all actions found

//     [MenuItem("Tools/MUGEN/AIR + SFFv2 Importer")]
//     public static void Open() => GetWindow<MugenAirImporter>("MUGEN Importer");

//     void OnGUI()
//     {
//         GUILayout.Label("M.U.G.E.N → Unity (AIR + SFFv2)", EditorStyles.boldLabel);
//         EditorGUILayout.HelpBox("This tool imports .air animations and reads sprites from .sff (v2) with embedded PNG/JPG. It will extract sprites as assets and build AnimationClips matching offsets, scales, blank frames, and loop points.", MessageType.Info);

//         airFile = EditorGUILayout.ObjectField("AIR File", airFile, typeof(Object), false);
//         sffFile = EditorGUILayout.ObjectField("SFF File (v2)", sffFile, typeof(Object), false);
//         outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
//         tickDuration = EditorGUILayout.FloatField("Tick Duration (sec)", tickDuration);
//         defaultPPU = EditorGUILayout.IntField("Default PPU", defaultPPU);
//         actionIdFilter = EditorGUILayout.TextField("Action ID filter", actionIdFilter);

//         GUILayout.Space(6);
//         if (GUILayout.Button("Import Now", GUILayout.Height(32)))
//         {
//             Import();
//         }

//         GUILayout.Space(8);
//         EditorGUILayout.HelpBox("SFF v1 note: This tool currently targets SFF v2 (MUGEN 1.1). If your character uses SFF v1, please export sprites to PNG via Fighter Factory (recommended), or extend the parser to decode PCX (8-bit RLE).", MessageType.None);
//     }

//     void Import()
//     {
//         if (airFile == null)
//         {
//             Debug.LogError("AIR file not set.");
//             return;
//         }
//         string airPath = AssetDatabase.GetAssetPath(airFile);
//         if (string.IsNullOrEmpty(airPath)) { Debug.LogError("Invalid AIR file."); return; }

//         string sffPath = sffFile ? AssetDatabase.GetAssetPath(sffFile) : null;
//         if (string.IsNullOrEmpty(sffPath)) { Debug.LogError("SFF file not set or invalid."); return; }

//         Directory.CreateDirectory(outputFolder);

//         // Parse AIR
//         var actions = MugenAirParser.ParseActions(airPath);
//         if (!string.IsNullOrEmpty(actionIdFilter))
//         {
//             if (actions.ContainsKey(actionIdFilter) == false)
//             {
//                 Debug.LogError($"Action {actionIdFilter} not found in AIR.");
//                 return;
//             }
//             // Keep only that action
//             var single = new Dictionary<string, MugenAirParser.ActionData>();
//             single[actionIdFilter] = actions[actionIdFilter];
//             actions = single;
//         }

//         // Parse SFF v2 and extract sprites as Texture2D assets
//         var sffBytes = File.ReadAllBytes(sffPath);
//         var sff = new MugenSffV2Parser(sffBytes);
//         sff.Parse();

//         // Create a folder for extracted sprites
//         string spritesFolder = Path.Combine(outputFolder, "Sprites");
//         Directory.CreateDirectory(spritesFolder);

//         // Build a map (group,index) → Sprite asset
//         var spriteMap = new Dictionary<(int group, int index), Sprite>();

//         foreach (var kv in sff.Subfiles)
//         {
//             var key = (kv.Value.GroupNumber, kv.Value.ImageNumber);
//             if (kv.Value.ImageBytes == null || kv.Value.ImageBytes.Length == 0) continue;

//             // Create/Load Texture2D asset from bytes
//             string pngPath = Path.Combine(spritesFolder, $"{key.group}_{key.index}.png");
//             pngPath = pngPath.Replace("\\", "/");
//             File.WriteAllBytes(pngPath, kv.Value.ImageBytes);
//         }
//         AssetDatabase.Refresh();

//         // After refresh, create Sprites from saved PNGs
//         foreach (var kv in sff.Subfiles)
//         {
//             var key = (kv.Value.GroupNumber, kv.Value.ImageNumber);
//             string pngPath = Path.Combine(spritesFolder, $"{key.group}_{key.index}.png").Replace("\\", "/");
//             if (!File.Exists(pngPath)) continue;

//             var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
//             if (importer != null)
//             {
//                 importer.textureType = TextureImporterType.Sprite;
//                 importer.spriteImportMode = SpriteImportMode.Single;
//                 importer.filterMode = FilterMode.Point;
//                 importer.spritePixelsPerUnit = defaultPPU;
//                 importer.mipmapEnabled = false;
//                 importer.SaveAndReimport();
//             }

//             var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
//             if (tex)
//             {
//                 var spr = AssetDatabase.LoadAssetAtPath<Sprite>(pngPath);
//                 // If direct load as Sprite fails (older Unity), create one from Texture2D
//                 if (spr == null)
//                 {
//                     // Try to get the default sprite sub-asset
//                     var subAssets = AssetDatabase.LoadAllAssetsAtPath(pngPath);
//                     foreach (var a in subAssets)
//                         if (a is Sprite) { spr = (Sprite)a; break; }
//                 }
//                 if (spr)
//                 {
//                     var mapKey = (kv.Value.GroupNumber, kv.Value.ImageNumber);
//                     spriteMap[mapKey] = spr;
//                 }
//             }
//         }

//         // Create an Animations folder
//         string animFolder = Path.Combine(outputFolder, "Animations").Replace("\\", "/");
//         Directory.CreateDirectory(animFolder);

//         // Generate clips per action
//         foreach (var pair in actions)
//         {
//             string actionId = pair.Key;
//             var data = pair.Value;

//             var clip = new AnimationClip();
//             clip.frameRate = 60f;

//             // Bindings
//             var spriteBinding = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
//             var posXBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.x");
//             var posYBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalPosition.y");
//             var scaleXBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x");
//             var scaleYBinding = EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y");
//             var enableBinding = EditorCurveBinding.FloatCurve("", typeof(SpriteRenderer), "m_Enabled");

//             var spriteKeys = new List<ObjectReferenceKeyframe>();
//             var posXKeys = new List<Keyframe>();
//             var posYKeys = new List<Keyframe>();
//             var scaleXKeys = new List<Keyframe>();
//             var scaleYKeys = new List<Keyframe>();
//             var enableKeys = new List<Keyframe>();

//             float t = 0f;

//             for (int i = 0; i < data.frames.Count; i++)
//             {
//                 var f = data.frames[i];

//                 // Sprite (blank if group == -1)
//                 Sprite s = null;
//                 float enabled = 1f;
//                 if (f.group == -1)
//                 {
//                     s = null; enabled = 0f; // blank frame
//                 }
//                 else
//                 {
//                     spriteMap.TryGetValue((f.group, f.index), out s);
//                     if (s == null) enabled = 0f; // missing sprite → treat as blank
//                 }

//                 spriteKeys.Add(new ObjectReferenceKeyframe { time = t, value = s });
//                 enableKeys.Add(new Keyframe(t, enabled));

//                 // Offset: note MUGEN Y down → Unity Y up
//                 float x = f.offset.x / data.pixelsPerUnit;
//                 float y = -f.offset.y / data.pixelsPerUnit;
//                 posXKeys.Add(new Keyframe(t, x));
//                 posYKeys.Add(new Keyframe(t, y));

//                 // Scale
//                 scaleXKeys.Add(new Keyframe(t, f.scale.x));
//                 scaleYKeys.Add(new Keyframe(t, f.scale.y));

//                 // Interpolate Scale: Unity will tween between keyframes automatically.

//                 // Advance time by duration (ticks → seconds)
//                 float dt = Mathf.Max(0, f.duration) * tickDuration;
//                 // Ensure at least tiny step to register key order
//                 if (dt <= 0f) dt = 1f / 6000f;
//                 t += dt;
//             }

//             AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeys.ToArray());
//             AnimationUtility.SetEditorCurve(clip, posXBinding, new AnimationCurve(posXKeys.ToArray()));
//             AnimationUtility.SetEditorCurve(clip, posYBinding, new AnimationCurve(posYKeys.ToArray()));
//             AnimationUtility.SetEditorCurve(clip, scaleXBinding, new AnimationCurve(scaleXKeys.ToArray()));
//             AnimationUtility.SetEditorCurve(clip, scaleYBinding, new AnimationCurve(scaleYKeys.ToArray()));
//             AnimationUtility.SetEditorCurve(clip, enableBinding, new AnimationCurve(enableKeys.ToArray()));

//             // Loop settings
//             var settings = AnimationUtility.GetAnimationClipSettings(clip);
//             settings.loopTime = data.loopStartIndex >= 0; // loop if LoopStart present
//             AnimationUtility.SetAnimationClipSettings(clip, settings);

//             string clipPath = Path.Combine(animFolder, $"Action_{actionId}.anim").Replace("\\", "/");
//             AssetDatabase.CreateAsset(clip, clipPath);
//         }

//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();

//         EditorUtility.DisplayDialog("MUGEN Import", "Import completed! Check the output folder for Sprites and AnimationClips.", "OK");
//     }
// }