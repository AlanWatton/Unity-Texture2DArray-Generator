using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class Texture2DArrayGeneratorWindow : EditorWindow
{

    [SerializeField] private List<Texture2D> textures;
    
    [MenuItem("Tools/Texture2DArray Generator")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<Texture2DArrayGeneratorWindow>();
        wnd.titleContent = new GUIContent("Texture2DArray Generator");
        wnd.minSize = new Vector2(250, 250);
    }

    #region Initialization
    public void CreateGUI()
    {
        textures = new List<Texture2D>();

        //Panel
        VisualElement panel = new VisualElement()
        {
            style =
            {
                marginLeft = 10f,
                marginRight = 10f,
                marginTop = 10f,
                marginBottom = 10f
            }
        };

        // Textures List
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty texturesProperty = serializedObject.FindProperty(nameof(textures));

        PropertyField texturesPropertyField = new PropertyField(texturesProperty);
        texturesPropertyField.Bind(serializedObject);
        panel.Add(texturesPropertyField);

        //Buttons Row Box
        GroupBox groupBox = MakeButtonGroupBox();
        
        //Clear Button
        Button clearBtn = MakeButton("ClearButton", "Clear");
        clearBtn.clicked += ClearTextures;
        groupBox.Add(clearBtn);
        
        //Generate Button
        Button generateBtn = MakeButton("GenerateButton", "Generate");
        generateBtn.clicked += GenerateTextureArray;
        groupBox.Add(generateBtn);
        
        //Add Button Box to Panel 
        panel.Add(groupBox);
        
        //Error Message
        Label errorLabel = MakeErrorText("ErrorText");
        panel.Add(errorLabel);
        
        rootVisualElement.Add(panel);
    }

    
    public void OnDestroy()
    {
        Button clearBtn = rootVisualElement.Q<Button>("ClearButton");
        clearBtn.clicked -= ClearTextures;
        
        Button generateBtn = rootVisualElement.Q<Button>("GenerateButton");
        generateBtn.clicked -= GenerateTextureArray;
    }
    #endregion
    
    #region Texture2D Array Generation
    private void GenerateTextureArray()
    {
        textures.RemoveAll(t => t == null);

        if (!TexturesAreValid(textures, out string errorMsg))
        {
            Label errorText = rootVisualElement.Q<Label>("ErrorText");
            errorText.text = errorMsg;
            return;
        }

        Texture2DArray texture2DArray = CreateTextureArray(textures);
        
        // Open the save dialog in the Assets folder
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Asset", 
            "NewTexture2DArray",
            "asset",
            "Save Texture2D Array"
        );
        
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(texture2DArray, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    private static Texture2DArray CreateTextureArray(List<Texture2D> textures)
    {
        int width = textures[0].width;
        int height = textures[0].height;
        int depth = textures.Count;
        TextureFormat textureFormat = textures[0].format;
        
        Texture2DArray textureArray = new Texture2DArray(width, height, depth, textureFormat, true, false);

        for (int i = 0; i < depth; i++)
        {
            Graphics.CopyTexture(textures[i], 0, 0, textureArray, i, 0);
        }

        textureArray.Apply();

        return textureArray;
    }
    #endregion

    #region Clear Textures
    private void ClearTextures()
    {
        textures = new List<Texture2D>();
    }
    #endregion
    
    #region Texture Validation
    private static bool TexturesAreValid(List<Texture2D> textures, out string errorMsg)
    {
        if (textures.Count <= 1)
        {
            errorMsg = $"Min 2 Textures Required, Texture Count ({textures.Count}); ";
            return false;
        }

        if (!TexturesAreSameSize(textures))
        {
            errorMsg = "Textures Should be the same size";
            return false;
        }

        errorMsg = "";
        return true;
    }
    
    private static bool TexturesAreSameSize(List<Texture2D> textures)
    {
        int width = textures[0].width;
        int height = textures[0].height;
        
        for (int x = 1; x < textures.Count; x++)
        {
            if (textures[x].width != width || textures[x].height != height)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Make Window Elements
    private static Button MakeButton(string buttonName, string buttonText)
    {
        return new Button()
        {
            name = buttonName,
            text = buttonText,
            style = { flexGrow = new StyleFloat(1f) }
        };
    }

    private static GroupBox MakeButtonGroupBox()
    {
        return new GroupBox()
        {
            style =
            {
                marginTop = 20f,
                marginBottom = 20f,
                marginLeft = 0f,
                marginRight = 0f,
                paddingLeft = 0f,
                paddingRight = 0f,
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween)
            }
        };
    }

    private static Label MakeErrorText(string labelName)
    {
        return new Label()
        {
            name = labelName,
            style =
            {
                color = new StyleColor(Color.red),
                alignSelf = new StyleEnum<Align>(Align.Center)
            }
        };
    }
    #endregion
}
