using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

public class PhysBoneCopierWindow : EditorWindow
{
    [SerializeField] private GameObject targetGameObject;  // 元のGameObject
    [SerializeField] private GameObject searchRoot;        // 探索するGameObject
    
    private readonly string AvatarDynamicsGameObjectName = "AvatarDynamics";
    private readonly string PhysboneGameObjectName = "PB";
    private readonly string PhysboneColliderGameObjectName = "PB_Collider";
    
    [MenuItem("Tools/Phys Bone Copier")]
    public static void ShowWindow()
    {
        GetWindow<PhysBoneCopierWindow>("Phys Bone Copier");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select GameObject", EditorStyles.boldLabel);
        targetGameObject = (GameObject)EditorGUILayout.ObjectField("Target GameObject", targetGameObject, typeof(GameObject), true);
        searchRoot = (GameObject)EditorGUILayout.ObjectField("Search Root GameObject", searchRoot, typeof(GameObject), true);

        if (GUILayout.Button("Copy VRC Phys Bones"))
        {
            if (targetGameObject&& searchRoot)
            {
                CopyPhysBones(targetGameObject, searchRoot);
            }
            else
            {
                Debug.LogWarning("Please assign both Target and Search Root GameObjects.");
            }
        }
    }

    private void CopyPhysBones(GameObject parent, GameObject searchRoot)
    {
        // Create PB GameObject under the target GameObject
        GameObject avatarDynamics = new GameObject(AvatarDynamicsGameObjectName);
        avatarDynamics.transform.SetParent(parent.transform);
        GameObject pbParent = new GameObject(PhysboneGameObjectName);
        pbParent.transform.SetParent(avatarDynamics.transform);
        GameObject pbColliderParent = new GameObject(PhysboneColliderGameObjectName);
        pbColliderParent.transform.SetParent(avatarDynamics.transform);

        // Start searching for VRC Phys Bones under the searchRoot
        CopyPhysBonesRecursive(searchRoot.transform, pbParent.transform);
        CopyPhysboneColliderRecursive(searchRoot.transform, pbColliderParent.transform);
    }
    
    private void CopyComponentsRecursive<T>(Transform current, Transform parent) where T : Component
    {
        foreach (Transform child in current)
        {
            T component = child.GetComponent<T>();
            if (component)
            {
                GameObject newGameObject = new GameObject(child.name);
                newGameObject.transform.SetParent(parent);
                T newComponent = newGameObject.AddComponent<T>();
                CopyComponent(component, newComponent);

                if (typeof(T) == typeof(VRCPhysBone))
                {
                    ((VRCPhysBone)(object)newComponent).rootTransform = child.transform;
                }
                else if (typeof(T) == typeof(VRCPhysBoneCollider))
                {
                    ((VRCPhysBoneCollider)(object)newComponent).rootTransform = child.transform;
                }
            }
            CopyComponentsRecursive<T>(child, parent);
        }
    }

    private void CopyPhysBonesRecursive(Transform current, Transform pbParent)
    {
        CopyComponentsRecursive<VRCPhysBone>(current, pbParent);
    }

    private void CopyPhysboneColliderRecursive(Transform current, Transform pbColliderParent)
    {
        CopyComponentsRecursive<VRCPhysBoneCollider>(current, pbColliderParent);
    }

    void CopyComponent(Component source, Component target)
    {
        var type = source.GetType();

        // Copy the enabled state
        var enabledProperty = type.GetProperty("enabled");
        if (enabledProperty != null)
        {
            enabledProperty.SetValue(target, enabledProperty.GetValue(source));
        }
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }
    }
}