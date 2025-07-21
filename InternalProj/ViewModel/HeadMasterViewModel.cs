using InternalProj.Models;
using System.Collections.Generic;

namespace InternalProj.ViewModel
{
    public class HeadMasterViewModel
    {
        public IEnumerable<MainHeadReg> MainHeads { get; set; } = new List<MainHeadReg>();
        public IEnumerable<SubHeadDetails> SubHeads { get; set; } = new List<SubHeadDetails>();
        public IEnumerable<ChildSubHead> ChildSubHeads { get; set; } = new List<ChildSubHead>();
        public IEnumerable<AlbumSizeDetails> AlbumSizes { get; set; } = new List<AlbumSizeDetails>();
        public IEnumerable<RateMaster> Rates { get; set; } = new List<RateMaster>();
    }    
}
