using System.Collections.Generic;
using System;
using Common.Models.Documents;

namespace WeSign.Models.Documents
{
    public class SharedAppendixDTO
    {
        public List<int> SignerIndexes{ get; set; }
        public Appendix Appendix { get; set; }
    }
}
