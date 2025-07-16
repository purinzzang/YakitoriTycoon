using UnityEngine;

[CreateAssetMenu(fileName = "YakitoriData", menuName = "Yakitori/Yakitori Data")]
public class YakitoriData : ScriptableObject
{
    public int index;             
    public string displayName;    
    public int price;             
    public Yakitori prefab;
    public RuntimeAnimatorController ac;
}
