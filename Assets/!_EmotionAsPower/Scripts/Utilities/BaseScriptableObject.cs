using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.IO;
using System.Linq;
using Object = UnityEngine.Object;
#endif

namespace LgTyUtils
{
    /// <summary>
    /// Abstract base class for ScriptableObject that support the string ID system and icon display
    /// Used AI in Editor script
    /// </summary>
    public abstract class BaseScriptableObject : ScriptableObject
    {
        [Header("Base properties")]
        [SerializeField]
        private string id = "";

        [Header("Visual")]
        [SerializeField]
        private Sprite icon;

        [SerializeField, HideInInspector]
        private string displayName;

        [SerializeField, HideInInspector]
        [TextArea(3, 5)]
        private string description;

        public string ID => id;
        public Sprite Icon => icon;
        public string DisplayName => displayName;
        public string Description => description;

#if UNITY_EDITOR
        private bool isInitializing = false;

        private void OnValidate()
        {
            // Prevent recursion and avoid running during asset import/compilation
            if (isInitializing || EditorApplication.isUpdating || EditorApplication.isCompiling)
                return;

            // Schedule asset name update for next editor update to avoid import restrictions
            if (!string.IsNullOrEmpty(displayName))
            {
                EditorApplication.delayCall += UpdateAssetName;
            }
        }

        private string ValidateAndCleanID(string inputID)
        {
            if (string.IsNullOrEmpty(inputID)) return inputID;

            // Remove invalid characters and convert to valid format
            string cleaned = inputID.Trim();

            // Replace spaces and invalid characters with underscores
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[^a-zA-Z0-9_]", "_");

            // Ensure it doesn't start with a number
            if (char.IsDigit(cleaned[0]))
            {
                cleaned = "_" + cleaned;
            }

            // Remove multiple consecutive underscores
            while (cleaned.Contains("__"))
            {
                cleaned = cleaned.Replace("__", "_");
            }

            // Remove leading/trailing underscores
            cleaned = cleaned.Trim('_');

            return cleaned;
        }

        private void CheckForIDConflicts()
        {
            if (string.IsNullOrEmpty(ID)) return;

            string[] conflictingAssets = GetAssetsWithSameID(ID);
            if (conflictingAssets.Length > 0)
            {
                Debug.LogWarning($"ID Conflict detected! Asset '{name}' has the same ID '{ID}' as {conflictingAssets.Length} other asset(s):\n" +
                               string.Join("\n", conflictingAssets.Select(path => $"- {AssetDatabase.LoadAssetAtPath<BaseScriptableObject>(path)?.name} at {path}")));
            }
        }

        private string[] GetAssetsWithSameID(string targetID)
        {
            // Find all BaseScriptableObject instances in the project
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BaseScriptableObject)}");
            var conflictingPaths = new System.Collections.Generic.List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<BaseScriptableObject>(path);

                // Check if it's a different asset with the same ID
                if (asset != null && asset != this && asset.ID == targetID)
                {
                    conflictingPaths.Add(path);
                }
            }

            return conflictingPaths.ToArray();
        }

        private string EnsureUniqueID(string baseID, string[] existingIDs)
        {
            string candidateID = baseID;
            int counter = 1;

            // Keep adding numbers until we find a unique ID
            while (existingIDs.Contains(candidateID))
            {
                candidateID = $"{baseID}_{counter:D3}";
                counter++;
            }

            return candidateID;
        }

        public bool ValidateID()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError($"Asset '{name}' has an empty ID");
                return false;
            }

            bool hasIssues = false;
            string originalID = id;

            // Validate and clean the ID
            string cleanedID = ValidateAndCleanID(id);
            if (cleanedID != id)
            {
                Debug.LogWarning($"ID '{originalID}' was cleaned to '{cleanedID}' for asset '{name}'");
                this.id = cleanedID;
                EditorUtility.SetDirty(this);
                hasIssues = true;
            }

            // Check for conflicts
            string[] conflictingAssets = GetAssetsWithSameID(id);
            if (conflictingAssets.Length > 0)
            {
                Debug.LogError($"ID Conflict! Asset '{name}' has the same ID '{id}' as {conflictingAssets.Length} other asset(s):\n" +
                             string.Join("\n", conflictingAssets.Select(path => $"- {AssetDatabase.LoadAssetAtPath<BaseScriptableObject>(path)?.name} at {path}")));
                hasIssues = true;
            }

            if (!hasIssues)
            {
                Debug.Log($"ID '{id}' for asset '{name}' is valid and unique!");
            }

            return !hasIssues;
        }

        public bool SetCustomID(string customID)
        {
            if (string.IsNullOrEmpty(customID))
            {
                Debug.LogError("Custom ID cannot be null or empty");
                return false;
            }

            // Validate and clean the custom ID
            string cleanedID = ValidateAndCleanID(customID);

            // Check if this ID is already in use
            string[] conflictingAssets = GetAssetsWithSameID(cleanedID);
            if (conflictingAssets.Length > 0)
            {
                Debug.LogError($"ID '{cleanedID}' is already in use by other assets. Please choose a different ID.");
                return false;
            }

            string oldID = this.ID;
            this.id = cleanedID;
            EditorUtility.SetDirty(this);

            Debug.Log($"Updated ID for {name}: '{oldID}' -> '{ID}'");
            return true;
        }

        private string[] GetAllExistingIDs()
        {
            // Find all ScriptableObject instances of the same EXACT type as this object in the project
            Type currentType = GetType();
            string[] guids = AssetDatabase.FindAssets($"t:{currentType.Name}");
            var existingIDs = new System.Collections.Generic.List<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                // Load the asset as the specific type
                var asset = AssetDatabase.LoadAssetAtPath(path, currentType) as BaseScriptableObject;

                // Only include IDs that are not empty and not the current object
                if (asset != null && asset != this && !string.IsNullOrEmpty(asset.ID))
                {
                    existingIDs.Add(asset.ID);
                }
            }

            return existingIDs.ToArray();
        }

        private void UpdateAssetName()
        {
            // Don't rename during asset importing or if display name is empty
            if (EditorApplication.isUpdating || EditorApplication.isCompiling || isInitializing)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(assetPath))
            {
                string newName;

                // If name is empty, set default asset name
                if (string.IsNullOrEmpty(displayName))
                {
                    newName = this.GetType().Name;
                }
                else
                {
                    newName = displayName;
                }
                newName += "SO";

                string currentFileName = Path.GetFileNameWithoutExtension(assetPath);

                // Only rename if the name is actually different
                if (currentFileName != newName)
                {
                    // Check if the target name already exists
                    string directory = Path.GetDirectoryName(assetPath);
                    string extension = Path.GetExtension(assetPath);
                    string targetPath = Path.Combine(directory, newName + extension);

                    // If target already exists, find a unique name
                    if (File.Exists(targetPath))
                    {
                        int counter = 1;
                        string baseNewName = newName;

                        do
                        {
                            newName = $"{baseNewName} {counter}";
                            targetPath = Path.Combine(directory, newName + extension);
                            counter++;
                        } while (File.Exists(targetPath));
                    }

                    string result = AssetDatabase.RenameAsset(assetPath, newName);
                    // Empty string means success
                    if (string.IsNullOrEmpty(result))
                    {
                        AssetDatabase.SaveAssets();
                        //Debug.Log($"Renamed asset from '{currentFileName}' to '{newName}'");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to rename asset: {result}");
                    }
                }
            }
        }

        public virtual void DrawCustomInspector()
        {
            // Base implementation - can be overridden
        }

        public virtual void OnEditorSelection()
        {
            // Base implementation - can be overridden
        }

        public override string ToString()
        {
            return $"{DisplayName} ({ID})";
        }
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BaseScriptableObject), true)]
    public class BaseScriptableObjectEditor : Editor
    {
        private BaseScriptableObject targetObject => target as BaseScriptableObject;
        private Sprite tempIcon;
        private string tempDisplayName;
        private string tempDescription;

        private bool isInitialized;
        private bool showIDSection = true;

        private void OnEnable()
        {
            tempIcon = targetObject.Icon;
            tempDisplayName = targetObject.DisplayName;
            tempDescription = targetObject.Description;
            isInitialized = true;
        }

        public override void OnInspectorGUI()
        {
            if (!isInitialized)
            {
                tempIcon = targetObject.Icon;
                tempDisplayName = targetObject.DisplayName;
                tempDescription = targetObject.Description;
                isInitialized = true;
            }

            serializedObject.Update();

            DrawHeaderSection();
            DrawIDSection();
            DrawBaseInformationSection();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(targetObject.GetType().Name, EditorStyles.boldLabel);

            // Draw the default inspector for derived class properties
            DrawPropertiesExcluding(serializedObject, "m_Script", "id", "icon", "displayName", "description");

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        // Draw header in inspector with icon, name and type
        protected virtual void DrawHeaderSection()
        {
            EditorGUILayout.BeginHorizontal();

            // Draw icon if available
            if (targetObject.Icon != null)
            {
                Rect iconRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
                DrawSprite(iconRect, targetObject.Icon);
            }
            else
            {
                // Placeholder for missing icon
                Rect iconRect = GUILayoutUtility.GetRect(64, 64, GUILayout.Width(64), GUILayout.Height(64));
                EditorGUI.DrawRect(iconRect, new Color(0.5f, 0.5f, 0.5f, 1f));
                GUI.Label(iconRect, "No Icon", EditorStyles.centeredGreyMiniLabel);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(targetObject.DisplayName, EditorStyles.largeLabel);
            EditorGUILayout.LabelField($"Type: {targetObject.GetType().Name}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        // Helper method to properly draw a sprite
        private void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null) return;

            Texture2D texture = sprite.texture;
            if (texture == null) return;

            // Get the sprite's rect in texture coordinates
            Rect spriteRect = sprite.rect;

            // Calculate UV coordinates for the sprite
            Vector2 uvMin = new Vector2(spriteRect.x / texture.width, spriteRect.y / texture.height);
            Vector2 uvMax = new Vector2((spriteRect.x + spriteRect.width) / texture.width,
                                       (spriteRect.y + spriteRect.height) / texture.height);

            // Draw the sprite using GUI.DrawTextureWithTexCoords
            GUI.DrawTextureWithTexCoords(rect, texture, new Rect(uvMin.x, uvMin.y, uvMax.x - uvMin.x, uvMax.y - uvMin.y));
        }

        // Draw ID information section
        protected virtual void DrawIDSection()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            showIDSection = EditorGUILayout.Foldout(showIDSection, "ID Information", true, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            if (showIDSection)
            {
                // Current ID display
                GUI.enabled = false;
                EditorGUILayout.TextField("Current ID", targetObject.ID);
                GUI.enabled = true;

                // ID input field
                EditorGUI.BeginChangeCheck();
                string newID = EditorGUILayout.TextField("ID", targetObject.ID);
                if (EditorGUI.EndChangeCheck())
                {
                    // Update the ID directly through SerializedObject
                    SerializedProperty idProp = serializedObject.FindProperty("id");
                    idProp.stringValue = newID;
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.BeginHorizontal();

                // Validate ID button
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Validate ID", GUILayout.Height(25)))
                {
                    targetObject.ValidateID();
                    Repaint();
                }
                GUI.backgroundColor = Color.white;

                // Copy ID button
                if (GUILayout.Button("Copy ID", GUILayout.Width(60), GUILayout.Height(25)))
                {
                    EditorGUIUtility.systemCopyBuffer = targetObject.ID;
                    Debug.Log($"Copied ID to clipboard: {targetObject.ID}");
                }

                EditorGUILayout.EndHorizontal();

                // Help text
                EditorGUILayout.HelpBox("• Edit the ID directly in the text field above\n" +
                                      "• Click 'Validate ID' to check for conflicts and clean formatting\n",
                                      MessageType.Info);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Draw base information section
        protected virtual void DrawBaseInformationSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Base Information", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            tempIcon = (Sprite)EditorGUILayout.ObjectField("Icon", tempIcon, typeof(Sprite), false);
            tempDisplayName = EditorGUILayout.TextField("Display Name", tempDisplayName);
            EditorGUILayout.LabelField("Description");
            tempDescription = EditorGUILayout.TextArea(tempDescription, GUILayout.Height(60));

            if (EditorGUI.EndChangeCheck())
            {
                // Update the actual values using SerializedObject
                SerializedProperty iconProp = serializedObject.FindProperty("icon");
                SerializedProperty displayNameProp = serializedObject.FindProperty("displayName");
                SerializedProperty descriptionProp = serializedObject.FindProperty("description");

                iconProp.objectReferenceValue = tempIcon;
                displayNameProp.stringValue = tempDisplayName;
                descriptionProp.stringValue = tempDescription;

                serializedObject.ApplyModifiedProperties();

                // Schedule asset name update
                EditorApplication.delayCall += () =>
                {
                    if (targetObject != null)
                    {
                        targetObject.GetType().GetMethod("UpdateAssetName",
                            BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(targetObject, null);
                    }
                };
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Render the icon as preview in Project window
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (targetObject.Icon != null)
            {
                Type t = GetType("UnityEditor.SpriteUtility");
                if (t != null)
                {
                    MethodInfo method = t.GetMethod("RenderStaticPreview", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
                    if (method != null)
                    {
                        object ret = method.Invoke("RenderStaticPreview", new object[] { targetObject.Icon, Color.white, width, height });
                        if (ret is Texture2D)
                        {
                            return ret as Texture2D;
                        }
                    }
                }
            }
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        // Helper method to get types from different assemblies
        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }

    // Handle asset renaming when renamed in project window
    [InitializeOnLoad]
    public class BaseScriptableObjectAssetProcessor : AssetPostprocessor
    {
        static BaseScriptableObjectAssetProcessor()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGui;
        }

        private static void OnProjectWindowItemGui(string guid, Rect selectionRect)
        {
            // This will be called for each item in the project window
            // Use this to detect when assets are renamed
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Handle moved/renamed assets
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string newPath = movedAssets[i];
                string oldPath = movedFromAssetPaths[i];

                // Check if this is BaseScriptableObject asset
                Object asset = AssetDatabase.LoadAssetAtPath<BaseScriptableObject>(newPath);
                if (asset is BaseScriptableObject baseScriptableObject)
                {
                    // Extract the new name from the file path (without 'SO' suffix and extension)
                    string fileName = Path.GetFileNameWithoutExtension(newPath);
                    if (fileName.EndsWith("SO") && fileName.Length > 2)
                    {
                        string newDisplayName = fileName.Substring(0, fileName.Length - 2);

                        // Update the display name if it's different
                        if (baseScriptableObject.DisplayName != newDisplayName)
                        {
                            SerializedObject so = new SerializedObject(baseScriptableObject);
                            SerializedProperty displayNameProp = so.FindProperty("displayName");
                            displayNameProp.stringValue = newDisplayName;
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(baseScriptableObject);

                            Debug.Log($"Updated display name for {newPath}: '{baseScriptableObject.DisplayName}' -> '{newDisplayName}'");
                        }
                    }
                }
            }
        }
    }

#endif
}