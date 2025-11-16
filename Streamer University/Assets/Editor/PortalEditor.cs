using UnityEditor;

[CustomEditor(typeof(Portal)), CanEditMultipleObjects]
public class PortalEditor : Editor
{
    public enum DisplayCategory
    {
       Speed, Gravity
    }
    public DisplayCategory categoryToDisplay;

    bool FirstTime = true;

    public override void OnInspectorGUI()
    {
        if (FirstTime)
        {
            switch (serializedObject.FindProperty("State").intValue)
            {
                case 0:
                    categoryToDisplay = DisplayCategory.Speed;
                    break;
                case 1:
                    categoryToDisplay = DisplayCategory.Gravity;
                    break;
            }
        }
        else
            categoryToDisplay = (DisplayCategory)EditorGUILayout.EnumPopup("Display", categoryToDisplay);

        EditorGUILayout.Space();

        switch (categoryToDisplay)
        {
            case DisplayCategory.Gravity:
                DisplayProperty("gravity", 1);
                break;

            case DisplayCategory.Speed:
                DisplayProperty("Speed", 0);
                break;

        }

        FirstTime = false;

        serializedObject.ApplyModifiedProperties();
    }

    void DisplayProperty(string property, int PropNumb)
    {
        try
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(property));
        }
        catch
        { }
        serializedObject.FindProperty("State").intValue = PropNumb;
    }
}