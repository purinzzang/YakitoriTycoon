using UnityEngine;

[CreateAssetMenu(fileName = "YakitoriData", menuName = "Yakitori/Yakitori Data")]
public class YakitoriData : ScriptableObject
{
    public int index;             // 0~8: ÀÎµ¦½º (Momo+Sweet = 0 ...)
    public string displayName;    // "´ÞÄÞ ´ß²¿Ä¡"
    public int price;             // ÃÑ °¡°Ý
    public Sprite icon;           // UI¿ë ÀÌ¹ÌÁö
}
