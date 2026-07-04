using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Reactions/Database")]
public class ReactionDatabase : ScriptableObject
{
    public List<ReactionEntry> reactions;

    // Tìm phản ứng giữa 2 hệ, không phân biệt thứ tự
    public ReactionEntry GetReaction(ElementType a, ElementType b)
    {
        return reactions.FirstOrDefault(r =>
            (r.elementA == a && r.elementB == b) ||
            (r.elementA == b && r.elementB == a));
    }
}

[System.Serializable]
public class ReactionEntry // Đổi từ struct thành class
{
    public ElementType elementA;
    public ElementType elementB;
    public ReactionEffect effect;
    public bool consumeElements;
}