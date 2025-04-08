using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetupTrainingArea : MonoBehaviour
{
    // Add this to make it appear in the Editor menu
    #if UNITY_EDITOR
    [MenuItem("Training Scene/Generate Materials")]
    public static void CreateURPMaterials()
    {
        // Create floor material
        Material floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        floorMaterial.SetColor("_BaseColor", new Color(0.3f, 0.3f, 0.3f, 1.0f));
        floorMaterial.SetFloat("_Metallic", 0.2f);
        floorMaterial.SetFloat("_Smoothness", 0.3f);
        
        // Create wall material
        Material wallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        wallMaterial.SetColor("_BaseColor", new Color(0.7f, 0.7f, 0.8f, 1.0f));
        wallMaterial.SetFloat("_Metallic", 0.1f);
        wallMaterial.SetFloat("_Smoothness", 0.2f);
        
        // Make sure the Materials folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        
        // Save materials
        AssetDatabase.CreateAsset(floorMaterial, "Assets/Materials/FloorMaterial.mat");
        AssetDatabase.CreateAsset(wallMaterial, "Assets/Materials/WallMaterial.mat");
        
        Debug.Log("Materials created successfully in Assets/Materials folder");
    }
    #endif

    // Optional: Method to create a complete training area with walls
    #if UNITY_EDITOR
    [MenuItem("Training Scene/Create Training Area")]
    public static void CreateTrainingArea()
    {
        // First ensure materials exist
        CreateURPMaterials();
        
        // Load the materials we just created
        Material floorMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/FloorMaterial.mat");
        Material wallMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WallMaterial.mat");
        
        // Create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "TrainingFloor";
        floor.transform.localScale = new Vector3(2, 1, 2); // 20x20 meter floor
        floor.GetComponent<Renderer>().material = floorMaterial;
        
        // Create walls
        // North Wall
        GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        northWall.name = "NorthWall";
        northWall.transform.position = new Vector3(0, 2.5f, 10);
        northWall.transform.localScale = new Vector3(20, 5, 0.2f);
        northWall.GetComponent<Renderer>().material = wallMaterial;
        
        // South Wall
        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.name = "SouthWall";
        southWall.transform.position = new Vector3(0, 2.5f, -10);
        southWall.transform.localScale = new Vector3(20, 5, 0.2f);
        southWall.GetComponent<Renderer>().material = wallMaterial;
        
        // East Wall
        GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eastWall.name = "EastWall";
        eastWall.transform.position = new Vector3(10, 2.5f, 0);
        eastWall.transform.localScale = new Vector3(0.2f, 5, 20);
        eastWall.GetComponent<Renderer>().material = wallMaterial;
        
        // West Wall
        GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        westWall.name = "WestWall";
        westWall.transform.position = new Vector3(-10, 2.5f, 0);
        westWall.transform.localScale = new Vector3(0.2f, 5, 20);
        westWall.GetComponent<Renderer>().material = wallMaterial;
        
        // Create parent object
        GameObject trainingArea = new GameObject("TrainingArea");
        floor.transform.parent = trainingArea.transform;
        northWall.transform.parent = trainingArea.transform;
        southWall.transform.parent = trainingArea.transform;
        eastWall.transform.parent = trainingArea.transform;
        westWall.transform.parent = trainingArea.transform;
        
        Debug.Log("Training Area created successfully");
    }
    #endif
}