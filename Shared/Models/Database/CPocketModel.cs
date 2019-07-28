namespace Shared.Models.Database
{

    public class CPocketModel
    {
        public ulong PPocketMask { get; set; }

        public CPocketModel(ulong _pocketMask)
        {
            PPocketMask = _pocketMask;
        }
    }
}
