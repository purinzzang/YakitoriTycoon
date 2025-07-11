using UnityEngine;

[CreateAssetMenu(fileName = "YakitoriData", menuName = "Yakitori/Yakitori Data")]
public class YakitoriData : ScriptableObject
{
    public int index;             // 0~8: �ε��� (Momo+Sweet = 0 ...)
    public string displayName;    // "���� �߲�ġ"
    public int price;             // �� ����
    public Sprite icon;           // UI�� �̹���
}
