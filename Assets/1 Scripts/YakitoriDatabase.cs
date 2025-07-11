using UnityEngine;

[CreateAssetMenu(fileName = "YakitoriDatabase", menuName = "Yakitori/Yakitori Database")]
public class YakitoriDatabase : ScriptableObject
{
    public YakitoriData[] yakitoriList;

    public YakitoriData GetYakitori(int index)
    {
        if (index >= 0 && index < yakitoriList.Length)
            return yakitoriList[index];
        return null;
    }
}
