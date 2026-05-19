
namespace CBS.Models
{
    public class CBSSpriteAvatar
    {
        public string ID;
        public bool HasLevelLimit;
        public bool HasPriсe;
        public int LevelLimit;
        public CBSPrice Price;

        public CBSAvatarState ToState()
        {
            return new CBSAvatarState
            {
                ID = this.ID,
                HasLevelLimit = this.HasLevelLimit,
                HasPriсe = this.HasPriсe,
                LevelLimit = this.LevelLimit,
                Price = this.Price
            };
        }
    }
}
