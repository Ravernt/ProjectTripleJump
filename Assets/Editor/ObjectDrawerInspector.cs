using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectDrawer))]
public class ObjectDrawerInspector : Editor
{
    ObjectDrawer drawer;
    GameObject lastCreated;
    bool isCreating;

    private void OnEnable()
    {
        drawer = (ObjectDrawer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Start Drawing"))
        {
            CreateObject();
        }
    }

    public void CreateObject()
    {
        Debug.Log("menu item selected");

        isCreating = true;
        lastCreated = null;

        // Add a callback for SceneView update
        SceneView.duringSceneGui -= UpdateSceneView;
        SceneView.duringSceneGui += UpdateSceneView;
    }

    void UpdateSceneView(SceneView sceneView)
    {
        if (lastCreated)
        {
            // Keep lastCreated focused
            Selection.activeGameObject = lastCreated;
        }

        if (isCreating)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                Vector3 pointsPos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

                //Todo create object here at pointsPos
                lastCreated = Instantiate(drawer.prefab);
                lastCreated.transform.position = new(pointsPos.x, pointsPos.y, 0);

                // Avoid the current event being propagated
                // I'm not sure which of both works better here
                Event.current.Use();
                Event.current = null;

                // Keep the created object in focus
                Selection.activeGameObject = lastCreated;
            }
            else if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                // exit creation mode
                isCreating = false;
            }
        }
        else
        {
            // Skip if event is Layout or Repaint
            if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
            {
                Selection.activeGameObject = lastCreated;
                return;
            }

            // Prevent Propagation
            Event.current.Use();
            Event.current = null;
            Selection.activeGameObject = lastCreated;
            lastCreated = null;

            // Remove the callback
            SceneView.duringSceneGui -= UpdateSceneView;
        }
    }
}
