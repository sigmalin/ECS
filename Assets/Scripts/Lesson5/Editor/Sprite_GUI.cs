using UnityEditor;
using UnityEngine;

public class Sprite_GUI : ShaderGUI 
{
	MaterialProperty _MainTex = null;
	MaterialProperty _Column = null;
	MaterialProperty _Row = null;

	MaterialProperty _diff_U = null;
	MaterialProperty _diff_V = null;

	public void FindProperties(MaterialProperty[] props) 
	{
		_MainTex = FindProperty("_MainTex", props, false);
		_Column = FindProperty("_Column", props, false);
		_Row = FindProperty("_Row", props, false);

		_diff_U = FindProperty("_diff_U", props, false);
		_diff_V = FindProperty("_diff_V", props, false);
	}

	bool init = false;
    public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background) 
	{
        base.OnMaterialPreviewGUI(materialEditor, r, background);
        if (init) return;
        RefreshHideProperty((Material)materialEditor.target);
        init = true;
    }

	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props) 
	{
        FindProperties(props);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(-7);
        EditorGUILayout.BeginVertical();
        EditorGUI.BeginChangeCheck();
        DrawGUI(materialEditor);
        if(EditorGUI.EndChangeCheck())
		{
            var material = (Material)materialEditor.target;
            EditorUtility.SetDirty(material);
            RefreshHideProperty(material);
        }
        EditorGUILayout.EndVertical();
        GUILayout.Space(1);
        EditorGUILayout.EndHorizontal();
        //base.OnGUI(materialEditor, props);
    }

	static readonly GUIContent mainTexContent = new GUIContent("Main Texture ");

	public void DrawGUI(MaterialEditor materialEditor) 
	{
		materialEditor.TexturePropertySingleLine(mainTexContent, _MainTex);

		materialEditor.ShaderProperty(_Column, _Column.displayName);
		materialEditor.ShaderProperty(_Row, _Row.displayName);
	}

	void RefreshHideProperty(Material _material)
	{
		float column = _material.GetFloat("_Column");
		_material.SetFloat("_diff_U", column == 0f ? 0f : 1f/column );

		float row = _material.GetFloat("_Row");
		_material.SetFloat("_diff_V", row == 0f ? 0f : 1f/row );
	}
}
